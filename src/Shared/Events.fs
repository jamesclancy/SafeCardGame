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

type BridgedMsg = Msg * PlayerId