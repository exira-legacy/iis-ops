namespace Exira.IIS.Tests

module WebsiteStepDefinitions =
    open Exira.IIS.Tests.InMemoryEventStoreRunner
    open TickSpec
    open NUnit.Framework

    open Exira.Railway
    open Exira.IIS.Domain.ErrorHandling
    open Exira.IIS.Domain.Commands
    open Exira.IIS.Domain.CommandHandler
    open Exira.IIS.Domain.DomainTypes
    open Exira.IIS.Domain.Events

    let es = startInMemoryEventStore()

    let hostname =
        "dummy"
        |> Hostname.create
        |> function
            | Some hostname -> hostname
            | None -> failwith "Dummy hostname is invalid."

    let mutable command: InitializeServerCommand =
        {
            InitializeServerCommand.ServerId = ServerId.newServerId()
            Name = ""
            Dns = hostname
            Description = ""
        }

    let mutable events: Result<Event list, Error list> = Success []

    let [<Given>] ``a server (.*)``  (serverName:string) =
        command <- {
            InitializeServerCommand.ServerId = ServerId.newServerId()
            Name = "Test"
            Dns = hostname
            Description = "Testing"
        }

    let [<Given>] ``a website (.*) on server (.*)`` (siteName:string, serverName:string) =
        ()

    let [<When>] ``I request a new website (.*) on server (.*)`` (siteName:string, serverName:string) =
        async {
            let! newEvents = handleCommand es.Connection (InitializeServer command)
            events <- newEvents
        } |> Async.RunSynchronously
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