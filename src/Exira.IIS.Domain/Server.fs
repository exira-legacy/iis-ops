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

    let toServerStreamId = toStreamId "server"
    let getServerState id = getState (evolve evolveOneServer) Init (toServerStreamId id)
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

    let handleInitializeServer (command: InitializeServerCommand) es =
        getServerState command.ServerId es
        >>= createServer command
        >>= save es
    // --------------------------------------

    let handleRetireServer command es =
        Success ()

    let handleServer = function
        | InitializeServer(serverCommand) -> handleInitializeServer serverCommand
        //| RetireServer(serverCommand) -> handleRetireServer serverCommand