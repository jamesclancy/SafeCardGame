module SampleCardDatabase

open CollectionManipulation
open Shared
open Shared.Domain

let creatureCreatureConstructor creatureId name description primaryResource resourceCost health weaknesses =
    let cardId = NonEmptyString.build creatureId |> Result.map CardId
    let cardImageUrl = ImageUrlString.build (sprintf "/images/full/%s.png" creatureId)

    match  cardId, cardImageUrl with
    |  Ok cid, Ok imgUrl ->
        Ok {
                CardId = cid
                ResourceCost =  resourceCost
                Name = name
                EnterSpecialEffects = None
                ExitSpecialEffects = None
                PrimaryResource = primaryResource
                Creature =
                      {
                        Health= health
                        Weaknesses=  weaknesses
                        Attack = List.empty
                      }
                ImageUrl = imgUrl
                Description =description
            }
    | _,_ -> Error "No"

let resourceCardConstructor resourceCardId resource =
    let cardId = NonEmptyString.build resourceCardId |> Result.map CardId
    let cardImageUrl = ImageUrlString.build (sprintf "/images/resource/%s.png" (resource.ToString()))

    match  cardId, cardImageUrl with
    |  Ok cid, Ok imgUrl ->
        Ok {
                CardId = cid
                ResourceCost =  Seq.empty |> ResourcePool
                Name = resource.ToString()
                EnterSpecialEffects = None
                ExitSpecialEffects = None
                PrimaryResource = resource
                ResourceAvailableOnFirstTurn = true
                ResourcesAdded = [ (resource, 1) ] |> ResourcePool
                ImageUrl = imgUrl
                Description = (sprintf "Add 1 %s to your available resource pool." (getSymbolForResource resource))
            }
    | _, _ -> Error "No"

let resourceCardDb =
    [
      "GrassEnergy", Resource. Grass;
      "FireEnergy", Resource.Fire;
      "WaterEnergy", Resource. Water;
      "LightningEnergy", Resource.Lightning;
      "PsychicEnergy", Resource.Psychic;
      "FightingEnergy", Resource.Fighting;
      "ColorlessEnergy", Resource.Colorless
    ] |> List.map (fun (x,y) -> resourceCardConstructor x y)
    |> selectAllOkayResults

let creatureCardDb =
    [
      ("001", "BulbMon", "It has a bulb on it bro",Resource.Grass, [ Resource.Grass, 1; ] |> Seq.ofList |> ResourcePool, 40, [Resource.Fire] )
      ("050", "DigMon", "Mole Monster", Resource.Fighting,[ Resource.Fighting, 1; ] |> Seq.ofList |> ResourcePool, 30, [ Resource.Lightning ] )
      ("086", "SeelMon", "See Lion Monster", Resource.Water, [ Resource.Water, 1; ] |> Seq.ofList |> ResourcePool, 60, [ Resource.Lightning ] )
    ]
   |> List.map (fun (x,y,z,q,q2,q3, q4) -> creatureCreatureConstructor  x y z q q2 q3 q4)
   |> selectAllOkayResults


