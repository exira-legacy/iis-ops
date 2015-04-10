namespace Exira.IIS.Domain

module CommandHandler =
    open Exira
    open Railway
    open Commands
    open ServerCommandHandler

    let parseCommand: obj -> Result<Command> = function
        | :? InitializeServerCommand as command -> Success (Server(InitializeServer(command)))
        | :? RetireServerCommand as command -> Success (Server(RetireServer(command)))
        | command -> Failure (UnknownCommand (getTypeName command))

    let private handleServer = function
        | InitializeServer serverCommand -> handleInitializeServer serverCommand
        | RetireServer serverCommand -> handleRetireServer serverCommand

    let handleCommand es = function
        | Server command -> handleServer command es
