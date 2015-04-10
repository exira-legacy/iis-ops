namespace Exira.IIS.Processor

module EventHandler =
    open Exira.IIS.Domain.Events

    let handleDomainEvent = function
        | ServerCreated e -> printf "%A\n" e
        | ServerDeleted e -> printf "%A\n" e