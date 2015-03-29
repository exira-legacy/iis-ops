namespace Exira.IIS.Domain

module CommandHandler =
    open Exira.IIS.Domain.Railway
    open Exira.IIS.Domain.Commands
    open Exira.IIS.Domain.Server

    let parseCommand: obj -> Result<Command> = function
        | :? InitializeServerCommand as command -> Success (Server(InitializeServer(command)))
        | :? RetireServerCommand as command -> Success (Server(RetireServer(command)))
        | command -> Failure (UnknownCommand (command.GetType().Name))

    let private handleServer = function
        | InitializeServer(serverCommand) -> handleInitializeServer serverCommand
        | RetireServer(serverCommand) -> handleRetireServer serverCommand

    let handleCommand es = function
        | Server(command) -> handleServer command es
