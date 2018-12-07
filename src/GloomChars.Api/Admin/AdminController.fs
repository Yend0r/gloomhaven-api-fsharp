namespace GloomChars.Api

module AdminController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open CompositionRoot
    open ResponseHandlers
    open FSharpPlus
    open AdminModels

    let private toResourceUri (ctx : HttpContext) userId = 
        sprintf "%s/admin/users/%i" (ctx.Request.Host.ToString()) userId

    let private toCreatedResult (ctx : HttpContext) (userViewModel : UserViewModel) : CreatedResult = 
        {
            Uri = toResourceUri ctx userViewModel.Id
            Obj = userViewModel
        }

    let private getUser (userId : int) = 
        UsersSvc.getUser userId
        |> map toUserViewModel

    let addUser (ctx : HttpContext) (addUserRequest : AddUserRequest) : HttpHandler = 
        addUserRequest
        |> validateNewUser 
        |> map toNewUser
        >>= UsersSvc.addUser
        >>= getUser
        |> map (toCreatedResult ctx)
        |> either toCreated (toError "Failed to add user.")
        
    let listUsers (ctx : HttpContext) : HttpHandler = 
        UsersSvc.getUsers()
        |> map toUserViewModel
        |> toSuccessList

