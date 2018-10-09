namespace GloomChars.Api

module UsersController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.ContextInsensitive
    open CompositionRoot
    open ResponseHandlers
    open FSharpPlus

    [<CLIMutable>]
    type AddUserRequest =
        {
            Email    : string
            Password : string
        }

    let addUser (ctx : HttpContext) (addUserRequest : AddUserRequest) : HttpHandler = 
        //TODO this can only called by someone with "SystemAdmin" permissions
        BAD_REQUEST "Not implemented" ""

module UsersRoutes = 
    open Giraffe
    open RequestHandlers
    open UsersController 

    let router : HttpHandler =  
        choose [
            POST >=>
                choose [
                    postCi "/users" addUser 
                ]
        ]