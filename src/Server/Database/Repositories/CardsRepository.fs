namespace Cards

open Database
open System.Threading.Tasks
open FSharp.Control.Tasks
open Npgsql

module Database =
  let getAll connectionString : Task<Result<CardDatabaseDto seq, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! query connection """
                    select
	                    card_id,
	                    card_name,
	                    card_description,
	                    card_image_url,
	                    card_thumbnail_image_url,
	                    card_primary_resource,
	                    card_type,
	                    card_enter_special_effects,
	                    card_exit_special_effects,
	                    card_creature_health,
	                    card_creature_weaknesses,
	                    card_creature_attacks,
	                    card_resources_available_on_first_turn,
	                    card_resources_added,
	                    card_resource_cost
                    from
	                    public.card;
                    """ None
    }

  let getById connectionString id : Task<Result<CardDatabaseDto option, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! querySingle connection """
                    select
	                    card_id,
	                    card_name,
	                    card_description,
	                    card_image_url,
	                    card_thumbnail_image_url,
	                    card_primary_resource,
	                    card_type,
	                    card_enter_special_effects,
	                    card_exit_special_effects,
	                    card_creature_health,
	                    card_creature_weaknesses,
	                    card_creature_attacks,
	                    card_resources_available_on_first_turn,
	                    card_resources_added,
	                    card_resource_cost
                    from
	                    public.card
                    where card_id = @id
                    """ (Some <| dict ["id" => id])
    }

  let update connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection """
                        update
	                        public.card
                        set
	                        card_name = @CardName
	                        card_description = @CardDescription,
	                        card_image_url = @CardImageUrl,
	                        card_thumbnail_image_url = @CardThumbnailImageUrl,
	                        card_primary_resource = @CardPrimaryResource,
	                        card_type = @CardType,
	                        card_enter_special_effects = @CardEntrySpecialEffects,
	                        card_exit_special_effects = @CardExitSpecialEffects,
	                        card_creature_health = @CardCreatureHealth,
	                        card_creature_weaknesses = @CardCreatureWeaknesses,
	                        card_creature_attacks = @CardCreatureAttacks,
	                        card_resources_available_on_first_turn = CardResourcesAvailableOnFirstTurn,
	                        card_resources_added = @CardResourcesAdded,
	                        card_resource_cost = @CardResourceCost
                        where
	                        card_id = @CardID;
                        """ v
    }

  let insert connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection """
                        insert
	                        into
	                        public.card (card_id,
	                        card_name,
	                        card_description,
	                        card_image_url,
	                        card_thumbnail_image_url,
	                        card_primary_resource,
	                        card_type,
	                        card_enter_special_effects,
	                        card_exit_special_effects,
	                        card_creature_health,
	                        card_creature_weaknesses,
	                        card_creature_attacks,
	                        card_resources_available_on_first_turn,
	                        card_resources_added,
	                        card_resource_cost)
                        values (@CardId,
                        @CardName,
                        @CardDescription,
                        @CardImageUrl,
                        @CardThumbnailImageUrl,
                        @CardPrimaryResource,
                        @CardType,
                        @CardEnterSpecialEffects,
                        @CardExitSpecialEffects,
                        @CardCreatureHealth,
                        @CardCreatureWeaknesses,
                        @CardCreatureAttacks,
                        @CardResourcesAvailableOnFirstTurn,
                        @CardResourcesAdded,
                        @CardResourceCost);
                        """ v
    }

  let delete connectionString id : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection "DELETE FROM Card WHERE CardId=@CardId" (dict ["id" => id])
    }

