namespace Exira.EventStore

module internal Serialization =
    open System
    open System.Text
    open EventStore.ClientAPI
    open Newtonsoft.Json
    open Microsoft.FSharp.Reflection

    let private jsonSettings =
        let settings = JsonSerializerSettings(TypeNameHandling = TypeNameHandling.Auto)
        settings

    let serialize (event: 'a) =
        let serializedEvent = JsonConvert.SerializeObject(event, jsonSettings)
        let data = Encoding.UTF8.GetBytes(serializedEvent)
        let case, _ = FSharpValue.GetUnionFields(event, typeof<'a>)
        EventData(Guid.NewGuid(), case.Name, true, data, null)

    let deserialize<'a> (event: ResolvedEvent) =
        let serializedString = Encoding.UTF8.GetString(event.Event.Data)
        let event = JsonConvert.DeserializeObject<'a>(serializedString, jsonSettings)
        event