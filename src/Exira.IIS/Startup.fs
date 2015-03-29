namespace Exira.IIS

open Owin
open Microsoft.Owin.Extensions
open System.Net
open System.Web.Http
open System.Web.Http.Cors
open Newtonsoft.Json.Serialization
open FSharp.Configuration
open Exira.EventStore.Owin

type WebConfig = YamlConfig<"Web.yaml">

[<Sealed>]
type Startup() =

    let webConfig = WebConfig()

    let configureRouting (config: HttpConfiguration) =
        config.MapHttpAttributeRoutes()
        config

    let configureFormatters (config: HttpConfiguration)  =
        config.Formatters.Remove config.Formatters.XmlFormatter |> ignore
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver()
        config

    let configureCors (config: HttpConfiguration) =
        let urls =
            webConfig.Web.CORS.AllowedOrigins
            |> Seq.map (fun uri -> uri.ToString().TrimEnd('/'))
            |> String.concat ","

        let cors = EnableCorsAttribute(urls, "*", "*")
        config.EnableCors cors
        config

    let configureApi (inner: IAppBuilder) (config: HttpConfiguration) =
        inner.UseWebApi config |> ignore
        inner.UseStageMarker PipelineStage.MapHandler |> ignore

    let registerWebApi (app: IAppBuilder) (basePath: string) =
        let config =
            new HttpConfiguration()
            |> configureRouting
            |> configureFormatters
            |> configureCors

        app.Map(basePath, fun inner -> configureApi inner config) |> ignore

    let registerEventStore (app: IAppBuilder) =
        let config =
            EventStoreOptions(Configuration =
                {
                    Address = IPAddress.Parse(webConfig.EventStore.Address)
                    Port = webConfig.EventStore.Port
                    Username = webConfig.EventStore.Username
                    Password = webConfig.EventStore.Password
                })

        app.UseEventStore(config) |> ignore

    member __.Configuration(app: IAppBuilder) =
        registerEventStore app
        registerWebApi app "/api"
        app.Run(fun c -> c.Response.WriteAsync("Hello iis-ops!"))
