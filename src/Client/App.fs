module App

open Elmish
open Elmish.React
open Events

#if DEBUG
open Elmish.Debug
open Elmish.HMR
open Index
#endif

match Index.init with
| Ok (gamesState, dispatch) ->
    let placeHolderFunc () =
        gamesState, dispatch
    Program.mkProgram placeHolderFunc Index.update Index.view
    |> Program.withConsoleTrace
    |> Program.withReactSynchronous "elmish-app"
    #if DEBUG
    |> Program.withDebugger
    #endif
    |> Program.run
| _ ->
    ()