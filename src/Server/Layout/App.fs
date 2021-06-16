module App

open Giraffe.ViewEngine

let signedOut () =
    html [_class "has-navbar-fixed-top"] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [encodedText "Hello oauthBlogPost"]
            link [_rel "stylesheet"; _href "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css" ]
            link [_rel "stylesheet"; _href "https://cdnjs.cloudflare.com/ajax/libs/bulmaswatch/0.8.1/darkly/bulmaswatch.min.css" ]
            link [_rel "stylesheet"; _href "/app.css" ]

        ]
        body [] [
            yield
              nav [ _class "navbar is-dark" ]
                [ div [ _class "container" ]
                    [ div [ _class "navbar-brand" ]
                        [ a [ _class "navbar-item"
                              _href "#" ]
                            [ str "SAFE Card Game" ] ]
                      div [ _class "navbar-menu" ]
                        [ div [ _class "navbar-start" ]
                            [ a [ _class "navbar-item"
                                  _href "#" ]
                                [ str "Home" ]
                              a [ _class "navbar-item"
                                  _href "#" ]
                                [ str "Leader Board" ]
                              a [ _class "navbar-item"
                                  _href "#" ]
                                [ str "About" ]
                              div [ _class "navbar-item has-dropdown is-hoverable" ]
                                [ a [ _class "navbar-link" ]
                                    [ str "Game" ]
                                  div [ _class "navbar-dropdown" ]
                                    [ a [ _class "navbar-item navbar-item-dropdown"
                                          _href "#" ]
                                        [ str "New Game" ]
                                      a [ _class "navbar-item navbar-item-dropdown"
                                          _href "#" ]
                                        [ str "Load Game" ] ] ] ]
                          div [ _class "navbar-end" ]
                            [ div [ _class "navbar-item" ]
                                [ div [ _class "buttons" ]
                                    [
                                      a [ _class "button is-dark"
                                          _href "https://github.com/jamesclancy/SafeCardGame" ]
                                        [ str "Source" ] ] ] ] ] ] ]
            yield div [_class "container "] [
                        h2 [ _class "title"] [encodedText "Logged Out"]
                        a [ _class "button is-dark"
                            _href "/" ] [ str "Log Back In" ] ]
            yield footer [_class "footer is-fixed-bottom"] [
                div [_class "container"] [
                    div [_class "content has-text-centered"] [
                        p [] [
                            rawText "Powered by "
                            a [_href "https://github.com/SaturnFramework/Saturn"] [rawText "Saturn"]
                            rawText " - F# MVC framework created by "
                            a [_href "http://lambdafactory.io"] [rawText "λFactory"]
                        ]
                    ]
                ]
            ]
            yield script [_src "/app.js"] []
        ]
    ]

let playerLink ( p : Option<Shared.Domain.Player>) =
      match p with
      | None ->
        [
          a [ _class "button is-light"
              _disabled
              _href "#" ]
            [ str "Not Logged In" ] ]
      | Some player ->
         [ a [ _class "button is-light"
               _href (sprintf "/player/%s" (player.PlayerId.ToString())) ]
              [ str player.Name ]
           a [ _class "button is-primary"
               _href "/signout" ]
            [ str "Sign Out" ] ]

let layout player (content: XmlNode list) =
    html [_class "has-navbar-fixed-top"] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [encodedText "Safe Card Game Static Page"]
            link [_rel "stylesheet"; _href "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css" ]
            link [_rel "stylesheet"; _href "https://cdnjs.cloudflare.com/ajax/libs/bulmaswatch/0.8.1/darkly/bulmaswatch.min.css" ]
            link [_rel "stylesheet"; _href "/app.css" ]

        ]
        body [] [
            yield
              nav [ _class "navbar is-dark" ]
                [ div [ _class "container" ]
                    [ div [ _class "navbar-brand" ]
                        [ a [ _class "navbar-item"
                              _href "#" ]
                            [ str "SAFE Card Game" ] ]
                      div [ _class "navbar-menu" ]
                        [ div [ _class "navbar-start" ]
                            [ a [ _class "navbar-item"
                                  _href "#" ]
                                [ str "Home" ]
                              a [ _class "navbar-item"
                                  _href "#" ]
                                [ str "Leader Board" ]
                              a [ _class "navbar-item"
                                  _href "#" ]
                                [ str "About" ]
                              div [ _class "navbar-item has-dropdown is-hoverable" ]
                                [ a [ _class "navbar-link" ]
                                    [ str "Game" ]
                                  div [ _class "navbar-dropdown" ]
                                    [ a [ _class "navbar-item navbar-item-dropdown"
                                          _href "#" ]
                                        [ str "New Game" ]
                                      a [ _class "navbar-item navbar-item-dropdown"
                                          _href "#" ]
                                        [ str "Load Game" ] ] ] ]
                          div [ _class "navbar-end" ]
                            [ div [ _class "navbar-item" ]
                                [ div [ _class "buttons" ]
                                    [
                                      yield! (playerLink player)
                                      a [ _class "button is-dark"
                                          _href "https://github.com/jamesclancy/SafeCardGame" ]
                                        [ str "Source" ] ] ] ] ] ] ]
            yield! content
            yield footer [_class "footer is-fixed-bottom"] [
                div [_class "container"] [
                    div [_class "content has-text-centered"] [
                        p [] [
                            rawText "Powered by "
                            a [_href "https://github.com/SaturnFramework/Saturn"] [rawText "Saturn"]
                            rawText " - F# MVC framework created by "
                            a [_href "http://lambdafactory.io"] [rawText "λFactory"]
                        ]
                    ]
                ]
            ]
            yield script [_src "/app.js"] []
        ]
    ]

let notFound =
    html [_class "has-navbar-fixed-top"] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [encodedText "oauthBlogPost - Error #404"]
        ]
        body [] [
           h1 [] [rawText "ERROR #404"]
           a [_href "/" ] [rawText "Go back to home page"]
        ]
    ]
