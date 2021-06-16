namespace Players

open Database
open Dto
open Shared.Domain
open Farmer.DiagnosticSettings.Logging.EventGrid
open Npgsql
open System.Threading.Tasks
open FSharp.Control.Tasks

module Database =
  let getAll connectionString : Async<Result<Player seq, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      let! res =  query connection """
                            select
                            	player_id,
                            	player_name,
                            	player_playmat_url,
                            	player_life_points,
                            	player_initial_health,
                            	date_created,
                            	last_login
                            from
                            	public.player;""" None

      match res with
      | Error e -> return (e |> Error)
      | Ok s -> return (s |> Seq.map Player.toDomain |> Seq.toList |> CollectionManipulation.selectAllOkayResults |> List.toSeq |> Ok)
    } |> Async.AwaitTask

  let getById connectionString id : Async<Result<Player option, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      let! res = querySingle connection """
                            select
                            	player_id,
                            	player_name,
                            	player_playmat_url,
                            	player_life_points,
                            	player_initial_health,
                            	date_created,
                            	last_login
                            from
                            	public.player
                            WHERE player_id=@Id""" (Some <| dict ["id" => id])

      match res with
      | Error e -> return (e |> Error)
      | Ok s ->
          match s with
          | Some e ->
              match e |> Player.toDomain with
              | Ok q -> return (q |> Some |> Ok)
              | Error e -> return (failwith e |> Error)
          | None ->
              return (None |> Ok)

    } |> Async.AwaitTask

  let update connectionString v : Async<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection """
                        update
                        	public.player
                        set
                        	player_name = @Name,
                        	player_playmat_url = @PlaymatUrl,
                        	player_life_points = @LifePoints,
                        	player_initial_health = @InitialHealth,
                        	date_created = @DateCreated,
                        	last_login = @LastLogin
                        where
                        	player_id = @PlayerId;
                        """ v } |> Async.AwaitTask

  let insert connectionString v : Async<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection """
                        insert
                        	into
                        	public.player (player_id,
                        	player_name,
                        	player_playmat_url,
                        	player_life_points,
                        	player_initial_health,
                        	date_created,
                        	last_login)
                        values(@PlayerId,
                        @Name,
                        @PlaymatUrl,
                        @LifePoints,
                        @InitialHealth,
                        @DateCreated,
                        @LastLogin);
                        """ v  } |> Async.AwaitTask

  let delete connectionString id : Async<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection "DELETE FROM User WHERE player_id=@Id" (dict ["id" => id])
    } |> Async.AwaitTask

