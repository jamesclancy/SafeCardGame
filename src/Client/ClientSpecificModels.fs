module ClientSpecificModels

open System.Data
open Events
open Shared.Domain

let buildModule =
    ()

type LoginToGameFormModel = { PlayerId: string; GameId: string; ErrorMessage: string }

type Model =
    { ConnectionState : ConnectionState
      GameState: GameState option
      GameId: GameId option
      PlayerId: PlayerId option
      LoginPageFormModel: LoginToGameFormModel }


type CurrentPage =
    | GamePlay of GameId * Player * GameState
    | WaitingForAnotherPlayer of GameId * Player
    | LoginPage of LoginToGameFormModel