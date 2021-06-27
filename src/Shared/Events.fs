module Events

open Shared.Domain
open System

type StartGameEvent =
    {
        GameId: GameId
        Players: Map<PlayerId, Player>
        Decks: Map<PlayerId, Deck>
        PlayerOne:PlayerId
        PlayerTwo:PlayerId
    }
type DrawCardEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
    }
type DiscardCardEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
        CardInstanceId: CardInstanceId
    }
type ToggleZoomOnCardEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
        CardInstanceId: CardInstanceId
    }
type PlayCardEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
        CardInstanceId: CardInstanceId
    }
type EndPlayStepEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
    }
type PerformAttackEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
        InPlayCreatureId: InPlayCreatureId
        Attack: Attack
    }
type SkipAttackEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
    }
type EndTurnEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
    }
type GameWonEvent =
    {
        GameId: GameId
        Winner: Option<PlayerId>
        Message: Option<Notification list>
    }

type Msg =
    | StartGame of StartGameEvent
    | DrawCard of DrawCardEvent
    | DiscardCard of DiscardCardEvent
    | ToggleZoomOnCard of ToggleZoomOnCardEvent
    | PlayCard of PlayCardEvent
    | EndPlayStep of EndPlayStepEvent
    | PerformAttack of PerformAttackEvent
    | SkipAttack of SkipAttackEvent
    | EndTurn of EndTurnEvent
    | DeleteNotification of Guid
    | GameWon of GameWonEvent
    | SwapPlayer

type BridgedMsg
    = Msg * PlayerId

type WebSocketClientMessage =
    | TextMessage of string

type RemoteServerMessage =
| Connect of PlayerId * GameId
| ServerCommand of Msg * GameState

type ServerMsg =
  | RS of RemoteServerMessage
  | StatePersistedToIO
  | Closed

type WsSender = Msg -> unit
type BroadcastMode = ViaWebSocket | ViaHTTP

type ConnectionState =
    | DisconnectedFromServer | ConnectedToServer of WsSender | Connecting
    member this.IsConnected =
        match this with
        | ConnectedToServer _ -> true
        | DisconnectedFromServer | Connecting -> false

type LoginToGameFormMsgType =
        | PlayerIdUpdated of string
        | GameIdUpdated of string
        | AttemptConnect
        | AttemptConnectToExistingGame of GameId
        | FailedLogin of string
        | SuccessfulLogin of Option<GameState> * ClientInternalMsg * GameId * PlayerId
and ClientInternalMsg  =
       | UpdatedModelForClient of GameState
       | CommandToServer of Msg
       | GameAvailable of PlayerId * GameId
       | GameNoLongerAvailable of GameId
       | ConnectionChange of ConnectionState
       | LoginPageFormMsg of LoginToGameFormMsgType
