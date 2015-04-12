namespace Exira.IIS.Processor

module Program =
    open System
    open System.Net
    open FSharp.Configuration
    open EventStore.ClientAPI

    open Exira.EventStore
    open Exira.EventStore.EventStore

    open Railway
    open Exira.IIS.Processor.EventHandler

    type ProcessorConfig = YamlConfig<"Processor.yaml">

    let processorConfig = ProcessorConfig()

    let port =
        processorConfig.EventStore.Port
        |> ServerPort.create
        |> function
            | Some port -> port
            | None -> failwith "Eventstore port is invalid."

    let config =
        {
            Address = IPAddress.Parse(processorConfig.EventStore.Address)
            Port = port
            Username = processorConfig.EventStore.Username
            Password = processorConfig.EventStore.Password
        }

    let es = connect config |> Async.RunSynchronously

    let checkpointStream = StreamId (sprintf "%s-checkpoint" processorConfig.Processor.Name)

    let map error =
        match error with
        | UnknownEvent e ->  sprintf "Unknown event: '%s'" e
        | DeserializeProblem e -> sprintf "Serializer problem: '%s'" e

    let eventAppeared = fun _ (resolvedEvent: ResolvedEvent) ->
        let handledEvent = handleEvent resolvedEvent

        match handledEvent with
        | Success dummy ->
            printf "%s" dummy
            storeCheckpoint es checkpointStream resolvedEvent.OriginalPosition.Value |> Async.RunSynchronously
        | Failure error ->
            // TODO: On failure, should either retry, or stop processing
            printfn "%s - %04i@%s" (map error) resolvedEvent.Event.EventNumber resolvedEvent.Event.EventStreamId

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
