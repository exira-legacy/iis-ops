namespace Exira.IIS

open Owin
open Microsoft.Owin.Extensions
open Microsoft.Owin.Security
open Microsoft.Owin.Security.Jwt
open System.Net
open System.Text
open System.Web.Http
open System.Web.Http.Cors
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open FSharp.Configuration

open Exira.EventStore
open Exira.EventStore.Owin

type WebConfig = YamlConfig<"Web.yaml">

[<Sealed>]
type Startup() =

    let webConfig = WebConfig()

    let registerAuthentication (app: IAppBuilder) =
        let secret = Encoding.UTF8.GetBytes webConfig.Web.JWT.TokenSigningKey
        let options = JwtBearerAuthenticationOptions(
                        AuthenticationMode = AuthenticationMode.Active,
                        AllowedAudiences = webConfig.Web.JWT.Audiences,
                        IssuerSecurityTokenProviders =
                            [SymmetricKeyIssuerSecurityTokenProvider(webConfig.Web.JWT.Issuer, secret)])

        app.UseJwtBearerAuthentication options |> ignore

    let configureRouting (config: HttpConfiguration) =
        config.MapHttpAttributeRoutes()
        config

    let configureFormatters (config: HttpConfiguration)  =
        config.Formatters.Remove config.Formatters.XmlFormatter |> ignore
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver()
        config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling <- ReferenceLoopHandling.Ignore
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
        let port =
            webConfig.EventStore.Port
            |> ServerPort.create
            |> function
                | Some port -> port
                | None -> failwith "Eventstore port is invalid."

        let config =
            EventStoreOptions(Configuration =
                {
                    Address = IPAddress.Parse(webConfig.EventStore.Address)
                    Port = port
                    Username = webConfig.EventStore.Username
                    Password = webConfig.EventStore.Password
                })

        app.UseEventStore(config) |> ignore

    member __.Configuration(app: IAppBuilder) =
        registerAuthentication app
        registerEventStore app
        registerWebApi app "/api"
        app.Run(fun c -> c.Response.WriteAsync("Hello iis-ops!"))
