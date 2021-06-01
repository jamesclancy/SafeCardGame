module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Shared.Domain
open Shared.Domain
open Shared.Domain
open Operators
open Events



let (>>=) twoTrackInput switchFunction =
    Result.bind switchFunction twoTrackInput

let (>=>) switch1 switch2 x =
    match switch1 x with
    | Ok s -> switch2 s
    | Error f -> Error f

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

let emptyPlayerBoard (player : Player) =
        Ok  {
                PlayerId=  player.PlayerId
                Deck= {
                    TopCardsExposed = 0
                    Cards =  List.empty
                }
                Hand=
                    {
                        Cards = List.empty
                    }
                ActiveCreature= None
                Bench=  None
                DiscardPile= {
                    TopCardsExposed = 0
                    Cards = List.empty
                }
                TotalResourcePool= ResourcePool Seq.empty
                AvailableResourcePool =  ResourcePool Seq.empty
                ZoomedCard = None
            }

let drawCardsFromDeck (cardsToDraw: int) (deck : Deck) (hand: Hand) =
    if deck.Cards.IsEmpty then
        deck, hand
    else
        let cardsToTake = List.truncate cardsToDraw deck.Cards
        { deck with Cards = List.skip cardsToTake.Length deck.Cards}, {hand with Cards = hand.Cards @ cardsToTake}


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
        ZoomedCard = None
    }

let intitalizeGameStateFromStartGameEvent (ev : StartGameEvent) =
            {
                GameId= ev.GameId
                NotificationMessages= None
                PlayerOne= ev.PlayerOne
                PlayerTwo= ev.PlayerTwo
                Players= ev.Players
                Boards= ev.Decks
                        |> Seq.map (fun x -> x.Key, takeDeckDealFirstHandAndReturnNewPlayerBoard 7 x.Key x.Value )
                        |> Map.ofSeq
                CurrentStep= ev.PlayerOne |> Draw
                TurnNumber= 1
            }

let moveCardsFromDeckToHand gs playerId pb =
    let newDeck, newHand =  drawCardsFromDeck 1 pb.Deck pb.Hand
    Ok { gs with Boards = (gs.Boards.Add (playerId, { pb with Deck = newDeck; Hand = newHand })  ) }


let modifyGameStateFromDrawCardEvent (ev: DrawCardEvent) (gs: GameState) =
    (getExistingPlayerBoardFromGameState ev.PlayerId gs)
    >>= (moveCardsFromDeckToHand gs ev.PlayerId)
    >>= (migrateGameStateToNewStep (ev.PlayerId |> Play))
    |> function
        | Ok g -> g
        | Error e -> { gs with NotificationMessages = appendNotificationMessageToListOrCreateList gs.NotificationMessages e }


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

                                Cards = (List.filter (fun x -> x.CardInstanceId <> cardInstanceId) playerBoard.Hand.Cards)

                          };
                         DiscardPile ={playerBoard.DiscardPile with Cards = playerBoard.DiscardPile.Cards @ [ x ] }
            } |> Ok
    | _ ->
        (sprintf "ERROR: located multiple cards in hand with card instance id %s. This shouldn't happen" (cardInstanceId.ToString())) |> Error

let applyErrorResultToGamesState originalGameState newGameState =
    match newGameState with
    | Ok gs -> gs
    | Error e ->
        { originalGameState with NotificationMessages = appendNotificationMessageToListOrCreateList originalGameState.NotificationMessages e }


let applyUpdatedPlayerBoardResultToGamesState playerId gs newBoard =
    match newBoard with
    | Ok pb -> { gs with Boards = (gs.Boards.Add (playerId, pb)  ) } |> Ok
    | Error e -> Error e
    |> applyErrorResultToGamesState gs

let toggleZoomOnCardForBoard (cardInstanceId : CardInstanceId) (playerBoard : PlayerBoard) =
    let newZoom = match playerBoard.ZoomedCard with
                    | Some c when c = cardInstanceId -> None
                    | Some c -> Some cardInstanceId
                    | None -> Some cardInstanceId
    Ok { playerBoard with ZoomedCard = newZoom }

let modifyGameStateFromToggleZoomOnCardEvent (ev :ToggleZoomOnCardEvent) gs =
        getExistingPlayerBoardFromGameState ev.PlayerId gs
        >>= (toggleZoomOnCardForBoard ev.CardInstanceId)
        |> applyUpdatedPlayerBoardResultToGamesState ev.PlayerId gs

let modifyGameStateFromDiscardCardEvent (ev: DiscardCardEvent) (gs: GameState) =
        getExistingPlayerBoardFromGameState ev.PlayerId gs
        >>= (discardCardFromBoard ev.CardInstanceId)
        |> applyUpdatedPlayerBoardResultToGamesState ev.PlayerId gs

let applyEffectIfDefinied effect gs =
    match effect with
    | Some e -> e.Function.Invoke gs |> Ok
    | None  -> gs |> Ok

