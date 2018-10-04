namespace GloomChars.Api

module AuthenticationController = 
    open System
    open Giraffe
    open FSharp.Control.Tasks.ContextInsensitive
    open GloomChars.Authentication
    open CompositionRoot

    [<CLIMutable>]
    type LoginRequest =
        {
            Email    : string
            Password : string
        }

    type LoginResponse =
        {
            Email                : string
            AccessToken          : string
            AccessTokenExpiresAt : DateTime
        }

    let private createLoginResponse (user : AuthenticatedUser) : LoginResponse = 
        {
            Email                = user.Email
            AccessToken          = user.AccessToken
            AccessTokenExpiresAt = user.AccessTokenExpiresAt
        }

    let login (loginRequest : LoginRequest) : HttpHandler = 
        let result = AuthenticationSvc.authenticate loginRequest.Email loginRequest.Password

        match result with 
        | Ok user ->
            json (createLoginResponse user)
        | Error _ ->
            ResponseUtils.BAD_REQUEST "Invalid email/password" ""


    //TODO: forgotten password


    let router : HttpHandler =  
        choose [
            POST >=>
                choose [
                    ControllerUtils.postCi "/login" login
                ]
        ]