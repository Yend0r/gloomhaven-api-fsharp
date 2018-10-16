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

type ApplicationUser(user : AuthenticatedUser, identity : ClaimsIdentity) = 
    inherit ClaimsPrincipal(identity)
    member this.Id = UserId user.Id
    member this.Email = user.Email
    member this.AccessToken = user.AccessToken
    member this.IsSystemAdmin = user.IsSystemAdmin

module WebAuthentication = 
    open Giraffe
    open ResponseHandlers

    let systemAdminRole = "SystemAdmin"
    let noRole = "None"
    let accessTokenClaim = "AccessToken"
    let authorizationHeader = "Authorization"

    let accessDenied : HttpHandler = 
        ResponseHandlers.UNAUTHORIZED "Please login to access this API."

    let accessDeniedInvalidToken : HttpHandler = 
        ResponseHandlers.UNAUTHORIZED "Please login to access this API (invalid credentials)."

    let accessDeniedSystemAdminOnly : HttpHandler = 
        ResponseHandlers.UNAUTHORIZED "System admin access is required for this url."

    let private isAuthenticated (user : ClaimsPrincipal) = 
        isNotNull user && user.Identity.IsAuthenticated

    let toApplicationUser (user : ClaimsPrincipal) =
       match user with
       | :? ApplicationUser as appUser -> Ok appUser
       | _ -> Error (Unauthorized "Invalid user in context") //Auth has a code problem if the cast fails

    let private getClaim claimType (user : ClaimsPrincipal) =
        user.Claims 
        |> Seq.tryFind (fun c -> c.Type = claimType) 
        |> function
        | Some claim -> Ok claim.Value
        | None -> Error (sprintf "Claim '%s' not found" claimType)

    let getLoggedInUser (ctx : HttpContext) : Result<ApplicationUser,AppError> =
        if isAuthenticated ctx.User
        then toApplicationUser ctx.User
        else Error (Unauthorized "Authenticated user not found.")

    let getLoggedInUserId (ctx : HttpContext) : Result<UserId,AppError> =
        getLoggedInUser ctx
        |> map (fun u -> u.Id)

    let requiresAuthenticatedUser : HttpHandler = 
        requiresAuthentication accessDenied

    let requiresSystemAdmin : HttpHandler = 
        requiresAuthentication accessDenied 
        >=> requiresRole systemAdminRole accessDeniedSystemAdminOnly


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