namespace Exira.IIS

open Owin
open Microsoft.Owin.Extensions
open System.Web.Http
open Newtonsoft.Json.Serialization
open System.Web.Http.Cors
open FSharp.Configuration

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
        // TODO: Need to make this more robust, apparently it adds a trailing slash
        //let urls = System.String.Join(",",  webConfig.Web.CORS.AllowedOrigins)
        let urls = "*"
        let cors = new EnableCorsAttribute(urls, "*", "*")
        config.EnableCors(cors);
        config

    let configureApi (inner: IAppBuilder) (config: HttpConfiguration) =
        inner.UseWebApi config |> ignore
        inner.UseStageMarker(PipelineStage.MapHandler) |> ignore

    let registerWebApi (app: IAppBuilder) (basePath: string) =
        let config = new HttpConfiguration()

        config
        |> configureRouting
        |> configureFormatters
        |> configureCors
        |> ignore

        app.Map(basePath, fun inner -> configureApi inner config) |> ignore

    member __.Configuration(app: IAppBuilder) =
        registerWebApi app "/api"
        app.Run(fun c -> c.Response.WriteAsync("Hello iis-ops!"))
