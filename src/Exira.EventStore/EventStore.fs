namespace Exira.EventStore

open System.Net

type StreamId = StreamId of string

type Configuration = {
    Address: IPAddress
    Port: int // TODO: turn into valid ServerPort type
    Username: string
    Password: string
}

module EventStore =
    open System
    open System.Net
    open EventStore.ClientAPI
    open EventStore.ClientAPI.SystemData
    open Serialization

    type private IEventStoreConnection with
        member this.AsyncConnect() =
            Async.AwaitTask(this.ConnectAsync())

        member this.AsyncReadStreamEventsForward stream start count resolveLinkTos =
            let (StreamId streamId) = stream
            Async.AwaitTask(this.ReadStreamEventsForwardAsync(streamId, start, count, resolveLinkTos))

        member this.AsyncAppendToStream stream expectedVersion events =
            let (StreamId streamId) = stream
            Async.AwaitTask(this.AppendToStreamAsync(streamId, expectedVersion, events))

        member this.AsyncSubscribeToAll resolveLinkTos eventAppeared userCredentials =
            Async.AwaitTask(this.SubscribeToAllAsync(resolveLinkTos, eventAppeared, userCredentials = userCredentials))

    let connect configuration =
        let endpoint = IPEndPoint(configuration.Address, configuration.Port)
        let esSettings =
            ConnectionSettings.Create()
                .UseConsoleLogger()
                .SetDefaultUserCredentials(UserCredentials(configuration.Username, configuration.Password))
                .Build()

        let connection = EventStoreConnection.Create(esSettings, endpoint, null)
        connection.AsyncConnect() |> Async.RunSynchronously
        connection

    let readFromStream (store: IEventStoreConnection) stream =
        let slice = store.AsyncReadStreamEventsForward stream 0 Int32.MaxValue false |> Async.RunSynchronously

        let events: list<'a> =
            slice.Events
            |> Seq.map deserialize<'a>
            |> Seq.cast
            |> Seq.toList

        let nextEventNumber =
            if slice.IsEndOfStream
            then None
            else Some slice.NextEventNumber

        events, slice.LastEventNumber, nextEventNumber

    let appendToStream (store: IEventStoreConnection) stream expectedVersion newEvents =
        let serializedEvents =
            newEvents
            |> List.map serialize
            |> List.toArray

        store.AsyncAppendToStream stream expectedVersion serializedEvents |> Async.RunSynchronously