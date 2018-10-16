namespace GloomChars.Api

module CharacterReadModels = 
    open GloomChars.Core
    open FSharpPlus

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

    type CharacterListModel =
        {
            Id           : int
            Name         : string
            ClassName    : string
            Experience   : int
            Gold         : int
        }

    let toPerkViewModel (perk : Perk) : PerkViewModel = 
        {
            Id       = perk.Id
            Quantity = perk.Quantity
            Actions  = perk.Actions |> PerkService.getText
        }

    let toViewModel (character : Character) : CharacterViewModel = 
        let (CharacterId cId) = character.Id 

        {
            Id           = cId
            Name         = character.Name
            ClassName    = character.ClassName.ToString()
            Experience   = character.Experience
            Gold         = character.Gold
            Achievements = character.Achievements
            Perks        = character.Perks |> map toPerkViewModel
        }

    let toListModel (character : CharacterListItem) : CharacterListModel = 
        let (CharacterId cId) = character.Id 

        {
            Id           = cId
            Name         = character.Name
            ClassName    = character.ClassName.ToString()
            Experience   = character.Experience
            Gold         = character.Gold
        }