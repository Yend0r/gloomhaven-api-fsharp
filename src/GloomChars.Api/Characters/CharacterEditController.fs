namespace GloomChars.Api

module CharacterEditController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open GloomChars.Core
    open ResponseHandlers
    open FSharpPlus
    open CompositionRoot
    open CharacterEditModels 
    open CharacterReadModels
    open GloomChars.Common.ResultExpr

    let private getGloomClassName (className : string) = 
        match GloomClassName.FromString className with
        | Some gloomClass -> Ok gloomClass
        | None -> Error (Msg "ClassName is invalid")

    let private toResourceUri (ctx : HttpContext) characterId = 
        sprintf "%s/characters/%i" (ctx.Request.Host.ToString()) characterId

    let private toCreatedResult (ctx : HttpContext) (characterViewModel : CharacterViewModel) : CreatedResult = 
        {
            Uri = toResourceUri ctx characterViewModel.Id
            Obj = characterViewModel
        }

    let private getCharacterViewModel (id : int) userId  = 
        CharactersSvc.getCharacter (CharacterId id) userId
        |> map toViewModel

    // Controller handlers below -----

    let addCharacter ctx (character : NewCharacterRequest) = 
        result {
            let! userId = WebAuthentication.getLoggedInUserId ctx
            let! validCharacter = validateNewCharacter character
            let! gloomClassName = getGloomClassName character.ClassName
            let newCharacter = toNewCharacter validCharacter gloomClassName userId

            let! characterId = CharactersSvc.addCharacter newCharacter

            // .Net wants to return the created item with a 201, so get the item
            let! viewModel = getCharacterViewModel characterId userId 

            return toCreatedResult ctx viewModel
        }
        |> either toCreated (toError "Failed to add character.")

    let updateCharacter ctx (character : CharacterUpdateRequest) (characterId : int) = 
        result {
            let! userId = WebAuthentication.getLoggedInUserId ctx
            let! validCharacter = validateCharacterUpdate character
            let update = toCharacterUpdate characterId validCharacter userId
            return! CharactersSvc.updateCharacter update
        }
        |> either toSuccessNoContent (toError "Failed to update character.")

    let patchCharacter ctx (patch : CharacterPatchRequest) (characterId : int) = 
        result {
            let! userId = WebAuthentication.getLoggedInUserId ctx
            let! character = CharactersSvc.getCharacter (CharacterId characterId) userId
            let update = mapPatchToUpdate patch character
            return! CharactersSvc.updateCharacter update            
        }
        |> either toSuccessNoContent (toError "Failed to patch character.")

    let deleteCharacter ctx (characterId : int) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        |> map (CharactersSvc.deleteCharacter (CharacterId characterId))
        |> either toSuccessNoContent (toError "Delete failed.")

