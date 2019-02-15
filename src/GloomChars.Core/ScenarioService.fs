namespace GloomChars.Core

module ScenarioService = 
    open System    
    open FSharpPlus

    let private mergeStats (statsUpdate : StatsUpdate) (scenario : ScenarioState) : ScenarioCharacterStats = 
        let max = scenario.Info.MaxHealth
        let stats = scenario.CharacterStats

        let getValidHp max hp =             
            if hp < 0 then 0 
            elif hp > max then max
            else hp
   
        let newHp = Option.defaultValue stats.Health statsUpdate.Health |> getValidHp max
            
        let getValidXp xp = 
            if xp < 0 then 0 
            else xp

        let newXp = Option.defaultValue stats.Experience statsUpdate.Experience |> getValidXp

        { 
            Health = newHp
            Experience = newXp
        }

    let newScenario 
        (dbInsertNewScenario : CharacterId -> string -> int -> Result<int, string>) 
        (reshuffle : Character -> ModifierDeck)
        (character : Character)
        name = 

        let gloomClass = GameData.gloomClass character.ClassName
        let characterHp = GameData.getHP gloomClass character.Experience

        reshuffle character |> ignore

        dbInsertNewScenario character.Id name characterHp

    let complete 
        (dbGetScenario : CharacterId -> (ScenarioInfo * ScenarioCharacterStats) option) 
        (dbCompleteScenario : CharacterId -> int) 
        (reshuffle : Character -> ModifierDeck)
        (character : Character) = 

        dbGetScenario character.Id 
        |> function
        | Some (scenario, stats) -> 
            reshuffle character |> ignore
            Ok (dbCompleteScenario character.Id)
        | None -> 
            Error "Scenario not found"

    let get 
        (dbGetScenario : CharacterId -> (ScenarioInfo * ScenarioCharacterStats) option) 
        (getDeck : Character -> ModifierDeck) 
        (character : Character) : Result<ScenarioState, string>  =

        dbGetScenario character.Id 
        |> function 
        | Some(info, stats) ->  
            Ok {
                Info           = info
                CharacterStats = stats
                ModifierDeck   = getDeck character
            }
        | None -> 
            Error "Could not find scenario." 

    let updateStats 
        (dbUpdateCharacterStats : int -> ScenarioCharacterStats -> unit)
        (getScenario : Character -> Result<ScenarioState, string>) 
        (character : Character) 
        (statsUpdate : StatsUpdate) =

        getScenario character
        |> map (fun scenarioState -> 
            let newStats = mergeStats statsUpdate scenarioState

            (scenarioState.Info.Id, newStats)
            ||> dbUpdateCharacterStats
    
            { scenarioState with CharacterStats = newStats }
        ) 

    let drawCard 
        (drawCard : Character -> ModifierDeck)
        (getScenario : Character -> Result<ScenarioState, string>) 
        (character : Character) =

        getScenario character
        |> map (fun scenarioState -> { scenarioState with ModifierDeck = drawCard character })  

    let reshuffle 
        (reshuffle : Character -> ModifierDeck)
        (getScenario : Character -> Result<ScenarioState, string>) 
        (character : Character) =

        getScenario character
        |> map (fun scenarioState -> { scenarioState with ModifierDeck = reshuffle character })

    let create db (deckSvc : IDeckService) = 

        let dbInsertNewScenario = ScenarioRepository.insertNewScenario db
        let dbCompleteScenario  = ScenarioRepository.completeActiveScenarios db
        let dbGetScenario = ScenarioRepository.getScenario db
        let dbUpdateCharacterStats = ScenarioRepository.updateCharacterStats db

        let getScenario = get dbGetScenario deckSvc.Get

        { new IScenarioService with 
            member __.Get character = 
                getScenario character

            member __.NewScenario character name = 
                newScenario dbInsertNewScenario deckSvc.Reshuffle character name

            member __.Complete character = 
                complete dbGetScenario dbCompleteScenario deckSvc.Reshuffle character

            member __.UpdateStats character stats = 
                updateStats dbUpdateCharacterStats getScenario character stats

            member __.DrawCard character = 
                drawCard deckSvc.Draw getScenario character

            member __.Reshuffle character = 
                reshuffle deckSvc.Reshuffle getScenario character
        }