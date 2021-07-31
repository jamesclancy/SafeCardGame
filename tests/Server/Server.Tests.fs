module Server.Tests

open Expecto

open Shared
open Server
open FParsec

let server = testList "Server" [
    testCase "Adding valid Todo" <| fun _ ->
        //let res = run SpecialEffectParser.pLine "{123ABS}Draw:CurrentPlayer,2"
        //Expect.isOk res "Draw line Fails to parse"
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