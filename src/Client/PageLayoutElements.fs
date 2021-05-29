module PageLayoutParts

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Shared.Domain
open GeneralUIHelpers


let topNavigation =
  nav [ Class "navbar is-dark" ]
    [ div [ Class "container" ]
        [ div [ Class "navbar-brand" ]
            [ a [ Class "navbar-item"
                  Href "#" ]
                [ str "SAFE Card Game" ]
              a [ Class "navbar-burger"
                  Role "button"
                  HTMLAttr.Custom ("aria-label", "menu")
                  AriaExpanded false ]
                [ span [ HTMLAttr.Custom ("aria-hidden", "true") ]
                    [ ]
                  span [ HTMLAttr.Custom ("aria-hidden", "true") ]
                    [ ]
                  span [ HTMLAttr.Custom ("aria-hidden", "true") ]
                    [ ] ] ]
          div [ Class "navbar-menu" ]
            [ div [ Class "navbar-start" ]
                [ a [ Class "navbar-item"
                      Href "#" ]
                    [ str "Home" ]
                  a [ Class "navbar-item"
                      Href "#" ]
                    [ str "Leader Board" ]
                  a [ Class "navbar-item"
                      Href "#" ]
                    [ str "About" ]
                  div [ Class "navbar-item has-dropdown is-hoverable" ]
                    [ a [ Class "navbar-link" ]
                        [ str "Game" ]
                      div [ Class "navbar-dropdown" ]
                        [ a [ Class "navbar-item navbar-item-dropdown"
                              Href "#" ]
                            [ str "New Game" ]
                          a [ Class "navbar-item navbar-item-dropdown"
                              Href "#" ]
                            [ str "Load Game" ] ] ] ]
              div [ Class "navbar-end" ]
                [ div [ Class "navbar-item" ]
                    [ div [ Class "buttons" ]
                        [ a [ Class "button is-light"
                              Href "#" ]
                            [ str "Log in" ]
                          a [ Class "button is-primary"
                              Href "#" ]
                            [ str "Sign up" ]
                          a [ Class "button is-dark"
                              Href "https://github.com/jamesclancy/SafeCardGame" ]
                            [ str "Source" ] ] ] ] ] ] ]

let playerStats  (player: Player) (playerBoard: PlayerBoard) =
    [             div [ Class "navbar-item" ]
                    [ a [ Class "button is-primary"
                          Href "#" ]
                        [ str (sprintf "💓 %i/10" player.RemainingLifePoints) ] ]
                  div [ Class "navbar-item" ]
                    [ a [ Class "button is-primary"
                          Href "#" ]
                        [ str (sprintf "🤚 %i" playerBoard.Hand.Cards.Length) ] ]
                  div [ Class "navbar-item" ]
                    [ a [ Class "button is-primary"
                          Href "#" ]
                        [ str (sprintf "🂠 %i" playerBoard.Deck.Cards.Length) ] ]
                  div [ Class "navbar-item" ]
                    [ a [ Class "button is-primary"
                          Href "#" ]
                        [ str (sprintf "🗑️ %i" playerBoard.DiscardPile.Cards.Length)] ] ]

let enemyStats (player: Player) (playerBoard: PlayerBoard) =
  nav [ Class "navbar is-fullwidth is-danger" ]
    [ div [ Class "container" ]
        [ div [ Class "navbar-brand" ]
            [ p [ Class "navbar-item title is-5"
                  Href "#" ]
                [ str player.Name ] ]
          div [ Class "navbar-menu" ]
            [ div [ Class "navbar-start" ]
                [ yield! playerStats player playerBoard ] ] ] ]

let renderEnemyActiveCreature (inPlayCreature : Option<InPlayCreature>) =
  match (Option.map (fun x -> (x, x.Card)) inPlayCreature) with
  | None -> strong [] [ str "No creature in active play" ]
  | Some (creature, EffectCard card) ->  strong [] [ str "Active creature is not a character?" ]
  | Some (creature, ResourceCard card) -> strong [] [ str "Active creature is not a character?" ]
  | Some (creature, CharacterCard card) ->
        a [ Href "#" ]
                    [ div [ Class "card" ]
                        [ header [ Class "card-header" ]
                            [ p [ Class "card-header-title" ]
                                [ str (sprintf "✫ %s" card.Name) ]
                              p [ Class "card-header-icon" ]
                                [ str (sprintf "💓 %i/%i" (card.Creature.Health - creature.CurrentDamage) card.Creature.Health)
                                  str (textDescriptionForListOfSpecialConditions creature.SpecialEffect) ] ]
                          div [ Class "card-image" ]
                            [ figure [ Class "image is-4by3" ]
                                [ img [ Src (card.ImageUrl.ToString())
                                        Alt card.Name
                                        Class "is-fullwidth" ] ] ]
                          div [ Class "card-content" ]
                            [ div [ Class "content" ]
                                [ p [ Class "is-italic" ]
                                    [ str card.Description ]
                                  h5 [ Class "IsTitle is5" ]
                                    [ str "Attacks" ]
                                  table [ ]
                                    [
                                        yield! seq {
                                            for a in card.Creature.Attack do
                                                (renderAttackRow a)
                                        } ] ] ] ] ]

