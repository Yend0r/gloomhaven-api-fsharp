namespace GloomChars.Api

module WebAuthentication = 

    open Giraffe
    open Microsoft.AspNetCore.Http
    open CompositionRoot
    open GloomChars.Authentication

    let accessDenied : HttpHandler = 
        ResponseUtils.UNAUTHORIZED "Please login to access this API."

    let accessDeniedInvalidToken : HttpHandler = 
        ResponseUtils.UNAUTHORIZED "Please login to access this API (invalid credentials)."

    let private mapToHandler f authenticatedUser : HttpHandler = 
        match authenticatedUser with
        | Some user -> (f user)
        | None -> accessDeniedInvalidToken

    let requiresAuthenticatedUser (f : AuthenticatedUser -> HttpHandler) : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            (match ctx.GetRequestHeader "Authorization" with
            | Ok accessToken -> mapToHandler f (AuthenticationSvc.getAuthenticatedUser accessToken)
            | Error _ -> accessDenied) next ctx