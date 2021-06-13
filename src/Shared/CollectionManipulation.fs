module CollectionManipulation

open System

let predicateForOkayResults z =
                        match z with
                        | Ok _ -> true
                        | _ -> false

let selectorForOkayResults z =
                        match z with
                        | Ok x ->  [ x ]
                        | _ -> []

let selectAllOkayResults (z : Result<'a,'b> seq) =
    z
    |> Seq.filter predicateForOkayResults
    |> Seq.map selectorForOkayResults
    |> Seq.fold (@) []


let selectAllOkayResultsAsync z = // (z : Async<Result<'a,'b> seq>) =
    async {
        let! z' = z;
        return z'
        |> Seq.filter predicateForOkayResults
        |> Seq.map selectorForOkayResults
        |> Seq.fold (@) []
    }

let shuffleG xs = xs |> Seq.sortBy (fun _ -> Guid.NewGuid())

let appendToResultListOrMaintainFailure p n =
    match p with
    | Ok l ->
        match n with
        | Ok o -> o @ l |> Ok
        | Error e -> Error e
    | Error e -> Error e


type ResultBuilder() =
    member _.Return(x) = Ok x
    member _.ReturnFrom(m: Result<_, _>) = m
    member _.Bind(m, f) = Result.bind f m


let result = ResultBuilder()