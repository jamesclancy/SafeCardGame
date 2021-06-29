module Index

open System
open Browser.Types
open Elmish
open Fable.Core.JS
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

let init : Model * Cmd<ClientInternalMsg>=

     let cmd = Cmd.OfAsync.perform cardGameServer.getCurrentLoggedInPlayer ()
                   (fun res -> match res with
                               | Ok p -> p.PlayerId.ToString() |> PlayerIdUpdated |> LoginPageFormMsg
                               | Error e -> e |> FailedLogin |> LoginPageFormMsg)

     { GameState = None
       ConnectionState = DisconnectedFromServer
       GameId = None
       LoginPageFormModel = (PageLayoutParts.LoginToGameForm.init None)
       PlayerId = None } , Cmd.batch [ cmd; Cmd.ofMsg NeedInformationForLobby ]

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
    | GameAvailable (pi, gi) ->
        { model with LoginPageFormModel =  {model.LoginPageFormModel with OpenGames = model.LoginPageFormModel.OpenGames.Add(gi, pi) }}, Cmd.none
    | GameNoLongerAvailable gi ->
        { model with LoginPageFormModel =  {model.LoginPageFormModel with OpenGames = model.LoginPageFormModel.OpenGames.Remove(gi) }}, Cmd.none
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
        | AttemptConnectToExistingGame gi ->
            { model with LoginPageFormModel = { model.LoginPageFormModel with  GameId = (gi.ToString())} }, Cmd.none
        | AttemptConnect  ->
                let cmd = Cmd.OfAsync.perform cardGameServer.getOrCreateGame
                            { PlayerId = model.LoginPageFormModel.PlayerId
                              GameId = model.LoginPageFormModel.GameId}
                            (fun gameStateResult ->
                                Console.WriteLine "get game api call completed...."
                                match gameStateResult with
                                | Error e ->
                                    e |> FailedLogin |> LoginPageFormMsg
                                | Ok (gs, cmd, gi, pi) ->
                                    Console.WriteLine "received game from server...."
                                    (gs, cmd, gi, pi)  |> SuccessfulLogin |> LoginPageFormMsg
                            )
                model, cmd
        | _ ->
            { model with LoginPageFormModel = PageLayoutParts.LoginToGameForm.update ltg model.LoginPageFormModel }, Cmd.none
    | NavigateBackToLobby ->
        {model with GameState = None; GameId = None}, Cmd.bridgeSend Closed
    | CommandToServer (msg) ->
        match model.GameState with
        | Some gs ->
            model, Cmd.bridgeSend  ( (msg, gs) |> ServerCommand |> RS)
        | None -> model, Cmd.none
    | NeedInformationForLobby ->

        let cmd = match model.LoginPageFormModel.OpenGames.IsEmpty with
                  | true -> Cmd.OfAsync.perform (fun _ -> async {
                                                    // quality resilience
                                                    do! Async.Sleep 500
                                                    Bridge.Send (GetCurrentAvailableGames |> RS)
                                                    return NeedInformationForLobby
                                             }) () id
                  | false -> Cmd.none

        let cmdBatch = Cmd.batch [ cmd ]//Cmd.bridgeSendOr (GetCurrentAvailableGames |> RS) (NeedInformationForLobby)]
        model, cmdBatch


let view (model : Model) (dispatch : ClientInternalMsg -> unit) =
    match model.GameId, model.GameState with
    | Some gameId, Some gs ->
        PageLayoutParts.mainLayout gs dispatch
    | Some gameId, None ->
        PageLayoutParts.WaitingForAnotherPlayerToJoin.view dispatch
    | _,_ ->
        PageLayoutParts.LoginToGameForm.view model.LoginPageFormModel (fun (x : LoginToGameFormMsgType) -> x |> LoginPageFormMsg |> dispatch)
