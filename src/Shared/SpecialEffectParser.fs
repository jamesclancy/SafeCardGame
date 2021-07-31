module SpecialEffectParser

open Parser
open Shared.Domain


type SpecialEffectPlayerType = CurrentPlayer | OpponentPlayer | AllPlayers
type SpecialEffectNumber = SpecialEffectNumber of int
type SpecialEffectLineNumber = SpecialEffectLineNumber of string

type SpecialEffectDataType =
    | SEPlayer of SpecialEffectPlayerType
    | SEBool of bool
    | SENumber of SpecialEffectNumber
    | SEExpression of SpecialEffectLineNumber * string * SpecialEffectDataType list
    //| SEPrimative of Primitive * (GameState -> GameState)



let ptrue = stringReturn "true" true
let pfalse = stringReturn "false" false
let pbool = (ptrue <|> pfalse) |>> fun x -> SEBool x
let pint = pint |>> fun n -> n |> SpecialEffectNumber |> SENumber

let pCurrentPlayer = stringReturn "CurrentPlayer" (CurrentPlayer |> SEPlayer)
let pOpponentPlayer = stringReturn "OpponentPlayer" (OpponentPlayer |> SEPlayer)
let pAllPlayers = stringReturn "AllPlayers" (AllPlayers |> SEPlayer)
let pplayer = (pCurrentPlayer <|> pOpponentPlayer <|> pAllPlayers)


let pLineNumber =
                    skipChar '{'
                    >>. (charsTillChar '}')
                    .>> skipChar '}'
                    |>> SpecialEffectLineNumber


let pvalue  = choice [
                        pbool
                        pint
                        pplayer
                    ]


let lineArguments = sepBy pvalue (skipChar ',' .>> spaces)
let pLine   =
             pLineNumber
             .>> spaces
             .>>. charsTillChar ':'
             .>> skipChar ':'
             .>> spaces
             .>>. lineArguments
             .>> spaces
             |>> (fun ((x, y), z) -> SEExpression (x,y,z))

let runPrimitiveAgainstGameState (primitiveName:string) (args : SpecialEffectDataType list) (gameState : GameState)
    : Result<GameState, string> =

    match (primitiveName.ToUpperInvariant (), args) with
    | ("DRAW", [ SEPlayer currentPlayer; SENumber (SpecialEffectNumber numOfCards) ]) ->
        CollectionManipulation.result {
               match currentPlayer with
               | CurrentPlayer ->
                   let! board = getExistingPlayerBoardFromGameState gameState.PlayerOne gameState
                   return! moveCardsFromDeckToHand numOfCards gameState gameState.PlayerOne  board
               | OpponentPlayer ->
                   let! board = getExistingPlayerBoardFromGameState gameState.PlayerTwo gameState
                   return! moveCardsFromDeckToHand numOfCards gameState gameState.PlayerTwo  board
               | AllPlayers ->
                   let! board = getExistingPlayerBoardFromGameState gameState.PlayerOne gameState
                   let! newState =  moveCardsFromDeckToHand numOfCards gameState gameState.PlayerOne board
                   let! board = getExistingPlayerBoardFromGameState newState.PlayerTwo newState
                   return! moveCardsFromDeckToHand numOfCards newState newState.PlayerTwo board
        }
    | _ -> "Unable To Parse" |> Result.Error


let getFuncForSpecialEffectText (text:string) : GameState -> GameState = //<GameState, string> =
    let parserOutput = run pLine text //"{123ABS}Draw:CurrentPlayer,2"
    match parserOutput with
    | Success (res, input) ->
        match res with
        | SEExpression (l, prim, args) -> (fun state -> match (runPrimitiveAgainstGameState prim args state) with
                                                        | Result.Ok res -> res
                                                        | Result.Error e -> state)
        | _ -> id
    | Failure(e, _,_) -> id