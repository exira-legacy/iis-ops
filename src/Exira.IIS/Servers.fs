namespace Exira.IIS

module Servers =
    open Application
    open System
    open System.Net
    open System.Net.Http
    open System.Web.Http
    open GNaP.WebApi.Versioning
    open Exira.IIS.Domain.Commands

    [<RoutePrefix("servers")>]
    type ServersController() =
        inherit ApiController()

        [<VersionedRoute>]
        member this.Post(command: InitializeServerCommand) =
            command |> application this

        [<VersionedRoute("{serverId:guid}")>]
        member this.Delete(serverId: Guid, command: RetireServerCommand) =
            {command with ServerId = serverId} |> application this