module GeneralUIHelpers

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Shared.Domain
open Events

let renderAttackRowWithoutActions (attack: Attack) =
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

let renderAttackRow displayAttackButton canAttack availableResources gameId playerId inPlayCreatureId (attack: Attack) dispatch =
    let execAttack =  (fun _ ->
                                                    ({
                                                        GameId = gameId
                                                        PlayerId = playerId
                                                        InPlayCreatureId = inPlayCreatureId
                                                        Attack = attack
                                                    } :  PerformAttackEvent) |> PerformAttack |>  dispatch)

    let displayAttackButton = displayAttackButton && canAttack && (hasEnoughResources availableResources (Map.toList attack.Cost))


    match attack.SpecialEffect with
    | Some se ->
        tr [ ]
            [ td [ ]
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
                ]
              td [ ]
                [ str (textDescriptionForResourcePool attack.Cost) ]
              td [ ]
                [ str attack.Name ]
              td [ ]
                [
                    p [] [ str (sprintf "%i" attack.Damage) ]
                    p [] [ str se.Description ]
                ]

                 ]
    | None ->
        tr [ ]
            [
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
                ]
              td [ ]
                [ str (textDescriptionForResourcePool attack.Cost) ]
              td [ ]
                [ str attack.Name ]
              td [ ]
                [
                    p [] [ str (sprintf "%i" attack.Damage) ]
                ] ]


