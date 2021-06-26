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

open Farmer

type ConnectionState =
  | Connected of PlayerId * GameId
  | Disconnected

let connections = ServerHub<ConnectionState, ServerMsg, ClientInternalMsg>().RegisterServer(RS)

let update (conf : Config.Config) clientDispatch msg state =
    match msg with
    | RS msg ->
        match msg with
        | Connect (x, y)->

            connections.SendClientIf (fun connectionState ->
                                            match connectionState with
                                            | Connected (x, y) -> false
                                            |_ -> true) ( (x, y)  |> GameAvailable)

            (x,y) |> Connected, Cmd.none
        | ServerCommand (m,gs) ->
            let (x, y) = (migrateGamesStateAndGetNewCommandsFromCommand gs m)

            match m with
            | StartGame sg ->
                    connections.SendClientIf (fun connectionState ->
                                            match connectionState with
                                            | Connected (x, y) -> false
                                            |_ -> true) ( sg.GameId |> GameNoLongerAvailable) |> ignore

            connections.SendClientIf (fun connectionState ->
                                            match connectionState with
                                            | Connected (x, y) when y = gs.GameId -> true
                                            |_ -> false) ( x  |> UpdatedModelForClient)

            let cmd : Cmd<ServerMsg> = Cmd.OfAsync.perform (Games.Database.updateGameState conf.connectionString) x (fun _ ->
                match y with
                | Some prom ->  (prom, x) |> ServerCommand |> RS
                | None -> StatePersistedToIO )

            state, cmd
    | Closed ->
        Disconnected, Cmd.none

let init _ () = Disconnected, Cmd.none

let socketServer (conf : Config.Config) : HttpHandler =
    Bridge.mkServer "" init (update conf)
    |> Bridge.register RS
    |> Bridge.whenDown Closed
    |> Bridge.withServerHub connections
    |> Bridge.run Giraffe.server

let socketServerRouter  = router {
    forward "/socket" (fun next ctx ->
        let conf = Config.getConfigFromContext ctx
        socketServer conf next ctx
    )


}