namespace GloomChars.Api

module AdminController = 
    open System
    open Giraffe
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.ContextInsensitive
    open GloomChars.Authentication
    open GloomChars.Common.Validation
    open CompositionRoot
    open ResponseHandlers
    open FSharpPlus

    [<CLIMutable>]
    type AddUserRequest =
        {
            Email    : string
            Password : string
        }

    type UserViewModel =
        {
            Id            : int
            Email         : string 
            DateCreated   : DateTime
            IsLockedOut   : bool
        }

    let private toUserViewModel (user : User) = 
        {
            Id           = user.Id
            Email        = user.Email
            DateCreated  = user.DateCreated
            IsLockedOut  = match user.LockedOutStatus with | LockedOut _ -> true | _ -> false 
        }

    let private validateNewUser (user : AddUserRequest) = 
        validateRequiredString (user.Email, "email") []
        |> validateRequiredString (user.Password, "password") 
        |> validateEmail user.Email 
        |> validatePassword user.Password
        |> function
        | [] -> Ok user
        | errors -> Error (Msg (errorsToString errors))

    let private toNewUser (user : AddUserRequest) : NewUser = 
        { Email = user.Email; Password = user.Password }

    let private toResourceLocation (ctx : HttpContext) userId = 
        sprintf "%s/admin/users/%i" (ctx.Request.Host.ToString()) userId

    let addUser (ctx : HttpContext) (addUserRequest : AddUserRequest) : HttpHandler = 
        addUserRequest
        |> validateNewUser 
        |> map toNewUser
        >>= UsersSvc.addUser
        |> map (toResourceLocation ctx)
        |> resultToResourceLocation "Failed to add user."
        
    let listUsers (ctx : HttpContext) : HttpHandler = 
        UsersSvc.getUsers()
        |> map toUserViewModel
        |> jsonList

