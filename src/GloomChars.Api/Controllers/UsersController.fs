namespace GloomChars.Api

module UsersController = 
    open System
    open Giraffe
    open FSharp.Control.Tasks.ContextInsensitive
    open GloomChars.Authentication
    open CompositionRoot

    [<CLIMutable>]
    type RegistrationRequest =
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

    let register (registrationRequest : RegistrationRequest) : HttpHandler = 
        let result = AuthenticationSvc.authenticate loginRequest.Email loginRequest.Password

        match result with 
        | Ok user ->
            json (createLoginResponse user)
        | Error _ ->
            ResponseUtils.BAD_REQUEST "Invalid email/password" ""

    let logout (user : AuthenticatedUser) : HttpHandler = 
        AuthenticationSvc.revokeToken user.AccessToken
        ResponseUtils.SUCCESS "Logged out"

    let router (user : AuthenticatedUser) : HttpHandler =  
        choose [
            POST >=>
                choose [
                    ControllerUtils.postCi "/users/register" register 
                ]
            DELETE >=>
                choose [
                    ControllerUtils.deleteCi "/users/logout" (logout user) 
                ]
        ]