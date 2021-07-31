module DatabaseRepositories

open System
open Npgsql.FSharp
open Dto
open Shared.Domain
open Newtonsoft.Json
open System.Collections.Generic

let returnOkayResultOrRaiseError res =
    match res with
    | Ok s -> s
    | Error e -> raise e

module JsonToDtoMappings =
    module Attack =

        let parseDtoFromJson (jsonValue : string) =
            jsonValue
            |>  JsonConvert.DeserializeObject<AttackDto[]>

    module Resource =

        let parseListFromJson (resources: string) : string[] =
            if String.IsNullOrWhiteSpace resources then
                Array.empty
            else
                resources
                |> JsonConvert.DeserializeObject<string[]>

    module ResourcePool =

        let parseDtoFromJson (dtoJson :string) =
            dtoJson |>  JsonConvert.DeserializeObject<Dictionary<string, int>>


        let dtoToJson (dto :Dictionary<string, int>) =
            dto |> JsonConvert.SerializeObject

    module GameStateSpecialEffect =

        let parseDtoFromJson (text: string option) =
            match text with
            | None -> None
            | Some s ->
                    if String.IsNullOrWhiteSpace s then
                        None
                    else
                        JsonConvert.DeserializeObject<SpecialEffectDto> s
                        |> Some

module RowMappings =

    let rowToPlayerDto (read : RowReader) : PlayerDto = {
                    PlayerId = read.string "player_id"
                    Name = read.string "player_name"
                    PlaymatUrl = read.string "player_playmat_url"
                    LifePoints = read.int "player_life_points"
                    InitialHealth = read.int "player_initial_health"
                    DateCreated = read.dateTime "date_created"
                    LastLogin = read.dateTime "last_login"
                }

    let rowToCardDto (read : RowReader) : CardDto =
            {
                CardId= read.string "card_id"
                Name = read.string "card_name"
                Description = read.string "card_description"
                ImageUrl =  read.string "card_image_url"
                ThumbnailImageUrl = read.string "card_thumbnail_image_url"
                ResourceCost =  read.string "card_resource_cost" |> JsonToDtoMappings.ResourcePool.parseDtoFromJson
                PrimaryResource =  read.string "card_primary_resource"
                CardType =  read.string "card_type"
                EnterSpecialEffects =  read.stringOrNone "card_enter_special_effects" |> JsonToDtoMappings.GameStateSpecialEffect.parseDtoFromJson
                ExitSpecialEffects =  read.stringOrNone "card_exit_special_effects" |>  JsonToDtoMappings.GameStateSpecialEffect.parseDtoFromJson
                CreatureHealth =  read.intOrNone "card_creature_health"
                Weaknesses = read.string "card_creature_weaknesses" |>  JsonToDtoMappings.Resource.parseListFromJson
                Attack =  read.string "card_creature_attacks" |>  JsonToDtoMappings.Attack.parseDtoFromJson
                ResourceAvailableOnFirstTurn =  read.boolOrNone "card_resources_available_on_first_turn"
                ResourcesAdded = read.string "card_resources_added"  |>  JsonToDtoMappings.ResourcePool.parseDtoFromJson
            }

    let rowToPreCreatedDeckDto (read : RowReader) : DeckDto = {
                    DeckId = read.string "deck_id"
                    DeckName = read.string "deck_name"
                    DeckDescription = "na"
                    DeckImageUrl =  "na"
                    DeckThumbnailImageUrl =  "na"
                    DeckPrimaryResource =  "na"
                    DeckOwner =  "na"
                    DeckPrivate =  false }

