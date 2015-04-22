namespace Exira.IIS.Domain

module Helpers =
    open System
    open ExtCore.Control

    open Exira.ErrorHandling

    open Exira.EventStore
    open Exira.EventStore.EventStore
    open DomainModel

    let getTypeName o = o.GetType().Name

    let stateTransitionFail event state =
        [InvalidStateTransition (sprintf "Invalid event %s for state %s" (event |> getTypeName) (state |> getTypeName))]
        |> fail

    // Apply each event on itself to get to the final state
    let applyEvents applyEvent initState events =
        let incrementVersion version state = succeed (version + 1, state)

        // let the called apply an event on the current state
        let foldEvent event foldState =
            let (version, state) = foldState

            applyEvent state event
            |> bind (incrementVersion version)

        // fold each event on the current state
        let eventsFolder foldState event =
            foldState
            |> bind (foldEvent event)

        let startEvent = succeed (-1, initState)

        List.fold
            eventsFolder
            startEvent
            events

    let getState applyEvents initState id es =
        readFromStream es id 0 Int32.MaxValue
        |> Async.map (fun (events, _, _) -> events)
        |> Async.map (applyEvents initState)

    let toStreamId prefix (id: Guid) = sprintf "%s-%O" prefix id |> StreamId

    let save es (id, version, events) =
        async {
            do! appendToStream es id version events
            return succeed events
        }