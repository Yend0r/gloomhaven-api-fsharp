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

    let private toCreatedResult (ctx : HttpContext) (character : CharacterViewModel) : CreatedResult<CharacterViewModel> = 
        let uri = sprintf "%s/characters/%i" (ctx.Request.Host.ToString()) character.Id

        {
            Uri = uri
            Item = character
        }

    let private getCharacterViewModel (id : int) userId  = 
        CharactersSvc.get (CharacterId id) userId
        |> map toViewModel 

    // Controller handlers below -----

    let addCharacter ctx (character : NewCharacterRequest) = 
        result {
            let! userId = WebAuthentication.getLoggedInUserId ctx
            let! validCharacter = validateNewCharacter character
            let! gloomClassName = getGloomClassName character.ClassName
            let newCharacter = toNewCharacter validCharacter gloomClassName userId
            let! characterId = CharactersSvc.add newCharacter
            // .Net wants to return the created item with a 201, so get the item
            let! viewModel = getCharacterViewModel characterId userId 

            return toCreatedResult ctx viewModel 
        }
        |> either toCreated (toError "Failed to add character.")

    let updateCharacter ctx (character : CharacterUpdateRequest) (characterId : int) = 
        result {
            let! userId = WebAuthentication.getLoggedInUserId ctx
            let! existingCharacter = CharactersSvc.get (CharacterId characterId) userId
            let glClass = GameDataSvc.getGlClass existingCharacter.ClassName
            let! validCharacter = validateCharacterUpdate glClass character
            let update = toCharacterUpdate characterId validCharacter userId
            return! CharactersSvc.update update   
        }
        |> either toSuccessNoContent (toError "Failed to update character.")

    let patchCharacter ctx (patch : CharacterPatchRequest) (characterId : int) = 
        result {
            let! userId = WebAuthentication.getLoggedInUserId ctx
            let! existingCharacter = CharactersSvc.get (CharacterId characterId) userId
            let glClass = GameDataSvc.getGlClass existingCharacter.ClassName
            let update = mapPatchToUpdate patch existingCharacter
            let! validUpdate = validatePatchPerks glClass update 
            return! CharactersSvc.update validUpdate           
        }
        |> either toSuccessNoContent (toError "Failed to patch character.")

    let deleteCharacter ctx (characterId : int) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        |> map (CharactersSvc.delete (CharacterId characterId))
        |> either toSuccessNoContent (toError "Delete failed.")