let appendCreatureToPlayerBoard inPlayCreature playerBoard =
        match playerBoard.ActiveCreature with
        | None ->
            { playerBoard with ActiveCreature = Some inPlayCreature }
        | Some a ->
            { playerBoard
                with Bench
                    = Some (Option.fold (fun x y -> x @ y)  [ inPlayCreature ]  playerBoard.Bench)  }


let addCreatureToGameState cardInstanceId x playerId gs playerBoard inPlayCreature=
        {
            playerBoard
                with Hand =
                        {
                            playerBoard.Hand with
                                    Cards = (List.filter (fun x -> x.CardInstanceId <> cardInstanceId) playerBoard.Hand.Cards)
                        };
                     DiscardPile =
                        {
                            playerBoard.DiscardPile with Cards = playerBoard.DiscardPile.Cards @ [ x ]
                        }
        } |> (appendCreatureToPlayerBoard inPlayCreature) |> Ok
        |> (applyUpdatedPlayerBoardResultToGamesState playerId gs)

let buildInPlayCreatureId idStr =
    idStr
    |> NonEmptyString.build
    |> Result.map InPlayCreatureId

let decrementRequiredResourcesFromModel cardToDiscard (playerId : PlayerId) (gs: GameState) (playerBoard : PlayerBoard) =
     getNeededResourcesForCard cardToDiscard
     |> Map.toList
     |> decrementResourcesFromPlayerBoard playerBoard
     >>= (fun updatedPlayerBoard -> Ok {gs with Boards = gs.Boards.Add(playerId, updatedPlayerBoard) })

let playCardFromBoardImp cardInstanceId playerId playerBoard (x : CardInstance) cardToDiscard gs =
    match x.Card with
               | CharacterCard cc ->
                     (System.Guid.NewGuid().ToString())
                     |> buildInPlayCreatureId
                     >>= (createInPlayCreatureFromCardInstance x.Card)
                     >>= (fun y -> (addCreatureToGameState cardInstanceId x playerId gs playerBoard y) |> Ok)
                     >>= (applyEffectIfDefinied cc.EnterSpecialEffects)

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
                 { gs with Boards = (gs.Boards.Add (playerId, newPb)) } |> (applyEffectIfDefinied rc.EnterSpecialEffects) >>= (applyEffectIfDefinied rc.ExitSpecialEffects)
               | EffectCard ec ->
                 let newPb =  {
                     playerBoard
                       with Hand =
                             { playerBoard.Hand with

                                   Cards = (List.filter (fun x -> x.CardInstanceId <> cardInstanceId) playerBoard.Hand.Cards)
                             };
                            DiscardPile = {playerBoard.DiscardPile with Cards = playerBoard.DiscardPile.Cards @ [ x ] }
                   }
                 { gs with Boards = (gs.Boards.Add (playerId, newPb) ) } |> (applyEffectIfDefinied ec.EnterSpecialEffects) >>= (applyEffectIfDefinied ec.ExitSpecialEffects)


let playCardFromBoard (cardInstanceId : CardInstanceId) (playerId : PlayerId) (gs: GameState) (playerBoard : PlayerBoard) =
    let cardToDiscard : CardInstance list = List.filter (fun x -> x.CardInstanceId = cardInstanceId) playerBoard.Hand.Cards

    match cardToDiscard with
    | [] ->
        (sprintf "Unable to locate card in hand with card instance id %s" (cardInstanceId.ToString())) |> Error
    | [ x ] ->
        decrementRequiredResourcesFromModel x.Card playerId gs playerBoard
        >>= (playCardFromBoardImp cardInstanceId playerId playerBoard x cardToDiscard)
    | _ ->
        (sprintf "ERROR: located multiple cards in hand with card instance id %s. This shouldn't happen" (cardInstanceId.ToString())) |> Error

let modifyGameStateFromPlayCardEvent (ev: PlayCardEvent) (gs: GameState) =
        getExistingPlayerBoardFromGameState ev.PlayerId gs
        >>= (playCardFromBoard  ev.CardInstanceId ev.PlayerId gs)
        |> applyErrorResultToGamesState gs

