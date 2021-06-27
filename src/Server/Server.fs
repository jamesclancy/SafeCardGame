module Server

open System
open System
open System
open Dto
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Players
open Saturn
open Giraffe

open Shared
open DatabaseRepositories
open System
open FSharp.Control.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication
open Events
open Elmish.Bridge

open Microsoft.Extensions.Logging
open Shared.Domain
open SocketServer

Dapper.DefaultTypeMap.MatchNamesWithUnderscores <- true;

let playerRepository = PlayerRepository()
let cardRepository = CardRepository()
let deckRepository = DeckRepository()

let getCurrentPlayer ctx =
    async {
        return (
            Config.getConfigFromContext ctx
            |> (fun x-> x.currentPlayer)
            |> (fun x->
                    match x with
                    | Some p -> p |> Ok
                    | None -> "Not Logged In" |> Error))
    }

let constructGameFromModelAndDatabaseInfo connectionString (model: GetOrCreateGameRequest)  (playerRes: Result<Player option, exn>) (gameRes : Result<GameDto option, exn>) =
    result {

        let! gameId = match NonEmptyString.build model.GameId with
                      | Ok  g -> g|> GameId |> Ok
                      | Error r -> new System.Exception(r) |> Error

        let! player = playerRes
        let! game = gameRes


            // Game already exists first check if the player is a part of that game,
            // if so return the game. If they are not already in the game can they join?
            // if so add them
        match player, game with
        | Some p, Some g
            when (g.Player1Id = p.PlayerId.ToString() || g.Player2Id = p.PlayerId.ToString()) ->
                return gameId, g, p
        | Some p, Some g
            when (g.Player1Id <> p.PlayerId.ToString() && String.IsNullOrWhiteSpace g.Player2Id) ->
                match ((Games.Database.attachPlayerAsPlayer2 connectionString g p gameId) |> Async.RunSynchronously) with
                | Ok (Some finalGame) ->
                    return gameId, finalGame, p
                | Error e -> return! (e |> Error)
                | _ -> return! (Exception("Unable to attach player to game. Please try again") |> Error)
        | Some p, None ->
            match ((Games.Database.createGameFromSinglePlayerAndDeck connectionString gameId p) |> Async.RunSynchronously) with
            | Ok (Some finalGame) ->
                    return gameId, finalGame, p
            | Ok None ->
                    return! (Exception "game dto not loaded" |> Error)
            | Error e -> return! (e |> Error)
        | _, _ -> return!  (Exception "Unable to create game" |> Error)


    }

let gameStateFromDtoOrEmpty (gameDto) =
      if String.IsNullOrWhiteSpace gameDto then
          None
      else
          match Thoth.Json.Net.Decode.Auto.fromString<GameState>(gameDto) with
          | Ok g -> Some g
          | Error e -> None //failwith ("Oh noe!" + e)

let getOrCreateGame (ctx : HttpContext) (model: GetOrCreateGameRequest)  (getDecks : unit -> Async<DeckDto seq>) (getCardsForDeck : string ->  Async<Card seq>)
        : Async<Result<GameDto * ClientInternalMsg * Player * GameId, string>>
    =
        async {
          let conf = Config.getConfigFromContext ctx

          let! player = (Players.Database.getOrCreatePlayer conf.connectionString model.PlayerId model.PlayerId)
          let! game = (Games.Database.getById conf.connectionString model.GameId)

          let gameResult = constructGameFromModelAndDatabaseInfo conf.connectionString model player game

          let! deck1 = GameSetup.testDeckSeqGenerator getDecks getCardsForDeck 60
          let! deck2 = GameSetup.testDeckSeqGenerator  getDecks getCardsForDeck 60

          match gameResult with
          | Ok (gameId, gameDto, player)
                when (String.IsNullOrWhiteSpace gameDto.Player2Id || String.IsNullOrWhiteSpace gameDto.Player2Id) ->
                    // still waiting for another player...
                    return (gameDto, {
                        GameId =  gameId
                        Winner = None
                        Message = None
                    } |> GameWon |> CommandToServer, player, gameId) |> Ok
          | Ok (gameId, gameDto, player) ->
              let! player1 = Players.Database.getById conf.connectionString gameDto.Player1Id
              let! player2 = Players.Database.getById conf.connectionString gameDto.Player2Id
              match player1, player2 with
                  | Ok (Some p1), Ok (Some p2) ->
                       return (gameDto, {
                            GameId = gameId
                            Players =  [
                                        p1.PlayerId, p1;
                                        p2.PlayerId, p2
                                       ] |> Map.ofList
                            PlayerOne = p1.PlayerId
                            PlayerTwo = p2.PlayerId
                            Decks = [   (p1.PlayerId, { TopCardsExposed = 0; Cards = deck1 });
                                        (p2.PlayerId, { TopCardsExposed = 0; Cards = deck2 })]
                                     |> Map.ofSeq
                         } |> StartGame |> CommandToServer, player, gameId) |> Ok
                  | _,_ ->
                      return Error "Unable to load players"
          | Error e -> return Error ("Error creating game instance. Try a different Game Id." + e.Message)
    }

