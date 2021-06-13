module UserViews

open Giraffe.ViewEngine

module AdminPage =
    let view = [
        h1 [] [rawText "I'm admin"]
    ]
    let layout = App.layout view

module UserPage =
    let view = [
        h1 [] [rawText "I'm logged user"]
    ]
    let layout = App.layout view