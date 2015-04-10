namespace Exira.IIS.Processor

module Program =
    open System
    open System.Net
    open FSharp.Configuration
    open EventStore.ClientAPI
    open Microsoft.FSharp.Reflection

    open Exira.EventStore
    open Exira.EventStore.Serialization
    open Exira.EventStore.EventStore
    open Exira.IIS.Domain.Events

    open EventHandler

    type ProcessorConfig = YamlConfig<"Processor.yaml">

    let processorConfig = ProcessorConfig()

    let config =
        {
            Address = IPAddress.Parse(processorConfig.EventStore.Address)
            Port = ServerPort processorConfig.EventStore.Port
            Username = processorConfig.EventStore.Username
            Password = processorConfig.EventStore.Password
        }

    let es = connect config |> Async.RunSynchronously

    let checkpointStream = StreamId (sprintf "%s-checkpoint" processorConfig.Processor.Name)

    let handleEvent resolvedEvent =
        // TODO: Railway oriented way to do this
        resolvedEvent
        |> deserialize<Event>
        |> handleDomainEvent

        storeCheckpoint es checkpointStream resolvedEvent.OriginalPosition.Value |> Async.RunSynchronously

    let possibleEvents =
        FSharpType.GetUnionCases typeof<Event>
        |> Seq.map (fun c -> c.Name)

    let eventAppeared = fun _ (resolvedEvent: ResolvedEvent) ->
        possibleEvents
        |> Seq.exists ((=) resolvedEvent.Event.EventType)
        |> function
            | true -> handleEvent resolvedEvent
            | _ -> ()

    let subscribe = fun reconnect ->
        let lastPosition = getCheckpoint es checkpointStream |> Async.RunSynchronously

        es.SubscribeToAllFrom(
            lastCheckpoint = lastPosition,
            resolveLinkTos = true,
            eventAppeared = Action<EventStoreCatchUpSubscription, ResolvedEvent>(eventAppeared),
            subscriptionDropped = Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception>(reconnect))

    let rec subscriptionDropped = fun _ reason _ ->
        printfn "Subscription Dropped: %O" reason
        subscribe subscriptionDropped |> ignore

    [<EntryPoint>]
    let main _ =
        initaliseCheckpoint es checkpointStream |> Async.RunSynchronously

        subscribe subscriptionDropped |> ignore

        Console.ReadLine() |> ignore

        es.Close()

        0
