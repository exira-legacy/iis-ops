namespace Exira.IIS.Domain

open Exira.ErrorHandling
open Exira.EventStore.EventStore
open DomainModel
open Helpers
open Events

module internal Server =
    open DomainTypes

    let applyServerEvent state event =
        match state with
        | Init ->
            match event with
            | ServerCreated e ->
                succeed (Created { ServerId = e.ServerId
                                   Name = e.Name
                                   Dns = e.Dns
                                   Description = e.Description })
            | _ -> stateTransitionFail event state

        | Created _ ->
            match event with
            | ServerDeleted _ ->
                succeed (Deleted)
            | _ -> stateTransitionFail event state

        | Deleted ->
            match event with
            | _ -> stateTransitionFail event state

    let toServerStreamId id = id |> ServerId.value |> toStreamId "server"
    let getServerState id = getState (applyEvents applyServerEvent) Init (toServerStreamId id)

module internal ServerCommandHandler =
    open Commands
    open Server

    let createServer (command: InitializeServerCommand) (version, state) =
        let serverCreated = { ServerCreatedEvent.ServerId = command.ServerId
                              Name = command.Name
                              Dns = command.Dns
                              Description = command.Description }

        // Anything else than coming from Init is invalid
        match state with
        | Init -> succeed ((toServerStreamId command.ServerId), version, [ServerCreated serverCreated])
        | Created _ -> fail [ServerAlreadyCreated]
        | _ -> fail [InvalidState "Server"]

    let deleteServer (command: RetireServerCommand) (version, state) =
        let serverDeleted = { ServerDeletedEvent.ServerId = command.ServerId }

        // Only previously created servers can be deleted
        match state with
        | Created _ -> succeed ((toServerStreamId command.ServerId), version, [ServerDeleted serverDeleted])
        | Init -> fail [ServerDoesNotExist]
        | Deleted -> fail [ServerAlreadyDeleted]

    let handleInitializeServer (command: InitializeServerCommand) es =
        async {
            let! state = getServerState command.ServerId es

            // TODO: If we use async from the top, this will read nicer
            return!
                state
                |> bind (createServer command)
                |> bindAsync (save es)
        }

    let handleRetireServer (command: RetireServerCommand) es =
        async {
            let! state = getServerState command.ServerId es

            // TODO: If we use async from the top, this will read nicer
            return!
                state
                |> bind (deleteServer command)
                |> bindAsync (save es)
        }