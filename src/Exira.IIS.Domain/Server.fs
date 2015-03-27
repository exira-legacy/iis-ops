namespace Exira.IIS.Domain

//module internal Server =
module Server =
    open System
    open Exira.EventStore.Types
    open Exira.EventStore.EventStore
    open Exira.IIS.Domain.Railway
    open Exira.IIS.Domain.Helpers
    open Exira.IIS.Domain.Commands
    open Exira.IIS.Domain.Events

    // This is your real domain -------------
    type ServerInfo = {
        ServerId: Guid // TODO: Change to ServerId type
        Name: string
        Dns: string
        Description: string
    }

    type Server =
        | Init
        | Created of ServerInfo
    // --------------------------------------

    // Helper stuff -------------------------
    let toServerStreamId = toStreamId "server"
    // --------------------------------------

    // This is the state machine which controls valid transitions
    let evolveOneServer state event =
        match state with
        | Init ->
            match event with
            | ServerCreated(e) ->
                Success(Created { ServerId = e.ServerId
                                  Name = e.Name
                                  Dns = e.Dns
                                  Description = e.Description })
            | _ -> stateTransitionFail event state
        | Created info ->
            match event with
            | _ -> stateTransitionFail event state

    let evolveServer = evolve evolveOneServer
    // --------------------------------------

    // This is your real command handler ------
    let createServer (command: InitializeServerCommand) (version, state) =
        let serverCreated = { ServerCreatedEvent.ServerId = command.ServerId
                              Name = command.Name
                              Dns = command.Dns
                              Description = command.Description}

        match state with
        | Init -> Success ((toServerStreamId command.ServerId), version, [Event.ServerCreated(serverCreated)])
        | _ -> Failure (InvalidState "Server")

    let handleInitializeServer es (command: InitializeServerCommand) =
        let events = readFromStream es (toServerStreamId command.ServerId)
        let getServerState = evolveServer Init (events |> (fun (_, e) -> e))

        getServerState
        >>= (createServer command)
        >>= save es
    // --------------------------------------

    let handleRetireServer es command =
        Success ()

    let handleServer es = function
        | InitializeServer(serverCommand) -> handleInitializeServer es serverCommand
        //| RetireServer(serverCommand) -> handleRetireServer es serverCommand