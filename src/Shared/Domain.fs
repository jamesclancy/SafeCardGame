namespace Shared


open System
open System.IO

type NonEmptyString = private NonEmptyString of string
    with override this.ToString() = match this with NonEmptyString s -> s
module NonEmptyString =
    let build str =
        if String.IsNullOrWhiteSpace str then "Value cannot be empty" |> Error
        else str |> NonEmptyString |> Ok


type UrlString = private UrlString of NonEmptyString
    with override this.ToString() = match this with UrlString s -> s.ToString()
module UrlString =
    let build str =
        if String.IsNullOrWhiteSpace str then "Value must be a url." |> Error
        //if Uri.IsWellFormedUriString(str, UriKind.RelativeOrAbsolute) then "Value must be a url." |> Error
        else str |> NonEmptyString.build |> Result.map UrlString


type ImageUrlString = private ImageUrlString of UrlString
    with override this.ToString() = match this with ImageUrlString s -> s.ToString()

module ImageUrlString =

    let isUrlImage (str : string) =
        true
        //let ext  = Path.GetExtension(str)
        (*let ext = str.Split [|'.'|] |> Seq.last    //Path.GetExtension(str)
        if String.IsNullOrWhiteSpace ext then false
        else
            match str with
            | "png" | "jpg" ->
                true
            | _ -> false *)

    let build str =
        if not (isUrlImage str) then "Value must be a url pointing to iamge." |> Error
        else str |> UrlString.build |> Result.map ImageUrlString

module Domain =

    type PlayerId = PlayerId of NonEmptyString
    type CardInstanceId = CardInstanceId of NonEmptyString
    type CardId = CardId of NonEmptyString
    type InPlayCreatureId = InPlayCreatureId of NonEmptyString
    type GameId = GameId of NonEmptyString

    type Player =
        {
            PlayerId: PlayerId
            Name: string
            PlaymatUrl: ImageUrlString
            RemainingLifePoints: int
        }

    type Resource =
        Grass | Fire |  Water | Lightning | Psychic | Fighting | Colorless


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
            CardInstanceId : CardInstanceId
            Card: Card
        }
    and Card =
       CharacterCard of CharacterCard
       | EffectCard of EffectCard
       | ResourceCard of ResourceCard
    and CharacterCard
        = { CardId: CardId;
            Name: string;
            Description: string;
            Creature: Creature;
            ImageUrl: ImageUrlString;
            ResourceCost: ResourcePool;
            PrimaryResource: Resource;
            EnterSpecialEffects: Option<GameStateSpecialEffect>;
            ExitSpecialEffects: Option<GameStateSpecialEffect>
          }
    and EffectCard
        = {
            CardId: CardId;
            Name: string;
            Description: string;
            ResourceCost: ResourcePool;
            ImageUrl: ImageUrlString;
            PrimaryResource: Resource;
            EnterSpecialEffects: Option<GameStateSpecialEffect>;
            ExitSpecialEffects: Option<GameStateSpecialEffect>;
        }
    and ResourceCard
        = {
            CardId: CardId;
            Name: string;
            Description: string;
            ResourceCost: ResourcePool;
            ImageUrl: ImageUrlString;
            PrimaryResource: Resource;
            EnterSpecialEffects: Option<GameStateSpecialEffect>;
            ExitSpecialEffects: Option<GameStateSpecialEffect>;
            ResourceAvailableOnFirstTurn: bool;
            ResourcesAdded: ResourcePool
        }
    and GameStateSpecialEffect =
        {
            Function: Func<GameState, GameState>
            Description: string
        }
    and Creature =
        {
            Health: int
            Weaknesses: Resource list
            Attack: Attack list
        }
    and Attack =
        {
            Damage: int
            Name: string
            Cost: ResourcePool
            SpecialEffect: Option<GameStateSpecialEffect>
        }
    and SpecialCondition = Asleep | Burned | Confused | Paralyzed | Poisoned
    and InPlayCreature =
        {
            InPlayCharacterId: InPlayCreatureId
            Card: Card
            CurrentDamage: int
            SpecialEffect: Option<SpecialCondition list>
            AttachedEnergy: ResourcePool
            SpentEnergy: ResourcePool
        }
    and GameOverStep =
        {
            WinnerId: Option<PlayerId>
            Message: string
        }
    and GameStep =
        NotCurrentlyPlaying
        | Draw of PlayerId
        | Play of PlayerId
        | Attack of PlayerId
        | Reconcile of PlayerId
        | GameOver of GameOverStep
    and Notification = Notification of string
    and GameState =
        {
            GameId: GameId
            NotificationMessages: Option<Notification list>
            PlayerOne: PlayerId
            PlayerTwo: PlayerId
            Players: Map<PlayerId, Player>
            Boards: Map<PlayerId, PlayerBoard>
            CurrentStep:GameStep
            TurnNumber: int
        }

    let opponentPlayer (model : GameState) =
        match model.Players.TryGetValue model.PlayerTwo with
        | true, p -> p |> Ok
        | false, _ -> "Unable to locate oppponent in player list" |> Error


    let opponentPlayerBoard (model : GameState) =
        match model.Boards.TryGetValue model.PlayerTwo with
        | true, p -> p |> Ok
        | false, _ -> "Unable to locate oppponent in board list" |> Error

    let currentPlayer (model : GameState) =
        match model.Players.TryGetValue model.PlayerOne with
        | true, p -> p |> Ok
        | false, _ -> "Unable to locate current player in player list" |> Error


    let currentPlayerBoard (model : GameState) =
        match model.Boards.TryGetValue model.PlayerOne with
        | true, p -> p |> Ok
        | false, _ -> "Unable to locate current player in board list" |> Error

    let extractNeededModelsFromState (model: GameState) =
        opponentPlayer model, opponentPlayerBoard model, currentPlayer model, currentPlayerBoard model


    let getSymbolForResource resource =
        match resource with
        | Grass -> "🍂"
        | Fire -> "🔥"
        | Water -> "💧"
        | Lightning -> "⚡"
        | Psychic -> "🧠"
        | Fighting -> "👊"
        | Colorless -> "□"


    let getSymbolForSpecialCondition status =
        match status with
        | Asleep -> "💤"
        | Burned -> "♨"
        | Confused -> "❓"
        | Paralyzed -> "🧊"
        | Poisoned -> "☠️"


    let textDescriptionForListOfSpecialConditions specialConditions =
        match specialConditions with
        | Some sc -> sc |> Seq.map getSymbolForSpecialCondition |> String.concat ";"
        | None -> ""

    let textDescriptionForResourcePool (resourcePool : ResourcePool) =
        resourcePool
        |> Seq.map (fun x -> sprintf "%s x%i" (getSymbolForResource x.Key) x.Value)
        |> String.concat ";"