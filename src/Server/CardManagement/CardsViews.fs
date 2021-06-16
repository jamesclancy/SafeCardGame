namespace Cards

open Microsoft.AspNetCore.Http
open Giraffe.ViewEngine
open Saturn

module Views =
  let index (ctx : HttpContext) (objs : CardDatabaseDto list) =
    let conf = Config.getConfigFromContext ctx
    let cnt = [
      div [_class "container "] [
        h2 [ _class "title"] [encodedText "Listing Cards"]

        table [_class "table is-hoverable is-fullwidth"] [
          thead [] [
            tr [] [
              th [] [encodedText "CardId"]
              th [] [encodedText "CardName"]
              th [] [encodedText "CardThumbnailImageUrl"]
              th [] [encodedText "CardPrimaryResource"]
              th [] [encodedText "CardType"]
              th [] [encodedText "CardResourceCost"]
              th [] []
            ]
          ]
          tbody [] [
            for o in objs do
              yield tr [] [
                td [] [encodedText (string o.CardId)]
                td [] [encodedText (string o.CardName)]
                td [] [img [_src o.CardThumbnailImageUrl]]
                td [] [encodedText (string o.CardPrimaryResource)]
                td [] [encodedText (string o.CardType)]
                td [] [encodedText (string o.CardResourceCost)]
                td [] [
                  a [_class "button is-text"; _href (Links.withId ctx o.CardId )] [encodedText "Show"]
                  a [_class "button is-text"; _href (Links.edit ctx o.CardId )] [encodedText "Edit"]
                  a [_class "button is-text is-delete"; attr "data-href" (Links.withId ctx o.CardId ) ] [encodedText "Delete"]
                ]
              ]
          ]
        ]

        a [_class "button is-text"; _href (Links.add ctx )] [encodedText "New Card"]
      ]
    ]
    App.layout conf.currentPlayer  ([section [_class "section"] cnt])


  let show (ctx : HttpContext) (o : CardDatabaseDto) =
    let conf = Config.getConfigFromContext ctx
    let cnt = [
      div [_class "container "] [
        h2 [ _class "title"] [encodedText "Show Card"]

        ul [] [
          li [] [ strong [] [encodedText "CardId: "]; encodedText (string o.CardId) ]
          li [] [ strong [] [encodedText "CardName: "]; encodedText (string o.CardName) ]
          li [] [ strong [] [encodedText "CardDescription: "]; encodedText (string o.CardDescription) ]
          li [] [ strong [] [encodedText "CardImageUrl: "]; encodedText (string o.CardImageUrl) ]
          li [] [ strong [] [encodedText "CardThumbnailImageUrl: "]; encodedText (string o.CardThumbnailImageUrl) ]
          li [] [ strong [] [encodedText "CardPrimaryResource: "]; encodedText (string o.CardPrimaryResource) ]
          li [] [ strong [] [encodedText "CardType: "]; encodedText (string o.CardType) ]
          li [] [ strong [] [encodedText "CardEnterSpecialEffects: "]; encodedText (string o.CardEnterSpecialEffects) ]
          li [] [ strong [] [encodedText "CardExitSpecialEffects: "]; encodedText (string o.CardExitSpecialEffects) ]
          li [] [ strong [] [encodedText "CardCreatureHealth: "]; encodedText (string o.CardCreatureHealth) ]
          li [] [ strong [] [encodedText "CardCreatureWeaknesses: "]; encodedText (string o.CardCreatureWeaknesses) ]
          li [] [ strong [] [encodedText "CardCreatureAttacks: "]; encodedText (string o.CardCreatureAttacks) ]
          li [] [ strong [] [encodedText "CardResourcesAvailableOnFirstTurn: "]; encodedText (string o.CardResourcesAvailableOnFirstTurn) ]
          li [] [ strong [] [encodedText "CardResourcesAdded: "]; encodedText (string o.CardResourcesAdded) ]
          li [] [ strong [] [encodedText "CardResourceCost: "]; encodedText (string o.CardResourceCost) ]
        ]
        a [_class "button is-text"; _href (Links.edit ctx o.CardId)] [encodedText "Edit"]
        a [_class "button is-text"; _href (Links.index ctx )] [encodedText "Back"]
      ]
    ]
    App.layout conf.currentPlayer  ([section [_class "section"] cnt])

  let private form (ctx: HttpContext) (o: CardDatabaseDto option) (validationResult : Map<string, string>) isUpdate =
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
        form [ _action (if isUpdate then Links.withId ctx o.Value.CardId else Links.index ctx ); _method "post"] [
          if not validationResult.IsEmpty then
            yield validationMessage
          yield field (fun i -> (string i.CardId)) "CardId" "CardId"
          yield field (fun i -> (string i.CardName)) "CardName" "CardName"
          yield field (fun i -> (string i.CardDescription)) "CardDescription" "CardDescription"
          yield field (fun i -> (string i.CardImageUrl)) "CardImageUrl" "CardImageUrl"
          yield field (fun i -> (string i.CardThumbnailImageUrl)) "CardThumbnailImageUrl" "CardThumbnailImageUrl"
          yield field (fun i -> (string i.CardPrimaryResource)) "CardPrimaryResource" "CardPrimaryResource"
          yield field (fun i -> (string i.CardType)) "CardType" "CardType"
          yield field (fun i -> (string i.CardEnterSpecialEffects)) "CardEnterSpecialEffects" "CardEnterSpecialEffects"
          yield field (fun i -> (string i.CardExitSpecialEffects)) "CardExitSpecialEffects" "CardExitSpecialEffects"
          yield field (fun i -> (string i.CardCreatureHealth)) "CardCreatureHealth" "CardCreatureHealth"
          yield field (fun i -> (string i.CardCreatureWeaknesses)) "CardCreatureWeaknesses" "CardCreatureWeaknesses"
          yield field (fun i -> (string i.CardCreatureAttacks)) "CardCreatureAttacks" "CardCreatureAttacks"
          yield field (fun i -> (string i.CardResourcesAvailableOnFirstTurn)) "CardResourcesAvailableOnFirstTurn" "CardResourcesAvailableOnFirstTurn"
          yield field (fun i -> (string i.CardResourcesAdded)) "CardResourcesAdded" "CardResourcesAdded"
          yield field (fun i -> (string i.CardResourceCost)) "CardResourceCost" "CardResourceCost"
          yield buttons
        ]
      ]
    ]
    App.layout conf.currentPlayer ([section [_class "section"] cnt])

  let add (ctx: HttpContext) (o: CardDatabaseDto option) (validationResult : Map<string, string>)=
    form ctx o validationResult false

  let edit (ctx: HttpContext) (o: CardDatabaseDto) (validationResult : Map<string, string>) =
    form ctx (Some o) validationResult true
