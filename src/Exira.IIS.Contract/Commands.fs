namespace Exira.IIS.Contract

module Commands =
    open System
    open Railway

    type Command =
        | InitializeServerCommand of InitializeServerCommand
        | RetireServerCommand of RetireServerCommand

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
        | :? InitializeServerCommand as d -> Success (Command.InitializeServerCommand(d))
        | :? RetireServerCommand as d -> Success (Command.RetireServerCommand(d))
        | _ -> Failure (UnknownDto (command.GetType().Name))

//    type Command =
//        | ServerCommand of ServerCommand
//    and ServerCommand =
//        | InitializeServerCommand of ServerId:Guid * Name:string * Description:string