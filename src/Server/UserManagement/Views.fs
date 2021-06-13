module UserManagementViews

  open Dto
  open Dto
  open Microsoft.AspNetCore.Http
  open Giraffe.ViewEngine
  open Saturn
  open Shared.Domain

  let index (ctx : HttpContext) (objs : PlayerDto list) =
    let cnt = [
      div [_class "container "] [
        h2 [ _class "title"] [encodedText "Listing Users"]

        table [_class "table is-hoverable is-fullwidth"] [
          thead [] [
            tr [] [
              th [] [encodedText "PlayerId"]
              th [] [encodedText "PlayerName"]
              th [] [encodedText "PlaymatUrl"]
              th [] [encodedText "StartingHealth"]
              th [] [encodedText "DateCreated"]
              th [] [encodedText "LastLogin"]
              th [] []
            ]
          ]
          tbody [] [
            for o in objs do
              yield tr [] [
                td [] [encodedText (string o.PlayerId)]
                td [] [encodedText (string o.Name)]
                td [] [encodedText (string o.PlaymatUrl)]
                td [] [encodedText (string o.InitialHealth)]
                td [] [encodedText (string o.DateCreated)]
                td [] [encodedText (string o.LastLogin)]
                td [] [
                  a [_class "button is-text"; _href (Links.withId ctx o.PlayerId )] [encodedText "Show"]
                  a [_class "button is-text"; _href (Links.edit ctx o.PlayerId )] [encodedText "Edit"]
                  a [_class "button is-text is-delete"; attr "data-href" (Links.withId ctx o.PlayerId ) ] [encodedText "Delete"]
                ]
              ]
          ]
        ]

        a [_class "button is-text"; _href (Links.add ctx )] [encodedText "New User"]
      ]
    ]
    App.layout ([section [_class "section"] cnt])


  let show (ctx : HttpContext) (o : PlayerDto) =
    let cnt = [
      div [_class "container "] [
        h2 [ _class "title"] [encodedText "Show User"]

        ul [] [
          li [] [ strong [] [encodedText "PlayerId: "]; encodedText (string o.PlayerId) ]
          li [] [ strong [] [encodedText "Name: "]; encodedText (string o.Name) ]
          li [] [ strong [] [encodedText "PlaymatUrl: "]; encodedText (string o.PlaymatUrl) ]
          li [] [ strong [] [encodedText "StartingHealth: "]; encodedText (string o.InitialHealth) ]
          li [] [ strong [] [encodedText "DateCreated: "]; encodedText (string o.DateCreated) ]
          li [] [ strong [] [encodedText "LastLogin: "]; encodedText (string o.LastLogin) ]
        ]
        a [_class "button is-text"; _href (Links.edit ctx o.PlayerId)] [encodedText "Edit"]
        a [_class "button is-text"; _href (Links.index ctx )] [encodedText "Back"]
      ]
    ]
    App.layout ([section [_class "section"] cnt])

  let private form (ctx: HttpContext) (o: PlayerDto option) (validationResult : Map<string, string>) isUpdate =
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
        form [ _action (if isUpdate then Links.withId ctx o.Value.PlayerId else Links.index ctx ); _method "post"] [
          if not validationResult.IsEmpty then
            yield validationMessage
          yield field (fun i -> (string i.PlayerId)) "PlayerId" "PlayerId"
          yield field (fun i -> (string i.Name)) "PlayerName" "PlayerName"
          yield field (fun i -> (string i.PlaymatUrl)) "PlaymatUrl" "PlaymatUrl"
          yield field (fun i -> (string i.InitialHealth)) "StartingHealth" "StartingHealth"
          yield field (fun i -> (string i.DateCreated)) "DateCreated" "DateCreated"
          yield field (fun i -> (string i.LastLogin)) "LastLogin" "LastLogin"
          yield buttons
        ]
      ]
    ]
    App.layout ([section [_class "section"] cnt])

  let add (ctx: HttpContext) (o: PlayerDto option) (validationResult : Map<string, string>)=
    form ctx o validationResult false

  let edit (ctx: HttpContext) (o: PlayerDto) (validationResult : Map<string, string>) =
    form ctx (Some o) validationResult true
