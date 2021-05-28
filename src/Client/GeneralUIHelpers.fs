module GeneralUIHelpers

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Shared.Domain


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

let renderAttackRow (attack: Attack) =
    match attack.SpecialEffect with
    | Some se ->
        tr [ ]
            [ td [ ]
                [ str (textDescriptionForResourcePool attack.Cost) ]
              td [ ]
                [ str attack.Name ]
              td [ ]
                [
                    p [] [ str (sprintf "%i" attack.Damage) ]
                    p [] [ str se.Description ]
                ] ]
    | None ->
        tr [ ]
            [ td [ ]
                [ str (textDescriptionForResourcePool attack.Cost) ]
              td [ ]
                [ str attack.Name ]
              td [ ]
                [
                    p [] [ str (sprintf "%i" attack.Damage) ]
                ] ]


