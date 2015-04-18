namespace Exira.IIS.Domain

open Railway
open Helpers
open Exira.IIS.Domain.DomainTypes
open Exira.IIS.Domain.Commands
open Exira.IIS.Domain.Events

module internal Server =

    type ServerInfo = {
        ServerId: ServerId.T
        Name: string
        Dns: Hostname.T
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

    let toServerStreamId id = id |> ServerId.value |> toStreamId "server"
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
        | Init -> Success ((toServerStreamId command.ServerId), version, [ServerCreated serverCreated])
        | _ -> Failure [InvalidState "Server"]

    let deleteServer (command: RetireServerCommand) (version, state) =
        let serverDeleted = { ServerDeletedEvent.ServerId = command.ServerId }

        // Only previously created servers can be deleted
        match state with
        | Created _ -> Success ((toServerStreamId command.ServerId), version, [ServerDeleted serverDeleted])
        | _ -> Failure [InvalidState "Server"]

    let handleInitializeServer (command: InitializeServerCommand) es =
        async {
            let! state = getServerState command.ServerId es

            // TODO: If we use async from the top, this will read nicer
            return!
                state
                >>= createServer command
                >>=! save es
        }

    let handleRetireServer (command: RetireServerCommand) es =
        async {
            let! state = getServerState command.ServerId es

            // TODO: If we use async from the top, this will read nicer
            return!
                state
                >>= deleteServer command
                >>=! save es
        }