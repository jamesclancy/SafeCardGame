module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Shared.Domain
open Operators
open Events
open GameSetup
open GameStateTransitions

let cardGameServer =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ICardGameApi>


let createInitiallyGameStateFromServer gameId player1Id player2Id =
    async {

          let! player1 = cardGameServer.getPlayer(player1Id)
          let! player2 = cardGameServer.getPlayer(player2Id)
          let! deck1 = testDeckSeqGenerator cardGameServer 60
          let! deck2 = testDeckSeqGenerator cardGameServer 60

          match player1, player2 with
          | Ok p1, Ok p2 ->
               return {
                    GameId = gameId
                    Players =  [
                                p1.PlayerId, p1;
                                p2.PlayerId, p2
                               ] |> Map.ofList
                    PlayerOne = p1.PlayerId
                    PlayerTwo = p2.PlayerId
                    Decks = [   (p1.PlayerId, { TopCardsExposed = 0; Cards = deck1 });
                                (p2.PlayerId, { TopCardsExposed = 0; Cards = deck2 })]
                             |> Map.ofSeq
                 } |> StartGame
          | _, _ ->
                    return {
                        GameId =  gameId
                        Winner = None
                        Message = None
                    } |> GameWon
    }

let init =
    let player1 = createPlayer "Player1" "Player1" 10 "https://picsum.photos/id/1000/2500/1667?blur=5"
    let player2 = createPlayer "Player2" "Player2" 10 "https://picsum.photos/id/10/2500/1667?blur=5"
    let gameId =  NonEmptyString.build "GameIDHere" |> Result.map GameId

    match player1, player2, gameId with
    | Ok p1, Ok p2, Ok g ->
        let playerBoard1 = SampleCardDatabase.emptyPlayerBoard p1
        let playerBoard2 = SampleCardDatabase.emptyPlayerBoard p2
        match playerBoard1, playerBoard2 with
        | Ok pb1, Ok pb2 ->
          let model : GameState =
            {
                GameId = g
                Players =  [
                    p1.PlayerId, p1;
                    p2.PlayerId, p2
                   ] |> Map.ofList
                Boards = [
                    p1.PlayerId, pb1;
                    p2.PlayerId, pb2
                   ] |> Map.ofList
                NotificationMessages = None
                CurrentStep =  NotCurrentlyPlaying
                TurnNumber = 0
                PlayerOne = p1.PlayerId
                PlayerTwo = p2.PlayerId
            }
          let cmd = Cmd.OfAsync.result (createInitiallyGameStateFromServer g "001" "002")
          Ok (model, cmd )
        | _ -> "Failed to create player boards" |> Error
    | _ -> "Failed to create players" |> Error


let extractGameWonCommandAfterAttack players (gs : GameState) =
        let deceasedPlayers = Map.toList players
                                |> List.filter (fun (x,y) -> y.RemainingLifePoints <=0 )

        match deceasedPlayers with
        | [] ->
            Cmd.none
        | [ (x, y) ] ->
            Cmd.ofMsg (({
                                    GameId= gs.GameId
                                    Winner= Some (getTheOtherPlayer gs x)
                                    Message= None
            } : GameWonEvent) |> GameWon)
        | _ ->
            Cmd.ofMsg ({GameId= gs.GameId; Winner=None; Message= None} |> GameWon)

let update (msg: Msg) (model: GameState): GameState * Cmd<Msg> =
    match msg with
    | StartGame ev ->
        initializeGameStateFromStartGameEvent ev, Cmd.none
    | DrawCard  ev ->
        modifyGameStateFromDrawCardEvent ev model, Cmd.none
    | DiscardCard ev ->
        modifyGameStateFromDiscardCardEvent ev model, Cmd.none
    | ToggleZoomOnCard ev ->
        modifyGameStateFromToggleZoomOnCardEvent ev model, Cmd.none
    | PlayCard ev ->
        modifyGameStateFromPlayCardEvent ev model, Cmd.none
    | EndPlayStep ev ->
        { model with CurrentStep = (Attack ev.PlayerId)}, Cmd.none
    | PerformAttack  ev ->
        let newModel = modifyGameStateFromPerformAttackEvent ev model
        let cmd = extractGameWonCommandAfterAttack newModel.Players newModel
        newModel, cmd
    | SkipAttack ev ->
        { model with CurrentStep = (Reconcile ev.PlayerId)}, Cmd.none
    | EndTurn ev ->
        modifyGameStateTurnToOtherPlayer ev.PlayerId model, Cmd.none
    | DeleteNotification dn ->
        removeNotificationFromGameState model dn, Cmd.none
    | GameWon ev ->
        let newStep =  { WinnerId =  ev.Winner; Message = formatGameOverMessage ev.Message } |> GameOver
        { model with CurrentStep = newStep}, Cmd.none
    | SwapPlayer ->
        { model with PlayerOne = model.PlayerTwo; PlayerTwo = model.PlayerOne }, Cmd.none

let view (model : GameState) (dispatch : Msg -> unit) =
    PageLayoutParts.mainLayout model dispatch
