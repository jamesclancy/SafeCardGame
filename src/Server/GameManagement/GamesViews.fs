namespace Games

open Microsoft.AspNetCore.Http
open Giraffe.ViewEngine
open Microsoft.Extensions.Configuration
open Saturn
open Dto

module Views =

  let index (ctx : HttpContext) (objs : GameDto list) =
    let conf = Config.getConfigFromContext ctx
    let cnt = [
      div [_class "container "] [
        h2 [ _class "title"] [encodedText "Listing Games"]

        table [_class "table is-hoverable is-fullwidth"] [
          thead [] [
            tr [] [
              th [] [encodedText "GameId"]
              th [] [encodedText "Player1Id"]
              th [] [encodedText "Player2Id"]
              th [] [encodedText "CurrentStep"]
              th [] [encodedText "CurrentPlayerMove"]
              th [] [encodedText "Winner"]
              th [] [encodedText "Notes"]
              th [] [encodedText "InProgress"]
              th [] [encodedText "DateStarted"]
              th [] [encodedText "LastMovement"]
              th [] []
            ]
          ]
          tbody [] [
            for o in objs do
              yield tr [] [
                td [] [encodedText (string o.GameId)]
                td [] [encodedText (string o.Player1Id)]
                td [] [encodedText (string o.Player2Id)]
                td [] [encodedText (string o.CurrentStep)]
                td [] [encodedText (string o.CurrentPlayerMove)]
                td [] [encodedText (string o.Winner)]
                td [] [encodedText (string o.Notes)]
                td [] [encodedText (string o.InProgress)]
                td [] [encodedText (string o.DateStarted)]
                td [] [encodedText (string o.LastMovement)]
                td [] [
                  a [_class "button is-text"; _href (Links.withId ctx o.GameId )] [encodedText "Show"]
                  a [_class "button is-text"; _href (Links.edit ctx o.GameId )] [encodedText "Edit"]
                  a [_class "button is-text is-delete"; attr "data-href" (Links.withId ctx o.GameId ) ] [encodedText "Delete"]
                ]
              ]
          ]
        ]

        a [_class "button is-text"; _href (Links.add ctx )] [encodedText "New Game"]
      ]
    ]
    App.layout conf.currentPlayer ([section [_class "section"] cnt])


  let show (ctx : HttpContext) (o : GameDto) =
    let conf = Config.getConfigFromContext ctx
    let cnt = [
      div [_class "container "] [
        h2 [ _class "title"] [encodedText "Show Game"]

        ul [] [
          li [] [ strong [] [encodedText "GameId: "]; encodedText (string o.GameId) ]
          li [] [ strong [] [encodedText "Player1Id: "]; encodedText (string o.Player1Id) ]
          li [] [ strong [] [encodedText "Player2Id: "]; encodedText (string o.Player2Id) ]
          li [] [ strong [] [encodedText "CurrentStep: "]; encodedText (string o.CurrentStep) ]
          li [] [ strong [] [encodedText "CurrentPlayerMove: "]; encodedText (string o.CurrentPlayerMove) ]
          li [] [ strong [] [encodedText "Winner: "]; encodedText (string o.Winner) ]
          li [] [ strong [] [encodedText "Notes: "]; encodedText (string o.Notes) ]
          li [] [ strong [] [encodedText "InProgress: "]; encodedText (string o.InProgress) ]
          li [] [ strong [] [encodedText "DateStarted: "]; encodedText (string o.DateStarted) ]
          li [] [ strong [] [encodedText "LastMovement: "]; encodedText (string o.LastMovement) ]
        ]
        a [_class "button is-text"; _href (Links.edit ctx o.GameId)] [encodedText "Edit"]
        a [_class "button is-text"; _href (Links.index ctx )] [encodedText "Back"]
      ]
    ]
    App.layout conf.currentPlayer ([section [_class "section"] cnt])

  let private form (ctx: HttpContext) (o: GameDto option) (validationResult : Map<string, string>) isUpdate =
    let conf = Config.getConfigFromContext ctx
    let validationMessage =
      div [_class "notification is-danger"] [
        a [_class "delete"; attr "aria-label" "delete"] []
        encodedText "Oops, something went wrong! Please check the errors below."
      ]

    let field selector lbl key =
      div [_class "field"] [
        yield label [_class "label"] [encodedText (string lbl)]
        yield div [_class "control has-icons-right"] [
          yield input [_class (if validationResult.ContainsKey key then "input is-danger" else "input"); _value (defaultArg (o |> Option.map selector) ""); _name key ; _type "text" ]
          if validationResult.ContainsKey key then
            yield span [_class "icon is-small is-right"] [
              i [_class "fas fa-exclamation-triangle"] []
            ]
        ]
        if validationResult.ContainsKey key then
          yield p [_class "help is-danger"] [encodedText validationResult.[key]]
      ]

    let buttons =
      div [_class "field is-grouped"] [
        div [_class "control"] [
          input [_type "submit"; _class "button is-link"; _value "Submit"]
        ]
        div [_class "control"] [
          a [_class "button is-text"; _href (Links.index ctx)] [encodedText "Cancel"]
        ]
      ]

    let cnt = [
      div [_class "container "] [
        form [ _action (if isUpdate then Links.withId ctx o.Value.GameId else Links.index ctx ); _method "post"] [
          if not validationResult.IsEmpty then
            yield validationMessage
          yield field (fun i -> (string i.GameId)) "GameId" "GameId"
          yield field (fun i -> (string i.Player1Id)) "Player1Id" "Player1Id"
          yield field (fun i -> (string i.Player2Id)) "Player2Id" "Player2Id"
          yield field (fun i -> (string i.CurrentStep)) "CurrentStep" "CurrentStep"
          yield field (fun i -> (string i.CurrentPlayerMove)) "CurrentPlayerMove" "CurrentPlayerMove"
          yield field (fun i -> (string i.Winner)) "Winner" "Winner"
          yield field (fun i -> (string i.Notes)) "Notes" "Notes"
          yield field (fun i -> (string i.InProgress)) "InProgress" "InProgress"
          yield field (fun i -> (string i.DateStarted)) "DateStarted" "DateStarted"
          yield field (fun i -> (string i.LastMovement)) "LastMovement" "LastMovement"
          yield buttons
        ]
      ]
    ]
    App.layout  conf.currentPlayer ([section [_class "section"] cnt])

  let add (ctx: HttpContext) (o: GameDto option) (validationResult : Map<string, string>)=
    form ctx o validationResult false

  let edit (ctx: HttpContext) (o: GameDto) (validationResult : Map<string, string>) =
    form ctx (Some o) validationResult true
