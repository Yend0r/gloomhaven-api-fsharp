namespace GloomChars.Api

module AuthenticationController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open CompositionRoot
    open FSharpPlus
    open ResponseHandlers
    open AuthenticationModels

    let private mapToPasswordUpdate changePasswordRequest ctx = 
        Ok toPasswordUpdate
        <*> validateChangePasswordRequest changePasswordRequest
        <*> WebAuthentication.getLoggedInUserAccessToken ctx

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
        (changePasswordRequest, ctx)
        ||> mapToPasswordUpdate 
        >>= AuthenticationSvc.changePassword 
        |> either toSuccessNoContent (toError "Failed to update password.")
