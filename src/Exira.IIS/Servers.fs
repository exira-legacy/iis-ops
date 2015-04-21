namespace Exira.IIS

module Servers =
    open System.Web.Http
    open GNaP.WebApi.Versioning

    open Model
    open Application

    // TODO: Pull this up if there are more controllers
    let await f =
        f |> Async.StartAsTask

    [<Authorize>]
    [<RoutePrefix("servers")>]
    type ServersController() =
        inherit ApiController()

        [<VersionedRoute>]
        member this.Post dto =
            Dto.CreateServer dto |> application this |> await

        [<VersionedRoute("{serverId:guid}")>]
        member this.Delete serverId =
            Dto.DeleteServer { ServerId = serverId } |> application this |> await