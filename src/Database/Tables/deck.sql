CREATE TABLE public.deck (
	deck_id varchar(256) NOT NULL,
	deck_name varchar(256) NOT NULL,
	deck_description varchar(500) NULL,
	deck_image_url varchar(500) NULL,
	deck_thumbnail_image_url varchar(500) NULL,
	deck_primary_resource varchar(256) NOT NULL,
    deck_owner varchar(256) NULL,
    deck_private boolean NOT NULL,
	CONSTRAINT deck_pk PRIMARY KEY (deck_id),
	CONSTRAINT deck_unique_name UNIQUE (deck_name),
    CONSTRAINT deck_fk_owner FOREIGN KEY(deck_owner) REFERENCES player(player_id)
);
