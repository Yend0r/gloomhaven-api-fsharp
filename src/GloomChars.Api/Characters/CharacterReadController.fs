namespace GloomChars.Api

module CharacterReadController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open GloomChars.Core
    open ResponseHandlers
    open FSharpPlus
    open CompositionRoot
    open CharacterReadModels

    let listCharacters (ctx : HttpContext) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        |> map CharactersSvc.getCharacters 
        |> map (List.map toListModel)
        |> toSuccessList 

    let getCharacter (ctx : HttpContext) (id : int) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        >>= CharactersSvc.getCharacter (CharacterId id)
        |> map toViewModel
        |> either toSuccess (toError "Character not found")

