namespace Exira.IIS.Domain

module CommandHandler =
    open Exira.IIS.Domain.Commands
    open Exira.IIS.Domain.Server

    let handleCommand = function
        | Server(command) -> handleServer command