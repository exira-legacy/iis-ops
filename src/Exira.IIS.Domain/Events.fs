namespace Exira.IIS.Domain

module Events =
    open System

    type Event =
        | ServerCreated of ServerCreatedEvent

    and ServerCreatedEvent = {
        ServerId: Guid
        Name: string
        Dns: string
        Description: string
    }