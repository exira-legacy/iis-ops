namespace Exira.IIS

module Application =
    open System.Net
    open System.Net.Http
    open System.Web.Http
    open Exira.EventStore.EventStore
    open Exira.IIS.Contract.Railway
    open Exira.IIS.Contract.Commands

    let es = connect()

    let map f =
        match f with
        | _ -> "Doh!"

    let matchToResult (controller:'T when 'T :> ApiController) res =
        match res with
        | Success _ -> controller.Request.CreateResponse(HttpStatusCode.Accepted)
        | Failure f -> controller.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, (map f))

    let application controller command =
        command |> parseCommand |> matchToResult controller