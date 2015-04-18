namespace Exira.IIS.Processor

module EventHandler =
    open Microsoft.FSharp.Reflection
    open EventStore.ClientAPI

    open Exira.EventStore.Serialization
    open Exira.IIS.Domain.DomainTypes
    open Exira.IIS.Domain.Events

    open Railway

    let handleDomainEvent = function
        | ServerCreated e ->
            Success (sprintf "{ServerId = '%O'; Name = '%s'; Dns = '%s'; Description = '%s';}\n" (e.ServerId |> ServerId.value) e.Name (e.Dns |> Hostname.value) e.Description)
        | ServerDeleted e ->
            Success (sprintf "{ServerId = '%O';}\n" (e.ServerId |> ServerId.value))

    let deserializeEvent (resolvedEvent: ResolvedEvent) =
        try
            let event = deserialize<Event> resolvedEvent
            Success event
        with
        | ex -> Failure (DeserializeProblem (ex.Message))

    let possibleEvents =
        FSharpType.GetUnionCases typeof<Event>
        |> Seq.map (fun c -> c.Name)

    let validateEvent (resolvedEvent: ResolvedEvent) =
        possibleEvents
        |> Seq.exists ((=) resolvedEvent.Event.EventType)
        |> function
            | true -> Success resolvedEvent
            | false -> Failure (UnknownEvent (resolvedEvent.Event.EventType))

    let handleEvent =
        validateEvent
        >> bind deserializeEvent
        >> bind handleDomainEvent