let renderEnemyBenchRow inPlayCreature =
    match inPlayCreature.Card with
    | EffectCard ec -> strong [] [ str "Bench creature is not a character code" ]
    | ResourceCard rc -> strong [] [ str "Bench creature is not a character code" ]
    | CharacterCard card ->
        tr [ ]
                            [ td [ ]
                                [ str card.Name ]
                              td [ ]
                                [ str (textDescriptionForListOfSpecialConditions inPlayCreature.SpecialEffect) ]
                              td [ ]
                                [ str (sprintf "💓 %i/%i" (card.Creature.Health - inPlayCreature.CurrentDamage) card.Creature.Health)  ] ]

let renderEnemyBench bench  =
    match bench with
    | None -> strong [] [ str "No creatures on bench" ]
    | Some  l->
                      table [ Class "table is-fullwidth" ]
                        [ tr [ ]
                            [ th [ ]
                                [ str "Creature Name" ]
                              th [ ]
                                [ str "Status" ]
                              th [ ]
                                [ str "Health" ] ]
                          yield! seq {
                                for b in l do
                                renderEnemyBenchRow b
                          }
                        ]

let enemyCreatures  (player: Player) (playerBoard: PlayerBoard) =
  section [
          Class "section"
          Style [ Background (sprintf "url('%s')" (player.PlaymatUrl.ToString()))
                  BackgroundSize "cover"
                  BackgroundRepeat "no-repeat"
                  BackgroundPosition "center center" ] ]
    [ div [ Class "container py-r" ]
        [ div [ Class "columns" ]
            [ div [ Class "column is-1" ]
                [ ]
              div [ Class "column is-3" ]
                [ renderEnemyActiveCreature playerBoard.ActiveCreature ]
              div [ Class "column is-7" ]
                [ div [ Class "columns is-mobile is-multiline" ]
                    [ h2 [ Class "title is-4" ]
                        [ str "Bench" ]
                      renderEnemyBench playerBoard.Bench
                    ] ] ] ] ]

let yourCurrentStepClasses (gameState : GameState) (gamesStep: GameStep) =
        if gameState.CurrentStep = gamesStep then "button is-danger"
        else "button is-primary"

let currentStepInformation (player: Player) (gameState : GameState)  =
    div [ Class "navbar-item" ]
                    [ div [ Class "field is-grouped has-addons is-grouped-right" ]
                        [ p [ Class "control" ]
                            [ button [ Class (yourCurrentStepClasses gameState (player.PlayerId |> GameStep.Draw))
                                       Disabled true ]
                                [ span [ ]
                                    [ str "Draw" ] ] ]
                          p [ Class "control" ]
                            [ button [ Class (yourCurrentStepClasses gameState (player.PlayerId |> GameStep.Draw))
                                       Disabled true ]
                                [ span [ ]
                                    [ str "Play" ] ] ]
                          p [ Class "control" ]
                            [ button [ Class (yourCurrentStepClasses gameState (player.PlayerId |> GameStep.Draw))
                                       Disabled true ]
                                [ span [ ]
                                    [ str "Attack" ] ] ]
                          p [ Class "control" ]
                            [ button [ Class (yourCurrentStepClasses gameState (player.PlayerId |> GameStep.Draw))
                                       Disabled true ]
                                [ span [ ]
                                    [ str "Reconcile" ] ] ] ] ]

let playerControlCenter  (player: Player) (playerBoard: PlayerBoard) (gameState : GameState) =
  nav [ Class "navbar is-fullwidth is-primary" ]
    [ div [ Class "container" ]
        [ div [ Class "navbar-brand" ]
            [ p [ Class "navbar-item title is-5"
                  Href "#" ]
                [ str player.Name ] ]
          div [ Class "navbar-menu" ]
            [ div [ Class "navbar-start" ]
                [ yield! playerStats player playerBoard
                  currentStepInformation player gameState ]
              div [ Class "navbar-end" ]
                [ div [ Class "navbar-item" ]
                    [ div [ Class "field is-grouped has-addons is-grouped-right" ]
                        [ p [ Class "control" ]
                            [ button [ Class "button is-success is-large" ]
                                [ span [ ]
                                    [ str "Attack" ] ] ]
                          p [ Class "control" ]
                            [ button [ Class "button is-warning is-large" ]
                                [ span [ ]
                                    [ str "Skip Step" ] ] ] ] ] ] ] ] ]

