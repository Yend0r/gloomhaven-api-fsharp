namespace GloomChars.Api

open System
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
        |> ApplicationUser

    let private createAuthenticationTicket (user : AuthenticatedUser) =
        let principal = createClaimsPrincipal user
        AuthenticationTicket(principal, authenticationScheme)

    let private getRequestHeader (request : HttpRequest) key = 
        match request.Headers.TryGetValue key with
        | true, value -> Ok (value.ToString())
        | _ -> Error (sprintf "Header '%s' missing" key) 

    let authenticationOptions (o : AuthenticationOptions) =
        o.DefaultAuthenticateScheme <- authenticationScheme
        o.DefaultChallengeScheme <- authenticationScheme

    type BearerTokenAuthOptions() = 
        inherit AuthenticationSchemeOptions()

    type BearerTokenAuth(options, logger, encoder, clock) = 
        inherit AuthenticationHandler<BearerTokenAuthOptions>(options, logger, encoder, clock)

        override this.HandleAuthenticateAsync() = 
            Task.FromResult(
                getRequestHeader this.Request authorizationHeader
                |> map AccessToken
                |> Result.mapError Unauthorized //Ensure error types are the same for the next line
                >>= AuthenticationSvc.getAuthenticatedUser 
                |> map createAuthenticationTicket
                |> function
                | Ok ticket -> AuthenticateResult.Success(ticket)
                | Error _ -> AuthenticateResult.NoResult()
            )

    type AuthenticationBuilder with
        member xs.AddBearerTokenAuth(configureOptions) =
            xs.AddScheme<BearerTokenAuthOptions, BearerTokenAuth>(authenticationScheme, configureOptions);