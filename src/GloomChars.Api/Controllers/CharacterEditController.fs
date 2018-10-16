namespace GloomChars.Api

module CharacterEditModels = 
    open GloomChars.Core
    open FSharpPlus

    [<CLIMutable>]
    type NewCharacterRequest = 
        {
            Name         : string
            ClassName    : string
        }

    [<CLIMutable>]
    type PerkRequest = 
        {
            Id       : string 
            Quantity : int
        }

    [<CLIMutable>]
    type CharacterUpdateRequest = 
        {
            Name         : string
            Experience   : int
            Gold         : int
            Achievements : int
            Perks        : PerkRequest list
        }

    [<CLIMutable>]
    type CharacterPatchRequest = 
        {
            Name         : string option
            Experience   : int option
            Gold         : int option
            Achievements : int option
            Perks        : PerkRequest list option
        }

    let toNewCharacter (character : NewCharacterRequest) (gloomClass : GloomClassName) (userId : UserId) = 
        {
            UserId    = userId
            Name      = character.Name
            ClassName = gloomClass
        }

    let perkRequestToUpdate (perk : PerkRequest) : PerkUpdate = 
        { 
            Id = perk.Id
            Quantity = perk.Quantity
        }

    let perkToUpdate (perk : Perk) : PerkUpdate = 
        { 
            Id = perk.Id
            Quantity = perk.Quantity
        }

    let toCharacterUpdate (characterId : int) (character : CharacterUpdateRequest) (userId : UserId) = 
        {
            Id           = CharacterId characterId
            UserId       = userId
            Name         = character.Name
            Experience   = character.Experience
            Gold         = character.Gold
            Achievements = character.Achievements
            Perks        = character.Perks |> List.map perkRequestToUpdate
        }

    let mapPatchToUpdate (patch : CharacterPatchRequest) (character : Character) = 
        let getPatchProp (patchOptionVal : 'a option) (existingVal : 'a) = 
            match patchOptionVal with
            | Some patchVal -> patchVal
            | None -> existingVal

        let perks = 
            match patch.Perks with
            | Some patchPerks -> patchPerks |> List.map perkRequestToUpdate
            | None -> character.Perks |> List.map perkToUpdate

        {
            Id           = character.Id
            UserId       = character.UserId
            Name         = getPatchProp patch.Name character.Name
            Experience   = getPatchProp patch.Experience character.Experience
            Gold         = getPatchProp patch.Gold character.Gold
            Achievements = getPatchProp patch.Achievements character.Achievements
            Perks        = perks
        }

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

