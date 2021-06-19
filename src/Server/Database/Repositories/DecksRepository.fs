namespace Decks

open Database
open System.Threading.Tasks
open Dto
open FSharp.Control.Tasks
open Npgsql

module Database =
  let getAll connectionString : Task<Result<DeckDto seq, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! query connection """
                        select
	                        deck_id DeckId,
	                        deck_name DeckName,
	                        deck_description DeckDescription,
	                        deck_image_url DeckImageUrl,
	                        deck_thumbnail_image_url DeckThumbnailImageUrl,
	                        deck_primary_resource DeckPrimaryResource,
	                        deck_owner DeckOwner,
	                        deck_private DeckPrivate
                        from
	                        deck""" None
    }

  let getById connectionString id : Task<Result<DeckDto option, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! querySingle connection """
                        select
	                        deck_id DeckId,
	                        deck_name DeckName,
	                        deck_description DeckDescription,
	                        deck_image_url DeckImageUrl,
	                        deck_thumbnail_image_url DeckThumbnailImageUrl,
	                        deck_primary_resource DeckPrimaryResource,
	                        deck_owner DeckOwner,
	                        deck_private DeckPrivate
                        from
	                        deck
                        where deck_id = @id""" (Some <| dict ["id" => id])
    }

  let update connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection """
                    update
	                    Deck
                    set
	                    Deck_Id = @DeckId,
	                    Deck_Name = @DeckName,
	                    Deck_Description = @DeckDescription,
	                    Deck_Image_Url = @DeckImageUrl,
	                    Deck_Thumbnail_Url = @DeckThumbnailImageUrl,
	                    Deck_Primary_Resource = @DeckPrimaryResource,
	                    Deck_Owner = @DeckOwner,
	                    Deck_Private = @DeckPrivate
                    where
	                    Deck_Id = @DeckId""" v
    }

  let insert connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection """
                      insert
	                        into
	                        Deck(Deck_Id,
	                        Deck_Name,
	                        Deck_Description,
	                        Deck_Image_Url,
	                        deck_thumbnail_image_url,
	                        deck_primary_resource ,
	                        deck_owner ,
	                        deck_private)
                        values (@DeckId,
                        @DeckName,
                        @DeckDescription,
                        @DeckImageUrl,
                        @DeckThumbnailImageUrl,
                        @DeckPrimaryResource,
                        @DeckOwner,
                        @DeckPrivate)""" v
    }

  let delete connectionString id : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection "DELETE FROM Deck WHERE Deck_Id=@id" (dict ["id" => id])
    }

