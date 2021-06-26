module GameSetup

open Shared
open Shared.Domain
open ClientSpecificModels

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
