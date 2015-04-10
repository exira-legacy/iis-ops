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
    open System
    open System.Net
    open EventStore.ClientAPI
    open EventStore.ClientAPI.SystemData
    open Serialization

    type private IEventStoreConnection with
        member this.AsyncConnect() =
            Async.AwaitTask(this.ConnectAsync())

        member this.AsyncReadEvent stream eventNumber resolveLinkTos =
            let (StreamId streamId) = stream
            Async.AwaitTask(this.ReadEventAsync(streamId, eventNumber, resolveLinkTos))

        member this.AsyncReadStreamEventsForward stream start count resolveLinkTos =
            let (StreamId streamId) = stream
            Async.AwaitTask(this.ReadStreamEventsForwardAsync(streamId, start, count, resolveLinkTos))

        member this.AsyncAppendToStream stream expectedVersion events =
            let (StreamId streamId) = stream
            Async.AwaitTask(this.AppendToStreamAsync(streamId, expectedVersion, events))

        member this.AsyncSubscribeToAll resolveLinkTos eventAppeared =
            Async.AwaitTask(this.SubscribeToAllAsync(resolveLinkTos, eventAppeared))

        member this.AsyncSetStreamMetadata stream expectedMetastreamVersion (metadata: StreamMetadata) =
            let (StreamId streamId) = stream
            Async.AwaitTask(this.SetStreamMetadataAsync(streamId, expectedMetastreamVersion, metadata))

    type InternalEvent =
        | LastCheckPoint of LastCheckPointEvent

    and LastCheckPointEvent = {
        LastPosition: Position
    }

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

    let appendToStream (store: IEventStoreConnection) stream expectedVersion newEvents =
        async {
            let serializedEvents =
                newEvents
                |> List.map serialize
                |> List.toArray

            do! store.AsyncAppendToStream stream expectedVersion serializedEvents |> Async.Ignore
        }

    let storeCheckpoint (store: IEventStoreConnection) stream position =
        async {
            // TODO: Only need to set the meta data once
            let metadata = StreamMetadata.Build().SetMaxCount(1).Build()
            do! store.AsyncSetStreamMetadata stream ExpectedVersion.Any metadata |> Async.Ignore

            let checkpoint = LastCheckPoint ({ LastPosition = position })
            do! appendToStream store stream ExpectedVersion.Any [checkpoint]
        }

    let getCheckpoint (store: IEventStoreConnection) stream =
        async {
            let! lastEvent = store.AsyncReadEvent stream -1 false
            // TODO: Check what happens if it doesnt exist yet (either lastEvent or lastEvent.Event

            // TODO: For some reason this does not properly deserialize the Positionm it's always 0/0
            let event =
                lastEvent.Event.Value
                |> deserialize<InternalEvent>

            return
                event
                |> function
                    | InternalEvent.LastCheckPoint e -> Nullable e.LastPosition
                    | _ -> AllCheckpoint.AllStart
        }
