namespace Exira.IIS

module Application =
    open System.Net
    open System.Net.Http
    open System.Web.Http
    open EventStore.ClientAPI
    open Exira.IIS.Domain.Railway
    open Exira.IIS.Domain.CommandHandler

    let map error =
        match error with
        | UnknownCommand cmd -> sprintf "Unknown command '%s'" cmd
        | _ -> "Doh!"

    // TODO: HttpStatusCode should also be mapped from FailureType
    let matchToResult (controller:'T when 'T :> ApiController) result =
        match result with
        | Success _ -> controller.Request.CreateResponse(HttpStatusCode.Accepted)
        | Failure error -> controller.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, (map error))

    let application (controller: ApiController) =
        let owinEnvironment = controller.Request.GetOwinEnvironment()
        let es = owinEnvironment.["ges.connection"] :?> IEventStoreConnection

        parseCommand
        >> bindAsync (handleCommand es)
        >> Async.map (matchToResult controller)
