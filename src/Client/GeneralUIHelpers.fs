module GeneralUIHelpers

open Fable.React
open Fable.React.Props
open Shared.Domain
open Events

let renderDamageInformationForAttack (attack: Attack) =
    match attack.SpecialEffect with
    | Some se ->
        td [ ]
                [
                    p [] [ str (sprintf "%i" attack.Damage) ]
                    p [] [ str se.Description ]
                ]
    | None ->
        td [ ]
                [
                    p [] [ str (sprintf "%i" attack.Damage) ]
                ]


let renderAttackRowWithoutActions (attack: Attack) =
        tr [ ]
            [ td [ ]
                [ str (textDescriptionForResourcePool attack.Cost) ]
              td [ ]
                [ str attack.Name ]
              renderDamageInformationForAttack attack ]

let renderAttackRow displayAttackButton canAttack availableResources gameId playerId inPlayCreatureId (attack: Attack) dispatch = // Lol this needs to be refactored
    let execAttack =  (fun _ ->
                                                    ({
                                                        GameId = gameId
                                                        PlayerId = playerId
                                                        InPlayCreatureId = inPlayCreatureId
                                                        Attack = attack
                                                    } :  PerformAttackEvent) |> PerformAttack |>  dispatch)

    let displayAttackButton = displayAttackButton && canAttack && (hasEnoughResources availableResources (Map.toList attack.Cost))

    tr [ ]
            [
              td [ ]
                [ str (textDescriptionForResourcePool attack.Cost) ]
              td [ ]
                [ str attack.Name ]
              renderDamageInformationForAttack attack
              td [ ]
                [
                    if displayAttackButton then
                        button [
                            Class "is-danger"
                            OnClick execAttack
                        ]
                            [
                                str "Exec"
                            ]
                    else
                        str ""
                ] ]


