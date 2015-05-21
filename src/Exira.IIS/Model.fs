namespace Exira.IIS

module Model =
    open System

    open Exira.ErrorHandling

    open Exira.IIS.Domain.DomainModel
    open Exira.IIS.Domain.Commands

    type Dto =
    | CreateServer of CreateServerDto
    | DeleteServer of DeleteServerDto

    and CreateServerDto = {
        Name: string
        Dns: string
        Description: string
    }

    and DeleteServerDto = {
        ServerId: Guid
    }

    /// Parses a DTO to a Command object
    let toCommand dto =
        match dto with
        | Dto.CreateServer d ->
            let serverIdOpt = createServerId(Guid.NewGuid())
            let hostnameOpt = createHostname d.Dns

            let createInitializeServer serverId hostname =
                InitializeServer { InitializeServerCommand.ServerId = serverId
                                   Name = d.Name
                                   Dns = hostname
                                   Description = d.Description }

            createInitializeServer
            <!> serverIdOpt
            <*> hostnameOpt

        | Dto.DeleteServer d ->
            let serverIdOpt = createServerId d.ServerId

            let createRetireServer serverId =
                RetireServer { RetireServerCommand.ServerId = serverId }

            createRetireServer
            <!> serverIdOpt