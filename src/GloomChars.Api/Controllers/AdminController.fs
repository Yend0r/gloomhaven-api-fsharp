namespace GloomChars.Api

module AdminModels = 
    open System
    open GloomChars.Authentication

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

    let toUserViewModel (user : User) = 
        {
            Id           = user.Id
            Email        = user.Email
            DateCreated  = user.DateCreated
            IsLockedOut  = match user.LockedOutStatus with | LockedOut _ -> true | _ -> false 
        }

    let toNewUser (user : AddUserRequest) : NewUser = 
        { 
            Email = user.Email
            Password = user.Password 
        }

module AdminController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open GloomChars.Common.Validation
    open CompositionRoot
    open ResponseHandlers
    open FSharpPlus
    open AdminModels
    
    let private validateNewUser (user : AddUserRequest) = 
        validateRequiredString (user.Email, "email") []
        |> validateRequiredString (user.Password, "password") 
        |> validateEmail user.Email 
        |> validatePassword user.Password
        |> function
        | [] -> Ok user
        | errors -> Error (Msg (errorsToString errors))

    let private toResourceUri (ctx : HttpContext) userId = 
        sprintf "%s/admin/users/%i" (ctx.Request.Host.ToString()) userId

    let addUser (ctx : HttpContext) (addUserRequest : AddUserRequest) : HttpHandler = 
        addUserRequest
        |> validateNewUser 
        |> map toNewUser
        >>= UsersSvc.addUser
        |> map (toResourceUri ctx)
        |> toContentCreatedResponse "Failed to add user."
        
    let listUsers (ctx : HttpContext) : HttpHandler = 
        UsersSvc.getUsers()
        |> map toUserViewModel
        |> jsonList

