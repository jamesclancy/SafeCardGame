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
        with override this.ToString() = match this with PlayerId s -> s.ToString()
    type CardInstanceId = CardInstanceId of NonEmptyString
        with override this.ToString() = match this with CardInstanceId s -> s.ToString()
    type CardId = CardId of NonEmptyString
        with override this.ToString() = match this with CardId s -> s.ToString()
    type InPlayCreatureId = InPlayCreatureId of NonEmptyString
        with override this.ToString() = match this with InPlayCreatureId s -> s.ToString()
    type GameId = GameId of NonEmptyString
        with override this.ToString() = match this with GameId s -> s.ToString()

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
            ZoomedCard: Option<CardInstanceId>
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
    and GameStateTransformation = GameState -> GameState
    and GameStateSpecialEffect =
        {
            Function: GameStateTransformation
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
            Name: string
            CurrentDamage: int
            TotalHealth: int
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
    and Notification =
        {
            Id: Guid
            Content: string
        }
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

    let createNotification message ={Id = Guid.NewGuid(); Content = message}

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

    let appendNotificationMessageToListOrCreateList (existingNotifications : Option<Notification list>) (newNotification : string) =
        match existingNotifications with
        | Some nl ->
            newNotification
            |> createNotification
            |> (fun y -> y :: nl)
            |> Some
        | None ->
            [ (newNotification |> createNotification) ]
            |> Some

    let currentResourceAmountFromPool (resourcePool : ResourcePool) (resource : Resource) =
        match resourcePool.TryGetValue(resource) with
        | true, t -> t
        | false, _ -> 0

    let rec addResourcesToPool (rp1 : Map<Resource, int>)  rp2 =
        match rp2 with
        | [] ->  rp1 |> Map.toSeq |> ResourcePool
        | [ (x,y) ] ->
            rp1.Add(x, y + (currentResourceAmountFromPool rp1 x))  |> Map.toSeq |> ResourcePool
        | (x,y) :: xs  ->
            addResourcesToPool (rp1.Add(x,  y + (currentResourceAmountFromPool rp1 x))) xs

    let filterNotificationList notificationId l =
        l |> List.filter (fun x -> x.Id <> notificationId)
        |> function
            | [] -> None
            | x -> Some x

    let removeNotificationFromGameState gs notificationId =
       { gs with NotificationMessages = match gs.NotificationMessages with
                                        | Some x -> filterNotificationList notificationId x
                                        | None -> None
       }

    let migrateGameStateToNewStep newStep (gs: GameState) =
        // maybe some vaidation could go here?
        Ok {
            gs with CurrentStep = newStep
        }

    let getNeededResourcesForCard card =
        match card with
        | CharacterCard cc -> cc.ResourceCost
        | ResourceCard rc -> rc.ResourceCost
        | EffectCard ec -> ec.ResourceCost

    let rec tryRemoveResourceFromPlayerBoard (playerBoard:PlayerBoard) x y =
        match playerBoard.AvailableResourcePool.TryGetValue(x) with
        | true, z when z >= y -> Ok {playerBoard with AvailableResourcePool = (addResourcesToPool playerBoard.AvailableResourcePool  [ (x, 0-y) ])  }
        | _, _ ->
                if x = Colorless then
                    let letBestMatch = playerBoard.AvailableResourcePool
                                        |> Map.toList
                                        |> List.filter (fun (k,v) -> v > 0)
                                        |> List.tryHead

                    match letBestMatch with
                    | None -> sprintf "Not enough %s" (getSymbolForResource x) |> Error
                    | Some (k, v) when v >= y  -> Ok {playerBoard with AvailableResourcePool = (addResourcesToPool playerBoard.AvailableResourcePool  [ (k, v-y) ])  }
                    | Some (k, v) ->
                        tryRemoveResourceFromPlayerBoard
                            {playerBoard with AvailableResourcePool = (addResourcesToPool playerBoard.AvailableResourcePool  [ (k, 0) ])  }
                            x (y - v)
                else
                    sprintf "Not enough %s" (getSymbolForResource x) |> Error

    let rec decrementResourcesFromPlayerBoard playerBoard resourcePool =

        let sortedPool = resourcePool |> List.sortBy (fun (x : Resource * int) ->
                                                                    match x with
                                                                    | Colorless, _ -> 2
                                                                    | _, _ -> 1
                                                                )
        match sortedPool with
        | [] -> Ok playerBoard
        | [ (x, y) ] -> tryRemoveResourceFromPlayerBoard playerBoard x y
        | (x, y) :: xs ->
            match tryRemoveResourceFromPlayerBoard playerBoard x y with
            | Error e -> e |> Error
            | Ok pb -> decrementResourcesFromPlayerBoard pb xs

    let hasEnoughResources rp1 rp2 =
        match decrementResourcesFromPlayerBoard rp1 rp2 with
        | Ok _ -> true
        | Error _ -> false

    let getTotalHealthFromCard card =
        match card with
        | CharacterCard cc -> cc.Creature.Health
        | _ -> 0

    let getNameFromCard card =
        match card with
        | CharacterCard cc -> cc.Name
        | ResourceCard cc -> cc.Name
        | EffectCard cc -> cc.Name

    let createInPlayCreatureFromCardInstance characterCard inPlayCreatureId =
                {
                    InPlayCharacterId=  inPlayCreatureId
                    Card = characterCard
                    CurrentDamage=  0
                    Name = getNameFromCard characterCard
                    TotalHealth = getTotalHealthFromCard characterCard
                    SpecialEffect=  None
                    AttachedEnergy =  Seq.empty |> ResourcePool
                    SpentEnergy = Seq.empty |> ResourcePool
                } |> Ok

    let createPlayer playerIdStr playerName playerCurrentLife playerPlaymatUrl =
        let playerId = NonEmptyString.build playerIdStr |> Result.map PlayerId
        let playerPlaymatUrl = ImageUrlString.build playerPlaymatUrl

        match playerId, playerPlaymatUrl with
        | Ok s, Ok pm ->
            Ok {
               PlayerId = s
               Name = playerName
               RemainingLifePoints = playerCurrentLife
               PlaymatUrl = pm
            }
        | _ -> Error "Unable to create player"

    let getExistingPlayerBoardFromGameState playerId gs =
     match gs.Boards.TryGetValue playerId with
        | true, pb ->
            pb |> Ok
        | false, _ ->
            (sprintf "Unable to locate player board for player id %s" (playerId.ToString())) |> Error

    let drawCardsFromDeck (cardsToDraw: int) (deck : Deck) (hand: Hand) =
        if deck.Cards.IsEmpty then
            deck, hand
        else
            let cardsToTake = List.truncate cardsToDraw deck.Cards
            { deck with Cards = List.skip cardsToTake.Length deck.Cards}, {hand with Cards = hand.Cards @ cardsToTake}

    let moveCardsFromDeckToHand gs playerId pb =
        let newDeck, newHand =  drawCardsFromDeck 1 pb.Deck pb.Hand
        Ok { gs with Boards = (gs.Boards.Add (playerId, { pb with Deck = newDeck; Hand = newHand })  ) }