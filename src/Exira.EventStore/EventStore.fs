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

        member this.AsyncGetStreamMetadata stream =
            let (StreamId streamId) = stream
            Async.AwaitTask(this.GetStreamMetadataAsync(streamId))

        member this.AsyncSetStreamMetadata stream expectedMetastreamVersion (metadata: StreamMetadata) =
            let (StreamId streamId) = stream
            Async.AwaitTask(this.SetStreamMetadataAsync(streamId, expectedMetastreamVersion, metadata))

    type InternalEvent =
        | LastCheckPoint of LastCheckPointEvent

    and LastCheckPointEvent = {
        LastCommitPosition: int64
        LastPreparePosition: int64
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

    let initaliseCheckpoint (store: IEventStoreConnection) stream =
        async {
            let! lastMetaData = store.AsyncGetStreamMetadata stream

            if (lastMetaData.MetastreamVersion <> -1) then
                return ()

            let metadata = StreamMetadata.Build().SetMaxCount(1).Build()

            do! store.AsyncSetStreamMetadata stream ExpectedVersion.Any metadata |> Async.Ignore
        }

    let storeCheckpoint (store: IEventStoreConnection) stream (position: Position) =
        async {
            let checkpoint = LastCheckPoint ({ LastCommitPosition = position.CommitPosition; LastPreparePosition = position.PreparePosition })

            do! appendToStream store stream ExpectedVersion.Any [checkpoint]
        }

    let getCheckpoint (store: IEventStoreConnection) stream =
        async {
            let buildCheckpoint event =
                event
                |> deserialize<InternalEvent>
                |> function
                    | InternalEvent.LastCheckPoint e -> Nullable(Position(e.LastCommitPosition, e.LastPreparePosition))

            let! lastEvent = store.AsyncReadEvent stream -1 false

            return
                match lastEvent.Status with
                    | EventReadStatus.Success ->
                        match lastEvent.Event.HasValue with
                        | true -> buildCheckpoint lastEvent.Event.Value
                        | false -> AllCheckpoint.AllStart
                    | _ -> AllCheckpoint.AllStart
        }
