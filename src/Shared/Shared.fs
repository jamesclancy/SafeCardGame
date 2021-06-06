namespace Shared

open System
open System.IO
open Domain

type Todo =
    { Id : Guid
      Description : string }


module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s" methodName
        //sprintf "/api/%s/%s" typeName methodName

type ITodosApi =
    { getTodos : unit -> Async<Todo list>
      addTodo : Todo -> Async<Todo> }


type ICardGameApi =
    {
        getPlayers : unit -> Async<Result<Player, string> seq>
        getPlayer: string -> Async<Result<Player, string>>
        getCards: unit -> Async<Result<Card, string> seq>
        getCard: string -> Async<Result<Card, string>>
    }