namespace GloomChars.Api

module CharactersModels = 
    open GloomChars.Core
    open FSharpPlus

    [<CLIMutable>]
    type NewCharacterRequest = 
        {
            Name         : string
            ClassName    : string
        }

    [<CLIMutable>]
    type CharacterUpdateRequest = 
        {
            Id           : int 
            Name         : string
            Experience   : int
            Gold         : int
            Achievements : int
            PerkIds      : string list
        }

    type PerkViewModel =
        {
            Id       : string 
            Quantity : int 
            Actions  : string 
        }

    type CharacterViewModel =
        {
            Id           : int
            Name         : string
            ClassName    : string
            Experience   : int
            Gold         : int
            Achievements : int
            Perks        : PerkViewModel list
        }

    let toNewCharacter (character : NewCharacterRequest) (gloomClass : GloomClassName) (userId:int) = 
        {
            UserId    = userId
            Name      = character.Name
            ClassName = gloomClass
        }

    let toCharacterUpdate (character : CharacterUpdateRequest) (userId:int) = 
        {
            Id           = character.Id
            UserId       = userId
            Name         = character.Name
            Experience   = character.Experience
            Gold         = character.Gold
            Achievements = character.Achievements
            PerkIds      = character.PerkIds
        }

    let toPerkViewModel (perk : Perk) : PerkViewModel = 
        {
            Id       = perk.Id
            Quantity = perk.Quantity
            Actions  = perk.Actions |> PerkService.getText
        }

    let toViewModel (character : Character) : CharacterViewModel = 
        {
            Id           = character.Id
            Name         = character.Name
            ClassName    = character.ClassName.ToString()
            Experience   = character.Experience
            Gold         = character.Gold
            Achievements = character.Achievements
            Perks        = character.Perks |> map toPerkViewModel
        }

module CharactersController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open GloomChars.Core
    open ResponseHandlers
    open FSharpPlus
    open CompositionRoot
    open GloomChars.Common.Validation
    open CharactersModels

    let private validateNewCharacter (character : NewCharacterRequest) validationErrors = 
        validateRequiredString (character.Name, "name") []
        |> validateRequiredString (character.ClassName, "className") 
        |> function
        | [] -> Ok character
        | errors -> Error (Msg (errorsToString errors))

    let private validateCharacterUpdate (character : CharacterUpdateRequest) validationErrors = 
        validateRequiredString (character.Name, "name") []
        |> validateNonZeroPositiveInt (character.Id, "id") 
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

    let listCharacters (ctx : HttpContext) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        |> map CharactersSvc.getCharacters 
        |> map (List.map toViewModel)
        |> toJsonListResponse 

    let getCharacter (ctx : HttpContext) (id : int) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        >>= CharactersSvc.getCharacter id
        |> Result.map toViewModel
        |> toJsonResponse "Character not found"

    let addCharacter (ctx : HttpContext) (character : NewCharacterRequest) = 
        Ok toNewCharacter
        <*> (validateNewCharacter character [])
        <*> (getGloomClassName character.ClassName)
        <*> (WebAuthentication.getLoggedInUserId ctx)
        >>= CharactersSvc.addCharacter 
        |> map (toResourceUri ctx)
        |> toContentCreatedResponse "Failed to add character."

    let updateCharacter (ctx : HttpContext) (character : CharacterUpdateRequest) = 
        Ok toCharacterUpdate
        <*> (validateCharacterUpdate character [])
        <*> (WebAuthentication.getLoggedInUserId ctx)
        >>= CharactersSvc.updateCharacter 
        |> toSuccessNoContent "Failed to update character."

    let deleteCharacter (ctx : HttpContext) (characterId : int) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        |> map (CharactersSvc.deleteCharacter characterId)
        |> toSuccessNoContent "Delete failed."