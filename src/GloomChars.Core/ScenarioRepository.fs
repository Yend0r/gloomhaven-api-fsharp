namespace GloomChars.Core
open System

type DbScenario = 
    {
        Id            : int
        CharacterId   : int
        Name          : string
        Health        : int
        MaxHealth     : int
        Experience    : int
        DateStarted   : DateTime
        DateLastEvent : DateTime 
    }

[<RequireQualifiedAccess>]
module ScenarioSql = 
    open GloomChars.Common.QueryUtils

    let getScenario characterId =
        let (CharacterId charId) = characterId

        sql
            """
                SELECT id           AS Id, 
                    character_id    AS CharacterId, 
                    name            AS Name,
                    health          AS Health, 
                    max_health      AS MaxHealth, 
                    experience      AS Experience, 
                    date_started    AS DateStarted, 
                    date_last_event AS DateLastEvent 
            FROM scenarios 
            WHERE character_id = @character_id 
                AND is_active = true
            """
            [ 
                p "character_id" charId 
            ]

    let insertNewScenario characterId name characterHp = 
        let (CharacterId charId) = characterId

        sql
            """
                INSERT INTO scenarios
                    (is_active, 
                    character_id, 
                    name, 
                    health, 
                    max_health, 
                    experience, 
                    date_started, 
                    date_last_event)
                VALUES 
                    (true,
                    @character_id, 
                    @name, 
                    @health, 
                    @max_health, 
                    0, 
                    @date_started, 
                    @date_last_event)
            """
            [
                p "character_id" charId
                p "name" name
                p "health" characterHp
                p "max_health" characterHp
                p "date_started" DateTime.Now
                p "date_last_event" DateTime.Now
            ]

    let completeScenario characterId = 
        let (CharacterId charId) = characterId

        sql
            """                
                UPDATE scenarios 
                SET date_completed = @date_completed,
                    is_active = false
                WHERE character_id = @character_id 
                    AND is_active = true;

            """
            [
                p "date_completed" DateTime.Now
                p "character_id" charId
            ]   

    let updateCharacterStats scenarioId (stats : ScenarioCharacterStats) = 
        sql
            """                
                UPDATE scenarios 
                SET health          = @health,
                    experience      = @experience,
                    date_last_event = @date_last_event
                WHERE id = @scenario_id;
            """
            [
                p "health" stats.Health
                p "experience" stats.Experience
                p "date_last_event" DateTime.Now
                p "scenario_id" scenarioId
            ]  

module ScenarioRepository = 
    open System
    open GloomChars.Core
    open GloomChars.Common
    open FSharpPlus

    let private toScenario (dbScenario : DbScenario) = 
        let scenario : ScenarioInfo = 
            {
                Id            = dbScenario.Id
                CharacterId   = CharacterId dbScenario.CharacterId
                Name          = dbScenario.Name
                MaxHealth     = dbScenario.MaxHealth
                DateStarted   = dbScenario.DateStarted
                DateLastEvent = dbScenario.DateLastEvent
            } 

        let stats : ScenarioCharacterStats = 
            {
                Health        = dbScenario.Health
                Experience    = dbScenario.Experience
            } 

        (scenario, stats)

    let completeActiveScenarios (dbContext : IDbContext) characterId = 
        ScenarioSql.completeScenario characterId
        |> dbContext.Execute

    let insertNewScenario (dbContext : IDbContext) characterId name characterHp = 
        //First complete any active scenarios
        completeActiveScenarios dbContext characterId 
        |> ignore

        // Insert new active scenario
        ScenarioSql.insertNewScenario characterId name characterHp
        |> dbContext.Execute
        |> Ok

    let getScenario (dbContext : IDbContext) characterId = 
        ScenarioSql.getScenario characterId
        |> dbContext.Query<DbScenario>
        |> Array.tryHead
        |> map toScenario 

    let updateCharacterStats (dbContext : IDbContext) scenarioId (stats : ScenarioCharacterStats) =
        ScenarioSql.updateCharacterStats scenarioId stats
        |> dbContext.Execute
        |> ignore
