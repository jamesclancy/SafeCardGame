# Creating a Card Game with the Safe Stack

I have become interested in playing around with the `SAFE` Stack and have decided that it would be interesting to create an online “Card Game” using the stack. This card game should largely be similar to a simplified version of *MTG* or *Pokemon TCG*.

## Starting Project ##

Created fold for project

```
 mkdir SafeCardGame
 cd SafeCardGame
 ```

 Installed and created project from a template
```
dotnet new -i SAFE.Template
dotnet new SAFE
```

```
dotnet tool restore
dotnet fake build --target run
```

This should scaffold a basic todo app. Progress to here is located in the branch `step-1-intial-from-temp`

## Game Rules
---

My idea would be that scoping out the rules for the game might be the best first step.

### Defining the basic Domain Model and Terms
---

I think defining the domain models might be the best way to start documenting the rules.  The domain model should correlate to the object defined in the shared path of the project (i.e. src/shared). For now, I am just defining the types in the existing `Shared` module found in the Shared.fs.


The game starts with two players.

Each `Player` will start with a name and an amount of health.

```
type PlayerId = string

type Player =
    {
        PlayerId: PlayerId
        Name: string
        RemainingLifePoints: int
    }
```

Each player will have a `board` which consists of a Deck of cards, a hand, a pile of discarded, an optional active creature, a list of in play but not active creatures (i.e. a bench), a total resource pool and an available resource pool.

```
type PlayerBoard =
    {
        PlayerId: PlayerId
        Deck: Deck
        Hand: Hand
        ActiveCreature: Option<InPlayCreature>
        Bench:  Option<InPlayCreature list>
        DiscardPile: Deck
        TotalResourcePool: ResourcePool
        AvailableResourcePool: ResourcePool
    }

```

`Resource`s are typed from a finite list, i.e.:
```
type Resource =
    Grass | Fire |
    Water | Lightning |
    Psychic | Fighting |
    Colorless
```

A `ResourcePool` is simply a list of the different types of resources along with a quantity value.

```
type ResourcePool = Map<Resource, int>
```

A `Deck` and a `Hand` are both lists of `CardInstance`s, the main difference being that potentially a deck could have a number of cards exposed or visible.
```
type Hand =
    {
        Cards: CardInstance list
    }
type Deck =
    {
        Cards: CardInstance list
        TopCardsExposed: int
    }
```

Each `CardInstance` will contain a unique CardInstanceId and refers to a generic card referenced via a CardId. I am also placing the Card Type on the CardInstance, this seems pretty memory inefficient but probably doesn't matter due to the limited size of decks. For the moment it makes it much easier to manipulate the decks and pull information.
The `Card` referenced by the `CardId` will have:
* a name
* a resource cost (a `ResourcePool`)
* a Primary resource type for determining weaknesses
* a classification under one of three different types
    * `ResourceCard` - Also contains a `ResourcePool` for `AddedResource` and a bool for `ResourceAvailableOnFirstTurn`
    * `CreatureCard` - Also contains a `Creature`
    * `EffectCard`
* a `GameStateSpecialEffect` for when the card enters and leaves the game
    * the `GameStateSpecialEffect` will consist of a function which takes an arguement of the `GameState` and returns a new manipulated `GameState`

```
type CardInstanceId = string
type CardId = string
type GameStateSpecialEffect = delegate of GameState -> GameState
type CardInstance =
    {
        CardIntanceId : CardInstanceId
        CardId: CardId
        Card: Card
    }
type Card =
   CharacterCard of { CardId: CardId; Name: string; Creature: Creature; ResourceCost: ResourcePool; PrimaryResource: Resource; EnterSpecialEffects: GameStateSpecialEffect; ExitSpecialEffects: GameStateSpecialEffect}
   | EffectCard of { CardId: CardId; Name: string; ResourceCost: ResourcePool; PrimaryResource: Resource; EnterSpecialEffects: GameStateSpecialEffect; ExitSpecialEffects: GameStateSpecialEffect; }
   | ResourceCard of { CardId: CardId; Name: string; ResourceCost: ResourcePool; PrimaryResource: Resource; EnterSpecialEffects: GameStateSpecialEffect; ExitSpecialEffects: GameStateSpecialEffect; ResourceAvailableOnFirstTurn: bool; ResourcesAdded: ResourcePool}
```

A `Creature` will have integer health for total health associated with the card, a list of `Resource`s the creature is weak to, and a list of `Attacks.
An `Attack` will have an integer for the damage it inflicts, a `ResourcePool` representing the cost to use, and a `GameStateSpecialEffect` defining any additional effects which may be triggered by the attack.
```
type Creature =
    {
        Health: int
        Weaknesses: Resource list
        Attacks: Attack list
    }
type Attack =
    {
        Damage: int
        Cost: ResourcePool
        SpecialEffect: GameStateSpecialEffect
    }
```
An `InPlayCreature` has an id, is associated with a `CardInstance`, a current amount of damage, an optional list of `SpecialCondition`s currently applied as well as attached and spent resource pools.
The `InPlayCreatureId` is a string and available `SpecialCondition`s are `Asleep | Burned | Confused | Paralyzed | Poisoned`

```
type SpecialCondition = Asleep | Burned | Confused | Paralyzed | Poisoned
type InPlayCreatureId = string
type InPlayCreature =
    {
        InPlayCharacterId: InPlayCharacterId
        Card: Card
        CurrentDamage: int
        SpecialEffect: Option<SpecialCondition list>
        AttachedEnergy: ResourcePool
        SpentEnergy: ResourcePool
    }
```

Lastly, the `GameState` contains a:
* Map of `PlayerId`s to `Player`s
* Map of boards for each player, a map from `PlayerId` to `PlayerBoard`
* A current turn which is an optional `PlayerId representing the player whose turn it currently is.
* A current step which is a `GameStep` representing the current step the player is on in their turn.
* Finally, it contains a turn number representing the number of turns that have passed since the beginning of the game.

The GameStep is just an enum represented by `NotCurrentlyPlaying | Draw | Play | Attack | Reconcile`

```

type GameStep =
    NotCurrentlyPlaying | Draw | Play | Attack | Reconcile

type GameState =
    {
        Players: Map<PlayerId, Player>
        Boards: Map<PlayerId, PlayerBoard>
        CurrentTurn: Option<PlayerId>
        CurrentStep:GameStep
        TurnNumber: int
    }
```


If you look at the current Shared.fs file you can see that many of the `type` declarations have been transformed into `and`s this is because F# cares about the order in which things are declared. The `type` declares a thing in a subsequent order and the `and` keyword forces the items to be declared at the same time.

Running `dotnet fake build --target run` the application **FAILED to Build** with an error message of
```
  Restored C:\tests\SafeCardGame\src\Shared\Shared.fsproj (in 197 ms).
C:\tests\SafeCardGame\src\Shared\Shared.fs(51,21): error FS0035: This construct is deprecated: Consider using a separate record type instead [C:\tests\SafeCardGame\src\Shared\Shared.fsproj]
C:\tests\SafeCardGame\src\Shared\Shared.fs(52,20): error FS0035: This construct is deprecated: Consider using a separate record type instead [C:\tests\SafeCardGame\src\Shared\Shared.fsproj]
C:\tests\SafeCardGame\src\Shared\Shared.fs(53,22): error FS0035: This construct is deprecated: Consider using a separate record type instead [C:\tests\SafeCardGame\src\Shared\Shared.fsproj]

Build FAILED.

C:\tests\SafeCardGame\src\Shared\Shared.fs(51,21): error FS0035: This construct is deprecated: Consider using a separate record type instead [C:\tests\SafeCardGame\src\Shared\Shared.fsproj]
C:\tests\SafeCardGame\src\Shared\Shared.fs(52,20): error FS0035: This construct is deprecated: Consider using a separate record type instead [C:\tests\SafeCardGame\src\Shared\Shared.fsproj]
C:\tests\SafeCardGame\src\Shared\Shared.fs(53,22): error FS0035: This construct is deprecated: Consider using a separate record type instead [C:\tests\SafeCardGame\src\Shared\Shared.fsproj]
    0 Warning(s)
    3 Error(s)
```

It appears that newer versions of F# no longer allow anonymous types in discriminated unions so I had to break out those types like:
```
type Card =
   CharacterCard of CharacterCard
   | EffectCard of EffectCard
   | ResourceCard of ResourceCard
and CharacterCard
    = { CardId: CardId; Name: string; Creature: Creature; ResourceCost: ResourcePool; PrimaryResource: Resource; EnterSpecialEffects: GameStateSpecialEffect; ExitSpecialEffects: GameStateSpecialEffect}
and EffectCard
    = { CardId: CardId; Name: string; ResourceCost: ResourcePool; PrimaryResource: Resource; EnterSpecialEffects: GameStateSpecialEffect; ExitSpecialEffects: GameStateSpecialEffect; }
and ResourceCard
    = { CardId: CardId; Name: string; ResourceCost: ResourcePool; PrimaryResource: Resource; EnterSpecialEffects: GameStateSpecialEffect; ExitSpecialEffects: GameStateSpecialEffect; ResourceAvailableOnFirstTurn: bool; ResourcesAdded: ResourcePool}

```

Now running `dotnet fake build --target run` builds.

### Defining and Drawing the Gameboard
---

My idea for the next step is to lay out the general game board.
This serves a great base to force me to actually think out the domain, show visible progress and have a base to actually plug future developments into and visualize.