let playerActiveCreature (inPlayCreature : Option<InPlayCreature>) =
  match (Option.map (fun x -> (x, x.Card)) inPlayCreature) with
  | None -> strong [] [ str "No creature in active play" ]
  | Some (creature, EffectCard card) ->  strong [] [ str "Active creature is not a character?" ]
  | Some (creature, ResourceCard card) -> strong [] [ str "Active creature is not a character?" ]
  | Some (creature, CharacterCard card) ->
        a [ Href "#" ]
                    [ div [ Class "card" ]
                        [ header [ Class "card-header" ]
                            [ p [ Class "card-header-title" ]
                                [ str (sprintf "✫ %s" card.Name) ]
                              p [ Class "card-header-icon" ]
                                [ str (sprintf "💓 %i/%i" (card.Creature.Health - creature.CurrentDamage) card.Creature.Health)
                                  str (textDescriptionForListOfSpecialConditions creature.SpecialEffect) ] ]
                          div [ Class "card-image" ]
                            [ figure [ Class "image is-4by3" ]
                                [ img [ Src (card.ImageUrl.ToString())
                                        Alt card.Name
                                        Class "is-fullwidth" ] ] ]
                          div [ Class "card-content" ]
                            [ div [ Class "content" ]
                                [ p [ Class "is-italic" ]
                                    [ str card.Description ]
                                  h5 [ Class "IsTitle is5" ]
                                    [ str "Attacks" ]
                                  table [ ]
                                    [
                                        yield! seq {
                                            for a in card.Creature.Attack do
                                                (renderAttackRow a)
                                        } ] ] ]
                          footer [ Class "card-footer" ]
                            [ a [ Href "#"
                                  Class "card-footer-item" ]
                                [ str "Tap Out" ] ] ] ]

let playerBenchCreature (inPlayCreature : InPlayCreature)=
  match (inPlayCreature, inPlayCreature.Card) with
  | (creature, EffectCard card) ->  strong [] [ str "Active creature is not a character?" ]
  | (creature, ResourceCard card) -> strong [] [ str "Active creature is not a character?" ]
  | (creature, CharacterCard card) ->
         div [ Class "column is-4-mobile is-12-tablet" ]
                    [ div [ Class "card" ]
                        [ header [ Class "card-header" ]
                            [ p [ Class "card-header-title" ]
                                [ str card.Name ]
                              p [ Class "card-header-icon" ]
                                [ str (sprintf "💓 %i/%i" (card.Creature.Health - creature.CurrentDamage) card.Creature.Health)
                                  str (textDescriptionForListOfSpecialConditions creature.SpecialEffect) ] ]
                          div [ Class "card-image" ]
                            [ figure [ Class "image is-4by3" ]
                                [ img [ Src (card.ImageUrl.ToString())
                                        Alt card.Name
                                        Class "is-fullwidth" ] ] ]
                          div [ Class "card-content" ]
                            [ div [ Class "content" ]
                                [ p [ Class "is-italic" ]
                                    [ str card.Description ]
                                  h5 [ Class "IsTitle is5" ]
                                    [ str "Attacks" ]
                                  table [ ]
                                    [
                                        yield! seq {
                                            for a in card.Creature.Attack do
                                                (renderAttackRow a)
                                        } ] ] ] ] ]

let playerBench (bench : Option<InPlayCreature list>) columnsInBench =
    match bench with
    | None -> strong [] [ str "No creatures on bench."]
    | Some l ->
      div [ Class "column is-9"] [
        div [ Class "container"] [
          h3 [Class "title is-3"] [str "Bench"]
          yield! seq {
            let numberOfRows = (l.Length / columnsInBench)
            for row in {0 .. numberOfRows }  do
                let innerl = Seq.truncate columnsInBench (Seq.skip (row * columnsInBench) l)

                div [ Class "columns is-mobile is-multiline" ]
                    [
                       yield! seq {
                           for creature in innerl do
                            div [ Class (sprintf "column is-%i" (12/columnsInBench)) ] [
                                playerBenchCreature creature
                            ]
                       }
                    ]
        } ] ]


