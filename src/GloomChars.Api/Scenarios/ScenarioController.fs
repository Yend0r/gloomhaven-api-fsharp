namespace GloomChars.Api

module ScenarioController = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open GloomChars.Core
    open ResponseHandlers
    open FSharpPlus
    open CompositionRoot
    open ScenarioModels 
    open GloomChars.Common.ResultExpr

    let private getCharacter ctx characterId = 
        WebAuthentication.getLoggedInUserId ctx
        >>= CharactersSvc.getCharacter (CharacterId characterId)

    let private toCreatedResult (ctx : HttpContext) (characterId : int) (scenario : ScenarioViewModel) : CreatedResult<ScenarioViewModel> = 
        let uri = sprintf "%s/characters/%i/scenarios/%i" (ctx.Request.Host.ToString()) characterId scenario.Id

        {
            Uri = uri
            Item = scenario
        }

    let getScenario (ctx : HttpContext) (characterId : int) : HttpHandler = 
        result {
            let! character = getCharacter ctx characterId
            let! scenario = ScenarioSvc.getScenario character 
            return toScenarioViewModel scenario
        }
        |> either toSuccess (toError "Scenario not found.")

        (* 
        // Alternative style that uses FSharpPlus
            WebAuthentication.getLoggedInUserId ctx
            >>= CharactersSvc.getCharacter (CharacterId characterId)
            |> map ScenarioSvc.getScenario
            >>= map toScenarioViewModel
            |> either toSuccess (toError "Scenario not found")
        *)

    let newScenario (ctx : HttpContext) (newScenarioRequest : NewScenarioRequest) (characterId : int) : HttpHandler = 
        result {
            let! character = getCharacter ctx characterId
            let! validScenario = validateNewScenario newScenarioRequest

            let newScenario = { Character = character; Name = validScenario.Name }

            let! addResult = ScenarioSvc.newScenario newScenario
            // .Net wants to return the created item with a 201, so get the item
            let! scenario = ScenarioSvc.getScenario character 
            let viewModel = toScenarioViewModel scenario

            return toCreatedResult ctx characterId viewModel 
        }
        |> either toCreated (toError "Failed to add scenario.")

    let scenarioStatsEvent (ctx : HttpContext) (eventRequest : EventRequest) (characterId : int) : HttpHandler = 
        result {
            let! event = 
                match eventRequest.Event.ToUpper() with 
                | "DAMAGED" ->          eventRequest.Amount |> Damaged          |> Ok
                | "HEALED" ->           eventRequest.Amount |> Healed           |> Ok
                | "EXPERIENCEGAINED" -> eventRequest.Amount |> ExperienceGained |> Ok
                | "EXPERIENCELOST" ->   eventRequest.Amount |> ExperienceLost   |> Ok
                | _ -> "Invalid event." |> Msg |> Error

            let! character = getCharacter ctx characterId
            let! scenario = ScenarioSvc.getScenario character 
            let updatedScenario = ScenarioSvc.processStatsEvent event character scenario
            return toScenarioViewModel updatedScenario
        }
        |> either toSuccess (toError "Failed to process event.")

    let scenarioDeckAction (ctx : HttpContext) (deckActionRequest : DeckActionRequest) (characterId : int) : HttpHandler = 
        result {
            let! action = 
                ScenarioDeckAction.FromString deckActionRequest.Action 
                |> optionToAppResultOrBadRequest "Invalid deck action."

            let! character = getCharacter ctx characterId
            let! scenario = ScenarioSvc.getScenario character 
            let updatedScenario = ScenarioSvc.processDeckAction action character scenario
            return toScenarioViewModel updatedScenario
        }
        |> either toSuccess (toError "Failed to process deck action.")

    let completeScenario ctx (characterId : int) : HttpHandler = 
        getCharacter ctx characterId
        |> map ScenarioSvc.completeScenario 
        |> either toSuccessNoContent (toError "Delete failed.")
        

    