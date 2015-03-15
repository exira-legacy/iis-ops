namespace Exira.IIS

module Servers =
    open Application
    open System
    open System.Net
    open System.Net.Http
    open System.Web.Http
    open GNaP.WebApi.Versioning
    open Exira.IIS.Contract.Commands

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

        [<VersionedRoute>]
        member this.Post(command: InitializeServerCommand) =
            command |> application this

        [<VersionedRoute("{id:int:min(1)}")>]
        member this.Get(id: int) =
            if id >= 0 && values.Length > id then
                this.Request.CreateResponse(values.[id])
            else
                this.Request.CreateResponse(HttpStatusCode.NotFound)

        [<VersionedRoute("{serverId:guid}")>]
        member this.Delete(serverId: Guid, command: RetireServerCommand) =
            {command with ServerId = serverId} |> application this