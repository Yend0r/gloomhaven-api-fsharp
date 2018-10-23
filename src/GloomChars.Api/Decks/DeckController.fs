﻿namespace GloomChars.Api

module DeckController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open GloomChars.Core
    open ResponseHandlers
    open FSharpPlus
    open CompositionRoot
    open GloomChars.Common.Validation
    open DeckModels 

    let getDeck (ctx : HttpContext) (characterId : int) : HttpHandler = 
        WebAuthentication.getLoggedInUserId ctx
        >>= CharactersSvc.getCharacter (CharacterId characterId)
        |> map DeckSvc.getDeck
        |> map toDeckViewModel
        |> toJsonResponse "Character not found"

    //The draw/reshuffle thing is a bit RPC like because it doesn't really fit REST well
    //So make it REST-like, the actions require a POST because they are not idempotent
    let newDeck (ctx : HttpContext) (newDeckRequest : NewDeckRequest) (characterId : int) : HttpHandler = 
        let action = 
            match newDeckRequest.Action.ToUpper() with
            | "DRAW" -> fun c ->  c |> DeckSvc.drawCard |> Ok
            | "RESHUFFLE" -> fun c -> c |> DeckSvc.reshuffle |> Ok
            | _ -> fun _ -> sprintf "Invalid 'action': %s" newDeckRequest.Action |> Msg |> Error

        WebAuthentication.getLoggedInUserId ctx
        >>= CharactersSvc.getCharacter (CharacterId characterId)
        >>= action
        |> map toDeckViewModel
        |> toJsonResponse "Character not found"