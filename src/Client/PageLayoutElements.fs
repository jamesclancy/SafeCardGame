module PageLayoutParts

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Shared.Domain
open GeneralUIHelpers
open Events


let topNavigation dispatch =
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
                              OnClick (fun _ -> SwapPlayer |> dispatch)
                              Href "#" ]
                            [ str "Switch Player" ]
                          a [ Class "button is-light"
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
                        [ str (sprintf "🗑️ %i" playerBoard.DiscardPile.Cards.Length)] ]
                  div [ Class "navbar-item" ]
                    [ a [ Class "button is-primary"
                          Href "#" ]
                        [ str (sprintf "Tot %s" (textDescriptionForResourcePool playerBoard.TotalResourcePool))
                          br []
                          str (sprintf "Ava %s" (textDescriptionForResourcePool playerBoard.AvailableResourcePool))] ] ]

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
                            [ button [ Class (yourCurrentStepClasses gameState (player.PlayerId |> GameStep.Play))
                                       Disabled true ]
                                [ span [ ]
                                    [ str "Play" ] ] ]
                          p [ Class "control" ]
                            [ button [ Class (yourCurrentStepClasses gameState (player.PlayerId |> GameStep.Attack))
                                       Disabled true ]
                                [ span [ ]
                                    [ str "Attack" ] ] ]
                          p [ Class "control" ]
                            [ button [ Class (yourCurrentStepClasses gameState (player.PlayerId |> GameStep.Reconcile))
                                       Disabled true ]
                                [ span [ ]
                                    [ str "Reconcile" ] ] ] ] ]


let renderStepOptionButton dispatch buttonText classType message =
                        p [ Class "control" ]
                            [ button [
                                Class (sprintf "button %s is-large" classType)
                                OnClick  (fun _-> (message |>  dispatch))
                                ]
                                [ span [ ]
                                    [ str buttonText ] ] ]

let stepNavigation  (player: Player) (playerBoard: PlayerBoard) (gameState : GameState) dispatch =
    match gameState.CurrentStep with
    |  GameStep.Draw d when d = player.PlayerId ->
            div [ Class "navbar-item" ]
                    [ div [ Class "field is-grouped has-addons is-grouped-right" ]
                        [
                            (renderStepOptionButton dispatch "Draw" "is-warning" (({ GameId = gameState.GameId; PlayerId = player.PlayerId; } : DrawCardEvent) |> DrawCard) )
                            ] ]
    |  GameStep.Play d when d = player.PlayerId ->
            div [ Class "navbar-item" ]
                    [ div [ Class "field is-grouped has-addons is-grouped-right" ]
                        [
                            (renderStepOptionButton dispatch "End Play" "is-danger" (({ GameId = gameState.GameId; PlayerId = player.PlayerId; } : EndPlayStepEvent) |> EndPlayStep) )
                            ] ]
    |  GameStep.Attack d when d = player.PlayerId ->
            div [ Class "navbar-item" ]
                    [ div [ Class "field is-grouped has-addons is-grouped-right" ]
                        [
                            (renderStepOptionButton dispatch "Skip Attack" "is-danger" (({ GameId = gameState.GameId; PlayerId = player.PlayerId; } : SkipAttackEvent) |> SkipAttack) )
                        ] ]
    |  GameStep.Reconcile d when d = player.PlayerId ->
            div [ Class "navbar-item" ]
                    [ div [ Class "field is-grouped has-addons is-grouped-right" ]
                        [
                            (renderStepOptionButton dispatch "End Turn" "is-danger" (({ GameId = gameState.GameId; PlayerId = player.PlayerId; } : EndTurnEvent) |> EndTurn) )
                        ] ]
    |  GameStep.NotCurrentlyPlaying ->
            div [ Class "navbar-item" ]
                    [ div [ Class "field is-grouped has-addons is-grouped-right" ]
                        [
                            (renderStepOptionButton dispatch "Start New Game" "is-danger" (({ GameId = gameState.GameId; PlayerId = player.PlayerId; } : EndTurnEvent) |> EndTurn) )
                        ] ]
    |  GameStep.GameOver g ->
            div [ Class "navbar-item" ]
                    [ div [ Class "field is-grouped has-addons is-grouped-right" ]
                        [
                            (renderStepOptionButton dispatch "Start New Game" "is-danger" (({ GameId = gameState.GameId; PlayerId = player.PlayerId; } : EndTurnEvent) |> EndTurn) )
                        ] ]
    | _ -> div [] []

let playerControlCenter  (player : Player) playerBoard gameState dispatch =
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
                [ stepNavigation player playerBoard gameState dispatch ] ] ] ]

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
     [
             header [ Class "card-header" ]
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
                                ]


let renderResourceCard (card: ResourceCard) =
    [
            header [ Class "card-header" ]
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
                           ] ] ]


