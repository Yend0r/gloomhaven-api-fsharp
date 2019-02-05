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
            | None -> character.ClaimedPerks |> List.map perkToUpdate

        {
            Id           = character.Id
            UserId       = character.UserId
            Name         = getPatchProp patch.Name character.Name
            Experience   = getPatchProp patch.Experience character.Experience
            Gold         = getPatchProp patch.Gold character.Gold
            Achievements = getPatchProp patch.Achievements character.Achievements
            Perks        = perks
        }

    let private validatePerk (availablePerks : Perk list) perkId perkQty (validationErrors : ValidationError list) = 
        let perkOpt = availablePerks |> List.tryFind(fun p -> p.Id = perkId)
        match perkOpt with
        | None -> 
            makeValidationError "Perks" (sprintf "Invalid perk id: %s" perkId) :: validationErrors
        | Some(perk) -> 
            if (perkQty > perk.Quantity) then
                makeValidationError "Perks" (sprintf "Too many perks claimed for id: %s" perkId) :: validationErrors
            elif (perkQty < 0) then
                makeValidationError "Perks" (sprintf "Perk quantity must be zero of more for id: %s" perkId) :: validationErrors
            else
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