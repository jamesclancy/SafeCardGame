module App

open Elmish
open Elmish.React
open Elmish.Bridge

#if DEBUG
open Elmish.Debug
open Elmish.HMR
open Index
#endif

let (gamesState, dispatch) =  Index.init

let placeHolderFunc () =
    gamesState, dispatch
Program.mkProgram placeHolderFunc Index.update Index.view
|> Program.withBridgeConfig (Bridge.endpoint "./socket" |> Bridge.withUrlMode Append |> Bridge.withRetryTime 1)
// |> Program.withSubscription Channel.subscription
|> Program.withReactSynchronous "elmish-app"
#if DEBUG
|> Program.withConsoleTrace
|> Program.withDebugger
#endif
|> Program.run