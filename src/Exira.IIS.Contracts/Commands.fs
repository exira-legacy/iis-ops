namespace Exira.IIS.Contracts

module Commands =
    open System

    type Command =
        | Server of ServerCommand

    and ServerCommand =
        | InitializeServer of InitializeServerCommand
        | RetireServer of RetireServerCommand

    and InitializeServerCommand = {
        ServerId: Guid
        Name: string
        Dns: string
        Description: string
    }

    and RetireServerCommand = {
        ServerId: Guid
    }