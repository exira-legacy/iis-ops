namespace Exira.IIS

module Model =
    open System
    open Exira.IIS.Domain.Railway
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

    let toCommand: Dto -> Result<obj> = function
        | Dto.CreateServer d ->
            Success ({ InitializeServerCommand.ServerId = Guid.NewGuid()
                       Name = d.Name
                       Dns = d.Dns
                       Description = d.Description } :> obj)
        | Dto.DeleteServer d ->
            Success ({ RetireServerCommand.ServerId = d.ServerId } :> obj)