namespace Exira.IIS

module Application =
    open System.Net
    open System.Net.Http
    open System.Web.Http
    open ExtCore.Control
    open EventStore.ClientAPI

    open Exira.Railway
    open Exira.IIS.Domain.DomainModel
    open Exira.IIS.Domain.CommandHandler

    open Model

    let private formatError = function
//        | ConstructionError (``type``, err) ->
//            sprintf "Could not create object '%s': %s" ``type`` err
        | _ -> "Doh!"

    let private format errors =
        errors
        |> Seq.map formatError
        |> String.concat "\n"

    let private determineErrorCode errors =
        // ConstructionError = BadRequest
        // InvalidState = BadRequest
        // InvalidStateTransition = InternalServerError
        HttpStatusCode.InternalServerError

    // TODO: HttpStatusCode should also be mapped from FailureType
    let private matchToResult (controller:'T when 'T :> ApiController) result =
        match result with
        | Success _ -> controller.Request.CreateResponse HttpStatusCode.Accepted
        | Failure errors -> controller.Request.CreateErrorResponse((errors |> determineErrorCode), (errors |> format))

    let private getConnection (controller: ApiController) =
        let owinEnvironment = controller.Request.GetOwinEnvironment()
        owinEnvironment.["ges.connection"] :?> IEventStoreConnection

    let application controller dto =
        dto
        |> toCommand
        |> bindAsync (controller |> getConnection |> handleCommand)
        |> Async.map (matchToResult controller)