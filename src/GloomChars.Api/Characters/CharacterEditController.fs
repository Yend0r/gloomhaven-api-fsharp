namespace GloomChars.Api

module CharacterEditController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open GloomChars.Core
    open ResponseHandlers
    open FSharpPlus
    open CompositionRoot
    open GloomChars.Common.Validation
    open CharacterEditModels 

    let private validateNewCharacter (character : NewCharacterRequest) validationErrors = 
        validateRequiredString (character.Name, "name") []
        |> validateRequiredString (character.ClassName, "className") 
        |> toValidationResult character
        |> Result.mapError Msg

    let private validateCharacterUpdate (character : CharacterUpdateRequest) validationErrors = 
        validateRequiredString (character.Name, "name") []
        |> validatePositiveInt (character.Experience, "experience") 
        |> validatePositiveInt (character.Gold, "gold") 
        |> validatePositiveInt (character.Achievements, "achievements") 
        |> toValidationResult character
        |> Result.mapError Msg

    let private getGloomClassName (className : string) = 
        match GloomClassName.FromString className with
        | Some gloomClass -> Ok gloomClass
        | None -> Error (Msg "ClassName is invalid")

    let private toResourceUri (ctx : HttpContext) userId = 
        sprintf "%s/characters/%i" (ctx.Request.Host.ToString()) userId

    let private mapToNewCharacter character userId = 
        Ok toNewCharacter
        <*> (validateNewCharacter character [])
        <*> (getGloomClassName character.ClassName)
        <*> userId

    let private mapToCharacterUpdate character characterId userId = 
        Ok (toCharacterUpdate characterId)
        <*> (validateCharacterUpdate character [])
        <*> userId

    // Controller handlers below -----

    let addCharacter ctx (character : NewCharacterRequest) = 
        WebAuthentication.getLoggedInUserId ctx
        |> mapToNewCharacter character 
        >>= CharactersSvc.addCharacter 
        |> map (toResourceUri ctx)
        |> either toCreated (toError "Failed to add character.")

    let updateCharacter ctx (character : CharacterUpdateRequest) (characterId : int) = 
        WebAuthentication.getLoggedInUserId ctx
        |> mapToCharacterUpdate character characterId
        >>= CharactersSvc.updateCharacter 
        |> either toSuccessNoContent (toError "Failed to update character.")

    let patchCharacter ctx (patch : CharacterPatchRequest) (characterId : int) = 
        let character = 
            WebAuthentication.getLoggedInUserId ctx
            >>= CharactersSvc.getCharacter (CharacterId characterId)

        Ok (mapPatchToUpdate patch)
        <*> character
        >>= CharactersSvc.updateCharacter 
        |> either toSuccessNoContent (toError "Failed to patch character.")

    let deleteCharacter ctx (characterId : int) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        |> map (CharactersSvc.deleteCharacter (CharacterId characterId))
        |> either toSuccessNoContent (toError "Delete failed.")

