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
        |> function
        | [] -> Ok character
        | errors -> Error (Msg (errorsToString errors))

    let private validateCharacterUpdate (character : CharacterUpdateRequest) validationErrors = 
        validateRequiredString (character.Name, "name") []
        |> validatePositiveInt (character.Experience, "experience") 
        |> validatePositiveInt (character.Gold, "gold") 
        |> validatePositiveInt (character.Achievements, "achievements") 
        |> function
        | [] -> Ok character
        | errors -> Error (Msg (errorsToString errors))

    let private getGloomClassName (className : string) = 
        match GloomClassName.fromString className with
        | Some gloomClass -> Ok gloomClass
        | None -> Error (Msg "ClassName is invalid")

    let private toResourceUri (ctx : HttpContext) userId = 
        sprintf "%s/characters/%i" (ctx.Request.Host.ToString()) userId

    let addCharacter (ctx : HttpContext) (character : NewCharacterRequest) = 
        Ok toNewCharacter
        <*> (validateNewCharacter character [])
        <*> (getGloomClassName character.ClassName)
        <*> (WebAuthentication.getLoggedInUserId ctx)
        >>= CharactersSvc.addCharacter 
        |> map (toResourceUri ctx)
        |> toContentCreatedResponse "Failed to add character."

    let updateCharacter (ctx : HttpContext) (character : CharacterUpdateRequest) (characterId : int) = 
        Ok (toCharacterUpdate characterId)
        <*> (validateCharacterUpdate character [])
        <*> (WebAuthentication.getLoggedInUserId ctx)
        >>= CharactersSvc.updateCharacter 
        |> toSuccessNoContent "Failed to update character."
        
    let patchCharacter (ctx : HttpContext) (patch : CharacterPatchRequest) (characterId : int) = 
        let character = 
            WebAuthentication.getLoggedInUserId ctx
            >>= CharactersSvc.getCharacter (CharacterId characterId)

        Ok (mapPatchToUpdate patch)
        <*> character
        >>= CharactersSvc.updateCharacter 
        |> toSuccessNoContent "Failed to patch character."

    let deleteCharacter (ctx : HttpContext) (characterId : int) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        |> map (CharactersSvc.deleteCharacter (CharacterId characterId))
        |> toSuccessNoContent "Delete failed."

