module Server.Tests

open Expecto

open Shared
open Server

let server = testList "Server" [
    testCase "Adding valid Todo" <| fun _ ->
        ()
]

let all =
    testList "All"
        [
            Shared.Tests.shared
            server
        ]

[<EntryPoint>]
let main _ = runTests defaultConfig all