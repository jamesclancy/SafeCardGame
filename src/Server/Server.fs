module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared
open System.Text.Json
open Dto
open Shared.Domain
open DatabaseRepositories

let playerRepository = PlayerRepository()
let cardRepository = CardRepository()

let gameApi : ICardGameApi =
    {
        getPlayers = fun () ->  playerRepository.GetAll()
        getPlayer = fun playerId ->  playerRepository.Get(playerId)
        getCards = fun () -> cardRepository.GetAll()
        getCard = fun cardId -> cardRepository.Get(cardId)
    }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue gameApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
