namespace Exira.IIS

module Model =
    open System

    open Exira.IIS.Domain.DomainTypes
    open Exira.IIS.Domain.Commands
    open Exira.IIS.Domain.ErrorHandling
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
    let toCommand = function
        | Dto.CreateServer dto ->
            errorState {
                let serverIdOpt = constructServerId(Guid.NewGuid())
                let hostnameOpt = constructHostname dto.Dns

                // TODO: Use applicative things for this

                let! (serverId, hostname) = (serverIdOpt, hostnameOpt)

                return InitializeServer { InitializeServerCommand.ServerId = serverId
                                          Name = dto.Name
                                          Dns = hostname
                                          Description = dto.Description }
            }
        | Dto.DeleteServer dto ->
            errorState {
                let serverIdOpt = constructServerId dto.ServerId

                // TODO: Use applicative things for this

                let! (serverId) = (serverIdOpt)

                return RetireServer { RetireServerCommand.ServerId = serverId }
            }