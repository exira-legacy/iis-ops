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
        | :? InitializeServerCommand as cmd -> Success (Server(InitializeServer(cmd)))
        | :? RetireServerCommand as cmd -> Success (Server(RetireServer(cmd)))
        | _ -> Failure (UnknownCommand (command.GetType().Name))

    let map error =
        match error with
        | UnknownCommand cmd -> sprintf "Unknown command '%s'" cmd
        | _ -> "Doh!"

    let matchToResult (controller:'T when 'T :> ApiController) result =
        match result with
        | Success _ -> controller.Request.CreateResponse(HttpStatusCode.Accepted)
        | Failure error -> controller.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, (map error))

    let application controller command =
        command
        |> parseCommand
        >>= handleCommand
        |> matchToResult controller
