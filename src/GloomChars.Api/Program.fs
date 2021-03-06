module GloomChars.Api.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.Serialization
open GloomChars.Common
open BearerTokenAuth

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    let allowedSites = [|"http://127.0.0.1:8080"; "http://127.0.0.1:8000"|];
    builder.WithOrigins(allowedSites)
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    match env.IsDevelopment() with
    | true  -> 
        app.UseDeveloperExceptionPage() |> ignore
    | false -> 
        app.UseGiraffeErrorHandler errorHandler |> ignore

    app.UseAuthentication()
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(Routing.router)

let configureServices (services : IServiceCollection) =
    services.AddCors()    
            .AddGiraffe() 
            .AddAuthentication(BearerTokenAuth.authenticationOptions)
            .AddBearerToken()
            |> ignore

    // Use custom json serialiser settings for option types and null handling
    let customSettings = JsonUtils.jsonSerializerSettings
    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer(customSettings)) 
            |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")

    PostgresDb.init()

    let config = Config.config

    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .UseUrls(config.Api.Url)
        .Build()
        .Run()
    0