let gameApi ctx  : ICardGameApi =
    {
        getCurrentLoggedInPlayer = fun () -> getCurrentPlayer ctx
        getPlayers = fun () ->  playerRepository.GetAll()
        getPlayer = fun playerId ->  playerRepository.Get(playerId)
        getCards = fun () -> cardRepository.GetAll()
        getCard = fun cardId -> cardRepository.Get(cardId)
        getDecks = fun () -> deckRepository.GetAll()
        getCardsForDeck = fun deckId -> deckRepository.GetCardsForDeck(deckId)
        getOrCreateGame = fun req ->
            async {
                     try
                        let! res = getOrCreateGame ctx req deckRepository.GetAll deckRepository.GetCardsForDeck
                        match res with
                        | Ok (a, b, c, d) ->
                            return (gameStateFromDtoOrEmpty a.GameState, b, d, c.PlayerId) |> Ok
                        | Error e -> return e |> Error
                     with e ->
                        return e.Message |> Error
                }
        }



let buildRemotingApi api next ctx = task {
    let handler =
        Remoting.createApi()
        |> Remoting.withRouteBuilder ApiSubRoute.builder
        |> Remoting.fromValue (api ctx)
        |> Remoting.buildHttpHandler
    return! handler next ctx }

let isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") = "Development";

let noAuthenticationRequired nxt ctx = task { return! nxt ctx }

let authChallenge : HttpFunc -> HttpContext -> HttpFuncResult =
    requiresAuthentication (Auth.challenge "GitHub")


let signOut (next : HttpFunc) (ctx : HttpContext) =
  task {
    do! ctx.SignOutAsync()
    return! next ctx
  }

let setUpWebSockets (next : HttpFunc) (ctx : HttpContext)  =
   task {
         let hub = ctx.GetService<Saturn.Channels.ISocketHub>()
         match ctx.TryGetQueryStringValue "message" with
         | None ->
           do! hub.SendMessageToClients "/channel" "greeting" "hello"
         | Some message ->
           do! hub.SendMessageToClients "/channel" "greeting" (sprintf "hello, %s" message)
         return! Successful.ok (text "Pinged the clients") next ctx
    }


let routes =
    choose [
        route "/" >=> (authChallenge >=>  htmlFile "public/app.html")
        route "/signout" >=> signOut >=>   htmlView (App.signedOut ())
        subRoute "/api" (authChallenge >=> buildRemotingApi gameApi)
        subRoute "/player"  (authChallenge >=> UserManagementController.resource)
        subRoute "/game"  (authChallenge >=> Games.Controller.resource)
        subRoute "/deck"  (authChallenge >=> Decks.Controller.resource)
        subRoute "/card"  (authChallenge >=> Cards.Controller.resource)
        route "/socket" >=> socketServerRouter
        //subRoute "/channel" setUpWebSockets
        //bridgeServer
    ]


type UserSecretsTarget = UserSecretsTarget of unit
let configureHost (hostBuilder : IHostBuilder) =
    hostBuilder.ConfigureAppConfiguration(fun ctx cfg ->

        if ctx.HostingEnvironment.IsDevelopment() then
            cfg.AddUserSecrets<UserSecretsTarget>() |> ignore

    ) |> ignore
    hostBuilder


let configureGitHubAuth (services : IServiceCollection) =

        let config = services.BuildServiceProvider().GetService<IConfiguration>()

        let c = services.AddAuthentication(fun cfg ->
          cfg.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
          cfg.DefaultSignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
          cfg.DefaultChallengeScheme <- "GitHub").AddCookie()
        c.AddOAuth("GitHub", fun (opt : Authentication.OAuth.OAuthOptions) ->
          opt.ClientId <- config.GetValue<string>("GitHubOAuthClientId")
          opt.ClientSecret <- config.GetValue<string>("GitHubOAuthKey")
          opt.CallbackPath <- PathString "/signin-github"
          opt.AuthorizationEndpoint <-  "https://github.com/login/oauth/authorize"
          opt.TokenEndpoint <- "https://github.com/login/oauth/access_token"
          opt.UserInformationEndpoint <- "https://api.github.com/user"
          [("login", "playerId"); ("name", "playerName")] |> Seq.iter (fun (k,v) -> opt.ClaimActions.MapJsonKey(v,k) )
          let ev = opt.Events

          ev.OnCreatingTicket <- Func<_,_> Application.parseAndValidateOauthTicket

         ) |> ignore

        services


let addAuth (app:IApplicationBuilder) =
    app.UseAuthentication()



let app =
    application {
        url "https://localhost:8085"
        host_config configureHost
        app_config addAuth
        use_router routes
        app_config Giraffe.useWebSockets
//        add_channel "/channel" Channel.channel
        memory_cache
        use_static "public"
        use_gzip
        service_config configureGitHubAuth
    }

run app
