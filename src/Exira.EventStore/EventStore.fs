namespace Exira.EventStore

module EventStore =
    open System
    open System.Net
    open System.Text
    open Newtonsoft.Json
    open EventStore.ClientAPI
    open EventStore.ClientAPI.SystemData
    open Microsoft.FSharp.Reflection

    type StreamId = StreamId of string

    type private IEventStoreConnection with
        member this.AsyncConnect() = Async.AwaitTask(this.ConnectAsync())

        member this.AsyncReadStreamEventsForward stream resolveLinkTos =
            let (StreamId streamName) = stream
            Async.AwaitTask(this.ReadStreamEventsForwardAsync(streamName, 0, Int32.MaxValue, resolveLinkTos))

        member this.AsyncAppendToStream stream expectedVersion events =
            let (StreamId streamName) = stream
            Async.AwaitTask(this.AppendToStreamAsync(streamName, expectedVersion, events))

        member this.AsyncSubscribeToAll resolveLinkTos eventAppeared userCredentials =
            Async.AwaitTask(this.SubscribeToAllAsync(resolveLinkTos, eventAppeared, userCredentials = userCredentials))

    // TODO: All of this should be configurable
    let connect() =
        let ipadress = IPAddress.Parse("127.0.0.1")
        let endpoint = IPEndPoint(ipadress, 1113)
        let esSettings =
            ConnectionSettings.Create()
                .UseConsoleLogger()
                .SetDefaultUserCredentials(UserCredentials("admin", "changeit"))
                .Build()

        let connection = EventStoreConnection.Create(esSettings, endpoint, null)
        connection.AsyncConnect() |> Async.RunSynchronously
        connection

    let private jsonSettings =
        let settings = JsonSerializerSettings(TypeNameHandling = TypeNameHandling.Auto)
        settings

    let private serialize (event: 'a)=
        let serializedEvent = JsonConvert.SerializeObject(event, jsonSettings)
        let data = Encoding.UTF8.GetBytes(serializedEvent)
        let case, _ = FSharpValue.GetUnionFields(event, typeof<'a>)
        EventData(Guid.NewGuid(), case.Name, true, data, null)

    let private deserialize<'a> (event: ResolvedEvent) =
        let serializedString = Encoding.UTF8.GetString(event.Event.Data)
        let event = JsonConvert.DeserializeObject<'a>(serializedString, jsonSettings)
        event

    let readFromStream (store: IEventStoreConnection) stream =
        let (StreamId streamName) = stream
        let slice = store.ReadStreamEventsForwardAsync(streamName, 0, Int32.MaxValue, false).Result

        let events: seq<'a> =
            slice.Events
            |> Seq.map deserialize<'a>
            |> Seq.cast

        let nextEventNumber =
            if slice.IsEndOfStream
            then None
            else Some slice.NextEventNumber

        slice.LastEventNumber, (events |> Seq.toList)

    let appendToStream (store:IEventStoreConnection) stream expectedVersion newEvents =
        let serializedEvents =
            newEvents
            |> List.map serialize
            |> List.toArray

        store.AsyncAppendToStream stream expectedVersion serializedEvents |> Async.RunSynchronously