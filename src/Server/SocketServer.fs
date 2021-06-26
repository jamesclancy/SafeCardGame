module SocketServer

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Players
open Saturn
open Giraffe

open Shared
open DatabaseRepositories
open System
open FSharp.Control.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication
open Events
open Elmish.Bridge

open Microsoft.Extensions.Logging
open Elmish
open Elmish.Bridge
open Shared
open Events

open GameStateTransitions
open Shared.Domain

type ConnectionState =
  | Connected of PlayerId * GameId
  | Disconnected

let connections = ServerHub<ConnectionState, ServerMsg, ClientInternalMsg>().RegisterServer(RS)

let update clientDispatch msg state =
    match msg with
    | RS msg ->
        match msg with
        | Connect (x, y)->
            Console.WriteLine("Connected {0},{1}", x.ToString(), y.ToString())
            (x,y) |> Connected, Cmd.none
        | ServerCommand (m,gs) ->
            let (x, y) = (migrateGamesStateAndGetNewCommandsFromCommand gs m)
            connections.SendClientIf (fun connectionState ->
                                            match connectionState with
                                            | Connected (x, y) when y = gs.GameId -> true
                                            |_ -> false) ( x  |> UpdatedModelForClient)

            state, Cmd.none
    | Closed ->
        Disconnected, Cmd.none

let init _ () = Disconnected, Cmd.none

let socketServer : HttpHandler =
    Bridge.mkServer "" init update
    |> Bridge.register RS
    |> Bridge.whenDown Closed
    |> Bridge.withServerHub connections
    |> Bridge.run Giraffe.server

let socketServerRouter = router {
    forward "/socket" socketServer
}