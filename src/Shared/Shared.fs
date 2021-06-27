namespace Shared

open Domain
open Dto
open Events


module ApiSubRoute =
    let builder typeName methodName =
        sprintf "/%s/%s" typeName methodName

module Bridge =
    let endpoint = "/live-game"

module Route =
    let builder typeName methodName =
        sprintf "/api%s" (ApiSubRoute.builder typeName methodName)

type GetOrCreateGameRequest =
    {
        PlayerId: string
        GameId: string
    }

type ICardGameApi =
    {
        getCurrentLoggedInPlayer : unit -> Async<Result<Player,string>>
        getPlayers : unit -> Async<Player seq>
        getPlayer: string -> Async<Result<Player, string>>
        getCards: unit -> Async<Card seq>
        getCard: string -> Async<Result<Card,string>>
        getDecks: unit -> Async<DeckDto seq>
        getCardsForDeck: string ->  Async<Card seq>
        getOrCreateGame: GetOrCreateGameRequest -> Async<Result<Option<GameState> * ClientInternalMsg * GameId * PlayerId, string>>
    }

module GameSetup =

    let createCardInstanceForCard (card : Card) =
            let cardInstanceId = NonEmptyString.build (System.Guid.NewGuid().ToString()) |> Result.map CardInstanceId

            match cardInstanceId with
            | Ok id ->
                Ok {
                        CardInstanceId  =  id
                        Card =  card
                }
            | _ ->
                (sprintf "Unable to create card instance for %s" (card.ToString())) |> Error

    let testDeckSeqGenerator (getDecks : unit -> Async<DeckDto seq>) (getCardsForDeck : string ->  Async<Card seq>) (numberOfCards :int) =
            async {
              let! values = getDecks ()
              let! cards = values |> CollectionManipulation.shuffleG |> Seq.head |> (fun x -> x.DeckId) |> getCardsForDeck

              return cards
                |> Seq.map createCardInstanceForCard
                |> CollectionManipulation.selectAllOkayResults
            }

