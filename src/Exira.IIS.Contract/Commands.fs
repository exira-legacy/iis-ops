namespace Exira.IIS.Contract

module Commands =
    open System
    open Railway

    type Command =
        | Server of ServerCommand

    and ServerCommand =
        | InitializeServer of InitializeServerCommand
        | RetireServer of RetireServerCommand

    and InitializeServerCommand = {
        ServerId: Guid
        Name: string
        Description: string
    }

    and RetireServerCommand = {
        ServerId: Guid
    }

    let parseCommand (command: obj) =
        match command with
        | :? InitializeServerCommand as cmd -> Success (ServerCommand.InitializeServer(cmd))
        | :? RetireServerCommand as cmd -> Success (ServerCommand.RetireServer(cmd))
        | _ -> Failure (UnknownDto (command.GetType().Name))
