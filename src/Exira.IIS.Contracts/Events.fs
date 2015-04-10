namespace Exira.IIS.Contracts

module Events =
    open System

    type Event =
        | ServerCreated of ServerCreatedEvent
        | ServerDeleted of ServerDeletedEvent

    and ServerCreatedEvent = {
        ServerId: Guid
        Name: string
        Dns: string
        Description: string
    }

    and ServerDeletedEvent = {
        ServerId: Guid
    }