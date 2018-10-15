namespace GloomChars.Api

module GameDataController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open GloomChars.Core
    open ResponseHandlers
    open FSharpPlus
    open CompositionRoot

    type PerkViewModel =
        {
            Id       : string 
            Quantity : int 
            Actions  : string 
        }

    type GloomClassViewModel =
        {
            ClassName  : string
            Name       : string
            Symbol     : string
            IsStarting : bool
            Perks      : PerkViewModel list
        }

    let private toPerkViewModel (perk : Perk) : PerkViewModel = 
        {
            Id       = perk.Id
            Quantity = perk.Quantity
            Actions  = perk.Actions |> PerkService.getText
        }

    let private toViewModel (gClass : GloomClass) : GloomClassViewModel = 
        {
            ClassName  = gClass.ClassName.ToString()
            Name       = gClass.Name
            Symbol     = gClass.Symbol
            IsStarting = gClass.IsStarting
            Perks      = gClass.Perks |> map toPerkViewModel
        }

    let listClasses (ctx : HttpContext) : HttpHandler = 
        GameDataSvc.gloomClasses
        |> map toViewModel
        |> jsonList

    let getClass (ctx : HttpContext) (className : string) : HttpHandler = 
        className
        |> GameDataSvc.getGloomClass
        |> map toViewModel
        |> resultToJson "Class not found"

