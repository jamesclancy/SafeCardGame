module GameStateTransitions

open Events
open Shared.Domain
open Operators
open Shared

let takeDeckDealFirstHandAndReturnNewPlayerBoard (initialHandSize: int) (playerId : PlayerId) (deck : Deck) =
    let emptyHand =
      {
        Cards = list.Empty
      }
    let deckAfterDraw, hand = drawCardsFromDeck initialHandSize deck emptyHand
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

let initializeGameStateFromStartGameEvent (ev : StartGameEvent) =
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
        sprintf "Unable to locate card in hand with card instance id %s" (cardInstanceId.ToString()) |> Error
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
        sprintf "ERROR: located multiple cards in hand with card instance id %s This shouldn't happen" (cardInstanceId.ToString()) |> Error

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

let applyEffectIfDefined effect gs =
    match effect with
    | Some e -> e.Function gs |> Ok
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

let decrementRequiredResourcePoolFromModel resource (playerId : PlayerId) (gs: GameState) (playerBoard : PlayerBoard) =
     resource
     |> Map.toList
     |> decrementResourcesFromPlayerBoard playerBoard
     >>= (fun updatedPlayerBoard -> Ok {gs with Boards = gs.Boards.Add(playerId, updatedPlayerBoard) })

let decrementRequiredCardResourcesFromModel cardToDiscard (playerId : PlayerId) (gs: GameState) (playerBoard : PlayerBoard) =
     getNeededResourcesForCard cardToDiscard
     |> (fun x -> decrementRequiredResourcePoolFromModel x playerId gs playerBoard)

let playCardFromBoardImp cardInstanceId playerId playerBoard (x : CardInstance) cardToDiscard gs =
    match x.Card with
               | CharacterCard cc ->
                     (System.Guid.NewGuid().ToString())
                     |> buildInPlayCreatureId
                     >>= (createInPlayCreatureFromCardInstance x.Card)
                     >>= (fun y -> (addCreatureToGameState cardInstanceId x playerId gs playerBoard y) |> Ok)
                     >>= (applyEffectIfDefined cc.EnterSpecialEffects)

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
                 { gs with Boards = (gs.Boards.Add (playerId, newPb)) } |> (applyEffectIfDefined rc.EnterSpecialEffects) >>= (applyEffectIfDefined rc.ExitSpecialEffects)
               | EffectCard ec ->
                 let newPb =  {
                     playerBoard
                       with Hand =
                             { playerBoard.Hand with

                                   Cards = (List.filter (fun x -> x.CardInstanceId <> cardInstanceId) playerBoard.Hand.Cards)
                             };
                            DiscardPile = {playerBoard.DiscardPile with Cards = playerBoard.DiscardPile.Cards @ [ x ] }
                   }
                 { gs with Boards = (gs.Boards.Add (playerId, newPb) ) } |> (applyEffectIfDefined ec.EnterSpecialEffects) >>= (applyEffectIfDefined ec.ExitSpecialEffects)


let playCardFromBoard (cardInstanceId : CardInstanceId) (playerId : PlayerId) (gs: GameState) (playerBoard : PlayerBoard) =
    let cardToDiscard : CardInstance list = List.filter (fun x -> x.CardInstanceId = cardInstanceId) playerBoard.Hand.Cards

    match cardToDiscard with
    | [] ->
        sprintf "Unable to locate card in hand with card instance id %s" (cardInstanceId.ToString()) |> Error
    | [ x ] ->
        decrementRequiredCardResourcesFromModel x.Card playerId gs playerBoard
        >>= (playCardFromBoardImp cardInstanceId playerId playerBoard x cardToDiscard)
    | _ ->
        sprintf "ERROR: located multiple cards in hand with card instance id %s This shouldn't happen" (cardInstanceId.ToString()) |> Error

let modifyGameStateFromPlayCardEvent (ev: PlayCardEvent) (gs: GameState) =
        getExistingPlayerBoardFromGameState ev.PlayerId gs
        >>= (playCardFromBoard  ev.CardInstanceId ev.PlayerId gs)
        |> applyErrorResultToGamesState gs


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

let applyBasicAttackToPlayBoard (attack : Attack) playBoard gs =
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
        let newPb, playerDamage, messages =  (applyBasicAttackToPlayBoard attack playBoard gs)

        { gs with Boards = (gs.Boards.Add (target, newPb)  ) }
        |> applyPlayerDamageToPlayer target playerDamage
        |> appendMessagesToGameState messages |>Ok
    | Error e ->
        Error e

let tryToModifyGameStateFromPerformAttackEvent (ev: PerformAttackEvent) (gs: GameState) =
    CollectionManipulation.result {
           let! pb = getExistingPlayerBoardFromGameState ev.PlayerId gs
           let! newGs = decrementRequiredResourcePoolFromModel ev.Attack.Cost ev.PlayerId gs pb
           let! postAttackGs = playAttackFromBoard ev.Attack ev.PlayerId newGs pb
           return! postAttackGs |> migrateGameStateToNewStep (ev.PlayerId |> Reconcile )
    }

let modifyGameStateFromPerformAttackEvent (ev: PerformAttackEvent) (gs: GameState) =
        tryToModifyGameStateFromPerformAttackEvent ev gs
        |> applyErrorResultToGamesState gs

let tryToModifyGameStateTurnToOtherPlayer model otherPlayer  =
         CollectionManipulation.result {
           let! pb = getExistingPlayerBoardFromGameState otherPlayer model

           return { model with
                            CurrentStep = (Draw otherPlayer)
                            Boards = model.Boards.Add(otherPlayer, { pb with AvailableResourcePool = pb.TotalResourcePool})
                  }
         }

let modifyGameStateTurnToOtherPlayer playerId model =
    getTheOtherPlayer model playerId
    |> tryToModifyGameStateTurnToOtherPlayer model
    |> applyErrorResultToGamesState model