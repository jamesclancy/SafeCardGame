namespace Cards

[<CLIMutable>]
type CardDatabaseDto = {
  CardId: string
  CardName: string
  CardDescription: string
  CardImageUrl: string
  CardThumbnailImageUrl: string
  CardPrimaryResource: string
  CardType: string
  CardEnterSpecialEffects: string
  CardExitSpecialEffects: string
  CardCreatureHealth: int
  CardCreatureWeaknesses: string
  CardCreatureAttacks: string
  CardResourcesAvailableOnFirstTurn: bool
  CardResourcesAdded: string
  CardResourceCost: string
}

module Validation =
  let validate v =
    let validators = [
      fun u -> if isNull u.CardId then Some ("CardId", "CardId shouldn't be empty") else None
    ]

    validators
    |> List.fold (fun acc e ->
      match e v with
      | Some (k,v) -> Map.add k v acc
      | None -> acc
    ) Map.empty