let playerCreatures  (player: Player) (playerBoard: PlayerBoard) =
  section [
          Class "section"
          Style [ Background (sprintf "url('%s')" (player.PlaymatUrl.ToString()))
                  BackgroundSize "cover"
                  BackgroundRepeat "no-repeat"
                  BackgroundPosition "center center" ] ]
    [ div [ Class "container py-r" ] [
          div [ Class "columns" ]
            [ div [ Class "column is-3" ]
                [ playerActiveCreature playerBoard.ActiveCreature ]
              playerBench playerBoard.Bench 4

               ] ] ]

let displayCardSpecialEffectDetailIfPresent title (value : Option<GameStateSpecialEffect>)=
    match value with
    | Some s ->
                p [ Class "is-italic" ]
                        [ strong [ ]
                             [ str title ]
                          str s.Description ]
    | None -> span [] []

let renderCharacterCard (card: CharacterCard) =
      div [ Class "column is-4" ]
                [ div [ Class "card" ]
                    [ header [ Class "card-header" ]
                        [ p [ Class "card-header-title" ]
                            [ str card.Name ]
                          p [ Class "card-header-icon" ]
                            [ str (textDescriptionForResourcePool card.ResourceCost) ] ]
                      div [ Class "card-image" ]
                        [ figure [ Class "image is-4by3" ]
                            [ img [ Src (card.ImageUrl.ToString())
                                    Alt card.Name
                                    Class "is-fullwidth" ] ] ]
                      div [ Class "card-content" ]
                        [ div [ Class "content" ]
                            [ p [ Class "is-italic" ]
                                [ str card.Description ]
                              displayCardSpecialEffectDetailIfPresent "On Enter Playing Field" card.EnterSpecialEffects
                              displayCardSpecialEffectDetailIfPresent "On Exit Playing Field" card.ExitSpecialEffects
                              h5 [ Class "IsTitle is5" ]
                                [ str "Attacks" ]
                              table [ ]
                                [
                                  yield! seq {
                                    for a in card.Creature.Attack do
                                      (renderAttackRow a)
                                  }
                                ] ] ]
                      footer [ Class "card-footer" ]
                        [ a [ Href "#"
                              Class "card-footer-item" ]
                            [ str "Play" ]
                          a [ Href "#"
                              Class "card-footer-item" ]
                            [ str "Discard" ] ] ] ]

let renderCardForHand (card: Card) =
    match card with
    | CharacterCard c -> renderCharacterCard c
    | _ ->
        strong [] [ str "IDK" ]

let renderCardInstanceForHand (card: CardInstance) =
    renderCardForHand card.Card

let playerHand (hand : Hand) =
  section [ Class "section" ]
    [ div [ Class "container py-4" ]
        [ h3 [ Class "title is-spaced is-4" ]
            [ str "Hand" ]
          div [ Class "columns is-mobile mb-5" ]
            [ yield! Seq.map renderCardInstanceForHand hand.Cards ] ] ]

let footerBand =
  footer [ Class "footer" ]
    [ div [ Class "container" ]
        [ div [ Class "level" ]
            [ div [ Class "level-left" ]
                [ div [ Class "level-item" ]
                    [ a [ Class "title is-4"
                          Href "#" ]
                        [ str "SAFE Card Game" ] ] ]
              div [ Class "level-right" ]
                [ a [ Class "level-item"
                      Href "#" ]
                    [ str "Home" ]
                  a [ Class "level-item"
                      Href "#" ]
                    [ str "Leader Board" ]
                  a [ Class "level-item"
                      Href "#" ]
                    [ str "About" ] ] ]
          hr [ ]
          div [ Class "columns" ]
            [ div [ Class "column" ]
                [ div [ Class "buttons" ]
                    [ a [ Class "button"
                          Href "#" ]
                        [ img [ Src "placeholder/icons/twitter.svg"
                                Alt "" ] ]
                      a [ Class "button"
                          Href "#" ]
                        [ img [ Src "placeholder/icons/facebook-f.svg"
                                Alt "" ] ]
                      a [ Class "button"
                          Href "#" ]
                        [ img [ Src "placeholder/icons/instagram.svg"
                                Alt "" ] ] ] ]
              div [ Class "column has-text-centered has-text-right-tablet" ]
                [ p [ Class "subtitle is-6" ]
                    [ str "© ??" ] ] ] ] ]

let mainLayout  model dispatch =
  match extractNeededModelsFromState model with
  | Ok op, Ok opb, Ok cp, Ok cpb ->
      div [ Class "container is-fluid" ]
        [ topNavigation
          br [ ]
          br [ ]
          enemyStats op opb
          enemyCreatures op opb
          playerControlCenter cp cpb model
          playerCreatures cp cpb
          playerHand cpb.Hand
          footerBand
        ]
  | _ -> strong [] [ str "Error in GameState encountered." ]