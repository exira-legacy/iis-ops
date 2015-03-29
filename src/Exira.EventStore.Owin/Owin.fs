namespace Exira.EventStore.Owin

open Owin
open System
open System.Net
open System.Collections.Generic
open System.Threading.Tasks
open System.Runtime.CompilerServices
open Exira.EventStore
open Exira.EventStore.EventStore

// TODO: Add some more checking for valid options

type EventStoreOptions() =
    let defaultConfiguration = { Address = IPAddress.Parse("127.0.0.1"); Port = 1113; Username = "admin"; Password = "changeit" }

    member val Configuration = defaultConfiguration with get, set

type EventStoreMiddleware(next: Func<IDictionary<string, obj>, Task>, options: EventStoreOptions) =
    let awaitTask = Async.AwaitIAsyncResult >> Async.Ignore

    let es = connect options.Configuration

    member this.Invoke (environment: IDictionary<string, obj>) : Task =
        let updatedEnvironment =
            environment
            |> Dict.updateOrAdd "ges.configuration" (options.Configuration :> obj)
            |> Dict.updateOrAdd "ges.connection" (es :> obj)

        async {
            do!
                next.Invoke updatedEnvironment |> awaitTask
        } |> Async.StartAsTask :> Task

[<ExtensionAttribute>]
type AppBuilderExtensions =
    [<ExtensionAttribute>]
    static member UseEventStore(appBuilder: IAppBuilder, options: EventStoreOptions) =
        appBuilder.Use<EventStoreMiddleware>(options);
