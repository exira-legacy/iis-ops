namespace Exira.IIS.Domain

module CommandHandler =
    open Exira.EventStore.EventStore
    open Exira.IIS.Domain.Railway
    open Exira.IIS.Domain.Commands
    open Exira.IIS.Domain.Server

    let private es = connect()

    let parseCommand: obj -> Result<Command> = function
        | :? InitializeServerCommand as cmd -> Success (Server(InitializeServer(cmd)))
        | :? RetireServerCommand as cmd -> Success (Server(RetireServer(cmd)))
        | cmd -> Failure (UnknownCommand (cmd.GetType().Name))

    let handleCommand = function
        | Server(command) -> handleServer command es