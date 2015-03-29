namespace Exira.IIS

module Application =
    open System.Net
    open System.Net.Http
    open System.Web.Http
    open Exira.EventStore.EventStore
    open Exira.IIS.Domain.Railway
    open Exira.IIS.Domain.Commands
    open Exira.IIS.Domain.CommandHandler

    let private es = connect { Address = IPAddress.Parse("127.0.0.1"); Port = 1113; Username = "admin"; Password = "changeit" }

    let map error =
        match error with
        | UnknownCommand cmd -> sprintf "Unknown command '%s'" cmd
        | _ -> "Doh!"

    // TODO: HttpStatusCode should also be mapped from FailureType
    let matchToResult (controller:'T when 'T :> ApiController) result =
        match result with
        | Success _ -> controller.Request.CreateResponse(HttpStatusCode.Accepted)
        | Failure error -> controller.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, (map error))

    let application controller =
        parseCommand
        >> bind (handleCommand es)
        >> matchToResult controller
