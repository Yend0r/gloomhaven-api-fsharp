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
            ClassName  : string
            Name       : string
            Symbol     : string
            IsStarting : bool
            Perks      : PerkViewModel list
        }

    let toPerkViewModel (perk : Perk) : PerkViewModel = 
        {
            Id       = perk.Id
            Quantity = perk.Quantity
            Actions  = perk.Actions |> PerkService.getText
        }

    let toViewModel (gClass : GloomClass) : GloomClassViewModel = 
        {
            ClassName  = gClass.ClassName.ToString()
            Name       = gClass.Name
            Symbol     = gClass.Symbol
            IsStarting = gClass.IsStarting
            Perks      = gClass.Perks |> map toPerkViewModel
        }

module GameDataController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open ResponseHandlers
    open FSharpPlus
    open CompositionRoot
    open GameDataModels

    let listClasses (ctx : HttpContext) : HttpHandler = 
        GameDataSvc.gloomClasses
        |> map toViewModel
        |> jsonList

    let getClass (ctx : HttpContext) (className : string) : HttpHandler = 
        className
        |> GameDataSvc.getGloomClass
        |> map toViewModel
        |> toJsonResponse "Class not found"

