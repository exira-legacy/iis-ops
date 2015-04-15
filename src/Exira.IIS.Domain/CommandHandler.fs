namespace Exira.IIS.Domain

module CommandHandler =
    open Exira.IIS.Contracts.Commands
    open ServerCommandHandler

    let handleCommand es = function
        | InitializeServer serverCommand -> handleInitializeServer serverCommand es
        | RetireServer serverCommand -> handleRetireServer serverCommand es
