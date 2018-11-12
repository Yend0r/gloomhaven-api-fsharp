namespace GloomChars.Api

open System
open System.Text
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication
open System.Threading.Tasks
open System.Security.Claims
open CompositionRoot
open GloomChars.Authentication
open GloomChars.Core
open FSharpPlus

module BearerTokenAuth = 
    open WebAuthentication
    open ResponseHandlers

    let authenticationScheme = "BearerToken"

    let private createClaimsPrincipal (user : AuthenticatedUser) =
        let role = if (user.IsSystemAdmin) then systemAdminRole else noRole
        let (AccessToken token) = user.AccessToken
        let claims =
            [
                Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.Integer)
                Claim(accessTokenClaim, token, ClaimValueTypes.String)
                Claim(ClaimTypes.Role, role, ClaimValueTypes.String)
            ]
        (user, ClaimsIdentity(claims, authenticationScheme))  
        |> TokenUser

    let private createAuthenticationTicket (user : AuthenticatedUser) =
        let principal = createClaimsPrincipal user
        AuthenticationTicket(principal, authenticationScheme)

    let private getAuthorizationHeader (request : HttpRequest) = 
        match request.Headers.TryGetValue "Authorization" with
        | true, value -> value.ToString() |> Ok
        | _ -> Error (Unauthorized "Authorization header not found.")

    let private getAccessTokenFromHeader (authHeader : string) = 
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        then authHeader.Substring("Bearer ".Length).Trim() |> AccessToken |> Ok
        else Error (Unauthorized "Authorization header not a bearer token.")

    let authenticationOptions (o : AuthenticationOptions) =
        o.DefaultAuthenticateScheme <- authenticationScheme
        o.DefaultChallengeScheme <- authenticationScheme

    type BearerTokenAuthOptions() = 
        inherit AuthenticationSchemeOptions()

    type BearerTokenAuth(options, logger, encoder, clock) = 
        inherit AuthenticationHandler<BearerTokenAuthOptions>(options, logger, encoder, clock)

        override this.HandleAuthenticateAsync() = 
            Task.FromResult(
                this.Request
                |> getAuthorizationHeader 
                >>= getAccessTokenFromHeader
                >>= AuthenticationSvc.getAuthenticatedUser 
                |> map createAuthenticationTicket
                |> either AuthenticateResult.Success (fun _ -> AuthenticateResult.NoResult())
            )

    type AuthenticationBuilder with
        member xs.AddBearerToken() =
            xs.AddScheme<BearerTokenAuthOptions, BearerTokenAuth>(authenticationScheme, fun _ -> ());