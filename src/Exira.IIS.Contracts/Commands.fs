namespace Exira.IIS.Contracts

module Commands =
    open DomainTypes

    type Command =
        | InitializeServer of InitializeServerCommand
        | RetireServer of RetireServerCommand

    and InitializeServerCommand = {
        ServerId: ServerId.T
        Name: string // TODO: Turn into StringXX types
        Dns: Hostname.T
        Description: string
    }

    and RetireServerCommand = {
        ServerId: ServerId.T
    }
