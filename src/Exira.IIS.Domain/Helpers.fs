namespace Exira.IIS.Domain

module Helpers =
    open Exira.IIS.Domain.Railway

    let getTypeName o = o.GetType().Name
    let stateTransitionFail event state = Failure (InvalidStateTransition (sprintf "Invalid event %s for state %s" (event |> getTypeName) (state |> getTypeName)))

    // Apply each event on itself to get to the final state
    let evolve evolveOne initState =
        List.fold
            (fun result event ->
                result
                >>= fun (version, state) ->
                    evolveOne state event
                    >>= fun state -> Success (version + 1, state))
            (Success (-1, initState))