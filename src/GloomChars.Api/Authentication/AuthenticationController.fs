namespace GloomChars.Api

module AuthenticationController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open CompositionRoot
    open FSharpPlus
    open ResponseHandlers
    open AuthenticationModels
    
    let login ctx (loginRequest : LoginRequest) : HttpHandler =
        (loginRequest.Email, loginRequest.Password)
        ||> AuthenticationSvc.authenticate 
        |> map toLoginResponse
        |> either toSuccess (toError "Login failed.")

    let logout (ctx : HttpContext) : HttpHandler = 
        WebAuthentication.getLoggedInUserAccessToken ctx
        |> map AuthenticationSvc.revokeToken
        |> either toSuccessNoContent (toError "Logout failed. No credentials supplied.")

    let changePassword ctx (changePasswordRequest : ChangePasswordRequest) : HttpHandler = 
        BAD_REQUEST "Not implemented" ""


