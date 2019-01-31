namespace GloomChars.Api

module GameDataModels = 
    open GloomChars.Core
    open FSharpPlus

    type PerkViewModel =
        {
            Id       : string 
            Quantity : int 
            Actions  : string 
        }

    type GloomClassViewModel =
        {
            ClassName   : string
            Name        : string
            Symbol      : string
            IsStarting  : bool
            Perks       : PerkViewModel list
            XPLevels    : int list
            HPLevels    : int list
            PetHPLevels : int list option
        }

    let toPerkViewModel (perk : Perk) : PerkViewModel = 
        {
            Id       = perk.Id
            Quantity = perk.Quantity
            Actions  = perk.Actions |> PerkService.getText
        }

    let toViewModel (gClass : GloomClass) : GloomClassViewModel = 
        {
            ClassName   = gClass.ClassName.ToString()
            Name        = gClass.Name
            Symbol      = gClass.Symbol
            IsStarting  = gClass.IsStarting
            Perks       = gClass.Perks |> map toPerkViewModel
            XPLevels    = GameData.xpLevels
            HPLevels    = gClass.HPLevels
            PetHPLevels = gClass.PetHPLevels
        }