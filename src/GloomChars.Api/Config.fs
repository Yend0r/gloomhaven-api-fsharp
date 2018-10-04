namespace GloomChars.Api

open System
open System.IO
open GloomChars.Common
open GloomChars.Authentication
open Microsoft.Extensions.Configuration

type ApiConfig = 
    {
        Url : string
    }

type AppConfig = 
    { 
        Environment    : string
        Api            : ApiConfig
        Authentication : AuthenticationConfig
        Database       : DatabaseConfig
    }

[<RequireQualifiedAccess>]
module Config = 

    let private configMissingMsg  key (config : IConfigurationRoot)= 
        sprintf "Missing config: '%s' in appsettings.%s.json" key config.["ASPNETCORE_ENVIRONMENT"]

    let private getStringConfig (config : IConfigurationRoot) key = 
        let value : string = config.[key]
        if not (String.IsNullOrEmpty value) then
            value.Trim()
        else
            failwith (configMissingMsg key config)

    let private getIntConfig (config : IConfigurationRoot) key = 
        let value = getStringConfig config key |> Parse.int32
        if value.IsSome then
            value.Value 
        else
            failwith (configMissingMsg key config)

    let private buildConfig () =
        // Load the environment vars first, so we know which appSettings.json to load
        // This only happens once, so isn't a performance problem
        let environmentConfig = ConfigurationBuilder().AddEnvironmentVariables().Build()

        ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(sprintf "appsettings.%s.json" environmentConfig.["ASPNETCORE_ENVIRONMENT"], false)
            .AddEnvironmentVariables()
            .Build()

    // This will only get processed once and subsequent calls will used the calculated value
    let config : AppConfig = 

        let appConfig = buildConfig()

        // The "bind" that comes with IConfiguration is C# based and requires mutability
        // It works like this: jsonConfig.GetSection("AppConfig").Get<AppConfig>();
        // So just hard code up the config... can always make this more automatic if the config gets bigger

        let apiConfig = 
            {
                Url = getStringConfig appConfig "AppConfig:Api:Url"
            } 

        let databaseConfig = 
            {
                ConnectionString = ConnectionString (getStringConfig appConfig "AppConfig:Database:ConnectionString")
            }  

        let authenticationConfig = 
            {
                AccessTokenDurationInMins  = getIntConfig appConfig "AppConfig:Authentication:AccessTokenDurationInMins"
                LoginAttemptsBeforeLockout = getIntConfig appConfig "AppConfig:Authentication:LoginAttemptsBeforeLockout"
                LockoutDurationInMins      = getIntConfig appConfig "AppConfig:Authentication:LockoutDurationInMins"
            }  

        { 
            Environment = appConfig.["AppConfig:Environment"]
            Api = apiConfig
            Authentication = authenticationConfig
            Database = databaseConfig
        }
