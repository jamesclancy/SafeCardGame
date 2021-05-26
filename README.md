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

Additionally, at this stage, I noticed that the playmat background on the creatures was missing a `'` around the url so I added that.

Now our layout is largely wired up to the GameState. Next, we will need to define some events which modify the GameState and attach those events to the UI.

This is the final commit in the branch `step-6-wire-up-layout-to-gamestate-part-2`

### Defining Events To Change the GameState
---

As players play the game they can perform a number of actions which trigger events that modify the GameState.

*Note from a technical standpoint:* The GameState is actaully immutable and never changes but rather a new GameState is generated from an acton and the previous state. This new state replaces the past state.


Initially, I am going to start just writing out some potenital events which could happen during the game and the associated event data they would carry.

* StartGame - Inititalizes a new game (Creates a new GameState)
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
        remove the card from the players hand
        - if a resource card add to the total resource pool
        - if an effect card trigger the effect
        - if a character card create an inplay creature and trigger the   enter event. If no active create exists place the in play creature in the active potition otherwise place on the bench.
    If the resources are not available add an error message to the gamestate
  * GameId
  * PlayerId
  * CardInstanceID
* EndPlayStep - Move the gamestate to the attack state
  * GameId
  * PlayerId
* Attack -
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
* EndTurn - Mode the gamestate to teh other player's draw state
  * GameId
  * PlayerId
* GameWon - Move the gamestate to the game over state
  * PlayerId of Winner
  * Winning Reason (string)

There will have to be additional events for things like tapping out/retreating active creatures but I think those would best be added after everything else is set up/works.

Going through this process I have identified a few inital things that need to be added.

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

I then had to update the init funciton for the new fields like
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

I similarly had to update the currentStepInformation to utilize the updated GameStep type. Using the fact that whos turn it is is now embeded in the state I was able to simplify the yourCurrentStepClasses function like:

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

In the Client/Index.fs I can now modify the Msg type to be a descriminate union of all the above listed types. First though I am moving the Msg type into its own module/file Events/Events.fs.