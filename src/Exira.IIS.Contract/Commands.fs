﻿namespace Exira.IIS.Contract

module Commands =
    open System
    open Railway

    type InitializeServerCommand = {
        ServerId: Guid
        Name: string
        Description: string
    }

    type RetireServerCommand = {
        ServerId: Guid
    }

    let parseCommand (command: obj) =
        match command with
        | :? InitializeServerCommand as d -> Success d
        //| :? RetireServerCommand as d -> Success d
        | _ -> Failure (UnknownDto (command.GetType().Name))

//    type Command =
//        | ServerCommand of ServerCommand
//    and ServerCommand =
//        | InitializeServerCommand of ServerId:Guid * Name:string * Description:string