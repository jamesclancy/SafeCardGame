module GameSetup
open Shared
open Shared.Domain

let testCreatureCardGenerator (cardGameServer : ICardGameApi)  cardInstanceIdStr =
        async {
            let cardInstanceId = NonEmptyString.build cardInstanceIdStr |> Result.map CardInstanceId

            match cardInstanceId with
            | Ok id ->
                let! card = cardGameServer.getCards ()

                return Ok  {
                        CardInstanceId  =  id
                        Card =  card |> CollectionManipulation.shuffleG |> Seq.head
                    }
            | _ ->
                return (sprintf "Unable to create card instance for %s" cardInstanceIdStr) |> Error
        }

let testResourceCardGenerator cardInstanceIdStr =
        async {
        let cardInstanceId = NonEmptyString.build cardInstanceIdStr |> Result.map CardInstanceId

        match cardInstanceId with
        | Ok id ->
            let card = SampleCardDatabase.resourceCardDb |> CollectionManipulation.shuffleG |> Seq.head
            return Ok  {
                    CardInstanceId  =  id
                    Card =  card |> ResourceCard
                }
        | _ ->
            return (sprintf "Unable to create card instance for %s" cardInstanceIdStr) |> Error
        }

let createRandomCardForSequence  (cardGameServer : ICardGameApi)  x =
                            if x % 2 = 1 then
                                testCreatureCardGenerator cardGameServer (sprintf "cardInstance-%i" x)
                            else
                                testResourceCardGenerator (sprintf "cardInstance-%i" x)

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

let testDeckSeqGenerator (cardGameServer : ICardGameApi) (numberOfCards :int) =
        async {
          let! values = cardGameServer.getDecks ()
          let! cards = values |> CollectionManipulation.shuffleG |> Seq.head |> (fun x -> x.DeckId) |> cardGameServer.getCardsForDeck

          return cards
            |> Seq.map createCardInstanceForCard
            |> CollectionManipulation.selectAllOkayResults
        }

