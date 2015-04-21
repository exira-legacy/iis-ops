namespace Exira.IIS.Domain

module CommandHandler =
    open Exira.IIS.Domain.Commands
    open ServerCommandHandler

    let handleCommand es command =
        match command with
        | InitializeServer serverCommand -> handleInitializeServer serverCommand es
        | RetireServer serverCommand -> handleRetireServer serverCommand es
