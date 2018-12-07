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

    let private getCharacter (ctx : HttpContext) (id : int) = 
        WebAuthentication.getLoggedInUserId ctx
        >>= CharactersSvc.getCharacter (CharacterId id)
        |> map toViewModel

    let private mapToNewCharacter character ctx = 
        Ok toNewCharacter
        <*> validateNewCharacter character
        <*> getGloomClassName character.ClassName
        <*> WebAuthentication.getLoggedInUserId ctx

    let private mapToCharacterUpdate character characterId ctx = 
        Ok (toCharacterUpdate characterId)
        <*> validateCharacterUpdate character
        <*> WebAuthentication.getLoggedInUserId ctx

    // Controller handlers below -----

    let addCharacter ctx (character : NewCharacterRequest) = 
        (character, ctx)
        ||> mapToNewCharacter 
        >>= CharactersSvc.addCharacter 
        >>= (getCharacter ctx) // .Net seems to want to return the created item with a 201
        |> map (toCreatedResult ctx)
        |> either toCreated (toError "Failed to add character.")

    let updateCharacter ctx (character : CharacterUpdateRequest) (characterId : int) = 
        (character, characterId, ctx)
        |||> mapToCharacterUpdate 
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

