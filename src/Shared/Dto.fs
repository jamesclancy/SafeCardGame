module Dto

open System
open System.Collections.Generic
open Thoth.Json.Net

[<CLIMutable>]
type DeckDto =
    {
        DeckId: string
        DeckName: string
        DeckDescription: string
        DeckImageUrl: string
        DeckThumbnailImageUrl: string
        DeckPrimaryResource: string
        DeckOwner: string
        DeckPrivate: bool
    }


[<CLIMutable>]
type PlayerDto =
    {
        PlayerId: string
        Name:string
        PlaymatUrl: string
        LifePoints: int
        InitialHealth: int
        LastLogin: DateTime
        DateCreated: DateTime
    }

type SpecialEffectDto =
    {
        Description: string
        Function: string
    }


[<CLIMutable>]
type AttackDto =
    {

            Damage: int
            Name: string
            Cost: Dictionary<string, int>
            SpecialEffect: Option<SpecialEffectDto>
    }

type CardDto =
    {
        CardId: string
        Name: string;
        Description: string
        ImageUrl: string
        ThumbnailImageUrl: string
        ResourceCost: Dictionary<string, int>
        PrimaryResource: string

        CardType: string

        EnterSpecialEffects: Option<SpecialEffectDto>
        ExitSpecialEffects: Option<SpecialEffectDto>

        CreatureHealth: Option<int>
        Weaknesses: string[]
        Attack: AttackDto[]

        ResourceAvailableOnFirstTurn: Option<bool>
        ResourcesAdded:Dictionary<string, int>
    }


[<CLIMutable>]
type GameDto = {
      GameId: string
      Player1Id: string
      Player2Id: string
      CurrentStep: string
      CurrentPlayerMove: string
      Winner: string
      Notes: string
      InProgress: bool
      DateStarted: System.DateTime
      LastMovement: System.DateTime
      GameState: string
    }


open Shared
open Shared.Domain
open Thoth.Json

module Game =

    let getPlayerAndStepFromGameStata (gameState: GameState) =
        match gameState.CurrentStep with
        | NotCurrentlyPlaying -> "NotCurrentlyPlaying", "", ""
        | Draw p -> "Draw", p.ToString(), ""
        | Play p -> "Play", p.ToString(), ""
        | Attack p -> "Attack", p.ToString(), ""
        | Reconcile p -> "Reconcile", p.ToString(), ""
        | GameOver p -> "GameOver",  "", p.WinnerId |> Option.fold (fun _  c ->  c.ToString()) ""

    let fromDto (gameDto: GameDto) : Result<GameState, string> =
            Decode.Auto.fromString<GameState>(gameDto.GameState)

    let gameStateStrFromDomain  (gameState: GameState) : string =
        Encode.Auto.toString(4, gameState)

    let fromDomain (gameState: GameState) : GameDto =
            let stateStr, currentPlayerStr, currentWinnerStr = getPlayerAndStepFromGameStata gameState

            //let gameStateStr = Decode.Auto.fromString<User>(json)
            let gameStateStr = gameStateStrFromDomain gameState

            {
              GameId = gameState.GameId.ToString()
              Player1Id = gameState.PlayerOne.ToString()
              Player2Id = gameState.PlayerTwo.ToString()
              CurrentStep = stateStr
              CurrentPlayerMove = currentPlayerStr
              Winner = currentWinnerStr
              Notes = ""
              InProgress = true
              DateStarted=  DateTime.Now
              LastMovement= DateTime.Now
              GameState = stateStr
            }



module Player =

    let fromDomain (person:Player) : PlayerDto =
       {
           PlayerId = person.PlayerId.ToString()
           Name = person.Name
           PlaymatUrl = person.PlaymatUrl.ToString()
           LifePoints = person.RemainingLifePoints
           InitialHealth = person.InitialHealth
           LastLogin = person.LastLogin
           DateCreated = person.DateCreated }: PlayerDto

    let toDomain (dto: PlayerDto) :Result<Player,string> =
        CollectionManipulation.result {
            let! playerId = dto.PlayerId |> NonEmptyString.build
            let! name = dto.Name |> Ok
            let! playmatUrl = dto.PlaymatUrl |> ImageUrlString.build
            let! lifePoints = dto.LifePoints |> Ok

            return {
               PlayerId = playerId |> PlayerId
               Name = name
               PlaymatUrl = playmatUrl
               RemainingLifePoints = lifePoints
               InitialHealth = dto.InitialHealth
               DateCreated = dto.DateCreated
               LastLogin = dto.LastLogin }
        }

module Resource =

    let fromDomain (resource: Resource) : string =
        (resource.ToString())

    let fromString (resource: string) : Result<Resource, string> =
        match resource with
         |"Grass"         -> Ok Grass
         |"Fire"          -> Ok Fire
         |"Water"         -> Ok Water
         |"Lightning"     -> Ok Lightning
         |"Psychic"       -> Ok Psychic
         |"Fighting"      -> Ok Fighting
         |"Colorless" -> Ok Colorless
         | e -> sprintf "Unable to parse resource type of %s" e |> Error

    let fromStringSeq (resources: string seq) : Result<Resource seq, string> =
        resources
        |> Seq.map fromString
        |> Seq.map (fun x ->
                            match x with
                            | Ok y -> Ok [y]
                            | Error e ->  Error e)
        |> Seq.fold CollectionManipulation.appendToResultListOrMaintainFailure (Ok [])
        |> Result.bind (fun x -> x |> Seq.ofList |> Ok)
