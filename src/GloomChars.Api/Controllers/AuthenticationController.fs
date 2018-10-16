namespace GloomChars.Api

module AuthenticationModels =
    open System
    open GloomChars.Authentication 

    [<CLIMutable>]
    type LoginRequest =
        {
            Email    : string
            Password : string
        }

    [<CLIMutable>]
    type ChangePasswordRequest =
        {
            OldPassword : string
            NewPassword : string
        }

    type LoginResponse =
        {
            Email                : string
            AccessToken          : string
            AccessTokenExpiresAt : DateTime
        }

    let createLoginResponse (user : AuthenticatedUser) : LoginResponse = 
        let (AccessToken token) = user.AccessToken

        {
            Email                = user.Email
            AccessToken          = token
            AccessTokenExpiresAt = user.AccessTokenExpiresAt
        }

module AuthenticationController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open CompositionRoot
    open FSharpPlus
    open ResponseHandlers
    open AuthenticationModels

    let login ctx (loginRequest : LoginRequest) : HttpHandler = 
        AuthenticationSvc.authenticate loginRequest.Email loginRequest.Password
        >>= AuthenticationSvc.getAuthenticatedUser
        |> map createLoginResponse
        |> toJsonResponse "Invalid email/password"

    let logout (ctx : HttpContext) : HttpHandler = 
        ctx
        |> WebAuthentication.getLoggedInUser 
        |> map (fun u -> u.AccessToken)
        |> map AuthenticationSvc.revokeToken
        |> toSuccessNoContent "Logout failed. No credentials supplied."

    let changePassword ctx (changePasswordRequest : ChangePasswordRequest) : HttpHandler = 
        BAD_REQUEST "Not implemented" ""


