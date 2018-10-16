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

module CharacterReadController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open GloomChars.Core
    open ResponseHandlers
    open FSharpPlus
    open CompositionRoot
    open CharacterReadModels

    let listCharacters (ctx : HttpContext) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        |> map CharactersSvc.getCharacters 
        |> map (List.map toListModel)
        |> toJsonListResponse 

    let getCharacter (ctx : HttpContext) (id : int) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        >>= CharactersSvc.getCharacter (CharacterId id)
        |> Result.map toViewModel
        |> toJsonResponse "Character not found"

