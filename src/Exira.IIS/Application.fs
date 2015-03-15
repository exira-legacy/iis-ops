namespace Exira.IIS

module Application =
    open System.Net
    open System.Net.Http
    open System.Web.Http
    open Exira.EventStore.EventStore
    open Exira.IIS.Domain.Railway
    open Exira.IIS.Domain.Commands
    open Exira.IIS.Domain.CommandHandler

    let es = connect()

    let parseCommand (command: obj) =
        match command with
        | :? InitializeServerCommand as cmd -> Success (Command.Server(ServerCommand.InitializeServer(cmd)))
        | :? RetireServerCommand as cmd -> Success (Command.Server(ServerCommand.RetireServer(cmd)))
        | _ -> Failure (UnknownDto (command.GetType().Name))

    let map f =
        match f with
        | _ -> "Doh!"

    let matchToResult (controller:'T when 'T :> ApiController) result =
        match result with
        | Success _ -> controller.Request.CreateResponse(HttpStatusCode.Accepted)
        | Failure f -> controller.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, (map f))

    let application controller command =
        command |> parseCommand >>= handle |> matchToResult controller
