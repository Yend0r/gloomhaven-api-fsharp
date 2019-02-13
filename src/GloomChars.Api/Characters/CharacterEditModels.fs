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
        let perks = 
            match patch.Perks with
            | Some patchPerks -> patchPerks |> List.map perkRequestToUpdate
            | None -> character.ClaimedPerks |> List.map perkToUpdate

        {
            Id           = character.Id
            UserId       = character.UserId
            Name         = Option.defaultValue character.Name patch.Name 
            Experience   = Option.defaultValue character.Experience patch.Experience
            Gold         = Option.defaultValue character.Gold patch.Gold
            Achievements = Option.defaultValue character.Achievements patch.Achievements
            Perks        = perks
        }

    let private validatePerk (availablePerks : Perk list) perkId perkQty (validationErrors : ValidationError list) = 

        let checkPerkQty qty (perk : Perk) = 
            if (qty > perk.Quantity) then
                Some (sprintf "Too many perks claimed for id: %s" perk.Id)
            elif (qty < 0) then
                Some (sprintf "Perk quantity must be zero of more for id: %s" perk.Id)
            else
                None

        availablePerks 
        |> List.tryFind(fun p -> p.Id = perkId)
        |> function
        | None -> 
            Some (sprintf "Invalid perk id: %s" perkId)
        | Some(perk) -> 
            checkPerkQty perkQty perk 
        |> function
        | Some(errorMsg) -> 
            makeValidationError "Perks" errorMsg :: validationErrors
        | None -> 
            validationErrors
            

    let validateNewCharacter (character : NewCharacterRequest) = 
        validateRequiredString (character.Name, "name") []
        |> validateRequiredString (character.ClassName, "className") 
        |> toValidationResult character
        |> Result.mapError Msg

    let validateCharacterUpdate (glClass : GloomClass) (character : CharacterUpdateRequest) = 
        ([], character.Perks)
        ||> List.fold(fun acc p -> validatePerk glClass.Perks p.Id p.Quantity acc) 
        |> validateRequiredString (character.Name, "name")
        |> validatePositiveInt (character.Experience, "experience") 
        |> validatePositiveInt (character.Gold, "gold") 
        |> validatePositiveInt (character.Achievements, "achievements") 
        |> toValidationResult character
        |> Result.mapError Msg

    let validatePatchPerks (glClass : GloomClass) (character : CharacterUpdate) = 
        ([], character.Perks)
        ||> List.fold(fun acc p -> validatePerk glClass.Perks p.Id p.Quantity acc) 
        |> toValidationResult character
        |> Result.mapError Msg