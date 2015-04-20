namespace Exira.IIS

module Application =
    open System.Net
    open System.Net.Http
    open System.Web.Http
    open ExtCore.Control
    open EventStore.ClientAPI

    open Exira.Railway
    open Exira.IIS.Domain.ErrorHandling
    open Exira.IIS.Domain.CommandHandler

    open Model

    type private ResponseMessage =
        | NotFound
        | BadRequest of string
        | InternalServerError of string

    let private classify msg =
        match msg with
        | ConstructionError (_, _)
        | InvalidState _ ->
            BadRequest (sprintf "%A" msg)

        | InvalidStateTransition _ ->
            InternalServerError (sprintf "%A" msg)

    let private primaryError errors =
        errors
        |> List.map classify
        |> List.sort
        |> List.head

    let private determineErrorCode errors =
        match primaryError errors with
        | NotFound ->
            HttpStatusCode.NotFound
        | BadRequest _ ->
            HttpStatusCode.BadRequest
        | InternalServerError _ ->
            HttpStatusCode.InternalServerError

    let private formatError error =
        match error with
        | ConstructionError (``type``, err) ->
            sprintf "Could not create object '%s': %s" ``type`` err
        | _ -> "Doh!"

    let private format errors =
        errors
        |> Seq.map formatError
        |> String.concat "\n"

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