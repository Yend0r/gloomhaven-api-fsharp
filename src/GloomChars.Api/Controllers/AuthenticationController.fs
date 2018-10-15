namespace GloomChars.Api

module AuthenticationController = 
    open System
    open Giraffe
    open Microsoft.AspNetCore.Http
    open GloomChars.Authentication
    open CompositionRoot
    open FSharpPlus
    open ResponseHandlers

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

    let private createLoginResponse (user : AuthenticatedUser) : LoginResponse = 
        let (AccessToken token) = user.AccessToken
        {
            Email                = user.Email
            AccessToken          = token
            AccessTokenExpiresAt = user.AccessTokenExpiresAt
        }

    let login ctx (loginRequest : LoginRequest) : HttpHandler = 
        AuthenticationSvc.authenticate loginRequest.Email loginRequest.Password
        >>= AuthenticationSvc.getAuthenticatedUser
        |> map createLoginResponse
        |> resultToJson "Invalid email/password"

    let logout (ctx : HttpContext) : HttpHandler = 
        ctx
        |> WebAuthentication.getLoggedInUser 
        |> map (fun u -> u.AccessToken)
        |> map AuthenticationSvc.revokeToken
        |> resultToSuccessNoContent "Logout failed. No credentials supplied."

    let changePassword ctx (changePasswordRequest : ChangePasswordRequest) : HttpHandler = 
        BAD_REQUEST "Not implemented" ""


