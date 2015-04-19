namespace Exira.IIS.Processor

module EventHandler =
    open Microsoft.FSharp.Reflection
    open EventStore.ClientAPI

    open Exira.Railway
    open Exira.EventStore.Serialization
    open Exira.IIS.Domain.DomainTypes
    open Exira.IIS.Domain.Events

    open ErrorHandling

    let handleDomainEvent = function
        | ServerCreated e ->
            succeed (sprintf "{ServerId = '%O'; Name = '%s'; Dns = '%s'; Description = '%s';}\n" (e.ServerId |> ServerId.value) e.Name (e.Dns |> Hostname.value) e.Description)
        | ServerDeleted e ->
            succeed (sprintf "{ServerId = '%O';}\n" (e.ServerId |> ServerId.value))

    let deserializeEvent (resolvedEvent: ResolvedEvent) =
        try
            let event = deserialize<Event> resolvedEvent
            succeed event
        with
        | ex -> fail (DeserializeProblem (ex.Message))

    let possibleEvents =
        FSharpType.GetUnionCases typeof<Event>
        |> Seq.map (fun c -> c.Name)

    let validateEvent (resolvedEvent: ResolvedEvent) =
        possibleEvents
        |> Seq.exists ((=) resolvedEvent.Event.EventType)
        |> function
            | true -> succeed resolvedEvent
            | false -> fail (UnknownEvent (resolvedEvent.Event.EventType))

    let handleEvent event =
        event
        |> validateEvent
        |> bind deserializeEvent
        |> bind handleDomainEvent
