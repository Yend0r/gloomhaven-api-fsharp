namespace GloomChars.Api

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
        |> either toCreated (toError "Failed to add user.")
        
    let listUsers (ctx : HttpContext) : HttpHandler = 
        UsersSvc.getUsers()
        |> map toUserViewModel
        |> toSuccessList

