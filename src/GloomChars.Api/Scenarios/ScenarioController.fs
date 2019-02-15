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
        >>= CharactersSvc.get (CharacterId characterId)

    let private toCreatedResult (ctx : HttpContext) (characterId : int) (scenario : ScenarioViewModel) : CreatedResult<ScenarioViewModel> = 
        let uri = sprintf "%s/characters/%i/scenarios" (ctx.Request.Host.ToString()) characterId 

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
            getCharacter ctx characterId
            |> map ScenarioSvc.getScenario
            >>= map toScenarioViewModel
            |> either toSuccess (toError "Scenario not found")
        *)

    let newScenario (ctx : HttpContext) (newScenarioRequest : NewScenarioRequest) (characterId : int) : HttpHandler = 
        result {
            let! character = getCharacter ctx characterId
            let! validScenario = validateNewScenario newScenarioRequest
            let! addResult = ScenarioSvc.newScenario character validScenario.Name
            // .Net wants to return the created item with a 201, so get the item
            let! scenario = ScenarioSvc.getScenario character 
            let viewModel = toScenarioViewModel scenario

            return toCreatedResult ctx characterId viewModel 
        }
        |> either toCreated (toError "Failed to add scenario.")

    let private toStatsUpdate (statsPatch : StatsPatchRequest) : StatsUpdate = 
        { 
            Health = statsPatch.Health 
            Experience = statsPatch.Experience 
        }

    let patchScenarioStats (ctx : HttpContext) (statsPatch : StatsPatchRequest) (characterId : int) : HttpHandler = 
        result {
            let! character = getCharacter ctx characterId
            let statsUpdate = toStatsUpdate statsPatch 
            let! updatedScenario = ScenarioSvc.updateStats character statsUpdate
            return toScenarioViewModel updatedScenario
        }
        |> either toSuccess (toError "Failed to process stats update.")

    let scenarioDeckAction (ctx : HttpContext) (deckActionRequest : DeckActionRequest) (characterId : int) : HttpHandler = 
        result {
            let! action = 
                ScenarioDeckAction.FromString deckActionRequest.Action 
                |> optionToAppResultOrBadRequest "Invalid deck action."

            let! character = getCharacter ctx characterId
            let! updatedScenario = 
                match action with
                | DrawCard -> ScenarioSvc.drawCard character 
                | Reshuffle -> ScenarioSvc.reshuffle character 
            return toScenarioViewModel updatedScenario
        }
        |> either toSuccess (toError "Failed to process deck action.")

    let completeScenario ctx (characterId : int) : HttpHandler = 
        getCharacter ctx characterId
        |> map ScenarioSvc.completeScenario 
        |> either toSuccessNoContent (toError "Scenario completion failed.")
        

    