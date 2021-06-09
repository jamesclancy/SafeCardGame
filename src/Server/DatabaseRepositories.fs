module DatabaseRepositories


open Npgsql.FSharp
open Dto

let connectionString = "Host=localhost; Database=postgres; Username=postgres; Password=trident1814;"

let returnOkayResultOrRaiseError res =
    match res with
    | Ok s -> s
    | Error e -> raise e

type PlayerRepository () =

    let rowToDto (read : RowReader) : PlayerDto = {
                    PlayerId = read.string "player_id"
                    Name = read.string "player_name"
                    PlaymatUrl = read.string "player_playmat_url"
                    LifePoints = 20
                }

    member __.GetAll () =
        async {
         let! query =
            connectionString
            |> Sql.connect
            |> Sql.query "SELECT * FROM player"
            |> Sql.executeAsync rowToDto
            |> Async.AwaitTask

         return query
            |> Seq.map Player.toDomain
            |> Seq.toList
            |> CollectionManipulation.selectAllOkayResults
            |> List.toSeq
        }

    member __.Get playerId =
        async {
         let! query =
            connectionString
            |> Sql.connect
            |> Sql.query "SELECT * FROM player WHERE player_id = @player_id"
            |> Sql.parameters [ "player_id", Sql.string playerId ]
            |> Sql.executeRowAsync rowToDto
            |> Async.AwaitTask

         return query
            |> Player.toDomain
        }

type CardRepository () =

    let rowToDto (read : RowReader) : CardDto =
            {
                CardId= read.string "card_id"
                Name = read.string "card_name"
                Description = read.string "card_description"
                ImageUrl =  read.string "card_image_url"
                ThumbnailImageUrl = read.string "card_thumbnail_image_url"
                ResourceCost =  read.string "card_resource_cost" |> ResourcePool.parseDtoFromJson
                PrimaryResource =  read.string "card_primary_resource"
                CardType =  read.string "card_type"
                EnterSpecialEffects =  read.stringOrNone "card_enter_special_effects" |> GameStateSpecialEffect.parseDtoFromJson
                ExitSpecialEffects =  read.stringOrNone "card_exit_special_effects" |> GameStateSpecialEffect.parseDtoFromJson
                CreatureHealth =  read.intOrNone "card_creature_health"
                Weaknesses = read.string "card_creature_weaknesses" |> Dto.Resource.parseListFromJson
                Attack =  read.string "card_creature_attacks" |> Attack.parseDtoFromJson
                ResourceAvailableOnFirstTurn =  read.boolOrNone "card_resources_available_on_first_turn"
                ResourcesAdded = read.string "card_resources_added"  |> ResourcePool.parseDtoFromJson

            }

    member __.GetAll () =
        async {
         let! query =
            connectionString
            |> Sql.connect
            |> Sql.query "SELECT * FROM card"
            |> Sql.executeAsync rowToDto
            |> Async.AwaitTask

         return query
            |> Seq.map Card.toDomain
            |> Seq.toList
            |> CollectionManipulation.selectAllOkayResults
            |> List.toSeq
        }

    member __.Get cardId =
        async {
         let! query =
            connectionString
            |> Sql.connect
            |> Sql.query "SELECT * FROM card WHERE card_id = @card_id"
            |> Sql.parameters [ "card_id", Sql.string cardId ]
            |> Sql.executeRowAsync rowToDto
            |> Async.AwaitTask

         return query |> Card.toDomain
        }