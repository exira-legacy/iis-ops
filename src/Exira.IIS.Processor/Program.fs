namespace Exira.IIS.Processor

module Program =
    open System
    open System.Net
    open ExtCore
    open FSharp.Configuration
    open EventStore.ClientAPI

    open Exira.EventStore
    open Exira.EventStore.Serialization
    open Exira.EventStore.EventStore

    open Exira.IIS.Domain.Events

    type ProcessorConfig = YamlConfig<"Processor.yaml">

    let processorConfig = ProcessorConfig()

    let config =
        {
            Address = IPAddress.Parse(processorConfig.EventStore.Address)
            Port = ServerPort processorConfig.EventStore.Port
            Username = processorConfig.EventStore.Username
            Password = processorConfig.EventStore.Password
        }

    [<EntryPoint>]
    let main argv =
        printfn "Connecting to %O:%d" processorConfig.EventStore.Address processorConfig.EventStore.Port

        let es = connect config |> Async.RunSynchronously

        let handleEvent resolvedEvent =
            resolvedEvent
            |> deserialize<Event>
            |> function
                | ServerCreated e -> sprintf "%A" e
                | ServerDeleted e -> sprintf "%A" e

        let eventAppeared = fun subscription (resolvedEvent: ResolvedEvent) ->
            (
                let systemEvent =
                    resolvedEvent.Event.EventStreamId
                    |> String.startsWith "$"

                if (not systemEvent) then
                    printfn "%s - %04i - %s" resolvedEvent.Event.EventStreamId resolvedEvent.Event.EventNumber resolvedEvent.Event.EventType
                    printfn "%s" (handleEvent resolvedEvent)
            )

        let liveProcessingStarted = fun subscription -> ()

        let subscriptionDropped = fun subscription reason ex -> () // TODO: Try Reconnect

        let subscription =
            es.SubscribeToAllFrom(
                lastCheckpoint = AllCheckpoint.AllStart,
                resolveLinkTos = true,
                eventAppeared = Action<EventStoreCatchUpSubscription, ResolvedEvent>(eventAppeared),
                liveProcessingStarted = Action<EventStoreCatchUpSubscription>(liveProcessingStarted),
                subscriptionDropped = Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception>(subscriptionDropped))

        Console.ReadLine() |> ignore

        es.Close()

        0
