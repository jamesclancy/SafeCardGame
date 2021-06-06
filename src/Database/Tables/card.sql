CREATE TABLE public.card (
	card_id varchar(256) NOT NULL,
	card_name varchar(256) NOT NULL,
	card_description varchar(500) NULL,
	card_image_url varchar(500) NOT NULL,
	card_thumbnail_image_url varchar(500) NOT NULL,
	card_primary_resource varchar(256) NOT NULL,
    card_type varchar(256) NOT NULL,
    card_enter_special_effects varchar NULL,
    card_exit_special_effects varchar NULL,
    card_creature_health int NULL,
    card_creature_weaknesses varchar NULL,
    card_creature_attacks varchar NULL,
    card_resources_available_on_first_turn bit NULL,
    card_resources_added  varchar NULL,
	CONSTRAINT card_pk PRIMARY KEY (card_id),
	CONSTRAINT card_unique_name UNIQUE (card_name)
);


