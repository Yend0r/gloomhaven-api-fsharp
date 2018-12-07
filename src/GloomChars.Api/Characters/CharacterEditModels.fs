namespace GloomChars.Api

module CharacterEditModels = 
    open GloomChars.Core
    open FSharpPlus
    open GloomChars.Common.Validation
    open ResponseHandlers

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

    let validateNewCharacter (character : NewCharacterRequest) = 
        validateRequiredString (character.Name, "name") []
        |> validateRequiredString (character.ClassName, "className") 
        |> toValidationResult character
        |> Result.mapError Msg

    let validateCharacterUpdate (character : CharacterUpdateRequest) = 
        validateRequiredString (character.Name, "name") []
        |> validatePositiveInt (character.Experience, "experience") 
        |> validatePositiveInt (character.Gold, "gold") 
        |> validatePositiveInt (character.Achievements, "achievements") 
        |> toValidationResult character
        |> Result.mapError Msg