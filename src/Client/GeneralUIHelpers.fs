module GeneralUIHelpers

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Shared.Domain


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


