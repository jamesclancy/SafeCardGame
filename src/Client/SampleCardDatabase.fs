module SampleCardDatabase

open Shared.Domain

let creatureCreatureConstructor creatureId name description resourceCost health weaknesses =
    let cardId = NonEmptyString.build creatureId |> Result.map CardId
    let cardImageUrl = ImageUrlString.build (sprintf "/public/images/thumbs/%s.png" creatureId)

    match  cardId, cardImageUrl with
    |  Ok cid, Ok imgUrl ->
        Ok {
                CardId = cid
                ResourceCost =  resourceCost
                Name = name
                EnterSpecialEffects = None
                ExitSpecialEffects = None
                PrimaryResource = Resource.Grass
                Creature =
                      {
                        Health= health
                        Weaknesses=  weaknesses
                        Attack = List.empty
                      }
                ImageUrl = imgUrl
                Description =description
            }
    | _, _ -> Error "No"

let predicateForOkayResults z =
                        match z with
                        | Ok _ -> true
                        | _ -> false


let selectorForOkayResults z =
                        match z with
                        | Ok x ->  [ x ]
                        | _ -> []


let selectAllOkayResults (z : List<Result<'a,'b>>) =
    z
    |> List.filter predicateForOkayResults
    |> List.map selectorForOkayResults
    |> List.fold (@) []


let creatureCardDb =
    [
      ("001", "Bulbasaur", "It has a bulb on it bro", [ Resource.Grass, 1; ] |> Seq.ofList |> ResourcePool, 40, [Resource.Fire] )

      ("086", "Seel", "See Lion Monster", [ Resource.Water, 1; ] |> Seq.ofList |> ResourcePool, 60, [ Resource.Lightning ] )

    ]
   |> List.map (fun (x,y,z,q,q2,q3) -> creatureCreatureConstructor  x y z q g2 g3)
   |> selectAllOkayResults