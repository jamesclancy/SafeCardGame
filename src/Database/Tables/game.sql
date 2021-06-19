create table public.game (
game_id varchar(256) not null,
game_player_1_id varchar(256) not null,
game_player_2_id varchar(256) not null,
game_current_step varchar(256) not null,
game_current_player_move varchar(256) null,
game_winner varchar(256) null,
game_notes varchar(256) not null,
game_in_progress boolean not null,
game_date_started timestamptz not null,
game_last_movement timestamptz not null,
constraint game_pk primary key (game_id),
constraint game_player_1_id_fk_player foreign key(game_player_1_id) references player(player_id),
constraint game_player_2_id_fk_player foreign key(game_player_2_id) references player(player_id),
constraint game_current_player_move_fk_player foreign key(game_current_player_move) references player(player_id)
);

