CREATE TABLE public.player (
	player_id varchar(256) NOT NULL,
	player_name varchar(256) NOT NULL,
	player_playmat_url varchar(500) NULL,
	CONSTRAINT player_pk PRIMARY KEY (player_id),
	CONSTRAINT player_unique_name UNIQUE (player_name)
);
