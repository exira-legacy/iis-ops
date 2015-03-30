namespace Exira.IIS.Tests

module WebsiteStepDefinitions =
    open Exira.IIS.Tests.InMemoryEventStoreRunner
    open TickSpec
    open NUnit.Framework

    open System
    open Exira.IIS.Domain.Railway
    open Exira.IIS.Domain.Commands
    open Exira.IIS.Domain.CommandHandler
    open Exira.IIS.Domain.Events

    let es = startInMemoryEventStore()

    let mutable dto: RetireServerCommand = { RetireServerCommand.ServerId = Guid.NewGuid() }
    let mutable events: Result<list<Event>> = Success []

    let [<Given>] ``a server (.*)``  (serverName:string) =
        dto <- { RetireServerCommand.ServerId = Guid.NewGuid() }

    let [<Given>] ``a website (.*) on server (.*)`` (siteName:string, serverName:string) =
        ()

    let [<When>] ``I request a new website (.*) on server (.*)`` (siteName:string, serverName:string) =
        async {
            let parsedCommandResult = parseCommand dto

            match parsedCommandResult with
            | Success parsedCommand ->
                let! handled = handleCommand es.Connection parsedCommand

                events <- handled
            | Failure _ ->
                events <- Success []
        } |> Async.RunSynchronously

//        events <-
//            parseCommand dto
//            |> bind (handleCommand es.Connection)
        ()

    let [<Then>] ``a new website (.*) should be added to server (.*)`` (siteName:string, serverName:string) =
        let containsEvent =
            match events with
            | Success s ->
                s
                |> List.length = 1
                |> Success
            | Failure f -> Failure f

        match containsEvent with
            | Success s -> Assert.IsTrue(s)
            | Failure f -> Assert.Fail(sprintf "%O" f)

    let [<Then>] ``a new binding (.*) should be added to website (.*) on server (.*)`` (bindingName:string, siteName:string, serverName:string) =
        ()
        //let passed = (stockItem.Count = n)
        //Assert.True(passed)

    let [<Then>] ``no website should be created`` () =
        ()
        //let passed = (stockItem.Count = n)
        //Assert.True(passed)