namespace Exira.IIS.Domain

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
        Description: string
    }

    and RetireServerCommand = {
        ServerId: Guid
    }
