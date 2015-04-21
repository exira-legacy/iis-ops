namespace Exira.IIS.Domain

module Events =
    open DomainTypes

    type Event =
    | ServerCreated of ServerCreatedEvent
    | ServerDeleted of ServerDeletedEvent

    and ServerCreatedEvent = {
        ServerId: ServerId.T
        Name: string
        Dns: Hostname.T
        Description: string
    }

    and ServerDeletedEvent = {
        ServerId: ServerId.T
    }