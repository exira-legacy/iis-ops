namespace Exira

[<AutoOpen>]
module Async =
    let map f workflow =
        async {
            let! res = workflow
            return f res
        }

    let await f =
        f |> Async.StartAsTask

[<AutoOpen>]
module General =
    let getTypeName o = o.GetType().Name