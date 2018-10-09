namespace GloomChars.Api

module WebAuthentication = 

    open System.Security.Claims
    open System.Security.Principal
    open Giraffe
    open Microsoft.AspNetCore.Http
    open CompositionRoot
    open GloomChars.Authentication

    let accessDenied : HttpHandler = 
        ResponseHandlers.UNAUTHORIZED "Please login to access this API."

    let accessDeniedInvalidToken : HttpHandler = 
        ResponseHandlers.UNAUTHORIZED "Please login to access this API (invalid credentials)."

    let createClaimsIdentity (user : AuthenticatedUser) =
        let role = if (user.IsSystemAdmin) then "SystemAdmin" else "None"
        let (AccessToken token) = user.AccessToken
        let claims =
            [
                Claim("UserId", user.Id.ToString(), ClaimValueTypes.Integer)
                Claim("AccessToken", token, ClaimValueTypes.String)
                Claim(ClaimTypes.Role, role, ClaimValueTypes.String)
            ]
        let identity = ClaimsIdentity(claims)
        ClaimsPrincipal(identity)

    (*type CustomPrincipal(user : AuthenticatedUser) =
        member this.AccessToken = user.AccessToken
        member this.IsSystemAdmin = user.IsSystemAdmin
        interface IPrincipal with 
            member this.IsInRole(role:string) = false
            member this.Identity = createClaimsIdentity user*)

    let private addUserToContext accessToken : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let result = AuthenticationSvc.getAuthenticatedUser accessToken
            match result with
            | Ok user -> 
                ctx.User <- createClaimsIdentity user
                next ctx
            | Error _ -> accessDeniedInvalidToken next ctx

    let getAccessToken (ctx : HttpContext) : Result<AccessToken,string> =
        match ctx.GetRequestHeader "Authorization" with
        | Ok accessToken -> Ok (AccessToken accessToken)
        | Error _ -> Error "No authorization header defined"

    let requiresAuthenticatedUser : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            (match getAccessToken ctx with
            | Ok accessToken -> addUserToContext accessToken
            | Error _ -> accessDenied) next ctx