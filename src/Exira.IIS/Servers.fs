namespace Exira.IIS

module Servers =
    open Application
    open System
    open System.Web.Http
    open GNaP.WebApi.Versioning
    open Exira.IIS.Domain.Commands

    [<RoutePrefix("servers")>]
    type ServersController() =
        inherit ApiController()

        let await f =
            async {
                return! f
            } |> Async.StartAsTask

        [<VersionedRoute>]
        member this.Post(command: InitializeServerCommand) =
            command |> application this |> await

        [<VersionedRoute("{serverId:guid}")>]
        member this.Delete(serverId: Guid) =
            { ServerId = serverId } |> application this |> await