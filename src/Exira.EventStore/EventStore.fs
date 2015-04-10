namespace Exira.EventStore

open System.Net

type ServerPort = ServerPort of int

type Configuration = {
    Address: IPAddress
    Port: ServerPort
    Username: string
    Password: string
}

type StreamId = StreamId of string

module EventStore =
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
        async {
            let (ServerPort port) = configuration.Port
            let endpoint = IPEndPoint(configuration.Address, port)
            let esSettings =
                ConnectionSettings
                    .Create()
                    .UseConsoleLogger()
                    .SetDefaultUserCredentials(UserCredentials(configuration.Username, configuration.Password))
                    .KeepReconnecting()
                    .KeepRetrying()
                    .Build()

            let connection = EventStoreConnection.Create(esSettings, endpoint, null)
            do! connection.AsyncConnect()
            return connection
        }

    let readFromStream (store: IEventStoreConnection) stream version count =
        async {
            let! slice = store.AsyncReadStreamEventsForward stream version count true

            let events: list<'a> =
                slice.Events
                |> Seq.map deserialize<'a>
                |> Seq.cast
                |> Seq.toList

            let nextEventNumber =
                if slice.IsEndOfStream
                then None
                else Some slice.NextEventNumber

            return events, slice.LastEventNumber, nextEventNumber
        }

    let appendToStream (store:IEventStoreConnection) stream expectedVersion newEvents =
        async {
            let serializedEvents =
                newEvents
                |> List.map serialize
                |> List.toArray

            do! store.AsyncAppendToStream stream expectedVersion serializedEvents |> Async.Ignore
        }