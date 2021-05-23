module App

open Elmish
open Elmish.React

#if DEBUG
open Elmish.Debug
open Elmish.HMR
open Index
#endif


//#region  Start Up Error Functions

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

//#endregion

match Index.init with
| Ok (gamesState, dispatch) ->
    let placeHolderFunc () =
        gamesState, dispatch
    Program.mkProgram placeHolderFunc Index.update Index.view
    #if DEBUG
    |> Program.withConsoleTrace
    #endif
    |> Program.withReactSynchronous "elmish-app"
    #if DEBUG
    |> Program.withDebugger
    #endif
    |> Program.run
| _ ->
    Program.mkProgram startUpErrorInit startUpErrorUpdate startUpErrorView
    #if DEBUG
    |> Program.withConsoleTrace
    #endif
    |> Program.withReactSynchronous "elmish-app"
    #if DEBUG
    |> Program.withDebugger
    #endif
    |> Program.run