type PlayerRepository () =

    member _.GetAll connectionString =
        async {
             let! query =
                connectionString
                |> Sql.connect
                |> Sql.query "SELECT * FROM player"
                |> Sql.executeAsync RowMappings.rowToPlayerDto
                |> Async.AwaitTask

             return query
                |> Seq.map Player.toDomain
                |> Seq.toList
                |> CollectionManipulation.selectAllOkayResults
                |> List.toSeq
        }

    member _.Get connectionString playerId =
        async {
             let! query =
                connectionString
                |> Sql.connect
                |> Sql.query "SELECT * FROM player WHERE player_id = @player_id"
                |> Sql.parameters [ "player_id", Sql.string playerId ]
                |> Sql.executeRowAsync RowMappings.rowToPlayerDto
                |> Async.AwaitTask

             return query
                |> Player.toDomain
        }

    member _.Update connectionString (player : Player) =
        async {
             let! query =
                connectionString
                |> Sql.connect
                |> Sql.query """UPDATE player
                SET
                player_name = @player_name
                player_playmat_url = @player_playmat_url
                player_life_points = @player_life_points
                player_initial_health = @player_initial_health
                date_created = @date_created
                last_login = @last_login
                WHERE player_id = @player_id"""
                |> Sql.parameters [
                        "player_id", Sql.string (player.PlayerId.ToString())
                        "player_name", Sql.string player.Name
                        "player_playmat_url", Sql.string (player.PlaymatUrl.ToString())
                        "player_life_points", Sql.int player.RemainingLifePoints
                        "player_initial_health", Sql.int player.InitialHealth
                        "date_created", Sql.timestamptz player.DateCreated
                        "last_login", Sql.timestamptz player.LastLogin
                        ]
                |> Sql.executeNonQueryAsync
                |> Async.AwaitTask

             return query
        }

    member _.UpdateLastLoginTime connectionString (player : Player) =
        async {
             let newPlayer = { player with LastLogin = DateTime.UtcNow}

             let! query =
                connectionString
                |> Sql.connect
                |> Sql.query """UPDATE player
                SET
                last_login = @last_login
                WHERE player_id = @player_id"""
                |> Sql.parameters [
                        "player_id", Sql.string (newPlayer.PlayerId.ToString())
                        "last_login", Sql.timestamptz newPlayer.LastLogin
                        ]
                |> Sql.executeNonQueryAsync
                |> Async.AwaitTask

             query |> ignore

             return newPlayer
        }

type CardRepository () =

    member _.GetAll connectionString =
        async {
         let! query =
            connectionString
            |> Sql.connect
            |> Sql.query "SELECT * FROM card"
            |> Sql.executeAsync RowMappings.rowToCardDto
            |> Async.AwaitTask

         return query
            |> Seq.map Card.toDomain
            |> Seq.toList
            |> CollectionManipulation.selectAllOkayResults
            |> List.toSeq
        }

    member _.Get connectionString cardId =
        async {
             let! query =
                connectionString
                |> Sql.connect
                |> Sql.query "SELECT * FROM card WHERE card_id = @card_id"
                |> Sql.parameters [ "card_id", Sql.string cardId ]
                |> Sql.executeRowAsync RowMappings.rowToCardDto
                |> Async.AwaitTask

             return query |> Card.toDomain
        }

type DeckRepository () =

    member _.GetAll connectionString =
            async {
                 let! query =
                    connectionString
                    |> Sql.connect
                    |> Sql.query "SELECT * FROM deck"
                    |> Sql.executeAsync RowMappings.rowToPreCreatedDeckDto
                    |> Async.AwaitTask

                 return query
                    |> List.toSeq
            }

    member _.GetCardsForDeck connectionString deck_id =
            async {
                 let! query =
                    connectionString
                    |> Sql.connect
                    |> Sql.query "select c.* from card c inner join deck_card_association dca on dca.card_id  = c.card_id where dca.deck_id = @deck_id order by RANDOM()"
                    |> Sql.parameters [ "deck_id", Sql.string deck_id ]
                    |> Sql.executeAsync RowMappings.rowToCardDto
                    |> Async.AwaitTask

                 return query
                    |> Seq.map Card.toDomain
                    |> Seq.toList
                    |> CollectionManipulation.selectAllOkayResults
                    |> List.toSeq
            }    

