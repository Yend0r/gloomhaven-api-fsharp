namespace GloomChars.Api

module AuthenticationController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open CompositionRoot
    open FSharpPlus
    open ResponseHandlers
    open AuthenticationModels
    open GloomChars.Authentication
    
    let login ctx (loginRequest : LoginRequest) : HttpHandler = 
        (loginRequest.Email, loginRequest.Password)
        ||> AuthenticationSvc.authenticate 
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


