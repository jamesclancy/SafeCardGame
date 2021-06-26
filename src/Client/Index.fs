module Index

open System
open Browser.Types
open Elmish
open Fable.Remoting.Client
open GeneralUIHelpers
open PageLayoutParts
open Shared
open Shared.Domain
open Operators
open Events
open GameSetup
open GameStateTransitions
open Fable
open ClientSpecificModels
open Elmish.Bridge

let cardGameServer =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ICardGameApi>

let createInitiallyGameStateFromServer gameId player1Id player2Id =
    async {

          //Bridge.Send (Connect |> RS)
          let! player1 = cardGameServer.getCurrentLoggedInPlayer()
          let! player2 = cardGameServer.getPlayer(player2Id)
          let! deck1 = testDeckSeqGenerator cardGameServer.getDecks cardGameServer.getCardsForDeck 60
          let! deck2 = testDeckSeqGenerator cardGameServer.getDecks cardGameServer.getCardsForDeck 60

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
                 } |> StartGame |> CommandToServer
          | _, _ ->
                    return {
                        GameId =  gameId
                        Winner = None
                        Message = None
                    } |> GameWon |> CommandToServer
    }

let init : Result<(Model * Cmd<ClientInternalMsg>),string>=

    //Bridge.Send (Connect |> RS)
    let player1 = createPlayer "Player1" "Player1" 10 "https://picsum.photos/id/1000/2500/1667?blur=5"
    let player2 = createPlayer "Player2" "Player2" 10 "https://picsum.photos/id/10/2500/1667?blur=5"
    let gameId =  NonEmptyString.build (Guid.NewGuid().ToString()) |> Result.map GameId

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
          //let cmd = Cmd.OfAsync.result (createInitiallyGameStateFromServer g "001" "002")
          Ok ( { GameState = Some model
                 ConnectionState = DisconnectedFromServer
                 GameId = None
                 LoginPageFormModel = (PageLayoutParts.LoginToGameForm.init ())
                 PlayerId = Some p1.PlayerId } , Cmd.none )
        | _ -> "Failed to create player boards" |> Error
    | _ -> "Failed to create players" |> Error


let update (cmsg: ClientInternalMsg) (model: Model): Model * Cmd<ClientInternalMsg> =
    match cmsg with
    | ConnectionChange status ->
        { model with ConnectionState = status }, Cmd.none
    | UpdatedModelForClient gs ->
        match model.PlayerId with
        | Some p when p = gs.PlayerOne ->
            { model with GameState = Some gs }, Cmd.none
        | Some p when p = gs.PlayerTwo ->
            let newPlayerTwo = gs.PlayerOne
            { model with GameState = Some { gs with PlayerOne = p; PlayerTwo = newPlayerTwo } }, Cmd.none
        | _ -> model, Cmd.none
    | LoginPageFormMsg ltg ->
        match ltg with
        | FailedLogin e ->
            { model with LoginPageFormModel =  {model.LoginPageFormModel with ErrorMessage = e }}, Cmd.none
        |SuccessfulLogin  (gs, cmd, gi, pi) ->
            match cmd, gs with
            | CommandToServer m, Some g ->
                let cmdBatch = Cmd.batch [ Cmd.bridgeSend ( (pi, gi) |> Connect)
                                           Cmd.bridgeSend ((m, g) |> ServerCommand)
                                         ]
                { model with GameState = gs; PlayerId = Some pi; GameId = Some gi }, cmdBatch
            | _ -> { model with GameState = gs; PlayerId = Some pi; GameId = Some gi }, Cmd.bridgeSend ( (pi, gi) |> Connect)
        | AttemptConnect  ->
                let cmd = Cmd.OfAsync.perform cardGameServer.getOrCreateGame
                            { PlayerId = model.LoginPageFormModel.PlayerId
                              GameId = model.LoginPageFormModel.GameId}
                            (fun gameStateResult ->
                                match gameStateResult with
                                | Error e ->
                                    e |> FailedLogin |> LoginPageFormMsg
                                | Ok (gs, cmd, gi, pi) ->
                                    (gs, cmd, gi, pi)  |> SuccessfulLogin |> LoginPageFormMsg
                            )
                model, cmd
        | _ ->
            { model with LoginPageFormModel = PageLayoutParts.LoginToGameForm.update ltg model.LoginPageFormModel }, Cmd.none
    | CommandToServer (msg) ->
        match model.GameState with
        | Some gs ->
            model, Cmd.bridgeSend  ( (msg, gs) |> ServerCommand |> RS)
        | None -> model, Cmd.none

let view (model : Model) (dispatch : ClientInternalMsg -> unit) =
    match model.GameId, model.GameState with
    | Some gameId, Some gs ->
        PageLayoutParts.mainLayout gs dispatch
    | Some gameId, None ->
        PageLayoutParts.WaitingForAnotherPlayerToJoin.view
    | _,_ ->
        PageLayoutParts.LoginToGameForm.view model.LoginPageFormModel (fun (x : LoginToGameFormMsgType) -> x |> LoginPageFormMsg |> dispatch)
