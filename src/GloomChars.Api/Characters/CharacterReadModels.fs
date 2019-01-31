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
            Level        : int
            HP           : int
            PetHP        : int option
            Gold         : int
            Achievements : int
            ClaimedPerks : PerkViewModel list
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
            Level        = character.Level
            HP           = character.HP
            PetHP        = character.PetHP
            Gold         = character.Gold
            Achievements = character.Achievements
            ClaimedPerks = character.Perks |> map toPerkViewModel
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