module ResourcePool =

    let fromDomain (resource: ResourcePool) : Dictionary<string, int> =
        Map.toList resource
        |>  List.map (fun (x,y) -> (Resource.fromDomain x, y))
        |> dict |> Dictionary

    let emptyDictionary : Dictionary<string, int> =
        Seq.empty
        |> dict |> Dictionary

    let singleKeyValuePairFromString (resource: KeyValuePair<string, int>) : Result<Resource * int, string> =
        CollectionManipulation.result {
            let! list =  Resource.fromString resource.Key

            return (list, resource.Value)

        }

    let fromDictionary  (resource: Dictionary<string, int>) =
        resource
        |> Seq.map singleKeyValuePairFromString
        |> Seq.map (fun x ->
                        match x with
                        | Ok y -> Ok [y]
                        | Error e ->  Error e)
        |> Seq.fold CollectionManipulation.appendToResultListOrMaintainFailure (Ok [])
        |> Result.bind (fun x -> x |> Seq.ofList |> Domain.ResourcePool |> Ok)


module GameStateSpecialEffect =

    let fromDomain (specialEffect : Option<GameStateSpecialEffect>) : Option<SpecialEffectDto> =
        match specialEffect with
        |None -> None
        |Some s ->
            Some ({
                    Description = s.Description
                    Function = s.Function
                    } : SpecialEffectDto)

    let fromDto (specialEffectDto : SpecialEffectDto) : Result<GameStateSpecialEffect, string> =
       let parsedFunc = SpecialEffectParser.getFuncForSpecialEffectText specialEffectDto.Function
       Ok {
           Description = specialEffectDto.Description
           Function = specialEffectDto.Function
       }


    let fromOptionalDto (specialEffectDto : Option<SpecialEffectDto>) : Result<Option<GameStateSpecialEffect>, string> =
       match specialEffectDto with
       | None -> {
                       Description = "n/a"
                       Function = ""
                   } |> Some |> Ok
       | Some d ->
                   let parsedFunc = SpecialEffectParser.getFuncForSpecialEffectText d.Function
                   {
                       Description = d.Description
                       Function = d.Function
                   } |> Some |> Ok

module Attack =

    let fromDomain (attack: Domain.Attack) : AttackDto =
        {
            Damage = attack.Damage
            Name = attack.Name
            Cost = ResourcePool.fromDomain attack.Cost
            SpecialEffect = GameStateSpecialEffect.fromDomain attack.SpecialEffect
        }

    let fromDto (attackDto: AttackDto) : Result<Attack, string> =
        CollectionManipulation.result {
            let! specialEffect = GameStateSpecialEffect.fromOptionalDto attackDto.SpecialEffect
            let! cost = ResourcePool.fromDictionary attackDto.Cost
            return {

                        Damage = attackDto.Damage
                        Name=  attackDto.Name
                        Cost = cost
                        SpecialEffect = specialEffect
            }
        }

    let getSeqFromSeqOfDto (attacks :AttackDto[]) : Result<Attack list, string> =
        attacks
        |> Seq.map fromDto
        |> Seq.map (fun x ->
                            match x with
                            | Ok y -> Ok [y]
                            | Error e ->  Error e)
        |> Seq.fold CollectionManipulation.appendToResultListOrMaintainFailure (Ok [])


