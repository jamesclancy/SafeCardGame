module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared
open System.Text.Json
open Dto
open Shared.Domain
open DatabaseRepositories

type Storage () =
    let todos = ResizeArray<_>()

    member __.GetTodos () =
        List.ofSeq todos

    member __.AddTodo (todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok ()
        else Error "Invalid todo"

let storage = Storage()
let playerRepository = PlayerRepository()
let cardRepository = CardRepository()


let gameApi : ICardGameApi =
    {
        getPlayers =fun () -> async { return playerRepository.GetAll() }
        getPlayer = fun playerId -> async { return playerRepository.Get(playerId) }
        getCards =fun () -> async { return cardRepository.GetAll() }
        getCard = fun cardId -> async { return cardRepository.Get(cardId) }
    }

let todosApi =
    { getTodos = fun () -> async { return storage.GetTodos() }
      addTodo =
        fun todo -> async {
            match storage.AddTodo todo with
            | Ok () -> return todo
            | Error e -> return failwith e
        } }

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
