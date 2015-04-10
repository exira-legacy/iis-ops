namespace Exira.IIS.Processor

module EventHandler =
    open Microsoft.FSharp.Reflection
    open EventStore.ClientAPI

    open Exira.EventStore.Serialization
    open Exira.IIS.Contracts.Events

    open Railway

    let handleDomainEvent = function
        | ServerCreated e ->
            Success (sprintf "%A\n" e)
        | ServerDeleted e ->
            Success (sprintf "%A\n" e)

    let deserializeEvent (resolvedEvent: ResolvedEvent) =
        try
            let event = deserialize<Event> resolvedEvent
            Success event
        with
        | ex -> Failure (DeserializeProblem(ex.Message))

    let possibleEvents =
        FSharpType.GetUnionCases typeof<Event>
        |> Seq.map (fun c -> c.Name)

    let validateEvent (resolvedEvent: ResolvedEvent) =
        possibleEvents
        |> Seq.exists ((=) resolvedEvent.Event.EventType)
        |> function
            | true -> Success resolvedEvent
            | false -> Failure (UnknownEvent(resolvedEvent.Event.EventType))

    let handleEvent =
        validateEvent
        >> bind deserializeEvent
        >> bind handleDomainEvent
