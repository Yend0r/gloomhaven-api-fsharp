namespace GloomChars.Api

module AdminController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open CompositionRoot
    open ResponseHandlers
    open FSharpPlus
    open AdminModels

    let private toCreatedResult (ctx : HttpContext) (user : UserViewModel) : CreatedResult<UserViewModel> = 
        let uri = sprintf "%s/admin/users/%i" (ctx.Request.Host.ToString()) user.Id

        {
            Uri = uri
            Item = user
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

