namespace Shared

open Domain
open Dto


module ApiSubRoute =
    let builder typeName methodName =
        sprintf "/%s/%s" typeName methodName

module Route =
    let builder typeName methodName =
        sprintf "/api%s" (ApiSubRoute.builder typeName methodName)

type ICardGameApi =
    {
        getPlayers : unit -> Async<Player seq>
        getPlayer: string -> Async<Result<Player, string>>
        getCards: unit -> Async<Card seq>
        getCard: string -> Async<Result<Card,string>>
        getDecks: unit -> Async<PreCreatedDeckDto seq>
        getCardsForDeck: string ->  Async<Card seq>
    }