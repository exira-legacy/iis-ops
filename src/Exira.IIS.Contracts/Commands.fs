namespace Exira.IIS.Contracts

module Commands =
    open System
    open DomainTypes

    type Command =
        | Server of ServerCommand

    and ServerCommand =
        | InitializeServer of InitializeServerCommand
        | RetireServer of RetireServerCommand

    and InitializeServerCommand = {
        ServerId: ServerId.T
        Name: string
        Dns: Hostname.T
        Description: string
    }

    and RetireServerCommand = {
        ServerId: ServerId.T
    }