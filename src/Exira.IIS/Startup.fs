namespace Exira.IIS

open Owin
//open Microsoft.Owin
//open System
//open System.Net.Http
//open System.Web
open System.Web.Http
//open System.Web.Http.Owin
open Newtonsoft.Json.Serialization

[<Sealed>]
type Startup() =

    static member RegisterWebApi(config: HttpConfiguration) =
        config.MapHttpAttributeRoutes()

        config.Formatters.Remove config.Formatters.XmlFormatter |> ignore
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver()

    member __.Configuration(app: IAppBuilder) =
        let config = new HttpConfiguration()

        Startup.RegisterWebApi(config)

        app.UseWebApi config |> ignore