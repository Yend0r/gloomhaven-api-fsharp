namespace GloomChars.Api

module AuthenticationController = 
    open System
    open Giraffe
    open FSharp.Control.Tasks.ContextInsensitive
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
    type ForgotPasswordRequest =
        {
            Email : string
        }

    [<CLIMutable>]
    type PasswordResetRequest =
        {
            NewPassword : string
            ResetToken  : string
        }

    type LoginResponse =
        {
            Email                : string
            AccessToken          : string
            AccessTokenExpiresAt : DateTime
        }

    type ForgotPasswordResponse =
        {
            ResetToken     : string
            TokenExpiresAt : DateTime
        }

    let private createLoginResponse (user : AuthenticatedUser) : LoginResponse = 
        let (AccessToken token) = user.AccessToken
        {
            Email                = user.Email
            AccessToken          = token
            AccessTokenExpiresAt = user.AccessTokenExpiresAt
        }

    let private createForgotPasswordResponse resetToken tokenExpiresAt : ForgotPasswordResponse = 
        {
            ResetToken     = resetToken
            TokenExpiresAt = tokenExpiresAt
        }

    let login ctx (loginRequest : LoginRequest) : HttpHandler = 
        AuthenticationSvc.authenticate loginRequest.Email loginRequest.Password
        >>= AuthenticationSvc.getAuthenticatedUser
        |> map createLoginResponse
        |> resultToJson "Invalid email/password"

    let logout ctx : HttpHandler = 
        WebAuthentication.getAccessToken ctx
        |> map AuthenticationSvc.revokeToken
        |> map (fun _ -> toMessage "Logged out")
        |> resultToJson "Logout failed. No credentials supplied."

    let sendPasswordResetEmail ctx (forgotPasswordRequest : ForgotPasswordRequest) : HttpHandler = 
        BAD_REQUEST "Not implemented" ""

    let passwordReset ctx (passwordResetRequest : PasswordResetRequest) : HttpHandler = 
        BAD_REQUEST "Not implemented" ""

module AuthenticationRoutes =
    open Giraffe
    open RequestHandlers
    open AuthenticationController 
    open WebAuthentication

    let router : HttpHandler =  
        choose [
            POST >=>
                choose [
                    postCi "/authentication/login" login
                    postCi "/authentication/forgotpassword" sendPasswordResetEmail
                    postCi "/authentication/resetpassword" passwordReset
                ]
            DELETE >=>
                choose [
                    requiresAuthenticatedUser >=>  deleteCi "/authentication/logout" logout 
                ]
        ]
