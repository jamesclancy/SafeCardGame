module Dto
open System
open System.Collections.Generic


type PlayerDto =
    {
        PlayerId: string
        Name:string
        PlaymatUrl: string
        LifePoints: int
    }

type CardTypeDto =

    Character | Effect | Resource

type SpecialEffectDto =
    {
        Description: string
        Function: string
    }

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

open Shared
open Shared.Domain
open System.Text.Json

module Player =

    let fromDomain (person:Domain.Player) : PlayerDto =
       {
           PlayerId = person.PlayerId.ToString()
           Name = person.Name
           PlaymatUrl = person.PlaymatUrl.ToString()
           LifePoints = person.RemainingLifePoints
       }: PlayerDto

    let toDomain (dto: PlayerDto) :Result<Domain.Player,string> =
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
            }
        }

module Resource =

    let fromDomain (resource: Domain.Resource) : string =
        (resource.ToString())

    let fromString (resource: string) : Result<Domain.Resource, string> =
        match resource with
         |"Grass"         -> Ok Grass
         |"Fire"          -> Ok Fire
         |"Water"         -> Ok Water
         |"Lightning"     -> Ok Lightning
         |"Psychic"       -> Ok Psychic
         |"Fighting"      -> Ok Fighting
         |"Colorless" -> Ok Colorless
         | e -> sprintf "Unable to parse resource type of %s" e |> Error

    let fromStringSeq (resources: string seq) : Result<Domain.Resource seq, string> =
        resources
        |> Seq.map fromString
        |> Seq.map (fun x ->
                            match x with
                            | Ok y -> Ok [y]
                            | Error e ->  Error e)
        |> Seq.fold CollectionManipulation.appendToResultListOrMaintanFailure (Ok [])
        |> Result.bind (fun x -> x |> Seq.ofList |> Ok)

module ResourcePool =

    let fromDomain (resource: Domain.ResourcePool) : Dictionary<string, int> =
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
        |> Seq.fold CollectionManipulation.appendToResultListOrMaintanFailure (Ok [])
        |> Result.bind (fun x -> x |> Seq.ofList |> Domain.ResourcePool |> Ok)

    let parseDtoFromJson (dtoJson :string) =
        dtoJson |> JsonSerializer.Deserialize<Dictionary<string, int>>


    let dtoToJson (dto :Dictionary<string, int>) =
        dto |> JsonSerializer.Serialize

module GameStateSpecialEffect =

    let fromDomain (specialEffect : Option<Domain.GameStateSpecialEffect>) : Option<SpecialEffectDto> =
        match specialEffect with
        |None -> None
        |Some s ->
            Some ({
                    Description = s.Description
                    Function = "Not Impl yet"
                    } : SpecialEffectDto)

    let fromDto (specialEffectDto : SpecialEffectDto) : Result<GameStateSpecialEffect, string> =
       Ok {
           Description = specialEffectDto.Description
           Function = fun x -> x
       }


    let fromOptionalDto (specialEffectDto : Option<SpecialEffectDto>) : Result<Option<GameStateSpecialEffect>, string> =
       match specialEffectDto with
       |None -> None |> Ok
       | Some d ->
                   {
                       Description = d.Description
                       Function = fun x -> x
                   } |> Some |> Ok

    let parseDtoFromJson (text: string option) =
        match text with
        | None -> None
        | Some s ->
                JsonSerializer.Deserialize<SpecialEffectDto> s
                |> Some

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
        |> Seq.fold CollectionManipulation.appendToResultListOrMaintanFailure (Ok [])

    let parseDtoFromJson (jsonValue : string) =
        jsonValue
        |> JsonSerializer.Deserialize<AttackDto[]>

module Card =

    let getCreatureHealthFromCardDto (dto: CardDto) : Result<int, string> =
        match dto.CreatureHealth with
        | None -> Error "Unable to parse creature health from card dto."
        | Some s -> Ok s

    let getResourceCardResourcesAddedFirstTurnFromCardDto (dto: CardDto) : Result<bool, string> =
        match dto.ResourceAvailableOnFirstTurn with
        | None -> Error "Unable to parse `add resources first turn` value from card dto."
        | Some s -> Ok s

    let fromDomain (person:Domain.Card) : CardDto =
        match person with
        | CharacterCard cc ->
               {
                    CardId = (cc.CardId.ToString())
                    Name = cc.Name
                    Description = cc.Description
                    ImageUrl = (cc.ImageUrl.ToString())
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

    let toDomain (cc:CardDto) :Result<Domain.Card,string> =
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
        }