let init =
    let player1 = createPlayer "Player1" "Player1" 10 "https://picsum.photos/id/1000/2500/1667?blur=5"
    let player2 = createPlayer "Player2" "Player2" 10 "https://picsum.photos/id/10/2500/1667?blur=5"
    let gameId =  NonEmptyString.build "GameIDHere" |> Result.map GameId

    match player1, player2, gameId with
    | Ok p1, Ok p2, Ok g ->
        let playerBoard1 = emptyPlayerBoard p1
        let playerBoard2 = emptyPlayerBoard p2
        match playerBoard1, playerBoard2 with
        | Ok pb1, Ok pb2 ->
          let model : GameState =
            {
                GameId = g
                Players =  [
                    p1.PlayerId, p1;
                    p2.PlayerId, p2
                   ] |> Map.ofList
                Boards = [
                    p1.PlayerId, pb1;
                    p2.PlayerId, pb2
                   ] |> Map.ofList
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

let getTheOtherPlayer (gameState : GameState) playerId =
    if gameState.PlayerOne = playerId then
        gameState.PlayerTwo
    else
        gameState.PlayerOne

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


let getPlayBoardToTargetAttack (playerId : PlayerId) gs =
    playerId
    |> gs.Boards.TryGetValue
    |> function
        | true, pb -> pb |> Ok
        | _, _ -> "Unable to locate target for attack" |> Error

let activeCreatureKilledFromPlayerBoard playBoard :PlayerBoard =
    match playBoard.Bench with
    | None | Some [] -> { playBoard with ActiveCreature = None}
    | Some [ x ] ->  { playBoard with ActiveCreature = Some x; Bench = None}
    | Some (x :: xs) -> { playBoard with ActiveCreature = Some x; Bench = Some xs}

let applyBasicAttackToPlayBoard (attack : Attack) (playBoard) gs =
        match playBoard.ActiveCreature with
        | Some cre when (cre.CurrentDamage + attack.Damage) < cre.TotalHealth ->
              ({
                playBoard with ActiveCreature =
                                    Some { cre with CurrentDamage = cre.CurrentDamage  + attack.Damage }
              }, 0, sprintf "%i damage dealt to %s" attack.Damage cre.Name)
        | Some cre ->
            ((activeCreatureKilledFromPlayerBoard playBoard), 0, sprintf "%i damage dealt to %s. It died." attack.Damage cre.Name)
        | None ->
            (playBoard, attack.Damage, sprintf "%i damage dealt to player" attack.Damage)


let applyPlayerDamageToPlayer (playerId : PlayerId) damage (gs: GameState) =
    let player = gs.Players.TryGetValue playerId
    match player with
    | true, p -> { gs with Players = gs.Players.Add(playerId, {p with RemainingLifePoints = p.RemainingLifePoints - damage})}
    | _,_ -> gs

let appendMessagesToGameState messages (gs : GameState) =
    { gs with NotificationMessages = appendNotificationMessageToListOrCreateList gs.NotificationMessages messages }

let playAttackFromBoard (attack : Attack) (playerId : PlayerId) (gs: GameState) (playerBoard : PlayerBoard) =

    let target = getTheOtherPlayer gs playerId
    let otherBoard = getPlayBoardToTargetAttack target gs

    match otherBoard with
    | Ok playBoard ->

        let (newPb, playerDamage, messages) =  (applyBasicAttackToPlayBoard attack playBoard gs)

        { gs with Boards = (gs.Boards.Add (target, newPb)  ) }
        |> applyPlayerDamageToPlayer target playerDamage
        |> appendMessagesToGameState messages |>Ok
    | Error e ->
        Error e


let modifyGameStateFromPerformAttackEvent (ev: PerformAttackEvent) (gs: GameState) =
        getExistingPlayerBoardFromGameState ev.PlayerId gs
        >>= (playAttackFromBoard ev.Attack ev.PlayerId gs)
        >>= migrateGameStateToNewStep (ev.PlayerId |> Reconcile )
        |> applyErrorResultToGamesState gs

let update (msg: Msg) (model: GameState): GameState * Cmd<Msg> =
    match msg with
    | StartGame ev ->
        intitalizeGameStateFromStartGameEvent ev, Cmd.none
    | DrawCard  ev ->
        modifyGameStateFromDrawCardEvent ev model, Cmd.none
    | DiscardCard ev ->
        modifyGameStateFromDiscardCardEvent ev model, Cmd.none
    | ToggleZoomOnCard ev ->
        modifyGameStateFromToggleZoomOnCardEvent ev model, Cmd.none
    | PlayCard ev ->
        modifyGameStateFromPlayCardEvent ev model, Cmd.none
    | EndPlayStep ev ->
        { model with CurrentStep = (Attack ev.PlayerId)}, Cmd.none
    | PerformAttack  ev ->
        modifyGameStateFromPerformAttackEvent ev model, Cmd.none
    | SkipAttack ev ->
        { model with CurrentStep = (Reconcile ev.PlayerId)}, Cmd.none
    | EndTurn ev ->
        let otherPlayer = getTheOtherPlayer model ev.PlayerId
        { model with CurrentStep = (Draw otherPlayer)}, Cmd.none
    | DeleteNotification dn ->
        removeNotificationFromGameState model dn, Cmd.none
    | GameWon ev ->
        let newStep =  { WinnerId =  ev.Winner; Message = formatGameOverMessage ev.Message } |> GameOver
        { model with CurrentStep = newStep}, Cmd.none
    | SwapPlayer ->
        { model with PlayerOne = model.PlayerTwo; PlayerTwo = model.PlayerOne }, Cmd.none


let view (model : GameState) (dispatch : Msg -> unit) =
    PageLayoutParts.mainLayout model dispatch
