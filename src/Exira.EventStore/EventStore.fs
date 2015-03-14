namespace Exira.EventStore

module EventStore =
    open System.Net
    open EventStore.ClientAPI
    open EventStore.ClientAPI.SystemData

    // TODO: All of this should be configurable
    let connect() =
        let ipadress = IPAddress.Parse("127.0.0.1")
        let endpoint = new IPEndPoint(ipadress, 1113)
        let esSettings =
            let s = ConnectionSettings.Create()
                        .UseConsoleLogger()
                        .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"))
                        .Build()
            s

        let connection = EventStoreConnection.Create(esSettings, endpoint, null)
        connection.AsyncConnect() |> Async.RunSynchronously
        connection