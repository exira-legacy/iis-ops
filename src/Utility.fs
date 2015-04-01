namespace Exira

[<AutoOpen>]
module Async =
    let await f =
        f |> Async.StartAsTask

[<AutoOpen>]
module General =
    let getTypeName o = o.GetType().Name