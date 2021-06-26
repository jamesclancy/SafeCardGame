create table public.game_state_transaction (
game_state_transaction_id SERIAL,
game_id varchar(256) not null,
game_current_step varchar(256) not null,
game_current_player_move varchar(256) null,
game_winner varchar(256) null,
game_notes varchar(256) not null,
game_state varchar null,
game_state_transaction_time timestamptz not null,
constraint game_state_transaction_pk primary key (game_state_transaction_id),
constraint game_player_1_id_fk_player foreign key(game_id) references game(game_id),
constraint game_state_transaction_current_player_move_fk_player foreign key(game_current_player_move) references player(player_id) );