module Card =

    let getCreatureHealthFromCardDto (dto: CardDto) : Result<int, string> =
        match dto.CreatureHealth with
        | None -> Error "Unable to parse creature health from card dto."
        | Some s -> Ok s

    let getResourceCardResourcesAddedFirstTurnFromCardDto (dto: CardDto) : Result<bool, string> =
        match dto.ResourceAvailableOnFirstTurn with
        | None -> Error "Unable to parse `add resources first turn` value from card dto."
        | Some s -> Ok s

    let fromDomain (person:Card) : CardDto =
        match person with
        | CharacterCard cc ->
               {
                    CardId = (cc.CardId.ToString())
                    Name = cc.Name
                    Description = cc.Description
                    ImageUrl = (cc.ImageUrl.ToString())
                    ThumbnailImageUrl = (cc.ImageUrl.ToString())
                    ResourceCost= cc.ResourceCost |> ResourcePool.fromDomain
                    PrimaryResource= cc.PrimaryResource |> Resource.fromDomain

                    CardType= "Character"

                    EnterSpecialEffects = GameStateSpecialEffect.fromDomain cc.EnterSpecialEffects
                    ExitSpecialEffects = GameStateSpecialEffect.fromDomain cc.ExitSpecialEffects

                    CreatureHealth = Some cc.Creature.Health
                    Weaknesses = cc.Creature.Weaknesses |> List.map Resource.fromDomain |> List.toArray
                    Attack = cc.Creature.Attack |> List.map Attack.fromDomain |> List.toArray

                    ResourceAvailableOnFirstTurn = None
                    ResourcesAdded = ResourcePool.emptyDictionary
               }: CardDto
        | ResourceCard cc ->
               {
                    CardId = (cc.CardId.ToString())
                    Name = cc.Name
                    Description = cc.Description
                    ImageUrl = (cc.ImageUrl.ToString())
                    ThumbnailImageUrl = (cc.ImageUrl.ToString())
                    ResourceCost= cc.ResourceCost |> ResourcePool.fromDomain
                    PrimaryResource= cc.PrimaryResource |> Resource.fromDomain

                    CardType= "Resource"

                    EnterSpecialEffects = GameStateSpecialEffect.fromDomain cc.EnterSpecialEffects
                    ExitSpecialEffects = GameStateSpecialEffect.fromDomain cc.ExitSpecialEffects

                    CreatureHealth = None
                    Weaknesses = Seq.empty |> Seq.toArray
                    Attack = Seq.empty |> Seq.toArray

                    ResourceAvailableOnFirstTurn = Some cc.ResourceAvailableOnFirstTurn
                    ResourcesAdded = ResourcePool.fromDomain cc.ResourcesAdded
               }: CardDto
        | EffectCard cc ->
               {
                    CardId = (cc.CardId.ToString())
                    Name = cc.Name
                    Description = cc.Description
                    ImageUrl = (cc.ImageUrl.ToString())
                    ThumbnailImageUrl = (cc.ImageUrl.ToString())
                    ResourceCost= cc.ResourceCost |> ResourcePool.fromDomain
                    PrimaryResource= cc.PrimaryResource |> Resource.fromDomain

                    CardType= "Effect"

                    EnterSpecialEffects = GameStateSpecialEffect.fromDomain cc.EnterSpecialEffects
                    ExitSpecialEffects = GameStateSpecialEffect.fromDomain cc.ExitSpecialEffects

                    CreatureHealth = None
                    Weaknesses = Seq.empty |> Seq.toArray
                    Attack = Seq.empty |> Seq.toArray

                    ResourceAvailableOnFirstTurn = None
                    ResourcesAdded = ResourcePool.emptyDictionary
               }: CardDto

    let toDomain (cc:CardDto) : Result<Card,string> =
        CollectionManipulation.result {
                    let! cardId = cc.CardId |> NonEmptyString.build
                    let! imageUrl = cc.ImageUrl |> ImageUrlString.build
                    let! resourceCost= cc.ResourceCost |> ResourcePool.fromDictionary
                    let! primaryResource= cc.PrimaryResource |> Resource.fromString

                    let! enterSpecialEffects = GameStateSpecialEffect.fromOptionalDto cc.EnterSpecialEffects
                    let! exitSpecialEffects = GameStateSpecialEffect.fromOptionalDto cc.ExitSpecialEffects

                    match cc.CardType with
                    | "Character" ->

                        let! creatureHealth = getCreatureHealthFromCardDto cc
                        let! weaknesses = cc.Weaknesses |> Resource.fromStringSeq
                        let! attack = cc.Attack |> Attack.getSeqFromSeqOfDto

                        return {
                                CardId = cardId |> CardId
                                ImageUrl = imageUrl
                                Description = cc.Description
                                Name = cc.Name
                                ResourceCost = resourceCost
                                PrimaryResource = primaryResource
                                EnterSpecialEffects = enterSpecialEffects
                                ExitSpecialEffects = exitSpecialEffects
                                Creature = {
                                        Health = creatureHealth
                                        Weaknesses = weaknesses |> Seq.toList
                                        Attack = attack
                                }
                        } |> CharacterCard
                    | "Resource" ->
                        let! resourcesAdded = cc.ResourcesAdded |> ResourcePool.fromDictionary
                        let! resourcesAvailableOnFirstTurn = getResourceCardResourcesAddedFirstTurnFromCardDto cc

                        return {
                                CardId = cardId |> CardId
                                ImageUrl = imageUrl
                                Description = cc.Description
                                Name = cc.Name
                                ResourceCost = resourceCost
                                PrimaryResource = primaryResource
                                EnterSpecialEffects = enterSpecialEffects
                                ExitSpecialEffects = exitSpecialEffects
                                ResourcesAdded =resourcesAdded
                                ResourceAvailableOnFirstTurn = resourcesAvailableOnFirstTurn
                        } |> ResourceCard
                    | "Effect" ->
                        return {
                                CardId = cardId |> CardId
                                ImageUrl = imageUrl
                                Description = cc.Description
                                Name = cc.Name
                                ResourceCost = resourceCost
                                PrimaryResource = primaryResource
                                EnterSpecialEffects = enterSpecialEffects
                                ExitSpecialEffects = exitSpecialEffects
                        } |> EffectCard
                    | _ -> return! ((sprintf "unknown card type \"%s\"encountered in json" cc.CardType) |> Error )
        }

