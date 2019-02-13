namespace GloomChars.Core

module ScenarioService = 
    open System    
    open FSharpPlus

    let newScenario 
        (dbInsertNewScenario : CharacterId -> string -> int -> Result<int, string>) 
        (reshuffle : Character -> ModifierDeck)
        (newScenario : NewScenario) = 

        let character = newScenario.Character
        let gloomClass = GameData.gloomClass character.ClassName
        let characterHp = GameData.getHP gloomClass character.Experience

        reshuffle character |> ignore

        dbInsertNewScenario character.Id newScenario.Name characterHp

    let completeScenario 
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

    let getScenario 
        (dbGetScenario : CharacterId -> (ScenarioInfo * ScenarioCharacterStats) option) 
        (getDeck : Character -> ModifierDeck) 
        (character : Character) : ScenarioState option  =

        dbGetScenario character.Id 
        |> map (fun (scenario, stats) -> 
            {
                Info           = scenario
                CharacterStats = stats
                ModifierDeck   = getDeck character
            })

    let updateStats 
        (dbUpdateCharacterStats : int -> ScenarioCharacterStats -> unit)
        (scenarioState : ScenarioState) 
        (stats : ScenarioCharacterStats) =

        let getValidHp hp = 
            if (hp < 0) then 0 
            elif hp > scenarioState.Info.MaxHealth then scenarioState.Info.MaxHealth
            else hp
        let getValidXp xp = 
            if (xp < 0) then 0 
            else xp

        let validStats = { stats with Health = getValidHp stats.Health; Experience = getValidXp stats.Experience }

        (scenarioState.Info.Id, validStats)
        ||> dbUpdateCharacterStats 

        { scenarioState with CharacterStats = validStats }

    let drawCard 
        (drawCard : Character -> ModifierDeck)
        (character : Character)
        (scenarioState : ScenarioState) =

        { scenarioState with ModifierDeck = drawCard character }  

    let reshuffle 
        (reshuffle : Character -> ModifierDeck)
        (character : Character)
        (scenarioState : ScenarioState) =

        { scenarioState with ModifierDeck = reshuffle character }
