namespace GloomChars.Api

module DeckController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open GloomChars.Core
    open ResponseHandlers
    open FSharpPlus
    open CompositionRoot
    open DeckModels 

    let getDeck (ctx : HttpContext) (characterId : int) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        >>= CharactersSvc.getCharacter (CharacterId characterId)
        |> map DeckSvc.getDeck
        |> map toDeckViewModel
        |> either toSuccess (toError "Character not found")

    //The draw/reshuffle thing is a bit RPC like because it doesn't really fit REST well
    //So make it REST-like, the actions require a POST because they are not idempotent
    let deckAction (ctx : HttpContext) (deckActionRequest : DeckActionRequest) (characterId : int) : HttpHandler = 
        let action = 
            match deckActionRequest.Action.ToUpper() with
            | "DRAW"      -> fun c -> c |> (DeckSvc.drawCard >> Ok)
            | "RESHUFFLE" -> fun c -> c |> (DeckSvc.reshuffle >> Ok)
            | _ -> fun _ -> sprintf "Invalid 'action': %s" deckActionRequest.Action |> Msg |> Error

        WebAuthentication.getLoggedInUserId ctx
        >>= CharactersSvc.getCharacter (CharacterId characterId)
        >>= action
        |> map toDeckViewModel
        |> either toSuccess (toError "Character not found")