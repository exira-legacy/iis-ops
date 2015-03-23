namespace Exira.IIS.Domain

module CommandHandler =
    open Exira.EventStore.EventStore
    open Exira.IIS.Domain.Commands
    open Exira.IIS.Domain.Server

    let es = connect()

    let handleCommand = function
        | Server(c) -> handleServer es c