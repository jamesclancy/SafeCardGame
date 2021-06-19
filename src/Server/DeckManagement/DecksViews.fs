namespace Decks

open Microsoft.AspNetCore.Http
open Giraffe.ViewEngine
open Dto
open Saturn

module Views =
  let index (ctx : HttpContext) (objs : DeckDto list) =
    let conf = Config.getConfigFromContext ctx
    let cnt = [
      div [_class "container "] [
        h2 [ _class "title"] [encodedText "Listing Decks"]

        table [_class "table is-hoverable is-fullwidth"] [
          thead [] [
            tr [] [
              th [] [encodedText "DeckId"]
              th [] [encodedText "DeckName"]
              th [] [encodedText "DeckDescription"]
              th [] [encodedText "DeckImageUrl"]
              th [] [encodedText "DeckThumbnailUrl"]
              th [] [encodedText "DeckPrimaryResource"]
              th [] [encodedText "DeckOwner"]
              th [] [encodedText "DeckPrivate"]
              th [] []
            ]
          ]
          tbody [] [
            for o in objs do
              yield tr [] [
                td [] [encodedText (string o.DeckId)]
                td [] [encodedText (string o.DeckName)]
                td [] [encodedText (string o.DeckDescription)]
                td [] [encodedText (string o.DeckImageUrl)]
                td [] [encodedText (string o.DeckThumbnailImageUrl)]
                td [] [encodedText (string o.DeckPrimaryResource)]
                td [] [encodedText (string o.DeckOwner)]
                td [] [encodedText (string o.DeckPrivate)]
                td [] [
                  a [_class "button is-text"; _href (Links.withId ctx o.DeckId )] [encodedText "Show"]
                  a [_class "button is-text"; _href (Links.edit ctx o.DeckId )] [encodedText "Edit"]
                  a [_class "button is-text is-delete"; attr "data-href" (Links.withId ctx o.DeckId ) ] [encodedText "Delete"]
                ]
              ]
          ]
        ]

        a [_class "button is-text"; _href (Links.add ctx )] [encodedText "New Deck"]
      ]
    ]
    App.layout  conf.currentPlayer ([section [_class "section"] cnt])


  let show (ctx : HttpContext) (o : DeckDto) =
    let conf = Config.getConfigFromContext ctx
    let cnt = [
      div [_class "container "] [
        h2 [ _class "title"] [encodedText "Show Deck"]

        ul [] [
          li [] [ strong [] [encodedText "DeckId: "]; encodedText (string o.DeckId) ]
          li [] [ strong [] [encodedText "DeckName: "]; encodedText (string o.DeckName) ]
          li [] [ strong [] [encodedText "DeckDescription: "]; encodedText (string o.DeckDescription) ]
          li [] [ strong [] [encodedText "DeckImageUrl: "]; encodedText (string o.DeckImageUrl) ]
          li [] [ strong [] [encodedText "DeckThumbnailUr: "]; encodedText (string o.DeckThumbnailImageUrl) ]
          li [] [ strong [] [encodedText "DeckPrimaryResource: "]; encodedText (string o.DeckPrimaryResource) ]
          li [] [ strong [] [encodedText "DeckOwner: "]; encodedText (string o.DeckOwner) ]
          li [] [ strong [] [encodedText "DeckPrivate: "]; encodedText (string o.DeckPrivate) ]
        ]
        a [_class "button is-text"; _href (Links.edit ctx o.DeckId)] [encodedText "Edit"]
        a [_class "button is-text"; _href (Links.index ctx )] [encodedText "Back"]
      ]
    ]
    App.layout  conf.currentPlayer ([section [_class "section"] cnt])

  let private form (ctx: HttpContext) (o: DeckDto option) (validationResult : Map<string, string>) isUpdate =
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
        form [ _action (if isUpdate then Links.withId ctx o.Value.DeckId else Links.index ctx ); _method "post"] [
          if not validationResult.IsEmpty then
            yield validationMessage
          yield field (fun i -> (string i.DeckId)) "DeckId" "DeckId"
          yield field (fun i -> (string i.DeckName)) "DeckName" "DeckName"
          yield field (fun i -> (string i.DeckDescription)) "DeckDescription" "DeckDescription"
          yield field (fun i -> (string i.DeckImageUrl)) "DeckImageUrl" "DeckImageUrl"
          yield field (fun i -> (string i.DeckThumbnailImageUrl)) "DeckThumbnailUr" "DeckThumbnailUr"
          yield field (fun i -> (string i.DeckPrimaryResource)) "DeckPrimaryResource" "DeckPrimaryResource"
          yield field (fun i -> (string i.DeckOwner)) "DeckOwner" "DeckOwner"
          yield field (fun i -> (string i.DeckPrivate)) "DeckPrivate" "DeckPrivate"
          yield buttons
        ]
      ]
    ]
    App.layout conf.currentPlayer ([section [_class "section"] cnt])

  let add (ctx: HttpContext) (o: DeckDto option) (validationResult : Map<string, string>)=
    form ctx o validationResult false

  let edit (ctx: HttpContext) (o: DeckDto) (validationResult : Map<string, string>) =
    form ctx (Some o) validationResult true
