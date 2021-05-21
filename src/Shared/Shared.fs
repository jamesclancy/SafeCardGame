namespace Shared

open System

type Todo =
    { Id : Guid
      Description : string }

(*
type PlayerId = string

type Player =
    {
        PlayerId: PlayerId
        Name: string
        RemainingLifePoints: int
    }

type Resource =
    Grass | Fire |  Water | Lightning | Psychic | Fighting | Colorless

type CardInstanceId = string
type CardId = string

type PlayerBoard =
    {
        PlayerId: PlayerId
        Deck: Deck
        Hand: Hand
        ActiveCreature: Option<InPlayCreature>
        Bench:  Option<InPlayCreature list>
        DiscardPile: Deck
        TotalResourcePool: ResourcePool
        AvailableResourcePool: ResourcePool
    }
and ResourcePool = Map<Resource, int>
and Hand =
    {
        Cards: CardInstance list
    }
and Deck =
    {
        Cards: CardInstance list
        TopCardsExposed: int
    }
and CardInstance =
    {
        CardIntanceId : CardInstanceId
        Card: CardId
    }
and Card =
   CharacterCard of CharacterCard
   | EffectCard of EffectCard
   | ResourceCard of ResourceCard
and CharacterCard
    = { CardId: CardId; Name: string; Creature: Creature; ResourceCost: ResourcePool; PrimaryResource: Resource; EnterSpecialEffects: GameStateSpecialEffect; ExitSpecialEffects: GameStateSpecialEffect}
and EffectCard
    = { CardId: CardId; Name: string; ResourceCost: ResourcePool; PrimaryResource: Resource; EnterSpecialEffects: GameStateSpecialEffect; ExitSpecialEffects: GameStateSpecialEffect; }
and ResourceCard
    = { CardId: CardId; Name: string; ResourceCost: ResourcePool; PrimaryResource: Resource; EnterSpecialEffects: GameStateSpecialEffect; ExitSpecialEffects: GameStateSpecialEffect; ResourceAvailableOnFirstTurn: bool; ResourcesAdded: ResourcePool}
and GameStateSpecialEffect = delegate of GameState -> GameState
and Creature =
    {
        Health: int
        Weaknesses: Resource list
        Attach: Attack list
    }
and Attack =
    {
        Damage: int
        Cost: ResourcePool
        SpecialEffect: GameStateSpecialEffect
    }
and SpecialCondition = Asleep | Burned | Confused | Paralyzed | Poisoned
and InPlayCreatureId = string
and InPlayCreature =
    {
        InPlayCharacterId: InPlayCreatureId
        Card: Card
        CurrentDamage: int
        SpecialEffect: Option<SpecialCondition list>
        AttachedEnergy: ResourcePool
        SpentEnergy: ResourcePool
    }
and GameStep =
    NotCurrentlyPlaying | Draw | Play | Attack | Reconile
and GameState =
    {
        Players: Map<PlayerId, Player>
        Boards: Map<PlayerId, PlayerBoard>
        CurrentTurn: Option<PlayerId>
        CurrentStep:GameStep
        TurnNumber: int
    }
*)

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ITodosApi =
    { getTodos : unit -> Async<Todo list>
      addTodo : Todo -> Async<Todo> }