I first made a sketch of what I was thinking about for the board.

![Basic Sketch](documentation/img/InitialSketchOfBoard.png)

I used shuffle.dev to scaffold a basic ui using Bulma ([editor avail here](https://shuffle.dev/editor?project=10d217c2a045a04ac447cd87a95c49662d831217)). Fulma, a F# strongly typed wrapper for Bulma is baked into the Safe Stack which will be utilized later on.

From my initial sketch, I scaffolded out some stuff using shuffle, downloaded the HTML, and modified the HTML to create a basic template based on my sketch.

([Bulma template for the board](documentation/html/InitialSketchOfBoard.html))

I moved things around a number of times, I was unable to make it all fit on one page (the goal) but I think I was able to improve it somewhat.

([Bulma template for the board v2](documentation/html/InitialSketchOfBoardv2.html))

I am going to move forward with this layout. In the future, I am thinking the user could toggle something to transform the cards into tables or something along those lines.

This is the final commit in the `step-3-build-the-game-board-basic-layout` branch.

### Bringing the board layout into the client
---

I feel like the next logical step is to bring the layout I just developed to the client.

First I updated the index.html file in the src/client to reference the bulmaswatch layout "darkly" like the template.

I then googled around and found an online tool to convert html to the format used by elmish to define html. This is usable and somewhat close to the Fulma format we would like to eventually support. ([HTML to Elmish Site] (https://mangelmaxime.github.io/html-to-elmish/))

Whatever we convert is going to be plugged into the <div id="elmish-app"></div> in the index.html so I needed to extract the content of the body of the layout from the body. I saved it separately: ([template to convert](documentation/html/InitialSketchOfBoardv2_toConvert.html))

This failed with no error message on the site so I am assuming it doesn't like the html somewhere.

In order to move forward, I began breaking down the html page into pieces and converting the page part by part (which is something that would eventually have to be done anyway).

This will require future refinement but I initially broke out the page into parts for the

```
    topNavigation
    enemyStats
    enemyCreatures
    playerControlCenter
    playerCreatures
    playerHand
    footerBand
```
With a general arangement of
```
<div class="container is-fluid">
    topNavigation
    <br /><br />
    enemyStats
    enemyCreatures
    <section class="section">
        <div class="container py-r">
            playerControlCenter
            playerCreatures
        </div>
    </section>
    playerHand
    footerBand
</div>

```

Going in order I dropped each of these parts into the converter. When I reached `playerControlCenter` the converter stopped working. After playing around with the html for a while (no error message was returned) I was able to discover that the converter did not like the `disabled` on the buttons.

i.e. the blocks like:
```
<button class="button is-primary" disabled>
    <span>Draw</span>
</button>
```
needed to be changed to

```
<button class="button is-primary" disabled="true">
    <span>Draw</span>
</button>
```

I don't think this is needed from an html standpoint? It is probably just a feature of the converter.


I was eventually able to convert all the html subsections defining each as a variable (will eventually be converted to a function).

i.e. like:
```
let footerBand =
  footer [ Class "footer" ]
    [ div [ Class "container" ]
        [ div [ Class "level" ]
        ...
```

I then manually created the mainLayout as

```
let mainLayout =
  div [ Class "container is-fluid" ]
    [ topNavigation
      br [ ]
      br [ ]
      enemyStats
      enemyCreatures
      section [ Class "section" ]
        [ div [ Class "container py-r" ]
            playerControlCenter
            playerCreatures ] ]
      playerHand
      footerBand
```
The results of this process ae located in ([the documentation > html > ElmishElements.fs](documentation/html/ElmishElements.fs))


I then added a `PageLayoutElements.fs` to the src/client and added the file into the `Client.fsproj` to be included and compiled coming before the `Index.fs`.

I copied and pasted the contents from the ElmishElements into this file.

This broke the build as the file needed a module to be defined and the references imported so I added

```
module PageLayoutParts

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
```

to the top of the file.


This still did not build. There was an issue with a `broken-css` being added by the converter for the background-image element. As well as an issue with the way `strong` elements were converted.

The indentation was

```

                                [ strong [ ]
                                    [ str "On Enter Playing Field" ]
                                      str ": Effect description." ]
```

I needed to change it to

```

                                [ strong [ ]
                                    [ str "On Enter Playing Field" ]
                                  str ": Effect description." ]
```

i.e. the function definition is strong str not strong and two lists like many other elements.

Finally, I found that the `mainLayout` thing I wrote was invalid. I had to change it to:

```
let mainLayout =
  div [ Class "container is-fluid" ]
    [ topNavigation
      br [ ]
      br [ ]
      enemyStats
      enemyCreatures
      section [ Class "section" ]
        [ div [ Class "container py-r" ]
            [
            playerControlCenter
            playerCreatures
            ]
        ]
      playerHand
      footerBand
    ]
```

It compiled and I was then able to upload the view function in Index.fs to reference the mainLayout function.

i.e.

```
let view (model : Model) (dispatch : Msg -> unit) =
    PageLayoutParts.mainLayout
```

After viewing the page a few elements had to be rearranged and some changes had to be made. For example, changing the html unicode codes to just the unicode characters.

For the next step, we will try to make the output actually pull information from the GameState model and break down these general page parts into smaller and more usable parts.

This is the final commit in the `step-4-implement-layout-in-client` branch.

### Wire up Layout to the GameState and BReaking it in to Smaller Parts

The first thing I need to do is alter the init() in the `Index.fs` to return a tuple with a GamesState instead of a Model. I am then going to work through the errors until everything compiles.

In order to alter the init() I will have to come up with what the initial state should be. By the end of this  section I am going to manually add some information (i.e. not leave it in a new game state) so that something is happening on the screen to render.


First I set up my type like:

```
    let model =
        {
            Players =  Map.empty
            Boards = Map.empty
            CurrentTurn = None
            CurrentStep=  GameStep
            TurnNumber = 1
        }
```

I then also deleted the Model type and updated the `Msg` type to consist solely of a GameStarted type.

I then had to update the cmd of the init() like
``` let cmd = Cmd.ofMsg GameStarted ```
so that it returned a valid cmd and update the `update` function
to only match on the `GamesStarted` type.

i.e.
```
let update (msg: Msg) (model: GameState): GameState * Cmd<Msg> =
    match msg with
    | GameStarted ->
        model, Cmd.none
```


I was not able to save and it compiled


Next I will need to wire in the model part by part into the layout potentially updating the domain with missing items as I go.

The first thing I noticed is that I didn't have any sort of validation that my Id's were non empty so I added some by defining a `NonEmptyString` type, making the constructor private and adding a NonEmptyString namespace witha  build function that creates a NonEmptyString after validating the provided string is not null or whitespace.

```
type NonEmptyString = private NonEmptyString of string
module NonEmptyString =
    let build str =
        if String.IsNullOrWhiteSpace str then "Value cannot be empty" |> Error
        else str |> NonEmptyString |> Ok

```

I then moved the domain types into a `Domain` module and made the various Ids types of NonEmptyStrings.

i.e.
```
module Domain =

    type PlayerId = NonEmptyString
    type CardInstanceId = NonEmptyString
    type CardId = NonEmptyString
    type InPlayCreatureId = NonEmptyString

    type Player =
        { ...
```

I then added the arguements `model` and `dispatch` to pass through from the `view` to the `mainLayout`.

Inside the mainLayout the enemyStats and enemyCreatures will depend on the opponent `Player` and opponent `PlayerBoard`
and  `playerCreatures` and `playerHand` will depend on your `PlayerBoard` and the `playerControlCenter` will depend on the entire `GameState`.

First, we will need to pull out the opponent and player board and player but I have noticed that the GamesState does not define the current player vs the opponent so I am adding a property to the GameState to store this information.

i.e.
```
    and GameState =
        {
            CurrentPlayer: PlayerId
            OpponentPlayer: PlayerId
            ...
```

I then wrote functions to extract the needed information, wrapped them in results and a single function to extract all the data.

I then wrapped the main layout to verify no errors were encountered in building the models and changed the function to return an error message if any issues are encountered.

i.e.

```

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
          enemyStats
          enemyCreatures
          playerControlCenter
          playerCreatures
          playerHand
          footerBand
        ]
  | _ -> strong [] [ str "Error in GameState encountered." ]
```

Looking further at the `Player` definition I see that it is missing a playmat URL for their background graphic. I added that as well as definitions for two new primates domain types which should only hold valid URLs:

```

type UrlString = private UrlString of NonEmptyString
module UrlString =
    let build str =
        if Uri.IsWellFormedUriString(str, UriKind.RelativeOrAbsolute) then "Value must be a url." |> Error
        else str |> NonEmptyString.build |> Result.map UrlString


type ImageUrlString = private ImageUrlString of UrlString
module ImageUrlString =

    let isUrlImage (str : string) =
        let ext  = Path.GetExtension(str)
        if String.IsNullOrWhiteSpace ext then false
        else
            match str with
            | "png" | "jpg" ->
                true
            | _ -> false

    let build str =
        if isUrlImage str then "Value must be a url pointing to iamge." |> Error
        else str |> UrlString.build |> Result.map ImageUrlString

```

and

```

    type Player =
        {
            PlayerId: PlayerId
            Name: string
            PlaymatUrl: ImageUrlString
            RemainingLifePoints: int
        }

```

In the Init I now have to add PlayerIds for the opponent and and current player. To do this I created a createPlayer function which takes a playerIdStr playerName playerCurrentLife playerPlaymatUrl and returns a `Player`.

```
let createPlayer playerIdStr playerName playerCurrentLife playerPlaymatUrl =
    let playerId = NonEmptyString.build playerIdStr |> Result.map PlayerId
    let playerPlaymatUrl = ImageUrlString.build playerPlaymatUrl

    match playerId, playerPlaymatUrl with
    | Ok s, Ok pm ->
        Ok {
           PlayerId = s
           Name = playerName
           RemainingLifePoints = playerCurrentLife
           PlaymatUrl = pm
        }
    | _ -> Error "Unable to create player"
```

I then modified the init to create two players and plug them into the GameState. In doing this modification I had to switch the init to return a Result type.

i.e.

```
let init =
    let player1 = createPlayer "Player1" "Player1" 10 "https://picsum.photos/id/1000/2500/1667?blur=5"
    let player2 = createPlayer "Player2" "Player2" 10 "https://picsum.photos/id/10/2500/1667?blur=5'"
    match player1, player2 with
    | Ok p1, Ok p2 ->
        let model =
            {
                Players =  Map.empty
                Boards = Map.empty
                CurrentTurn = None
                CurrentStep=  NotCurrentlyPlaying
                TurnNumber = 1
                CurrentPlayer = p1.PlayerId
                OpponentPlayer = p2.PlayerId
            }
        let cmd = Cmd.ofMsg GameStarted
        Ok (model, cmd)
    | _ -> "Failed to create players" |> Error
```


I then had to modify the App.fs in the client to accept a result set back from the init like

```

match Index.init with
| Ok (gamesState, dispatch) ->
    let placeHolderFunc () =
        gamesState, dispatch
    Program.mkProgram placeHolderFunc Index.update Index.view
    ...
```

At this point, I ran into an error where the methods I was using from Path and Uri to validate the URLs are not implemented in FABLE. While FABLE implements a large part of the .NET framework some parts are not implemented. for now, I basically made the functions just to validate that the strings not null.


It now builds and displays the `Error in GameState encountered.` message from the view definition (since the players and boards are not registered in their respective maps).

Now I will have to update the init to populate these values.

I updated the Players value to
```

                Players =  [
                            p1.PlayerId, p1;
                            p2.PlayerId, p2
                           ] |> Map.ofList
```

Additionally, I will need to create a function to generate a player board.

```
let playerBoard player =
    {
            PlayerId=  player.PlayerId
            Deck= {
                TopCardsExposed = 0
                Cards =  List.empty
            }
            Hand= { Cards = List.empty }
            ActiveCreature= None
            Bench=  None
            DiscardPile= {
                TopCardsExposed = 0
                Cards = List.empty
            }
            TotalResourcePool= ResourcePool Seq.empty
            AvailableResourcePool =  ResourcePool Seq.empty
    }
```

This was not working for me. I spent a long time trying to figure out why. The end answer was that my ImageUrlString build method needed a `not` and was actually verifying that the image URL was invalid.

Now it is once again loading the page and I am able to start padding these values to my layout parts.

First I will pass the opponent `Player` and `PlayerBoard` to the `enemyStats` and plug those in.


Similarly, I will do the same for the player control center.


Here I can notice that the health, hand, deck and discard items are shared between the two nav bars and extract that as a shared function.

I, therefore, pulled out a playerStats function:

```
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

```

I can also pull out the step information into a `currentStepInformation` function. I can then utilize it if it is the player's turn and the GameState.CurrentStep to select the classes for the steps like:

```
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
```

All this builds so I am not able to try to populate the Player Hand from the PlayerBoard.Hand. This can be rendered by rendering each card from the list in a yield.

I can do this like
```
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

```

Next to see some data I will have to populate some hand data into the init object.

To do this I am going to create a `testCardSeqGenerator` function. This will take a number and return that number of randomly generated cards.

While doing this I realized the all the `GameStateSpecialEffect` in the cards and attacks should be Optional and updated the domain like
```
    and CharacterCard
        = { CardId: CardId;
            Name: string;
            Creature: Creature;
            ResourceCost: ResourcePool;
            PrimaryResource: Resource;
            EnterSpecialEffects: Option<GameStateSpecialEffect>;
            ExitSpecialEffects: Option<GameStateSpecialEffect>
          }
    and EffectCard
        = {
            CardId: CardId;
            Name: string;
            ResourceCost: ResourcePool;
            PrimaryResource: Resource;
            EnterSpecialEffects: Option<GameStateSpecialEffect>;
            ExitSpecialEffects: Option<GameStateSpecialEffect>;
        }
    and ResourceCard
        = {
            CardId: CardId;
            Name: string;
            ResourceCost: ResourcePool;
            PrimaryResource: Resource;
            EnterSpecialEffects: Option<GameStateSpecialEffect>;
            ExitSpecialEffects: Option<GameStateSpecialEffect>;
            ResourceAvailableOnFirstTurn: bool;
            ResourcesAdded: ResourcePool
        }
    ...
    and Attack =
        {
            Damage: int
            Cost: ResourcePool
            SpecialEffect: Option<GameStateSpecialEffect>
        }

```

After playing around for a while I was eventually able to create functions to generate test data:

```

let testCardGenerator cardInstanceIdStr cardIdStr =

    let cardInstanceId = NonEmptyString.build cardInstanceIdStr |> Result.map CardInstanceId

    let cardId = NonEmptyString.build cardInstanceIdStr |> Result.map CardId

    match cardInstanceId, cardId with
    | Ok id, Ok cid ->
        let creature =
          {
            Health= 95
            Weaknesses=  List.empty
            Attach = List.empty
          }
        let card =
            {
                CardId = cid
                ResourceCost = [ Resource.Grass, 4;
                                 Resource.Colorless, 1 ] |> Seq.ofList |> ResourcePool
                Name = cardIdStr
                EnterSpecialEffects = None
                ExitSpecialEffects = None
                PrimaryResource = Resource.Grass
                Creature = creature
            }
        Ok  {
                CardIntanceId  =  id
                Card =  card |> CharacterCard
            }
    | _, _ ->
        sprintf "Unable to create card instance for %s\t%s" cardInstanceIdStr cardIdStr
        |> Error

let testCardSeqGenerator (numberOfCards : int) =
    seq { 0 .. (numberOfCards - 1) }
    |> Seq.map (sprintf "Exciting Character #%i")
    |> Seq.map (fun x -> testCardGenerator x x)
    |> Seq.map (fun x ->
                        match x with
                        | Ok s -> [ s ] |> Ok
                        | Error e -> e |> Error)
    |> Seq.fold (fun x y ->
                    match x, y with
                    | Ok accum, Ok curr -> curr @ accum |> Ok
                    | _,_ -> "Eating Errors lol" |> Error
                ) (Ok List.empty)
```

One of the major issues I encountered was transforming the Sequence of results into a Result of a sequence. I ended up having to map the Seq<Result<Card, string>> to a Seq<Result<Card list, string>> and fold across that.

I was then able to update the `playerBoard` function to

```

let playerBoard (player : Player) =
    let deckTemp =  testCardSeqGenerator 35
    let handTemp = testCardSeqGenerator 3

    match deckTemp, handTemp with
    | Ok deck, Ok hand ->
        Ok  {
                PlayerId=  player.PlayerId
                Deck= {
                    TopCardsExposed = 0
                    Cards =  deck
                }
                Hand=
                    {
                        Cards = hand
                    }
                ActiveCreature= None
                Bench=  None
                DiscardPile= {
                    TopCardsExposed = 0
                    Cards = List.empty
                }
                TotalResourcePool= ResourcePool Seq.empty
                AvailableResourcePool =  ResourcePool Seq.empty
            }
    | _,_ -> "Error creating deck or hand" |> Error
```

I feel like there has to be a much better way to deal with these Result types but I am going to keep driving on and revisit later.

In building out this generator I realized I needed to add an image URL for the cards so I added an `ImageUrl` to the types of type `ImageUrlString` and added this to the builder.

Now I need to update the `renderCardForHand`. First I switch the card type and push the character cards into a `renderCharacterCard`.

Apparently, a description is also needed on the card types.

Also, a description is needed on the `GameStateSpecialEffect`.

Also, a `ToString` override is needed for the `ImageUrlString` type. I set this by overriding all the ToString methods on the `NonEmptyString`, `UrlString`, and `ImageUrlString`.

i.e.

```
type NonEmptyString = private NonEmptyString of string
    with override this.ToString() = match this with NonEmptyString s -> s
...

type UrlString = private UrlString of NonEmptyString
    with override this.ToString() = match this with UrlString s -> s.ToString()
...

type ImageUrlString = private ImageUrlString of UrlString
    with override this.ToString() = match this with ImageUrlString s -> s.ToString()

```

This has proven to be extremely time-consuming and I am calling it quits for the day.

This is the last commit in the branch `step-5-wire-up-layout-to-gamestate`



### Wiring up the Layout to the GameState Part 2
---

Now I am going to continue to wire up the layout to the GameState. Previously, I was able to generate some cards and decks. Doing that I realize I can pull out a function to map Resources to a symbol.

In order to facilitate this mapping I created a function:

```
let getSymbolForResource resource =
    match resource with
    | Grass -> "🍂"
    | Fire -> "🔥"
    | Water -> "💧"
    | Lightning -> "⚡"
    | Psychic -> "🧠"
    | Fighting -> "👊"
    | Colorless -> "□"
```

Using this I can create a function mapping a `ResourcePool' to a string like:

```
let textDescriptionForResourcePool (resourcePool : ResourcePool) =
    resourcePool
    |> Seq.map (fun x -> sprintf "%s x%i" (getSymbolForResource x.Key) x.Value)
    |> String.concat ";"
```

Using this function I can break out a renderAttackRow function. To do this I had to add a Name property to the `Attack` type. After adding the `Name` I can create a method like:

```

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
```

and reference it like

```
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
                                    for a in card.Creature.Attach do
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
```

Now I have to plug in the information into the player and enemy creature hopefully reusing much of the previously created functions.

First, I can plug in the playmat backgrounds.

Then, I see I can use a mapping function for the `SpecialCondition`s to status symbols similar to what was done for the Resource symbols as well a function which takes an optional list of these conditions and appends them together. *These mappings do rely on emojis, this is not a great idea but I am sticking with it for now*.

Like:
```
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
```

I am starting to have a number of UI helper functions so I am going to move them into another file/module `GeneralUIHelpers`. I will have to register this file in Client.fsproj making sure it is compiled before the PageLayoutElement.fs.


I can now pull out a `renderEnemyActiveCreature` function which takes an optional `InPlayCreature` as well as a `renderEnemyBench` which takes an optional list of `InPlayCreature`s.

I wrote these and modified the enemyCreatures function to be like:

```

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
                                            for a in card.Creature.Attach do
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
          Style [ Background (sprintf "url(%s')" (player.PlaymatUrl.ToString()))
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
```

This now builds (and looks very weird with no content).

I now need to populate some test data for the enemy creatures.

Similar to the cards I am able to generate the data like:

```

let inPlayCreatureGenerator inPlayCreatureIdStr cardInstanceIdStr cardIdStr cardImageUrlStr =
        let inPlayCreatureId = NonEmptyString.build inPlayCreatureIdStr |> Result.map InPlayCreatureId
        let card = testCardGenerator cardInstanceIdStr cardIdStr cardImageUrlStr

        match inPlayCreatureId, card with
        | Ok id, Ok c ->
            Ok {
                InPlayCharacterId=  id
                Card = c.Card
                CurrentDamage=  0
                SpecialEffect=  None
                AttachedEnergy = [ Resource.Grass, 4;
                                     Resource.Colorless, 1 ] |> Seq.ofList |> ResourcePool
                SpentEnergy = [ Resource.Grass, 4;
                                     Resource.Colorless, 1 ] |> Seq.ofList |> ResourcePool
            }
        | _, _ -> "Unable to create in play creature." |> Error


let inPlayCreatureSeqGenerator (numberOfCards : int) =
    seq { 0 .. (numberOfCards - 1) }
    |> Seq.map (fun x -> inPlayCreatureGenerator
                            (sprintf "ExcitingCharacter%i" x)
                            (sprintf "ExcitingCharacter%i" x)
                            (sprintf "Exciting Character #%i" x)
                            (sprintf "https://picsum.photos/320/200?%i" x))
    |> Seq.map (fun x ->
                        match x with
                        | Ok s -> [ s ] |> Ok
                        | Error e -> e |> Error)
    |> Seq.fold (fun x y ->
                    match x, y with
                    | Ok accum, Ok curr -> curr @ accum |> Ok
                    | _,_ -> "Eating Errors lol" |> Error
                ) (Ok List.empty)
```

Next, I need to modify the player creatures to also reference the state.

To do this I created three functions:

```

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
                                            for a in card.Creature.Attach do
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
                                            for a in card.Creature.Attach do
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
```

This compiles and displays now. Here note that the playerBench takes an argument for the number of columns to display. I wasn't sure if it should be 3 or 4 cards per column so I made it a variable.

The playerCreatures can then be changed to

```

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
```

Additionally, at this stage, I noticed that the playmat background on the creatures was missing a `'` around the URL so I added that.

Now our layout is largely wired up to the GameState. Next, we will need to define some events which modify the GameState and attach those events to the UI.

This is the final commit in the branch `step-6-wire-up-layout-to-gamestate-part-2`

### Defining Events To Change the GameState
---

As players play the game they can perform a number of actions that trigger events that modify the GameState.

*Note from a technical standpoint:* The GameState is actually immutable and never changes but rather a new GameState is generated from an acton and the previous state. This new state replaces the past state.


Initially, I am going to start just writing out some potential events which could happen during the game and the associated event data they would carry.

* StartGame - Initializes a new game (Creates a new GameState)
  * A unique GameID
  * Pair of Players with associated decks
  * PlayerID of Player to start game
* DrawCard - Moves the top card from the deck to the hand on the playboard referenced by the PlayerId
  * GameId
  * PlayerId
* DiscardCard - Move card with CardInstanceId from the playerboard's hand to discard pile
  * GameId
  * PlayerId
  * CardInstanceID
* PlayCard -
    If resources are available on the player's board:
        remove the card from the player's hand
        - if a resource card add to the total resource pool
        - if an effect card trigger the effect
        - if a character card creates an inplay creature and trigger the enter event. If no active create exists place the in-play creature in the active position otherwise place it on the bench.
    If the resources are not available add an error message to the gamestate
  * GameId
  * PlayerId
  * CardInstanceID
* EndPlayStep - Move the gamestate to the attack state
  * GameId
  * PlayerId
* PerformAttack -
    If the resources are available on the player's board:
        if the opponent has an inplay creature deal the damage from the attack to that creature. If the creature has <= 0 heath the creature dies
        if the opponent has no inplay creature deal the damage to the player
    if the resources are not available on the player's board display a message
  * GameId
  * PlayerId
  * InPlayCreatureId
  * Attack
* SkipAttack - Move the gamestate to reconcile
  * GameId
  * PlayerId
* EndTurn - Mode the gamestate to the other player's draw state
  * GameId
  * PlayerId
* GameWon - Move the gamestate to the game over state
  * PlayerId of Winner
  * Winning Reason (string)

There will have to be additional events for things like tapping out/retreating active creatures but I think those would best be added after everything else is set up/works.

Going through this process I have identified a few initial things that need to be added.

    * need a list of notifications on the GameState
    * need a GameStep for GameOver
    * need an optional winner on the GameOver Step
    * need a game id on the GameState

I am not thinking that the CurrentPlayerTurn should actually be on the Game Step so I am refactoring that type.

I modified the domain models with:

```
    and GameOverStep =
        {
            WinnerId: Option<PlayerId>
            Message: string
        }
    and GameStep =
        NotCurrentlyPlaying
        | Draw of PlayerId
        | Play of PlayerId
        | Attack of PlayerId
        | Reconcile of PlayerId
        | GameOver of GameOverStep
    and Notification = string
    and GameState =
        {
            GameId: GameId
            NotificationMessages: Option<Notification list>
            CurrentPlayer: PlayerId
            OpponentPlayer: PlayerId
            Players: Map<PlayerId, Player>
            Boards: Map<PlayerId, PlayerBoard>
            CurrentStep:GameStep
            TurnNumber: int
        }
```

I then had to update the init function for the new fields like
```

let init =
    let player1 = createPlayer "Player1" "Player1" 10 "https://picsum.photos/id/1000/2500/1667?blur=5"
    let player2 = createPlayer "Player2" "Player2" 10 "https://picsum.photos/id/10/2500/1667?blur=5"
    let gameId =  NonEmptyString.build "GameIDHere" |> Result.map GameId

    match player1, player2, gameId with
    | Ok p1, Ok p2, Ok g ->
        let playerBoard1 = playerBoard p1
        let playerBoard2 = playerBoard p2
        match playerBoard1, playerBoard2 with
        | Ok pb1, Ok pb2 ->
          let model =
            {
                GameId = g
                Players =  [
                            p1.PlayerId, p1;
                            p2.PlayerId, p2
                           ] |> Map.ofList
                Boards =   [
                            pb1.PlayerId, pb1;
                            pb2.PlayerId, pb2
                           ] |> Map.ofList
                NotificationMessages = None
                CurrentStep=  p1.PlayerId |> Attack
                TurnNumber = 1
                CurrentPlayer = p1.PlayerId
                OpponentPlayer = p2.PlayerId
            }
          let cmd = Cmd.ofMsg GameStarted
          Ok (model, cmd)
        | _ -> "Failed to create player boards" |> Error
    | _ -> "Failed to create players" |> Error
```

I similarly had to update the currentStepInformation to utilize the updated GameStep type. Using the fact that whose turn it is now embedded in the state I was able to simplify the yourCurrentStepClasses function like:

```
let yourCurrentStepClasses (gameState : GameState) (gamesStep: GameStep) =
        if gameState.CurrentStep = gamesStep then "button is-danger"
        else "button is-primary"

let currentStepInformation (player: Player) (gameState : GameState) =
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
```

At this point everything builds and I am checking it in with a commit message of `Step 7 Updates to GameState`.

In the Client/Index.fs I can now modify the Msg type to be a discriminated union of all the above-listed types. First, I am moving the Msg type into its own module/file Events/Events.fs.

Based on the above description I was able to define the events as follows:

```
type StartGameEvent =
    {
        GameId: GameId
        Players: Map<Player, Deck>
        StartingPlayer:PlayerId
    }
type DrawCardEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
    }
type DiscardCardEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
        CardInstanceId: CardInstanceId
    }
type PlayCardEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
        CardInstanceId: CardInstanceId
    }
type EndPlayStepEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
    }
type PerformAttackEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
        InPlayCreatureId: InPlayCreatureId
        Attack: Attack
    }
type SkipAttackEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
    }
type EndTurnEvent =
    {
        GameId: GameId
        PlayerId: PlayerId
    }
type GameWonEvent =
    {
        GameId: GameId
        Winner: Option<PlayerId>
        Message: Option<Notification list>
    }

type Msg =
    | GameStarted
    | StartGame of StartGameEvent
    | DrawCard of DrawCardEvent
    | DiscardCard of DiscardCardEvent
    | PlayCard of PlayCardEvent
    | EndPlayStep of EndPlayStepEvent
    | PerformAttack of PerformAttackEvent
    | SkipAttack of SkipAttackEvent
    | EndTurn of EndTurnEvent
    | GameWon of GameWonEvent

```

I am not receiving compiler errors for incomplete match statements but it builds. At this point, I am committing these changes with the message `Update Msg type to include a variety of events`.


I am keeping this as the final commit in the branch `step-7-creating-events-to-update-state`

In the next step/section I will be wiring up the events to actually modify the game state. Finally, we will be removing the `GameStarted` event and replace the initializing function with a hydrated `StartGame` event.

### Wire up events to modify GameState
---

These updates to the gamestate will occur in the update function of the client Index.fs


First I scaffolded out the update function like:

```
let update (msg: Msg) (model: GameState): GameState * Cmd<Msg> =
    match msg with
    | GameStarted ->
        model, Cmd.none
    | StartGame ev ->
        model, Cmd.none
    | DrawCard  ev ->
        model, Cmd.none
    | DiscardCard ev ->
        model, Cmd.none
    | PlayCard ev ->
        model, Cmd.none
    | EndPlayStep ev ->
        model, Cmd.none
    | PerformAttack  ev ->
        model, Cmd.none
    | SkipAttack ev ->
        model, Cmd.none
    | EndTurn ev ->
        model, Cmd.none
    | GameWon ev ->
        model, Cmd.none
```

First I will implement to `StartGame` handler.


In order to do this I first created a `takeDeckDealFirstHandAndReturnNewPlayerBoard` function:

```
let takeDeckDealFirstHandAndReturnNewPlayerBoard (intitalHandSize: int) (playerId : PlayerId) (deck : Deck) =
    let emptyHand =
      {
        Cards = list.Empty
      }
    let deckAfterDraw, hand = drawCardsFromDeck intitalHandSize deck emptyHand

    {
        PlayerId=  playerId
        Deck= deckAfterDraw
        Hand=hand
        ActiveCreature= None
        Bench=  None
        DiscardPile= {
            TopCardsExposed = 0
            Cards = List.empty
        }
        TotalResourcePool= ResourcePool Seq.empty
        AvailableResourcePool =  ResourcePool Seq.empty
    }
```

This references a new function to draw cards:
```
let drawCardsFromDeck (cardsToDraw: int) (deck : Deck) (hand: Hand) =
    if deck.Cards.IsEmpty then
        deck, hand
    else
        let cardsToTake = List.truncate cardsToDraw deck.Cards
        { deck with Cards = List.skip cardsToTake.Length deck.Cards}, {hand with Cards = hand.Cards @ cardsToTake}

```

Then I was able to intialize the state as:
```
let intitalizeGameStateFromStartGameEvent (ev : StartGameEvent) =
            {
                GameId= ev.GameId
                NotificationMessages= None
                CurrentPlayer= ev.CurrentPlayer
                OpponentPlayer= ev.OpponentPlayer
                Players= ev.Players
                Boards= ev.Decks
                        |> Seq.map (fun x -> x.Key, takeDeckDealFirstHandAndReturnNewPlayerBoard 7 x.Key x.Value )
                        |> Map.ofSeq
                CurrentStep= ev.CurrentPlayer |> Draw
                TurnNumber= 1
            }

```
Then I just have to call intitalizeGameStateFromStartGameEvent from my update match statement.

Using the same draw function I can implement the draw event.

```
let appendNotificationMessageToListOrCreateList (existingNotifications : Option<Notification list) (newNotification : string) =
    match existingNotifications with
    | Some nl ->
        newNotification
        |> Notification
        |> (fun y -> y :: nl)
        |> Some
    | None ->
        [ (newNotification |> Notification) ]
        |> Some

let modifyGameStateFromDrawCardEvent (ev: DrawCardEvent) (gs: GameState) =
    match gs.Boards.TryGetValue ev.PlayerId with
    | true, pb ->
        let newDeck, newHand =  drawCardsFromDeck 1 pb.Deck pb.Hand
        { gs with Boards = (gs.Boards.Add (ev.PlayerId, { pb with Deck = newDeck; Hand = newHand })  ) }
    | false, _ ->
        { gs with NotificationMessages = appendNotificationMessageToListOrCreateList gs.NotificationMessages "Unable to lookup player board" }
```

I realize there has to be a better way to pipe into a list than `|> (fun y -> y :: nl)` but I am just going to leave that for now.

Discard card is next. I noticed a typo in the name `CardInstanceId` so I corrected that. Also, I notice I will similarly have to pull the player board from the state so I should extract that to be a function like:

```
let getExistingPlayerBoardFromGameState playerId gs =
 match gs.Boards.TryGetValue playerId with
    | true, pb ->
        pb |> Ok
    | false, _ ->
        (sprintf "Unable to locate player board for player id %s" (playerId.ToString())) |> Error
```

I am then able to implement a discardCardFromBoard function like:

```
let discardCardFromBoard (cardInstanceId : CardInstanceId) (playerBoard : PlayerBoard) =
    let cardToDiscard : CardInstance list = List.filter (fun x -> x.CardInstanceId = cardInstanceId) playerBoard.Hand.Cards

    match cardToDiscard with
    | [] ->
        (sprintf "Unable to locate card in hand with card instance id %s" (cardInstanceId.ToString())) |> Error
    | [ x ] ->
            {
                playerBoard
                    with Hand =
                          { playerBoard.Hand with

                                Cards = (List.filter (fun x -> x.CardInstanceId = cardInstanceId) playerBoard.Hand.Cards)

                          };
                         DiscardPile ={playerBoard.DiscardPile with Cards = playerBoard.DiscardPile.Cards @ [ x ] }
            } |> Ok
    | _ ->
        (sprintf "ERROR: located multiple cards in hand with card instance id %s. This shouldn't happen" (cardInstanceId.ToString())) |> Error

let modifyGameStateFromDiscardCardEvent (ev: DiscardCardEvent) (gs: GameState) =
    let newBoard =
        getExistingPlayerBoardFromGameState ev.PlayerId gs
        |> Result.bind (discardCardFromBoard ev.CardInstanceId)

    match newBoard with
    | Ok pb ->
        { gs with Boards = (gs.Boards.Add (ev.PlayerId, pb)  ) }
    | Error e ->
        { gs with NotificationMessages = appendNotificationMessageToListOrCreateList gs.NotificationMessages e }

```

The end play step should just move the player to the Attack step like:

```
    | EndPlayStep ev ->
        { model with CurrentStep = (Attack ev.PlayerId)}, Cmd.none
```

Similarly, SkipAttack should just move the player to the Reconcile step.

```
    | SkipAttack ev ->
        { model with CurrentStep = (Reconcile ev.PlayerId)}, Cmd.none
```

EndTurn should just move the game to the draw step of the other player.

```
    | EndTurn ev ->
        let otherPlayer = getTheOtherPlayer model ev.PlayerId
        { model with CurrentStep = (Draw otherPlayer)}, Cmd.none
```

Game Won should transition to the GameOverState, set a winner and a message but I will first have to define a function to format the GameOverMessage like:

```
let formatGameOverMessage (notifications : Option<Notification list>) =
    match notifications with
    | None ->
        "Game Over for unknown reason"
    | Some [] ->
        "Game Over for unknown reason"
    | Some x ->
        x
        |> Seq.map (fun x -> x.ToString())
        |> String.concat ";"
```

then I can

```
    | GameWon ev ->
        let newStep =  { WinnerId =  ev.Winner; Message = formatGameOverMessage ev.Message } |> GameOver
        { model with CurrentStep = newStep}, Cmd.none
```

This involved some copypasta which could be removed and refactored but I am just going to keep driving on for now.

This now just leaves me to complete the play card and attack events.

For the play card action, I will need to implement a variety of functions. These will require refactoring but just to get something down I added

```
let applyEffectIfDefinied effect gs =
    match effect with
    | Some e -> e.Function.Invoke gs |> Ok
    | None  -> gs |> Ok

let currentResourceAmountFromPool (resourcePool : ResourcePool) (resource : Resource) =
    match resourcePool.TryGetValue(resource) with
    | true, t -> t
    | false, _ -> 0

let rec addResourcesToPool (rp1 : Map<Resource, int>)  rp2 =
    match rp2 with
    | [] ->  rp1 |> Map.toSeq |> ResourcePool
    | [ (x,y) ] ->
        rp1.Add(x, y + (currentResourceAmountFromPool rp1 x))  |> Map.toSeq |> ResourcePool
    | (x,y) :: xs  ->
        addResourcesToPool (rp1.Add(x,  y + (currentResourceAmountFromPool rp1 x))) xs


let createInPlayCreatureFromCardInstance characterCard inPlayCreatureId =
            {
                InPlayCharacterId=  inPlayCreatureId
                Card = characterCard
                CurrentDamage=  0
                SpecialEffect=  None
                AttachedEnergy =  Seq.empty |> ResourcePool
                SpentEnergy = Seq.empty |> ResourcePool
            } |> Ok

let appendCreatureToPlayerBoard inPlayCreature playerBoard =
        match playerBoard.ActiveCreature with
        | None ->
            { playerBoard with ActiveCreature = Some inPlayCreature }
        | Some a ->
            { playerBoard
                with Bench
                    = Some (Option.fold (fun x y -> x @ y)  [ inPlayCreature ]  playerBoard.Bench)  }


let addCreatureToGameState cardInstanceId x playerId gs playerBoard inPlayCreature=
        let playerBoard =
                        {
                            playerBoard
                                with Hand =
                                        {
                                          playerBoard.Hand with
                                            Cards = (List.filter (fun x -> x.CardInstanceId = cardInstanceId) playerBoard.Hand.Cards)
                                        };
                                     DiscardPile ={playerBoard.DiscardPile with Cards = playerBoard.DiscardPile.Cards @ [ x ] }
                        } |> (appendCreatureToPlayerBoard inPlayCreature)

        { gs with Boards = gs.Boards.Add ( playerId, playerBoard) } |> Ok

let buildInPlayCreatureId idStr =
    let res = idStr |> NonEmptyString.build

    match res with
    | Ok r -> InPlayCreatureId r |> Ok
    | Error e -> Error e


let playCardFromBoard (cardInstanceId : CardInstanceId) (playerId : PlayerId) (gs: GameState) (playerBoard : PlayerBoard) =
    let cardToDiscard : CardInstance list = List.filter (fun x -> x.CardInstanceId = cardInstanceId) playerBoard.Hand.Cards

    match cardToDiscard with
    | [] ->
        (sprintf "Unable to locate card in hand with card instance id %s" (cardInstanceId.ToString())) |> Error
    | [ x ] ->
            match x.Card with
            | CharacterCard cc ->

              System.Guid.NewGuid().ToString()
              |> buildInPlayCreatureId
              |> (Result.bind (createInPlayCreatureFromCardInstance x.Card))
              |> (Result.bind (addCreatureToGameState cardInstanceId x playerId gs playerBoard))
              |> (Result.bind  (applyEffectIfDefinied cc.EnterSpecialEffects))

            | ResourceCard rc ->
              let newPb = {
                  playerBoard
                    with Hand =
                          { playerBoard.Hand with

                                Cards = (List.filter (fun x -> x.CardInstanceId = cardInstanceId) playerBoard.Hand.Cards)
                          };
                         TotalResourcePool = addResourcesToPool playerBoard.TotalResourcePool (Map.toList rc.ResourcesAdded)
                         DiscardPile ={playerBoard.DiscardPile with Cards = playerBoard.DiscardPile.Cards @ [ x ] }
                }
              { gs with Boards = (gs.Boards.Add (playerId, newPb)) } |> (applyEffectIfDefinied rc.EnterSpecialEffects) |> Result.bind (applyEffectIfDefinied rc.ExitSpecialEffects)
            | EffectCard ec ->
              let newPb =  {
                  playerBoard
                    with Hand =
                          { playerBoard.Hand with

                                Cards = (List.filter (fun x -> x.CardInstanceId = cardInstanceId) playerBoard.Hand.Cards)
                          };
                         DiscardPile = {playerBoard.DiscardPile with Cards = playerBoard.DiscardPile.Cards @ [ x ] }
                }
              { gs with Boards = (gs.Boards.Add (playerId, newPb) ) } |> (applyEffectIfDefinied ec.EnterSpecialEffects) |> Result.bind (applyEffectIfDefinied ec.ExitSpecialEffects)
    | _ ->
        (sprintf "ERROR: located multiple cards in hand with card instance id %s. This shouldn't happen" (cardInstanceId.ToString())) |> Error


let modifyGameStateFromPlayCardEvent (ev: PlayCardEvent) (gs: GameState) =
    let newBoard =
        getExistingPlayerBoardFromGameState ev.PlayerId gs
        |> Result.bind (playCardFromBoard  ev.CardInstanceId ev.PlayerId gs)

    match newBoard with
    | Ok pb ->
        pb
    | Error e ->
        { gs with NotificationMessages = appendNotificationMessageToListOrCreateList gs.NotificationMessages e }

```

I am leaving the attack logic for later and am now focuses on testing to verify what I have done so far is working.

I am now interested in building a more realistic gamestate for testing.

This is the final commit in the branch `step-8-wire-up-events-to-update-game-step`

## Adding more realistic cards data and wiring up the Ui to send some events

In order to create better test data I am going to be creating a temporary SampleCardDatabase in the Client. This SampleCardDatabase will also require several generic list methods I am putting in a CollectionManipulation module also in the client.

I am going to create a creatureCardDb which creates a list of sample creature cards and a resourceCardDb which contains a list of sample resource cards. I am ignoring the effect cards for now.

Both of these "Db"s are generated by piping lists of tuples through a constructor function and taking all the valid "Ok" results.

I implemented them like:

```
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
    | _, _ -> Error "No"

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
    ] |> List.map (fun (x,y) -> resourceCardConstructor  x y)
    |> selectAllOkayResults

let creatureCardDb =
    [
      ("001", "BulbMon", "It has a bulb on it bro",Resource.Grass, [ Resource.Grass, 1; ] |> Seq.ofList |> ResourcePool, 40, [Resource.Fire] )
      ("050", "DigMon", "Mole Monster", Resource.Fighting,[ Resource.Fighting, 1; ] |> Seq.ofList |> ResourcePool, 30, [ Resource.Lightning ] )
      ("086", "SeelMon", "See Lion Monster", Resource.Water, [ Resource.Water, 1; ] |> Seq.ofList |> ResourcePool, 60, [ Resource.Lightning ] )
    ]
   |> List.map (fun (x,y,z,q,q2,q3, q4) -> creatureCreatureConstructor  x y z q q2 q3 q4)
   |> selectAllOkayResults
```

with selectAllOkayResults defined in the CollectinManipulation as

```S
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

let shuffleG xs = xs |> Seq.sortBy (fun _ -> System.Guid.NewGuid())
```

I now need to utilize these functions when setting up teh game. I want to add a mixture of resource cards and creature cards. For now I can alternate decks between creature and resource cards.

First I renamed `testCardGenerator` to `testCeatureCardGenerator` and change the function to pull information  from the sample database:

```
let testCreatureCardGenerator cardInstanceIdStr =
    let cardInstanceId = NonEmptyString.build cardInstanceIdStr |> Result.map CardInstanceId

    match cardInstanceId with
    | Ok id ->
        let card = SampleCardDatabase.creatureCardDb |> CollectionManipulation.shuffleG |> Seq.head
        Ok  {
                CardInstanceId  =  id
                Card =  card |> CharacterCard
            }
    | _ ->
        sprintf "Unable to create card instance for %s" cardInstanceIdStr
        |> Error
```

I can then create a similar function `testResourceCardGenerator`:

```
let testResourceCardGenerator cardInstanceIdStr =
    let cardInstanceId = NonEmptyString.build cardInstanceIdStr |> Result.map CardInstanceId

    match cardInstanceId with
    | Ok id ->
        let card = SampleCardDatabase.resourceCardDb |> CollectionManipulation.shuffleG |> Seq.head
        Ok  {
                CardInstanceId  =  id
                Card =  card |> ResourceCard
            }
    | _ ->
        sprintf "Unable to create card instance for %s" cardInstanceIdStr
        |> Error
```

I can then combine these two functions into `testDeckSeqGenerator`:

```
let testDeckSeqGenerator (numberOfCards :int) =
    seq { 0 .. (numberOfCards - 1)}
    |> Seq.map (fun x ->
                    if x % 2 = 1 then
                        testCreatureCardGenerator (sprintf "cardInstance-%i" x)
                    else
                        testResourceCardGenerator (sprintf "cardInstance-%i" x)
                    )
    |> List.ofSeq
    |> CollectionManipulation.selectAllOkayResults
```

I can then rewrite the init to reference the new Dbs like:

```
let init =
    let player1 = createPlayer "Player1" "Player1" 10 "https://picsum.photos/id/1000/2500/1667?blur=5"
    let player2 = createPlayer "Player2" "Player2" 10 "https://picsum.photos/id/10/2500/1667?blur=5"
    let gameId =  NonEmptyString.build "GameIDHere" |> Result.map GameId

    match player1, player2, gameId with
    | Ok p1, Ok p2, Ok g ->
        let playerBoard1 = playerBoard p1
        let playerBoard2 = playerBoard p2
        match playerBoard1, playerBoard2 with
        | Ok pb1, Ok pb2 ->
          let model : GameState =
            {
                GameId = g
                Players =  Map.empty
                Boards =   Map.empty
                NotificationMessages = None
                CurrentStep =  NotCurrentlyPlaying
                TurnNumber = 0
                PlayerOne = p1.PlayerId
                PlayerTwo = p2.PlayerId
            }

          let startGameEvent =
            {
                GameId = g
                Players =  [
                            p1.PlayerId, p1;
                            p2.PlayerId, p2
                           ] |> Map.ofList
                PlayerOne = p1.PlayerId
                PlayerTwo = p2.PlayerId
                Decks = [   (p1.PlayerId, { TopCardsExposed = 0; Cards = testDeckSeqGenerator 60 });
                            (p2.PlayerId, { TopCardsExposed = 0; Cards = testDeckSeqGenerator 60 })]
                         |> Map.ofSeq
            } |> StartGame
          let cmd = Cmd.ofMsg startGameEvent
          Ok (model, cmd)
        | _ -> "Failed to create player boards" |> Error
    | _ -> "Failed to create players" |> Error
```

I am able to see that the application does in fact now compile and renders the correct more realistic hands.

I am now able to try to wire up some functionality.

The first action I will try to implement is the "discard card" functionality on the discard button.

To do this I will need to wire up the OnClick functionality on the button.

I will wire it up by having this OnClick be a lambda sending a discard Msg like:

```
let renderCardInstanceForHand  (dispatch : Msg -> unit)  gameId playerId (card: CardInstance) =
    div [ Class "column is-4" ]
          [ div [ Class "card" ]
             [   yield! (renderCardForHand card.Card)
                 yield   footer [ Class "card-footer" ]
                      [ a [ Href "#"
                            Class "card-footer-item" ]
                          [ str "Play" ]
                        a [ Href "#"
                            OnClick (fun _->
                                            ({
                                                GameId = gameId
                                                PlayerId = playerId
                                                CardInstanceId = card.CardInstanceId
                                            } : DiscardCardEvent) |> DiscardCard |>  dispatch)
                            Class "card-footer-item" ]
                          [ str "Discard" ] ] ] ]

```

To modify the renderCardInstance like this I had to alter the function to render the card frame and card footer as opposed to the renderCardForHand.

I did this by changing the renderCardForHand like:

```
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

let renderCardForHand (card: Card) : ReactElement list=
    match card with
    | CharacterCard c -> renderCharacterCard c
    | _ ->
         [
            strong [] [ str "IDK" ]
         ]
```

I am not able to click the discard button but it doesn't function as expected! It is discarding all the cards other than the desired card!

I now have to fix the behavior of the `discardCardFromBoard` function. I have found that the bug lies in the filter on the Hand being ` (List.filter (fun x -> x.CardInstanceId = cardInstanceId) playerBoard.Hand.Cards)` and not ` (List.filter (fun x -> x.CardInstanceId <> cardInstanceId) playerBoard.Hand.Cards)`. Scanning through the code I will also have to update the similar funcationality in `addCreatureToGameState` and `playCardFromBoard`. (the fact that I had to update this logic is a real sign I need to refactor but I am ignoring this for now).

I reload the webpage and discard appears to be correctly functioning.

Now I can similarly implement the play card functionality by altering the

Refreshing the browser, nothing happens when I click play.

The first thing I did was add a notification area to the page to see if there was an error message like:

```
let notificationArea (messages :Option<Notification list>) =
    match messages with
    | Some s ->
        div []
            [
               yield!  Seq.map (fun x -> div [Class "notification is-danger"] [ str (x.ToString()) ]) s
            ]
    | _ ->
        div [] []

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
          notificationArea model.NotificationMessages
          playerCreatures cp cpb
          playerHand model.GameId cp.PlayerId cpb.Hand dispatch
          footerBand
        ]
  | _ -> strong [] [ str "Error in GameState encountered." ]
```

After poking around for a while I realized this was never wired up at all. In the update function, I updated the
```
    | PlayCard ev ->
        model, Cmd.none
```
to
```
    | PlayCard ev ->
        modifyGameStateFromPlayCardEvent ev model, Cmd.none
```

Now it appears to be adding the creatures to the playfield. One thing that needs to be added to the UI is the total and available resources.

To do this I modified the `playerStats` function on the PageLayoutElements to include the resources like:

```
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
                        [ str (sprintf "Total Res: %s" (textDescriptionForResourcePool playerBoard.TotalResourcePool))] ]
                  div [ Class "navbar-item" ]
                    [ a [ Class "button is-primary"
                          Href "#" ]
                        [ str (sprintf "Avail Res: %s" (textDescriptionForResourcePool playerBoard.AvailableResourcePool))] ] ]
```

I can now see the resources and see that the total resources are modifying but not available so I will need to update the `playCardFromBoard` function on index like:

```
let playCardFromBoard (cardInstanceId : CardInstanceId) (playerId : PlayerId) (gs: GameState) (playerBoard : PlayerBoard) =
    let cardToDiscard : CardInstance list = List.filter (fun x -> x.CardInstanceId = cardInstanceId) playerBoard.Hand.Cards

    match cardToDiscard with
    | [] ->
        (sprintf "Unable to locate card in hand with card instance id %s" (cardInstanceId.ToString())) |> Error
    | [ x ] ->
            match x.Card with
            | CharacterCard cc ->

              System.Guid.NewGuid().ToString()
              |> buildInPlayCreatureId
              |> (Result.bind (createInPlayCreatureFromCardInstance x.Card))
              |> (Result.bind (addCreatureToGameState cardInstanceId x playerId gs playerBoard))
              |> (Result.bind  (applyEffectIfDefinied cc.EnterSpecialEffects))

            | ResourceCard rc ->
              let newPb = {
                  playerBoard
                    with Hand =
                          { playerBoard.Hand with

                                Cards = (List.filter (fun x -> x.CardInstanceId <> cardInstanceId) playerBoard.Hand.Cards)
                          };
                         TotalResourcePool = addResourcesToPool playerBoard.TotalResourcePool (Map.toList rc.ResourcesAdded)
                         AvailableResourcePool =
                            if rc.ResourceAvailableOnFirstTurn then
                                addResourcesToPool playerBoard.AvailableResourcePool (Map.toList rc.ResourcesAdded)
                            else playerBoard.AvailableResourcePool
                         DiscardPile ={playerBoard.DiscardPile with Cards = playerBoard.DiscardPile.Cards @ [ x ] }
                }
              { gs with Boards = (gs.Boards.Add (playerId, newPb)) } |> (applyEffectIfDefinied rc.EnterSpecialEffects) |> Result.bind (applyEffectIfDefinied rc.ExitSpecialEffects)
            | EffectCard ec ->
              let newPb =  {
                  playerBoard
                    with Hand =
                          { playerBoard.Hand with

                                Cards = (List.filter (fun x -> x.CardInstanceId <> cardInstanceId) playerBoard.Hand.Cards)
                          };
                         DiscardPile = {playerBoard.DiscardPile with Cards = playerBoard.DiscardPile.Cards @ [ x ] }
                }
              { gs with Boards = (gs.Boards.Add (playerId, newPb) ) } |> (applyEffectIfDefinied ec.EnterSpecialEffects) |> Result.bind (applyEffectIfDefinied ec.ExitSpecialEffects)
    | _ ->
        (sprintf "ERROR: located multiple cards in hand with card instance id %s. This shouldn't happen" (cardInstanceId.ToString())) |> Error
```

With both the total and available populating the layout completely breaks so I put these two values on top of each other. This looks bad but is functional for the moment:

```
...
                  div [ Class "navbar-item" ]
                    [ a [ Class "button is-primary"
                          Href "#" ]
                        [ str (sprintf "Tot %s" (textDescriptionForResourcePool playerBoard.TotalResourcePool))
                          br []
                          str (sprintf "Ava %s" (textDescriptionForResourcePool playerBoard.AvailableResourcePool))] ] ]
...
```
I now notice that I am able to play cards for which I don't have the resources, as well as my available resources, are not being decremented.

To implement this I had to create a number of functions and rewrite the play logic to take this into account like:

```

let getNeededResourcesForCard card =
    match card with
    | CharacterCard cc -> cc.ResourceCost
    | ResourceCard rc -> rc.ResourceCost
    | EffectCard ec -> ec.ResourceCost

let tryRemoveResourceFromPlayerBoard (playerBoard:PlayerBoard) x y =
    match playerBoard.AvailableResourcePool.TryGetValue(x) with
    | true, z when z >= y -> Ok {playerBoard with AvailableResourcePool = (addResourcesToPool playerBoard.AvailableResourcePool  [ (x,  y) ])  }
    | _, _ -> sprintf "Not enough %s" (getSymbolForResource x) |> Error

let rec decrementResourcesFromPlayerBoard playerBoard resourcePool =
    match resourcePool with
    | [] -> Ok playerBoard
    | [ (x, y) ] -> tryRemoveResourceFromPlayerBoard playerBoard x y
    | (x, y) :: xs ->
        match tryRemoveResourceFromPlayerBoard playerBoard x y with
        | Error e -> e |> Error
        | Ok pb -> decrementResourcesFromPlayerBoard pb xs


let decrementRequiredResourcesFromModel cardToDiscard (playerId : PlayerId) (gs: GameState) (playerBoard : PlayerBoard) =
     getNeededResourcesForCard cardToDiscard
     |> Map.toList
     |> decrementResourcesFromPlayerBoard playerBoard
     |> Result.bind (fun updatedPlayerBoard -> Ok {gs with Boards = gs.Boards.Add(playerId, updatedPlayerBoard) })


let playCardFromBoardImp cardInstanceId playerId playerBoard (x : CardInstance) cardToDiscard gs =
    match x.Card with
               | CharacterCard cc ->

                 System.Guid.NewGuid().ToString()
                 |> buildInPlayCreatureId
                 |> (Result.bind (createInPlayCreatureFromCardInstance x.Card))
                 |> (Result.bind (addCreatureToGameState cardInstanceId x playerId gs playerBoard))
                 |> (Result.bind  (applyEffectIfDefinied cc.EnterSpecialEffects))

               | ResourceCard rc ->
                 let newAvailResourcePool =
                               if rc.ResourceAvailableOnFirstTurn then
                                   addResourcesToPool playerBoard.AvailableResourcePool (Map.toList rc.ResourcesAdded)
                               else
                                   playerBoard.AvailableResourcePool
                 let newPb = {
                     playerBoard
                       with Hand =
                             {
                               playerBoard.Hand with
                                   Cards = (List.filter (fun x -> x.CardInstanceId <> cardInstanceId) playerBoard.Hand.Cards)
                             }
                            TotalResourcePool = addResourcesToPool playerBoard.TotalResourcePool (Map.toList rc.ResourcesAdded)
                            AvailableResourcePool = newAvailResourcePool
                            DiscardPile ={playerBoard.DiscardPile with Cards = playerBoard.DiscardPile.Cards @ [ x ] }
                   }
                 { gs with Boards = (gs.Boards.Add (playerId, newPb)) } |> (applyEffectIfDefinied rc.EnterSpecialEffects) |> Result.bind (applyEffectIfDefinied rc.ExitSpecialEffects)
               | EffectCard ec ->
                 let newPb =  {
                     playerBoard
                       with Hand =
                             { playerBoard.Hand with

                                   Cards = (List.filter (fun x -> x.CardInstanceId <> cardInstanceId) playerBoard.Hand.Cards)
                             };
                            DiscardPile = {playerBoard.DiscardPile with Cards = playerBoard.DiscardPile.Cards @ [ x ] }
                   }
                 { gs with Boards = (gs.Boards.Add (playerId, newPb) ) } |> (applyEffectIfDefinied ec.EnterSpecialEffects) |> Result.bind (applyEffectIfDefinied ec.ExitSpecialEffects)


let playCardFromBoard (cardInstanceId : CardInstanceId) (playerId : PlayerId) (gs: GameState) (playerBoard : PlayerBoard) =
    let cardToDiscard : CardInstance list = List.filter (fun x -> x.CardInstanceId = cardInstanceId) playerBoard.Hand.Cards

    match cardToDiscard with
    | [] ->
        (sprintf "Unable to locate card in hand with card instance id %s" (cardInstanceId.ToString())) |> Error
    | [ x ] ->
        decrementRequiredResourcesFromModel x.Card playerId gs playerBoard
        |> Result.bind (playCardFromBoardImp cardInstanceId playerId playerBoard x cardToDiscard)
    | _ ->
        (sprintf "ERROR: located multiple cards in hand with card instance id %s. This shouldn't happen" (cardInstanceId.ToString())) |> Error
```

From a testing standpoint, the lack of rendering the resource cards has become quite difficult to deal with so I have added a simple rendering for the resource cards:

```

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
```


One thing I have realized is that I need to add functionality to remove old notifications. To do this I will need to add an id to the `Notification`s, define a Msg `DeleteNotification` which removes the notification with a specified id. Then I will add a delete button the to UI which sends a `DeleteNotification` message.

The UI update will be:
```
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
```

The domain update to the notification will be:
```
    ...
    and Notification =
        {
            Id: Guid
            Content: string
        }
    ...

    let createNotification message ={Id = Guid.NewGuid(); Content = message}
```

The Msg type I also updated to remove the outdated GameStarted type:

```
type Msg =
    | StartGame of StartGameEvent
    | DrawCard of DrawCardEvent
    | DiscardCard of DiscardCardEvent
    | PlayCard of PlayCardEvent
    | EndPlayStep of EndPlayStepEvent
    | PerformAttack of PerformAttackEvent
    | SkipAttack of SkipAttackEvent
    | EndTurn of EndTurnEvent
    | DeleteNotification of Guid
    | GameWon of GameWonEvent
```

I can then update `update` to include the new event
```
    | DeleteNotification dn ->
        removeNotification model dn, Cmd.none
```

and define `removeNotification` as

```
let filterNotificationList notificationId l =
    l |> List.filter (fun x -> x.Id <> notificationId)
    |> function
        | [] -> None
        | x -> Some x

let removeNotification gs notificationId =
   { gs with NotificationMessages = match gs.NotificationMessages with
                                    | Some x -> filterNotificationList notificationId x
                                    | None -> None
   }
```

This all appears to build so I am leave this as the final change in the branch `step-9-better-sample-data-and-wiring-up-ui`.


## Adding more realistic cards data and wiring up the Ui to send some events -Continued - the Player Control Center

Now I will try to implement the turn step changes via conditional buttons in the player control center.

Currently, a placeholder for these buttons is located in the `playerControlCenter` function. I will pull out a `stepNavigation` function and rewrite the `playerControlCenter` as

```
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
```
* note I also added the dispatch to the function

This `stepNavigation` function is going to rely on switching the current step and player to tell what options & actions are available.

I can implement the changes like:
```

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
```

Now I can see I start on the draw stage and am able to draw but that is not transitioning me to the next step. I will have to investigate the `modifyGameStateFromDrawCardEvent` function.

Investigating this function it appears I forgot to add the state shifting functionality.

I implemented this by changing the `modifyGameStateFromDrawCardEvent` function like
```
let migrateGameStateToNewStep newStep (gs: GameState) =
    // maybe some vaidation could go here?
    Ok {
        gs with CurrentStep = newStep
    }

let moveCardsFromDeckToHand gs playerId pb =
    let newDeck, newHand =  drawCardsFromDeck 1 pb.Deck pb.Hand
    Ok { gs with Boards = (gs.Boards.Add (playerId, { pb with Deck = newDeck; Hand = newHand })  ) }


let modifyGameStateFromDrawCardEvent (ev: DrawCardEvent) (gs: GameState) =
    getExistingPlayerBoardFromGameState ev.PlayerId gs
    |> Result.bind (moveCardsFromDeckToHand gs ev.PlayerId)
    |> Result.bind (migrateGameStateToNewStep (ev.PlayerId |> Attack))
    |> function
        | Ok g -> g
        | Error e -> { gs with NotificationMessages = appendNotificationMessageToListOrCreateList gs.NotificationMessages e }
```

Refreshing the site I am not able to click through all the steps of my turn!

I am leaving this as the final commit of branch `step-10-better-sample-data-and-wiring-up-ui-cont`.


## Switching the player for the GameState

To facilate testing I am not going to add logic to swap Player 1 and Player 2. This should allow us to step through an entire game.

To do this I will add a Msg of type SwapPlayer.

i.e.

```
type Msg =
    | StartGame of StartGameEvent
    | DrawCard of DrawCardEvent
    | DiscardCard of DiscardCardEvent
    | PlayCard of PlayCardEvent
    | EndPlayStep of EndPlayStepEvent
    | PerformAttack of PerformAttackEvent
    | SkipAttack of SkipAttackEvent
    | EndTurn of EndTurnEvent
    | DeleteNotification of Guid
    | GameWon of GameWonEvent
    | SwapPlayer
```

I will then handle the message in the update like:

```
let update (msg: Msg) (model: GameState): GameState * Cmd<Msg> =
    match msg with
    ...
    | SwapPlayer ->
        { model with PlayerOne = model.PlayerTwo; PlayerTwo = model.PlayerOne }, Cmd.none

```

Now I can add a button to switch players on the top nav bar like:

```
let topNavigation dispatch =
             ...
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
             ...
```

While doing this I found a bug in the, it should have been written like:

```
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
```
Also, I just noticed that the Draw is moving to the Attack step instead of the play step.

To fix this I changed the event that was being dispatched like

```
let modifyGameStateFromDrawCardEvent (ev: DrawCardEvent) (gs: GameState) =
    getExistingPlayerBoardFromGameState ev.PlayerId gs
    |> Result.bind (moveCardsFromDeckToHand gs ev.PlayerId)
    |> Result.bind (migrateGameStateToNewStep (ev.PlayerId |> Play))
    |> function
        | Ok g -> g
        | Error e -> { gs with NotificationMessages = appendNotificationMessageToListOrCreateList gs.NotificationMessages e }
```

Also, in this branch it was recomended to me that you can zoom cards. I am attempting to add a model that displays card details to the hand.

To try this I am trying to add a modal on click for the hand.


Meanwhile I am noticing I really need to refactor out operators for dutff like :
```
    |        |> Result.bind (applyUpdatedPlayerBoardResultToGamesState playerId gs x)
```
and
```
        |> Result.bind (fun x -> (applyUpdatedPlayerBoardResultToGamesState playerId gs x) |> Ok)

```
i did this by implementing infix operators like

```

let (>>=) twoTrackInput switchFunction =
    Result.bind switchFunction twoTrackInput

let (>=>) switch1 switch2 x =
    match switch1 x with
    | Ok s -> switch2 s
    | Error f -> Error f

```

Addtionally, I moved many of the constructors for domain objects to the Shared Domain and deleted unused test generator functions.

Eventually I was able to get the modal to work by adding a property to the game board : `ZoomedInCard` this is an optional CardInstanceId. I then added functions to check if the zoomed in card was set to teh current card. If it is the view displays the modal, if not it is hidden. I also added a Msg ZoomedInCardToggled and wired that up to the update funtion and the click of the a thumbnail image in the hand.



