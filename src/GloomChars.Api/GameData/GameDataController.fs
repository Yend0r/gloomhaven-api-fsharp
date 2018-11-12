namespace GloomChars.Api

module GameDataController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open ResponseHandlers
    open FSharpPlus
    open CompositionRoot
    open GameDataModels

    let listClasses (ctx : HttpContext) : HttpHandler = 
        GameDataSvc.gloomClasses
        |> map toViewModel
        |> toSuccessList

    let getClass (ctx : HttpContext) (className : string) : HttpHandler = 
        className
        |> GameDataSvc.getGloomClass
        |> map toViewModel
        |> either toSuccess (toError "Class not found.")

