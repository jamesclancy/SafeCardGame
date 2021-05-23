module PageLayoutParts

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Shared.Domain

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

let enemyCreatures  (player: Player) (playerBoard: PlayerBoard) =
  section [
          Class "section"
          Style [ Background "url('https://picsum.photos/id/10/2500/1667?blur=5')"
                  BackgroundSize "cover"
                  BackgroundRepeat "no-repeat"
                  BackgroundPosition "center center" ] ]
    [ div [ Class "container py-r" ]
        [ div [ Class "columns" ]
            [ div [ Class "column is-1" ]
                [ ]
              div [ Class "column is-3" ]
                [ a [ Href "#" ]
                    [ div [ Class "card" ]
                        [ header [ Class "card-header" ]
                            [ p [ Class "card-header-title" ]
                                [ str "✫ Card Name" ]
                              p [ Class "card-header-icon" ]
                                [ str "50/100 Health"
                                  str "☠️
                                    💤" ] ]
                          div [ Class "card-image" ]
                            [ figure [ Class "image is-4by3" ]
                                [ img [ Src "https://picsum.photos/600/300?1"
                                        Alt "Placeholder image"
                                        Class "is-fullwidth" ] ] ]
                          div [ Class "card-content" ]
                            [ div [ Class "content" ]
                                [ p [ Class "is-italic" ]
                                    [ str "This is a sweet description." ]
                                  h5 [ Class "IsTitle is5" ]
                                    [ str "Attacks" ]
                                  table [ ]
                                    [ tr [ ]
                                        [ td [ ]
                                            [ str "🍂 x1" ]
                                          td [ ]
                                            [ str "Leaf Cut" ]
                                          td [ ]
                                            [ str "10" ] ]
                                      tr [ ]
                                        [ td [ ]
                                            [ str "🍂 x2" ]
                                          td [ ]
                                            [ str "Vine Whip" ]
                                          td [ ]
                                            [ str "30" ] ] ] ] ] ] ] ]
              div [ Class "column is-7" ]
                [ div [ Class "columns is-mobile is-multiline" ]
                    [ h2 [ Class "title is-4" ]
                        [ str "Bench" ]
                      table [ Class "table is-fullwidth" ]
                        [ tr [ ]
                            [ th [ ]
                                [ str "Creature Name" ]
                              th [ ]
                                [ str "Status" ]
                              th [ ]
                                [ str "Health" ] ]
                          tr [ ]
                            [ td [ ]
                                [ str "Creature A" ]
                              td [ ]
                                [ ]
                              td [ ]
                                [ str "90/100" ] ]
                          tr [ ]
                            [ td [ ]
                                [ str "Creature A" ]
                              td [ ]
                                [ ]
                              td [ ]
                                [ str "90/100" ] ]
                          tr [ ]
                            [ td [ ]
                                [ str "Creature A" ]
                              td [ ]
                                [ ]
                              td [ ]
                                [ str "90/100" ] ] ] ] ] ] ] ]


let yourCurrentStepClasses (player: Player) (gameState : GameState) (gamesStep: GameStep) =
    if not (player.PlayerId = gameState.CurrentPlayer) then "button is-primary"
    else
        if gameState.CurrentStep = gamesStep then "button is-danger"
        else "button is-primary"

let currentStepInformation (player: Player) (playerBoard: PlayerBoard) (gameState : GameState) =
    div [ Class "navbar-item" ]
                    [ div [ Class "field is-grouped has-addons is-grouped-right" ]
                        [ p [ Class "control" ]
                            [ button [ Class (yourCurrentStepClasses player gameState GameStep.Draw)
                                       Disabled true ]
                                [ span [ ]
                                    [ str "Draw" ] ] ]
                          p [ Class "control" ]
                            [ button [ Class (yourCurrentStepClasses player gameState GameStep.Play)
                                       Disabled true ]
                                [ span [ ]
                                    [ str "Play" ] ] ]
                          p [ Class "control" ]
                            [ button [ Class (yourCurrentStepClasses player gameState GameStep.Attack)
                                       Disabled true ]
                                [ span [ ]
                                    [ str "Attack" ] ] ]
                          p [ Class "control" ]
                            [ button [ Class (yourCurrentStepClasses player gameState GameStep.Reconcile)
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
                  currentStepInformation player playerBoard gameState ]
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

let playerCreatures  (player: Player) (playerBoard: PlayerBoard) =
  section [
          Class "section"
          Style [ Background "url('https://picsum.photos/id/1000/2500/1667?blur=5')"
                  BackgroundSize "cover"
                  BackgroundRepeat "no-repeat"
                  BackgroundPosition "center center" ] ]
    [ div [ Class "container py-r" ] [
          div [ Class "columns" ]
            [ div [ Class "column is-3" ]
                [ a [ Href "#" ]
                    [ div [ Class "card" ]
                        [ header [ Class "card-header" ]
                            [ p [ Class "card-header-title" ]
                                [ str "✫ Card Name" ]
                              p [ Class "card-header-icon" ]
                                [ str "50/100 Health"
                                  str "☠️
                                            💤" ] ]
                          div [ Class "card-image" ]
                            [ figure [ Class "image is-4by3" ]
                                [ img [ Src "https://picsum.photos/600/300?1"
                                        Alt "Placeholder image"
                                        Class "is-fullwidth" ] ] ]
                          div [ Class "card-content" ]
                            [ div [ Class "content" ]
                                [ p [ Class "is-italic" ]
                                    [ str "This is a sweet description." ]
                                  h5 [ Class "IsTitle is5" ]
                                    [ str "Attacks" ]
                                  table [ ]
                                    [ tr [ ]
                                        [ td [ ]
                                            [ str "🍂 x1" ]
                                          td [ ]
                                            [ str "Leaf Cut" ]
                                          td [ ]
                                            [ str "10" ]
                                          td [ ]
                                            [ button [ ]
                                                [ str "Use" ] ] ]
                                      tr [ ]
                                        [ td [ ]
                                            [ str "🍂 x2" ]
                                          td [ ]
                                            [ str "Vine Whip" ]
                                          td [ ]
                                            [ str "30" ]
                                          td [ ]
                                            [ ] ] ] ] ]
                          footer [ Class "card-footer" ]
                            [ a [ Href "#"
                                  Class "card-footer-item" ]
                                [ str "Tap Out" ] ] ] ] ]
              div [ Class "column is-3" ]
                [ div [ Class "columns is-mobile is-multiline" ]
                    [ div [ Class "column is-4-mobile is-12-tablet" ]
                        [ div [ Class "card" ]
                            [ header [ Class "card-header" ]
                                [ p [ Class "card-header-title" ]
                                    [ str "Card Name" ]
                                  p [ Class "card-header-icon" ]
                                    [ str "50/100 Health"
                                      str "☠️
                                                💤" ] ]
                              div [ Class "card-image" ]
                                [ figure [ Class "image is-4by3" ]
                                    [ img [ Src "https://picsum.photos/320/200?2"
                                            Alt "Placeholder image"
                                            Class "is-fullwidth" ] ] ]
                              div [ Class "card-content" ]
                                [ div [ Class "content" ]
                                    [ p [ Class "is-italic" ]
                                        [ str "This is a sweet description." ]
                                      h5 [ Class "IsTitle is5" ]
                                        [ str "Attacks" ]
                                      table [ ]
                                        [ tr [ ]
                                            [ td [ ]
                                                [ str "🍂 x1" ]
                                              td [ ]
                                                [ str "Leaf Cut" ]
                                              td [ ]
                                                [ str "10" ] ]
                                          tr [ ]
                                            [ td [ ]
                                                [ str "🍂 x2" ]
                                              td [ ]
                                                [ str "Vine Whip" ]
                                              td [ ]
                                                [ str "30" ] ] ] ] ] ] ]
                      div [ Class "column is-4-mobile is-12-tablet" ]
                        [ div [ Class "card" ]
                            [ header [ Class "card-header" ]
                                [ p [ Class "card-header-title" ]
                                    [ str "Card Name" ]
                                  p [ Class "card-header-icon" ]
                                    [ str "50/100 Health"
                                      str "☠️
                                                💤" ] ]
                              div [ Class "card-image" ]
                                [ figure [ Class "image is-4by3" ]
                                    [ img [ Src "https://picsum.photos/320/200?2"
                                            Alt "Placeholder image"
                                            Class "is-fullwidth" ] ] ]
                              div [ Class "card-content" ]
                                [ div [ Class "content" ]
                                    [ p [ Class "is-italic" ]
                                        [ str "This is a sweet description." ]
                                      h5 [ Class "IsTitle is5" ]
                                        [ str "Attacks" ]
                                      table [ ]
                                        [ tr [ ]
                                            [ td [ ]
                                                [ str "🍂 x1" ]
                                              td [ ]
                                                [ str "Leaf Cut" ]
                                              td [ ]
                                                [ str "10" ] ]
                                          tr [ ]
                                            [ td [ ]
                                                [ str "🍂 x2" ]
                                              td [ ]
                                                [ str "Vine Whip" ]
                                              td [ ]
                                                [ str "30" ] ] ] ] ] ] ] ] ]
              div [ Class "column is-3" ]
                [ div [ Class "columns is-mobile is-multiline" ]
                    [ div [ Class "column is-4-mobile is-12-tablet" ]
                        [ div [ Class "card" ]
                            [ header [ Class "card-header" ]
                                [ p [ Class "card-header-title" ]
                                    [ str "Card Name" ]
                                  p [ Class "card-header-icon" ]
                                    [ str "50/100 Health"
                                      str "☠️
                                                💤" ] ]
                              div [ Class "card-image" ]
                                [ figure [ Class "image is-4by3" ]
                                    [ img [ Src "https://picsum.photos/320/200?2"
                                            Alt "Placeholder image"
                                            Class "is-fullwidth" ] ] ]
                              div [ Class "card-content" ]
                                [ div [ Class "content" ]
                                    [ p [ Class "is-italic" ]
                                        [ str "This is a sweet description." ]
                                      h5 [ Class "IsTitle is5" ]
                                        [ str "Attacks" ]
                                      table [ ]
                                        [ tr [ ]
                                            [ td [ ]
                                                [ str "🍂 x1" ]
                                              td [ ]
                                                [ str "Leaf Cut" ]
                                              td [ ]
                                                [ str "10" ] ]
                                          tr [ ]
                                            [ td [ ]
                                                [ str "🍂 x2" ]
                                              td [ ]
                                                [ str "Vine Whip" ]
                                              td [ ]
                                                [ str "30" ] ] ] ] ] ] ]
                      div [ Class "column is-4-mobile is-12-tablet" ]
                        [ div [ Class "card" ]
                            [ header [ Class "card-header" ]
                                [ p [ Class "card-header-title" ]
                                    [ str "Card Name" ]
                                  p [ Class "card-header-icon" ]
                                    [ str "50/100 Health"
                                      str "☠️
                                                💤" ] ]
                              div [ Class "card-image" ]
                                [ figure [ Class "image is-4by3" ]
                                    [ img [ Src "https://picsum.photos/320/200?2"
                                            Alt "Placeholder image"
                                            Class "is-fullwidth" ] ] ]
                              div [ Class "card-content" ]
                                [ div [ Class "content" ]
                                    [ p [ Class "is-italic" ]
                                        [ str "This is a sweet description." ]
                                      h5 [ Class "IsTitle is5" ]
                                        [ str "Attacks" ]
                                      table [ ]
                                        [ tr [ ]
                                            [ td [ ]
                                                [ str "🍂 x1" ]
                                              td [ ]
                                                [ str "Leaf Cut" ]
                                              td [ ]
                                                [ str "10" ] ]
                                          tr [ ]
                                            [ td [ ]
                                                [ str "🍂 x2" ]
                                              td [ ]
                                                [ str "Vine Whip" ]
                                              td [ ]
                                                [ str "30" ] ] ] ] ] ] ] ] ]
              div [ Class "column is-3" ]
                [ div [ Class "columns is-mobile is-multiline" ]
                    [ div [ Class "column is-4-mobile is-12-tablet" ]
                        [ div [ Class "card" ]
                            [ header [ Class "card-header" ]
                                [ p [ Class "card-header-title" ]
                                    [ str "Card Name" ]
                                  p [ Class "card-header-icon" ]
                                    [ str "50/100 Health"
                                      str "☠️
                                                💤" ] ]
                              div [ Class "card-image" ]
                                [ figure [ Class "image is-4by3" ]
                                    [ img [ Src "https://picsum.photos/320/200?2"
                                            Alt "Placeholder image"
                                            Class "is-fullwidth" ] ] ]
                              div [ Class "card-content" ]
                                [ div [ Class "content" ]
                                    [ p [ Class "is-italic" ]
                                        [ str "This is a sweet description." ]
                                      h5 [ Class "IsTitle is5" ]
                                        [ str "Attacks" ]
                                      table [ ]
                                        [ tr [ ]
                                            [ td [ ]
                                                [ str "🍂 x1" ]
                                              td [ ]
                                                [ str "Leaf Cut" ]
                                              td [ ]
                                                [ str "10" ] ]
                                          tr [ ]
                                            [ td [ ]
                                                [ str "🍂 x2" ]
                                              td [ ]
                                                [ str "Vine Whip" ]
                                              td [ ]
                                                [ str "30" ] ] ] ] ] ] ]
                      div [ Class "column is-4-mobile is-12-tablet" ]
                        [ div [ Class "card" ]
                            [ header [ Class "card-header" ]
                                [ p [ Class "card-header-title" ]
                                    [ str "Card Name" ]
                                  p [ Class "card-header-icon" ]
                                    [ str "50/100 Health"
                                      str "☠️
                                                💤" ] ]
                              div [ Class "card-image" ]
                                [ figure [ Class "image is-4by3" ]
                                    [ img [ Src "https://picsum.photos/320/200?2"
                                            Alt "Placeholder image"
                                            Class "is-fullwidth" ] ] ]
                              div [ Class "card-content" ]
                                [ div [ Class "content" ]
                                    [ p [ Class "is-italic" ]
                                        [ str "This is a sweet description." ]
                                      h5 [ Class "IsTitle is5" ]
                                        [ str "Attacks" ]
                                      table [ ]
                                        [ tr [ ]
                                            [ td [ ]
                                                [ str "🍂 x1" ]
                                              td [ ]
                                                [ str "Leaf Cut" ]
                                              td [ ]
                                                [ str "10" ] ]
                                          tr [ ]
                                            [ td [ ]
                                                [ str "🍂 x2" ]
                                              td [ ]
                                                [ str "Vine Whip" ]
                                              td [ ]
                                                [ str "30" ] ] ] ] ] ] ] ] ] ] ] ]


let renderCardForHand (card: Card) =
  div [ Class "column is-4" ]
                [ div [ Class "card" ]
                    [ header [ Class "card-header" ]
                        [ p [ Class "card-header-title" ]
                            [ str "Card Name" ]
                          p [ Class "card-header-icon" ]
                            [ str "🍂 x4" ] ]
                      div [ Class "card-image" ]
                        [ figure [ Class "image is-4by3" ]
                            [ img [ Src "https://picsum.photos/320/200?1"
                                    Alt "Placeholder image"
                                    Class "is-fullwidth" ] ] ]
                      div [ Class "card-content" ]
                        [ div [ Class "content" ]
                            [ p [ Class "is-italic" ]
                                [ str "This is a sweet description." ]
                              p [ Class "is-italic" ]
                                [ strong [ ]
                                    [ str "On Enter Playing Field" ]
                                  str ": Effect description." ]
                              p [ Class "is-italic" ]
                                [ strong [ ]
                                    [ str "On Exit Playing Field" ]
                                  str ": Effect description." ]
                              h5 [ Class "IsTitle is5" ]
                                [ str "Attacks" ]
                              table [ ]
                                [ tr [ ]
                                    [ td [ ]
                                        [ str "🍂 x1" ]
                                      td [ ]
                                        [ str "Leaf Cut" ]
                                      td [ ]
                                        [ str "10" ] ]
                                  tr [ ]
                                    [ td [ ]
                                        [ str "🍂 x2" ]
                                      td [ ]
                                        [ str "Vine Whip" ]
                                      td [ ]
                                        [ str "30" ] ] ] ] ]
                      footer [ Class "card-footer" ]
                        [ a [ Href "#"
                              Class "card-footer-item" ]
                            [ str "Play" ]
                          a [ Href "#"
                              Class "card-footer-item" ]
                            [ str "Discard" ] ] ] ]

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

let opponentPlayer (model : GameState) =
    match model.Players.TryGetValue model.OpponentPlayer with
    | true, p -> p |> Ok
    | false, _ -> "Unable to locate oppponent in player list" |> Error


let opponentPlayerBoard (model : GameState) =
    match model.Boards.TryGetValue model.OpponentPlayer with
    | true, p -> p |> Ok
    | false, _ -> "Unable to locate oppponent in board list" |> Error

let currentPlayer (model : GameState) =
    match model.Players.TryGetValue model.CurrentPlayer with
    | true, p -> p |> Ok
    | false, _ -> "Unable to locate current player in player list" |> Error


let currentPlayerBoard (model : GameState) =
    match model.Boards.TryGetValue model.CurrentPlayer with
    | true, p -> p |> Ok
    | false, _ -> "Unable to locate current player in board list" |> Error

let extractNeededModelsFromState (model: GameState) =
    opponentPlayer model, opponentPlayerBoard model, currentPlayer model, currentPlayerBoard model


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