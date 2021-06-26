namespace Games

open System
open Database
open System.Threading.Tasks
open Events
open FSharp.Control.Tasks
open FSharp.Control.Websockets
open Npgsql
open Dto
open Shared
open Shared.Domain

module Database =
  let getAll connectionString : Async<Result<GameDto seq, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! query connection """
                    select
                    	game_id GameId,
                    	game_player_1_id Player1Id,
                    	game_player_2_id Player2Id,
                    	game_current_step CurrentStep,
                    	game_current_player_move CurrentPlayerMove,
                    	game_winner Winner,
                    	game_notes Notes,
                    	game_in_progress InProgress,
                    	game_date_started DateStarted,
                    	game_last_movement LastMovement
                    from
                    	game
      """ None
    } |> Async.AwaitTask

  let getById connectionString id : Async<Result<GameDto option, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! querySingle connection """
                    select
                    	game_id GameId,
                    	game_player_1_id Player1Id,
                    	game_player_2_id Player2Id,
                    	game_current_step CurrentStep,
                    	game_current_player_move CurrentPlayerMove,
                    	game_winner Winner,
                    	game_notes Notes,
                    	game_in_progress InProgress,
                    	game_date_started DateStarted,
                    	game_last_movement LastMovement
                    from
                    	game
                    where
                    	game_id = @id""" (Some <| dict ["id" => id])
    } |> Async.AwaitTask

  let update connectionString v : Async<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection """
                update
                	Game
                set
                	game_player_1_id = @Player1Id,
                	game_player_2_id = @Player2Id,
                	game_current_step = @CurrentStep,
                	game_current_player_move = @CurrentPlayerMove,
                	game_winner = @Winner,
                	game_notes = @Notes,
                	game_in_progress = @InProgress,
                	game_date_started = @DateStarted,
                	game_last_movement = @LastMovement,
                    game_state = @GameState
                where
                	game_id = @GameId""" v
    } |> Async.AwaitTask

  let insert connectionString v : Async<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection """
                insert
                into
                	game(game_id,
                	game_player_1_id,
                	game_player_2_id,
                	game_current_step,
                	game_current_player_move,
                	game_winner,
                	game_notes,
                	game_in_progress,
                	game_date_started,
                	game_last_movement)
                values (@GameId,
                @Player1Id,
                @Player2Id,
                @CurrentStep,
                @CurrentPlayerMove,
                @Winner,
                @Notes,
                @InProgress,
                @DateStarted,
                @LastMovement)""" v
    } |> Async.AwaitTask

  let delete connectionString id : Async<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection """
                delete
                from
                	game
                where
                	game_id = @id""" (dict ["id" => id])
    } |> Async.AwaitTask

  let updateGameState connectionString (gameState : GameState) =
    task {
      let currentStep, currentPlayer, winner  = Game.getPlayerAndStepFromGameStata gameState
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection """
                update
                	Game
                set
                    game_current_step = @GameStep,
                    game_last_movement = @CurrentTime,
                    game_current_player_move = @PlayerMove,
                    game_winner = @GameWinner,
                    game_state = @GameState
                where
                	game_id = @GameId""" (dict [
                                             "GameId" => (gameState.GameId.ToString())
                                             "GameStep" =>  currentStep
                                             "GameWinner" => winner
                                             "CurrentTime" => DateTime.UtcNow
                                             "PlayerMove" => currentPlayer
                                             "GameState" => Thoth.Json.Net.Encode.Auto.toString(4,gameState)])
    } |> Async.AwaitTask

  let attachPlayerAsPlayer2 connectionString gameDto (player : Player) (gameId: GameId) : Async<Result<GameDto option, exn>> =
        task {
          use connection = new NpgsqlConnection(connectionString)

          let! playerOneRes = Players.Database.getOrCreatePlayer connectionString gameDto.Player1Id gameDto.Player1Id |> Async.StartAsTask
          //let gameState = Thoth.Json.Net.Decode.Auto.fromString<GameState>(gameDto.GameState)

          match playerOneRes with
          | Ok (Some playerOne) ->
              let gameState = {
                                                        GameId= gameId
                                                        NotificationMessages= None
                                                        PlayerOne= playerOne.PlayerId
                                                        PlayerTwo= player.PlayerId
                                                        Players= [playerOne.PlayerId, playerOne; player.PlayerId, player] |> Map.ofList
                                                        Boards=  [
                                                            playerOne.PlayerId, GameStateTransitions.takeDeckDealFirstHandAndReturnNewPlayerBoard 0 playerOne.PlayerId { Cards = list.Empty; TopCardsExposed = 0 }
                                                            player.PlayerId, GameStateTransitions.takeDeckDealFirstHandAndReturnNewPlayerBoard 0 player.PlayerId { Cards = list.Empty; TopCardsExposed = 0 }
                                                        ] |> Map.ofList
                                                        CurrentStep= playerOne.PlayerId |> Draw
                                                        TurnNumber= 1
                                                    }

              let newGameDto= { gameDto  with

                                   Player2Id = player.PlayerId.ToString()
                                   GameState = Thoth.Json.Net.Encode.Auto.toString(4,gameState)
                                    }
              let! updateResult = ((update connectionString newGameDto) |> Async.StartAsTask)
              match updateResult  with
              | Ok i ->
                    return (Ok (Some newGameDto))
              | Error e ->
                  return (e |> Error)
          | Error e  -> return (e |> Error)
          | _ -> return (new Exception("welp idk") |> Error)
        } |> Async.AwaitTask


  let createGameFromSinglePlayerAndDeck connectionString (gameId: GameId) (player : Player) : Async<Result<GameDto option, exn>> =
        task {
          use connection = new NpgsqlConnection(connectionString)

          let gameDto =  {
                              GameId = gameId.ToString()
                              Player1Id = player.PlayerId.ToString()
                              Player2Id = null
                              CurrentStep= "NotCurrentlyPlaying"
                              CurrentPlayerMove = null
                              Winner = null
                              Notes = ""
                              InProgress = false
                              DateStarted= System.DateTime.Now
                              LastMovement= System.DateTime.Now
                              GameState =  ""
                            }
          let! gameState = insert connectionString gameDto
          match gameState with
          | Error e  -> return (e |> Error)
          | Ok g ->
                return (Ok (Some gameDto))
        } |> Async.AwaitTask

