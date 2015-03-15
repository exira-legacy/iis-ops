namespace Exira.IIS.Domain

module CommandHandler =
    open Exira.IIS.Domain.Commands
    open Exira.IIS.Domain.Server

    let handle = function
        | Command.Server(serverCommand) -> handleServer serverCommand