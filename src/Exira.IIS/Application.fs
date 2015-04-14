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

    let private formatError = function
        | ConstructionError err -> sprintf "Could not create object '%s'" err
        | UnknownDto dto -> sprintf "Unknown dto '%s'" dto
        | UnknownCommand cmd -> sprintf "Unknown command '%s'" cmd
        | _ -> "Doh!"

    let private formatErrors errors =
        errors
        |> Seq.map formatError
        |> String.concat "\n"

    // TODO: HttpStatusCode should also be mapped from FailureType
    let matchToResult (controller:'T when 'T :> ApiController) result =
        match result with
        | Success _ -> controller.Request.CreateResponse HttpStatusCode.Accepted
        | Failure errors -> controller.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, (formatErrors errors))

    let private getConnection (controller: ApiController) =
        let owinEnvironment = controller.Request.GetOwinEnvironment()
        owinEnvironment.["ges.connection"] :?> IEventStoreConnection

    let application controller =
        toCommand
        >> bind parseCommand
        >> bindAsync (controller |> getConnection |> handleCommand)
        >> Async.map (matchToResult controller)