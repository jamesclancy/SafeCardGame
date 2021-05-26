module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Shared.Domain
open Shared.Domain
open Shared.Domain
open Events


let todosApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let createPlayer playerIdStr playerName playerCurrentLife playerPlaymatUrl =
    let playerId = NonEmptyString.build playerIdStr |> Result.map PlayerId
    let playerPlaymatUrl = ImageUrlString.build playerPlaymatUrl

    match playerId, playerPlaymatUrl with
    | Ok s, Ok pm ->
        Ok {
           PlayerId = s
           Name = playerName
           RemainingLifePoints = playerCurrentLife
           PlaymatUrl = pm
        }
    | _ -> Error "Unable to create player"


let testCardGenerator cardInstanceIdStr cardIdStr cardImageUrlStr =
    let cardInstanceId = NonEmptyString.build cardInstanceIdStr |> Result.map CardInstanceId
    let cardId = NonEmptyString.build cardInstanceIdStr |> Result.map CardId
    let cardImageUrl = ImageUrlString.build cardImageUrlStr

    match cardInstanceId, cardId, cardImageUrl with
    | Ok id, Ok cid, Ok imgUrl ->
        let creature =
          {
            Health= 95
            Weaknesses=  List.empty
            Attach = List.empty
          }
        let card =
            {
                CardId = cid
                ResourceCost = [ Resource.Grass, 4;
                                 Resource.Colorless, 1 ] |> Seq.ofList |> ResourcePool
                Name = cardIdStr
                EnterSpecialEffects = None
                ExitSpecialEffects = None
                PrimaryResource = Resource.Grass
                Creature = creature
                ImageUrl = imgUrl
                Description = "A rare creature with stange and powers."
            }
        Ok  {
                CardIntanceId  =  id
                Card =  card |> CharacterCard
            }
    | _, _, _ ->
        sprintf "Unable to create card instance for %s\t%s" cardInstanceIdStr cardIdStr
        |> Error

let testCardSeqGenerator (numberOfCards : int) =
    seq { 0 .. (numberOfCards - 1) }
    |> Seq.map (fun x -> testCardGenerator
                            (sprintf "ExcitingCharacter%i" x)
                            (sprintf "Exciting Character #%i" x)
                            (sprintf "https://picsum.photos/320/200?%i" x))
    |> Seq.map (fun x ->
                        match x with
                        | Ok s -> [ s ] |> Ok
                        | Error e -> e |> Error)
    |> Seq.fold (fun x y ->
                    match x, y with
                    | Ok accum, Ok curr -> curr @ accum |> Ok
                    | _,_ -> "Eating Errors lol" |> Error
                ) (Ok List.empty)

let inPlayCreatureGenerator inPlayCreatureIdStr cardInstanceIdStr cardIdStr cardImageUrlStr =
        let inPlayCreatureId = NonEmptyString.build inPlayCreatureIdStr |> Result.map InPlayCreatureId
        let card = testCardGenerator cardInstanceIdStr cardIdStr cardImageUrlStr

        match inPlayCreatureId, card with
        | Ok id, Ok c ->
            Ok {
                InPlayCharacterId=  id
                Card = c.Card
                CurrentDamage=  0
                SpecialEffect=  None
                AttachedEnergy = [ Resource.Grass, 4;
                                     Resource.Colorless, 1 ] |> Seq.ofList |> ResourcePool
                SpentEnergy = [ Resource.Grass, 4;
                                     Resource.Colorless, 1 ] |> Seq.ofList |> ResourcePool
            }
        | _, _ -> "Unable to create in play creature." |> Error


let inPlayCreatureSeqGenerator (numberOfCards : int) =
    seq { 0 .. (numberOfCards - 1) }
    |> Seq.map (fun x -> inPlayCreatureGenerator
                            (sprintf "ExcitingCharacter%i" x)
                            (sprintf "ExcitingCharacter%i" x)
                            (sprintf "Exciting Character #%i" x)
                            (sprintf "https://picsum.photos/320/200?%i" x))
    |> Seq.map (fun x ->
                        match x with
                        | Ok s -> [ s ] |> Ok
                        | Error e -> e |> Error)
    |> Seq.fold (fun x y ->
                    match x, y with
                    | Ok accum, Ok curr -> curr @ accum |> Ok
                    | _,_ -> "Eating Errors lol" |> Error
                ) (Ok List.empty)

let playerBoard (player : Player) =
    let deckTemp =  testCardSeqGenerator 35
    let handTemp = testCardSeqGenerator 3

    let activeCreature = inPlayCreatureGenerator "InPlayCharacter" "InPlayCharacter" "InPlayCharacter" (sprintf "https://picsum.photos/320/200?%s" (System.Guid.NewGuid().ToString()))
    let benchCreatures = inPlayCreatureSeqGenerator 4
    match deckTemp, handTemp, activeCreature, benchCreatures with
    | Ok deck, Ok hand, Ok cre, Ok ben ->
        Ok  {
                PlayerId=  player.PlayerId
                Deck= {
                    TopCardsExposed = 0
                    Cards =  deck
                }
                Hand=
                    {
                        Cards = hand
                    }
                ActiveCreature= Some cre
                Bench=  Some ben
                DiscardPile= {
                    TopCardsExposed = 0
                    Cards = List.empty
                }
                TotalResourcePool= ResourcePool Seq.empty
                AvailableResourcePool =  ResourcePool Seq.empty
            }
    | _,_, _,_ -> "Error creating deck or hand" |> Error


let init =
    let player1 = createPlayer "Player1" "Player1" 10 "https://picsum.photos/id/1000/2500/1667?blur=5"
    let player2 = createPlayer "Player2" "Player2" 10 "https://picsum.photos/id/10/2500/1667?blur=5"
    let gameId =  NonEmptyString.build "GameIDHere" |> Result.map GameId

    match player1, player2, gameId with
    | Ok p1, Ok p2, Ok g ->
        let playerBoard1 = playerBoard p1
        let playerBoard2 = playerBoard p2
        match playerBoard1, playerBoard2 with
        | Ok pb1, Ok pb2 ->
          let model : GameState =
            {
                GameId = g
                Players =  [
                            p1.PlayerId, p1;
                            p2.PlayerId, p2
                           ] |> Map.ofList
                Boards =   [
                            pb1.PlayerId, pb1;
                            pb2.PlayerId, pb2
                           ] |> Map.ofList
                NotificationMessages = None
                CurrentStep =  (p1.PlayerId |> Attack)
                TurnNumber = 1
                CurrentPlayer = p1.PlayerId
                OpponentPlayer = p2.PlayerId
            }
          let cmd = Cmd.ofMsg GameStarted
          Ok (model, cmd)
        | _ -> "Failed to create player boards" |> Error
    | _ -> "Failed to create players" |> Error

let update (msg: Msg) (model: GameState): GameState * Cmd<Msg> =
    match msg with
    | GameStarted ->
        model, Cmd.none


let view (model : GameState) (dispatch : Msg -> unit) =
    PageLayoutParts.mainLayout model dispatch


