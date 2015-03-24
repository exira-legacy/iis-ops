namespace Exira.EventStore

module EventStore =
    open System
    open System.Net
    open Newtonsoft.Json
    open EventStore.ClientAPI
    open EventStore.ClientAPI.SystemData
    open Microsoft.FSharp.Reflection

    // TODO: All of this should be configurable
    let connect() =
        let ipadress = IPAddress.Parse("127.0.0.1")
        let endpoint = IPEndPoint(ipadress, 1113)
        let esSettings =
            let s = ConnectionSettings.Create()
                        .UseConsoleLogger()
                        .SetDefaultUserCredentials(UserCredentials("admin", "changeit"))
                        .Build()
            s

        let connection = EventStoreConnection.Create(esSettings, endpoint, null)
        connection.AsyncConnect() |> Async.RunSynchronously
        connection

    let jsonSettings =
        let settings = JsonSerializerSettings()
        settings.TypeNameHandling <- TypeNameHandling.Auto
        settings

    let serialize (event:'a)=
        let serializedEvent = JsonConvert.SerializeObject(event, jsonSettings)
        let data = System.Text.Encoding.UTF8.GetBytes(serializedEvent)
        let case, _ = FSharpValue.GetUnionFields(event, typeof<'a>)
        EventData(Guid.NewGuid(), case.Name, true, data, null)

    let deserialize<'a> (event: ResolvedEvent) =
        let serializedString = System.Text.Encoding.UTF8.GetString(event.Event.Data)
        let event = JsonConvert.DeserializeObject<'a>(serializedString, jsonSettings)
        event

    let readFromStream (store: IEventStoreConnection) streamId =
        let slice = store.ReadStreamEventsForwardAsync(streamId, 0, Int32.MaxValue, false).Result

        let events: seq<'a> =
            slice.Events
            |> Seq.map deserialize<'a>
            |> Seq.cast

        let nextEventNumber =
            if slice.IsEndOfStream
            then None
            else Some slice.NextEventNumber

        slice.LastEventNumber, (events |> Seq.toList)

    let appendToStream (store:IEventStoreConnection) streamId expectedVersion newEvents =
        let serializedEvents = newEvents |> List.map serialize |> List.toArray
        Async.RunSynchronously <| store.AsyncAppendToStream streamId expectedVersion serializedEvents