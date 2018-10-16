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

    let toNewCharacter (character : NewCharacterRequest) (gloomClass : GloomClassName) (userId : int) = 
        {
            UserId    = UserId userId
            Name      = character.Name
            ClassName = gloomClass
        }

    let toPerkUpdate (perk : PerkRequest) : PerkUpdate = 
        { 
            Id = perk.Id
            Quantity = perk.Quantity
        }

    let toCharacterUpdate (characterId : int) (character : CharacterUpdateRequest) (userId : int) = 
        {
            Id           = CharacterId characterId
            UserId       = UserId userId
            Name         = character.Name
            Experience   = character.Experience
            Gold         = character.Gold
            Achievements = character.Achievements
            Perks        = character.Perks |> List.map toPerkUpdate
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

    let deleteCharacter (ctx : HttpContext) (characterId : int) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        |> map UserId
        |> map (CharactersSvc.deleteCharacter (CharacterId characterId))
        |> toSuccessNoContent "Delete failed."