module CollectionManipulation

let predicateForOkayResults z =
                        match z with
                        | Ok _ -> true
                        | _ -> false

let selectorForOkayResults z =
                        match z with
                        | Ok x ->  [ x ]
                        | _ -> []

let selectAllOkayResults (z : List<Result<'a,'b>>) =
    z
    |> List.filter predicateForOkayResults
    |> List.map selectorForOkayResults
    |> List.fold (@) []

let shuffleG xs = xs |> Seq.sortBy (fun _ -> System.Guid.NewGuid())