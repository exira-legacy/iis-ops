namespace Exira.IIS

module Servers =
    open System.Web.Http
    open GNaP.WebApi.Versioning

    open Exira
    open Model
    open Application

    [<RoutePrefix("servers")>]
    type ServersController() =
        inherit ApiController()

        let values =
            [| { Name = "test"
                 Dns = "win1.exira.com"
                 Description = "Windows 2012 R2 @ Frankfurt" }
               { Name = "test2"
                 Dns = "win2.exira.com"
                 Description = "Windows 2008 @ Dublin" } |]

        [<VersionedRoute>]
        member this.Get() = values

        [<VersionedRoute>]
        member this.Post dto =
            Dto.CreateServer dto |> application this |> await

        [<VersionedRoute("{serverId:guid}")>]
        member this.Delete serverId =
            Dto.DeleteServer { ServerId = serverId } |> application this |> await