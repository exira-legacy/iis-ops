namespace Exira.IIS

open System.Net
open System.Net.Http
open System.Web.Http
open GNaP.WebApi.Versioning

type Server =
    { Dns : string
      Description : string }

[<RoutePrefix("servers")>]
type ServersController() =
    inherit ApiController()

    let values =
        [| { Dns = "win1.exira.com"
             Description = "Windows 2012 R2 @ Frankfurt" }
           { Dns = "win2.exira.com"
             Description = "Windows 2008 @ Dublin" } |]

    [<VersionedRoute>]
    member this.Get() = values

    [<VersionedRoute("{id:int:min(1)}")>]
    member this.Get(request : HttpRequestMessage, id : int) =
        if id >= 0 && values.Length > id then
            request.CreateResponse(values.[id])
        else
            request.CreateResponse(HttpStatusCode.NotFound)
