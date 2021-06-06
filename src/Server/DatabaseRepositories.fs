module DatabaseRepositories


open Npgsql.FSharp
open Dto

let connectionString = "Host=localhost; Database=postgres; Username=postgres; Password=trident1814;"

type PlayerRepository () =

    let rowToDto (read : RowReader) : PlayerDto = {
                    PlayerId = read.string "player_id"
                    Name = read.string "player_name"
                    PlaymatUrl = read.string "player_playmat_url"
                    LifePoints = 20
                }

    member __.GetAll () =
            connectionString
            |> Sql.connect
            |> Sql.query "SELECT * FROM player"
            |> Sql.execute rowToDto
            |> Seq.map Player.toDomain

    member __.Get playerId =
            connectionString
            |> Sql.connect
            |> Sql.query "SELECT * FROM player WHERE player_id = @player_id"
            |> Sql.parameters [ "player_id", Sql.string playerId ]
            |> Sql.executeRow rowToDto
            |> Player.toDomain

type CardRepository () =

    let rowToDto (read : RowReader) : CardDto =
            {

                CardId= read.string "card_id"
                Name = read.string "card_name"
                Description = read.string "card_description"
                ImageUrl =  read.string "card_image_url"
                ResourceCost =  read.string "card_resource_cost" |> ResourcePool.parseDtoFromJson
                PrimaryResource =  read.string "card_primary_resource"
                CardType =  read.string "card_type"
                EnterSpecialEffects =  read.stringOrNone "card_enter_special_effects" |> GameStateSpecialEffect.parseDtoFromJson
                ExitSpecialEffects =  read.stringOrNone "card_exit_special_effects" |> GameStateSpecialEffect.parseDtoFromJson
                CreatureHealth =  read.intOrNone "card_creature_health"
                Weaknesses = read.stringArray "card_creature_weaknesses"
                Attack =  read.string "card_creature_attacks" |> Attack.parseDtoFromJson
                ResourceAvailableOnFirstTurn =  read.boolOrNone "card_resources_available_on_first_turn"
                ResourcesAdded = read.string "card_resources_added"  |> ResourcePool.parseDtoFromJson

            }

    member __.GetAll () =
            connectionString
            |> Sql.connect
            |> Sql.query "SELECT * FROM card"
            |> Sql.execute rowToDto
            |> Seq.map Card.toDomain

    member __.Get cardId =
            connectionString
            |> Sql.connect
            |> Sql.query "SELECT * FROM card WHERE card_id = @card_id"
            |> Sql.parameters [ "card_id", Sql.string cardId ]
            |> Sql.executeRow rowToDto
            |> Card.toDomain