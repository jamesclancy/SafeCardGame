module Config

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open Microsoft.Extensions.Configuration
open Saturn
open Shared.Domain
open DatabaseRepositories

type Config = {
    connectionString : string
    currentPlayer: Option<Player>
}

let playerInformationFromContext (ctx :HttpContext) =
    let nameClaim = Seq.filter (fun (x: System.Security.Claims.Claim) -> x.Type = "playerName") ctx.User.Claims |> Seq.toList
    let idClaim = Seq.filter (fun (x: System.Security.Claims.Claim) -> x.Type = "playerId") ctx.User.Claims |> Seq.toList

    match nameClaim, idClaim with
    | [ x ], [ y ] -> Some (x.Value, y.Value)
    | [] , [y] -> Some (y.Value, y.Value)
    | _,_ -> None

let getConfigFromContext (ctx : HttpContext) : Config =
    let settings = ctx.GetService<IConfiguration>()
    let connectionString = settings.["ConnectionString"]

    match playerInformationFromContext ctx with
    | None ->
        {
            connectionString = connectionString
            currentPlayer = None
        }
    | Some (x, y) ->
        async {

            let playerRepo = PlayerRepository ()

            match! Players.Database.getById connectionString x with
            | Ok (Some s) ->
                return {
                    connectionString = connectionString
                    currentPlayer = Some s
                }
            | Error ex ->
                return {
                    connectionString = connectionString
                    currentPlayer = None
                }

            | Ok None ->
                return {
                    connectionString = connectionString
                    currentPlayer = None
                }
        } |> Async.RunSynchronously
