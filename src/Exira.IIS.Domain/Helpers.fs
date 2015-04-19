namespace Exira.IIS.Domain

module Helpers =
    open System
    open ExtCore.Control

    open Exira.Railway
    open Exira.EventStore
    open Exira.EventStore.EventStore
    open Railway
    open DomainTypes

    let getTypeName o = o.GetType().Name
    let stateTransitionFail event state = Failure [InvalidStateTransition (sprintf "Invalid event %s for state %s" (event |> getTypeName) (state |> getTypeName))]

    // Apply each event on itself to get to the final state
    let evolve evolveOne initState =
        List.fold
            (fun result event ->
                result
                |> bind (fun (version, state) ->
                    evolveOne state event
                    |> bind (fun state -> Success (version + 1, state))))
            (Success (-1, initState))

    let getState evolveOne initState id es =
        readFromStream es id 0 Int32.MaxValue
        |> Async.map (fun (events, _, _) -> events)
        |> Async.map (evolveOne initState)

    let toStreamId prefix (id: Guid) = sprintf "%s-%O" prefix id |> StreamId

    let save es (id, version, events) =
        async {
            do! appendToStream es id version events
            return Success events
        }

    // TODO: This should go somewhere else
    let constructServerId =
        construct ServerId.createWithCont

    let constructHostname =
        construct Hostname.createWithCont