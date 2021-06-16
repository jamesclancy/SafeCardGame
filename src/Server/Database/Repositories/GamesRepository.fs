namespace Games

open Database
open System.Threading.Tasks
open FSharp.Control.Tasks
open Npgsql
open Dto

module Database =
  let getAll connectionString : Task<Result<GameDto seq, exn>> =
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
    }

  let getById connectionString id : Task<Result<GameDto option, exn>> =
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
    }

  let update connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection """
                update
                	Game
                set
                	game_id = @GameId,
                	game_player_1_id = @Player1Id,
                	game_player_2_id = @Player2Id,
                	game_current_step = @CurrentStep,
                	game_current_player_move = @CurrentPlayerMove,
                	game_winner = @Winner,
                	game_notes = @Notes,
                	game_in_progress = @InProgress,
                	game_date_started = @DateStarted,
                	game_last_movement = @LastMovement
                where
                	game_id = @GameId""" v
    }

  let insert connectionString v : Task<Result<int,exn>> =
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
    }

  let delete connectionString id : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection """
                delete
                from
                	game
                where
                	game_id = @id""" (dict ["id" => id])
    }

