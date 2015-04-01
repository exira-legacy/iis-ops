namespace Exira.IIS

module Application =
    open System.Net
    open System.Net.Http
    open System.Web.Http
    open ExtCore.Control
    open EventStore.ClientAPI

    open Exira.IIS.Domain.Railway
    open Exira.IIS.Domain.CommandHandler

    open Model

    let map error =
        match error with
        | UnknownDto dto -> sprintf "Unknown dto '%s'" dto
        | UnknownCommand cmd -> sprintf "Unknown command '%s'" cmd
        | _ -> "Doh!"

    // TODO: HttpStatusCode should also be mapped from FailureType
    let matchToResult (controller:'T when 'T :> ApiController) result =
        match result with
        | Success _ -> controller.Request.CreateResponse(HttpStatusCode.Accepted)
        | Failure error -> controller.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, (map error))

    let private getConnection (controller: ApiController) =
        let owinEnvironment = controller.Request.GetOwinEnvironment()
        owinEnvironment.["ges.connection"] :?> IEventStoreConnection

    let application controller =
        toCommand
        >> bind parseCommand
        >> bindAsync (controller |> getConnection |> handleCommand)
        >> Async.map (matchToResult controller)