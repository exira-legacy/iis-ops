namespace Exira.IIS.Domain

open Railway
open Helpers
open Commands
open Events

module internal Server =
    open System

    type ServerInfo = {
        ServerId: Guid // TODO: Change to ServerId type
        Name: string
        Dns: string
        Description: string
    }

    type Server =
        | Init
        | Created of ServerInfo
        | Deleted

module internal ServerState =
    open Server

    let evolveOneServer state event =
        match state with
        | Init ->
            match event with
            | ServerCreated e ->
                Success (Created { ServerId = e.ServerId
                                   Name = e.Name
                                   Dns = e.Dns
                                   Description = e.Description })
            | _ -> stateTransitionFail event state

        | Created _ ->
            match event with
            | ServerDeleted _ ->
                Success (Deleted)
            | _ -> stateTransitionFail event state

        | Deleted ->
            match event with
            | _ -> stateTransitionFail event state

    let toServerStreamId = toStreamId "server"
    let getServerState id = getState (evolve evolveOneServer) Init (toServerStreamId id)

module internal ServerCommandHandler =
    open Server
    open ServerState

    let createServer (command: InitializeServerCommand) (version, state) =
        let serverCreated = { ServerCreatedEvent.ServerId = command.ServerId
                              Name = command.Name
                              Dns = command.Dns
                              Description = command.Description }

        // Anything else than coming from Init is invalid
        match state with
        | Init -> Success ((toServerStreamId command.ServerId), version, [Event.ServerCreated(serverCreated)])
        | _ -> Failure (InvalidState "Server")

    let deleteServer (command: RetireServerCommand) (version, state) =
        let serverDeleted = { ServerDeletedEvent.ServerId = command.ServerId }

        // Only previously created servers can be deleted
        match state with
        | Created _ -> Success ((toServerStreamId command.ServerId), version, [Event.ServerDeleted(serverDeleted)])
        | _ -> Failure (InvalidState "Server")

    let handleInitializeServer (command: InitializeServerCommand) es =
        async {
            let! state = getServerState command.ServerId es

            return!
                state
                >>= createServer command
                >>=! save es
        }

    let handleRetireServer (command: RetireServerCommand) es =
        async {
            let! state = getServerState command.ServerId es

            return!
                state
                >>= deleteServer command
                >>=! save es
        }