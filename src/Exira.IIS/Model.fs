namespace Exira.IIS

module Model =
    open System

    open Exira.IIS.Contracts.Commands
    open Exira.IIS.Domain.Railway
    open Exira.IIS.Domain.Helpers

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
    let toCommand : Dto -> Result<Command> = function
        | Dto.CreateServer d ->
            errorState {
                let serverIdOpt = constructServerId(Guid.NewGuid())
                let hostnameOpt = constructHostname d.Dns

                // TODO: Use applicative things for this

                let! (serverId, hostname) = (serverIdOpt, hostnameOpt)

                return InitializeServer { InitializeServerCommand.ServerId = serverId
                                          Name = d.Name
                                          Dns = hostname
                                          Description = d.Description }
            }
        | Dto.DeleteServer d ->
            errorState {
                let serverIdOpt = constructServerId d.ServerId

                // TODO: Use applicative things for this

                let! (serverId) = (serverIdOpt)

                return RetireServer { RetireServerCommand.ServerId = serverId }
            }