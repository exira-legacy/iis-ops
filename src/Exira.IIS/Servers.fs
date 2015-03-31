namespace Exira.IIS

module Servers =
    open System.Web.Http
    open GNaP.WebApi.Versioning

    open Model
    open Application

    [<RoutePrefix("servers")>]
    type ServersController() =
        inherit ApiController()

        let await f =
            async {
                return! f
            } |> Async.StartAsTask

        [<VersionedRoute>]
        member this.Post dto =
            Dto.CreateServer dto |> application this |> await

        [<VersionedRoute("{serverId:guid}")>]
        member this.Delete serverId =
            Dto.DeleteServer { ServerId = serverId } |> application this |> await