namespace Exira.IIS

module Sites =
    open System.Net
    open System.Net.Http
    open System.Web.Http
    open GNaP.WebApi.Versioning

    type SitesController() =
        inherit ApiController()

        [<VersionedRoute("servers/{id:int:min(1)}/sites")>]
        member this.Get(id: int) =
            this.Request.CreateResponse(HttpStatusCode.OK, "sites")