let renderCardForHand (card: Card) : ReactElement list=
    match card with
    | CharacterCard c -> renderCharacterCard c
    | ResourceCard rc -> renderResourceCard rc
    | _ ->
         [
            strong [] [ str "IDK" ]
         ]

let isCardZoomed playerBoard cardInstanceId =
    match playerBoard.ZoomedCard with
    | Some c when c = cardInstanceId -> true
    | _ -> false

let renderCardInstanceForHand  (dispatch : Msg -> unit)  playerBoard gameId playerId (card: CardInstance) =
    let cardModal isActive closeDisplay =
       Modal.modal [ Modal.IsActive isActive ]
        [ Modal.background [ Props [ OnClick closeDisplay ] ] [ ]
          Modal.Card.card [ ]
            [ Modal.Card.head [ ]
                [ Modal.Card.title [ ]
                    [ str "Modal title" ]
                  Delete.delete [ Delete.OnClick closeDisplay ] [ ] ]
              Modal.Card.body [ ]
                        [ yield! renderCardForHand card.Card ]
              Modal.Card.foot [ ]
                              [ button [
                                    OnClick (fun _->
                                                    ({
                                                        GameId = gameId
                                                        PlayerId = playerId
                                                        CardInstanceId = card.CardInstanceId
                                                    } : PlayCardEvent) |> PlayCard |>  dispatch)
                                    Class "card-footer-item" ]
                                  [ str "Play" ]
                                button [
                                    OnClick (fun _->
                                                    ({
                                                        GameId = gameId
                                                        PlayerId = playerId
                                                        CardInstanceId = card.CardInstanceId
                                                    } : DiscardCardEvent) |> DiscardCard |>  dispatch)
                                    Class "card-footer-item" ]
                                  [ str "Discard" ] ] ] ]

    div [ ]
        [ cardModal (isCardZoomed playerBoard card.CardInstanceId) (fun _->
                                                    ({
                                                        GameId = gameId
                                                        PlayerId = playerId
                                                        CardInstanceId = card.CardInstanceId
                                                    } : ToggleZoomOnCardEvent) |> ToggleZoomOnCard |>  dispatch)
          Button.button [ Button.OnClick  (fun _->
                                                    ({
                                                        GameId = gameId
                                                        PlayerId = playerId
                                                        CardInstanceId = card.CardInstanceId
                                                    } : ToggleZoomOnCardEvent) |> ToggleZoomOnCard |>  dispatch) ]
            [ str (sprintf "Show card modal-%b-%O vs %O" (isCardZoomed playerBoard card.CardInstanceId) card.CardInstanceId playerBoard.ZoomedCard ) ] ]


let playerHand columnsInHand gameId playerId (hand : Hand)  (dispatch : Msg -> unit) pb=
  section [ Class "section" ]
    [ div [ Class "container py-4" ]
        [     h3 [ Class "title is-spaced is-4" ]  [ str "Hand" ]
              yield! seq {
                let numberOfRows = (hand.Cards.Length / columnsInHand)
                for row in {0 .. numberOfRows }  do
                    let innerl = Seq.truncate columnsInHand (Seq.skip (row * columnsInHand) hand.Cards)
                    div [ Class "columns is-mobile is-multiline" ]
                        [
                           yield! seq {
                               for creature in innerl do
                                div [ Class (sprintf "column is-%i" (12/columnsInHand)) ] [
                                    (renderCardInstanceForHand dispatch pb gameId playerId creature)
                                ]
                           }
                        ]
            } ] ]

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

let notificationArea (messages :Option<Notification list>) dispatch=
    match messages with
    | Some s ->
        div []
            [
               yield!  Seq.map (fun x -> div [Class "notification is-danger"] [
                                                    button [ Class "delete"
                                                             OnClick (fun y -> x.Id |> DeleteNotification |> dispatch)
                                                            ] []
                                                    str x.Content ]) s
            ]
    | _ ->
        div [] []

let mainLayout  model dispatch =
  match extractNeededModelsFromState model with
  | Ok op, Ok opb, Ok cp, Ok cpb ->
      div [ Class "container is-fluid is-full-width" ]
        [ topNavigation dispatch
          div [ Class "columns is-fluid is-full-width"] [
              div [ Class "column is-4"] [
                    div [ Class "container is-fluid is-full-width"] [
                    enemyStats op opb
                    enemyCreatures op opb
                    ] ]
              div [ Class "column is-8"] [
                    playerControlCenter cp cpb model dispatch
                    notificationArea model.NotificationMessages dispatch
                    playerCreatures cp cpb
                    playerHand 6 model.GameId cp.PlayerId cpb.Hand dispatch opb
                ]
          ]
          footerBand
        ]
  | _ -> strong [] [ str "Error in GameState encountered." ]