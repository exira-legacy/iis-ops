namespace Exira.IIS

module Model =
    open System

    open Exira.Railway
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
            errorState {
                let serverIdOpt = createServerId(Guid.NewGuid())
                let hostnameOpt = createHostname d.Dns

                // TODO: Use applicative things for this

                let! (serverId, hostname) = (serverIdOpt, hostnameOpt)

                return InitializeServer { InitializeServerCommand.ServerId = serverId
                                          Name = d.Name
                                          Dns = hostname
                                          Description = d.Description }
            }
        | Dto.DeleteServer d ->
            errorState {
                let serverIdOpt = createServerId d.ServerId

                // TODO: Use applicative things for this

                let! (serverId) = (serverIdOpt)

                return RetireServer { RetireServerCommand.ServerId = serverId }
            }