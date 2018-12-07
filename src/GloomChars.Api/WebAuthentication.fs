namespace GloomChars.Api

open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication
open System.Threading.Tasks
open System.Security.Claims
open CompositionRoot
open GloomChars.Users
open GloomChars.Core
open FSharpPlus

type TokenUser(user : AuthenticatedUser, identity : ClaimsIdentity) = 
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

    let forbiddenSystemAdminOnly : HttpHandler = 
        ResponseHandlers.FORBIDDEN "System admin access is required for this url."

    let private isAuthenticated (user : ClaimsPrincipal) = 
        isNotNull user && user.Identity.IsAuthenticated

    let toTokenUser (user : ClaimsPrincipal) =
       match user with
       | :? TokenUser as appUser -> Ok appUser
       | _ -> Error (Unauthorized "Invalid user in context.") //Auth has a code problem if the cast fails
       
    let getLoggedInUser (ctx : HttpContext) : Result<TokenUser,AppError> =
        if isAuthenticated ctx.User
        then toTokenUser ctx.User
        else Error (Unauthorized "Authenticated user not found.")

    let getLoggedInUserId (ctx : HttpContext) : Result<UserId,AppError> =
        getLoggedInUser ctx
        |> map (fun u -> u.Id)

    let getLoggedInUserAccessToken (ctx : HttpContext) : Result<AccessToken,AppError> =
        getLoggedInUser ctx
        |> map (fun u -> u.AccessToken)

    let requiresAuthenticatedUser : HttpHandler = 
        requiresAuthentication accessDenied

    let requiresSystemAdmin : HttpHandler = 
        requiresAuthentication accessDenied 
        >=> requiresRole systemAdminRole forbiddenSystemAdminOnly
