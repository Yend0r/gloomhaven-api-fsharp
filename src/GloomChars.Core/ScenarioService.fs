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

    let processStatsEvent 
        (dbUpdateCharacterStats : int -> ScenarioCharacterStats -> unit)
        (event : ScenarioStatsEvent) 
        (character : Character)
        (scenarioState : ScenarioState)=

        let stats = scenarioState.CharacterStats

        let getValidHp hp = 
            if (hp < 0) then 0 
            elif hp > scenarioState.Info.MaxHealth then scenarioState.Info.MaxHealth
            else hp
        let getValidXp xp = 
            if (xp < 0) then 0 
            else xp

        let updatedStats = 
            match event with
            | Damaged amount ->
                { stats with Health = getValidHp (stats.Health - amount) }              
            | Healed amount ->
                { stats with Health = getValidHp (stats.Health + amount) } 
            | ExperienceGained amount ->
                { stats with Experience = getValidXp (stats.Experience + amount) } 
            | ExperienceLost amount ->
                { stats with Experience = getValidXp (stats.Experience - amount) } 

        (scenarioState.Info.Id, updatedStats)
        ||> dbUpdateCharacterStats 

        { scenarioState with CharacterStats = updatedStats }

    let processDeckAction 
        (drawCard : Character -> ModifierDeck)
        (reshuffle : Character -> ModifierDeck)
        (action : ScenarioDeckAction)
        (character : Character)
        (scenarioState : ScenarioState) =

        let updatedDeck = 
            match action with
            | DrawCard -> drawCard character  
            | Reshuffle -> reshuffle character   

        { scenarioState with ModifierDeck = updatedDeck }  


