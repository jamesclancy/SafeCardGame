module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Shared.Domain
open Shared.Domain


type Msg =
    | GameStarted

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

let playerBoard (player : Player) =
    {
            PlayerId=  player.PlayerId
            Deck= {
                TopCardsExposed = 0
                Cards =  List.empty
            }
            Hand= { Cards = List.empty }
            ActiveCreature= None
            Bench=  None
            DiscardPile= {
                TopCardsExposed = 0
                Cards = List.empty
            }
            TotalResourcePool= ResourcePool Seq.empty
            AvailableResourcePool =  ResourcePool Seq.empty
    }


let init =
    let player1 = createPlayer "Player1" "Player1" 10 "https://picsum.photos/id/1000/2500/1667?blur=5"
    let player2 = createPlayer "Player2" "Player2" 10 "https://picsum.photos/id/10/2500/1667?blur=5'"

    match player1, player2 with
    | Ok p1, Ok p2 ->
        let playerBoard1 = playerBoard p1
        let playerBoard2 = playerBoard p2
        let model =
            {
                Players =  [
                            p1.PlayerId, p1;
                            p2.PlayerId, p2
                           ] |> Map.ofList
                Boards =   [
                            playerBoard1.PlayerId, playerBoard1;
                            playerBoard2.PlayerId, playerBoard2
                           ] |> Map.ofList
                CurrentTurn = Some p1.PlayerId
                CurrentStep=  Attack
                TurnNumber = 1
                CurrentPlayer = p1.PlayerId
                OpponentPlayer = p2.PlayerId
            }
        let cmd = Cmd.ofMsg GameStarted
        Ok (model, cmd)
    | _ -> "Failed to create players" |> Error

let update (msg: Msg) (model: GameState): GameState * Cmd<Msg> =
    match msg with
    | GameStarted ->
        model, Cmd.none


let view (model : GameState) (dispatch : Msg -> unit) =
    PageLayoutParts.mainLayout model dispatch


type FailedStartupModel =FailedStartupModel of string
let startUpErrorInit () =
    let model = FailedStartupModel "Failed to start"
    let cmd = Cmd.ofMsg GameStarted
    model, cmd

let startUpErrorUpdate (msg: Msg) (model: FailedStartupModel): FailedStartupModel * Cmd<Msg> =
    match msg with
    | GameStarted ->
        model, Cmd.none


let startUpErrorView (model : FailedStartupModel) (dispatch : Msg -> unit) =
    Fable.React.Standard.strong [] [ Fable.React.Helpers.str "Start up error" ]
