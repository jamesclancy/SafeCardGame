module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Saturn
open Giraffe

open Shared
open DatabaseRepositories
open System
open FSharp.Control.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication


Dapper.DefaultTypeMap.MatchNamesWithUnderscores <- true;

let playerRepository = PlayerRepository()
let cardRepository = CardRepository()
let deckRepository = DeckRepository()

let gameApi ctx  : ICardGameApi =
    {
        getPlayers = fun () ->  playerRepository.GetAll()
        getPlayer = fun playerId ->  playerRepository.Get(playerId)
        getCards = fun () -> cardRepository.GetAll()
        getCard = fun cardId -> cardRepository.Get(cardId)
        getDecks = fun () -> deckRepository.GetAll()
        getCardsForDeck = fun deckId -> deckRepository.GetCardsForDeck(deckId)
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

let routes =
    choose [
        route "/" >=> (authChallenge >=>  htmlFile "public/app.html")
        route "/signout" >=> signOut >=>   htmlView (App.signedOut ())
        subRoute "/api" (authChallenge >=> buildRemotingApi gameApi)
        subRoute "/player"  (authChallenge >=> UserManagementController.resource)
        subRoute "/game"  (authChallenge >=> Games.Controller.resource)
        subRoute "/deck"  (authChallenge >=> Decks.Controller.resource)
        subRoute "/card"  (authChallenge >=> Cards.Controller.resource)
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
        memory_cache
        use_static "public"
        use_gzip
        service_config configureGitHubAuth
    }

run app
