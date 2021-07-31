module Shared.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open SpecialEffectParser
open Shared
let testParse name parser input expectedOut =

    let tryMatch = match (Parser.run parser input) with
                    | Parser.Success (o, input) when o = expectedOut -> true
                    | _ -> false

    testCase name <| fun _ ->
        Expect.isTrue tryMatch name


let shared = testList "Shared" [
    testCase "Empty string is not a valid description" <| fun _ ->
        ()
    testParse "ptrue true" ptrue "true" true
    testParse "pfalse true" pfalse "false" false
    testParse "pbool true" pbool "true" (true |> SEBool)
    testParse "pbool false" pbool "false" (false |> SEBool)

    testParse "pint 1" pint "1" (1 |> SpecialEffectNumber |> SENumber)
    testParse "pint 0" pint "0" (0 |> SpecialEffectNumber|> SENumber)
    testParse "pint 12312" pint "12312" (12312 |> SpecialEffectNumber |> SENumber)

    testParse "pplayer CurrentPlayer" pplayer "CurrentPlayer" (CurrentPlayer |> SEPlayer)
    testParse "pplayer OpponentPlayer" pplayer "OpponentPlayer" (OpponentPlayer |> SEPlayer)
    testParse "pplayer AllPlayers" pplayer "AllPlayers" (AllPlayers |> SEPlayer)

    testParse "pvalue true" pvalue "true" (true |> SEBool)
    testParse "pvalue false" pvalue "false" (false |> SEBool)
    testParse "pvalue 1" pvalue "1" (1 |> SpecialEffectNumber |> SENumber)
    testParse "pvalue 0" pvalue "0" (0 |> SpecialEffectNumber|> SENumber)
    testParse "pvalue 12312" pvalue "12312" (12312 |> SpecialEffectNumber |> SENumber)
    testParse "pvalue CurrentPlayer" pvalue "CurrentPlayer" (CurrentPlayer |> SEPlayer)
    testParse "pvalue OpponentPlayer" pvalue "OpponentPlayer" (OpponentPlayer |> SEPlayer)
    testParse "pvalue AllPlayers" pvalue "AllPlayers" (AllPlayers |> SEPlayer)

    testParse "pLineNumber {123ABC}" pLineNumber "{123ABC}" ("123ABC" |> SpecialEffectLineNumber)
    testParse "pLineNumber {123}" pLineNumber "{123}" ("123" |> SpecialEffectLineNumber)
    testParse "pLineNumber {ABC}" pLineNumber "{ABC}" ("ABC" |> SpecialEffectLineNumber)

    testParse "lineArguments true,1" lineArguments "true,1" [ (true |> SEBool); (1 |> SpecialEffectNumber |> SENumber) ]
    testParse "lineArguments true,0" lineArguments "true,0" [ (true |> SEBool); (0 |> SpecialEffectNumber |> SENumber) ]
    testParse "lineArguments false,1" lineArguments "false,1" [ (false |> SEBool); (1 |> SpecialEffectNumber |> SENumber) ]
    testParse "lineArguments 1, false" lineArguments "1,false" [ (1 |> SpecialEffectNumber |> SENumber); (false |> SEBool)]
    testParse "lineArguments 1, CurrentPlayer" lineArguments "1,CurrentPlayer" [ (1 |> SpecialEffectNumber |> SENumber); (CurrentPlayer |> SEPlayer) ]

    testParse "pLine {123ABS}Draw:CurrentPlayer,2" pLine "{123ABS}Draw:CurrentPlayer,2 "
                ((("123ABS" |> SpecialEffectLineNumber), "Draw", [(CurrentPlayer |> SEPlayer);(2 |> SpecialEffectNumber |> SENumber) ]) |> SEExpression)
]