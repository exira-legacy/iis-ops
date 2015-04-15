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
    let toCommand: Dto -> Result<obj> = function
        | Dto.CreateServer d ->
            errorState {
                let serverIdOpt = constructServerId(Guid.NewGuid())
                let hostnameOpt = constructHostname d.Dns

                let! (serverId, hostname) = (serverIdOpt, hostnameOpt)

                return { InitializeServerCommand.ServerId = serverId
                         Name = d.Name
                         Dns = hostname
                         Description = d.Description } :> obj
            }
        | Dto.DeleteServer d ->
            let serverIdOpt = constructServerId d.ServerId

            match serverIdOpt with
            | Success serverId -> Success ({ RetireServerCommand.ServerId = serverId } :> obj)
            | Failure f -> Failure f
