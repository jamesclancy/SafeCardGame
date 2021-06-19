CREATE TABLE public.player (
	player_id varchar(256) NOT NULL,
	player_name varchar(256) NOT NULL,
	player_playmat_url varchar(500) NULL,
    player_life_points int NOT NULL,
    player_initial_health int NOT NULL,
    date_created timestamptz NOT NULL,
    last_login timestamptz NOT NULL,
	CONSTRAINT player_pk PRIMARY KEY (player_id),
	CONSTRAINT player_unique_name UNIQUE (player_name)
);
