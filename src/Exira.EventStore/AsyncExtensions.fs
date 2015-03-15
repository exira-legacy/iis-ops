namespace Exira.EventStore

// This module implements AwaitTask for non generic Task
// It should be useless in F# 4 since it should be implemented in FSharp.Core
[<AutoOpen>]
module internal AsyncExtensions =
    open System
    open System.Threading.Tasks

    type Microsoft.FSharp.Control.Async with
        static member Raise(ex) = Async.FromContinuations(fun (_, econt, _) -> econt ex)

        static member AwaitTask (t: Task) =
            let tcs = new TaskCompletionSource<unit>(TaskContinuationOptions.None)

            t.ContinueWith((fun _ ->
                if t.IsFaulted then tcs.SetException t.Exception
                elif t.IsCanceled then tcs.SetCanceled()
                else tcs.SetResult(())), TaskContinuationOptions.ExecuteSynchronously) |> ignore

            async {
                try
                    do! Async.AwaitTask tcs.Task
                with
                | :? AggregateException as ex ->
                    do! Async.Raise (ex.Flatten().InnerExceptions |> Seq.head) }

    open EventStore.ClientAPI

    type IEventStoreConnection with
        member this.AsyncConnect() = Async.AwaitTask(this.ConnectAsync())

        member this.AsyncReadStreamEventsForward stream resolveLinkTos =
            Async.AwaitTask(this.ReadStreamEventsForwardAsync(stream, 0, Int32.MaxValue, resolveLinkTos))

        member this.AsyncAppendToStream stream expectedVersion events =
            Async.AwaitTask(this.AppendToStreamAsync(stream, expectedVersion, events))

        member this.AsyncSubscribeToAll resolveLinkTos eventAppeared userCredentials =
            Async.AwaitTask(this.SubscribeToAllAsync(resolveLinkTos, eventAppeared, userCredentials = userCredentials))
