namespace GloomChars.Core

open System
open GloomChars.Common
open GloomChars.Common.QueryUtils
open FSharpPlus

[<RequireQualifiedAccess>]
module internal CharactersEditSql = 

    let insertNewCharacter (character : NewCharacter) = 
        let (UserId uId) = character.UserId

        sql
            """
            INSERT INTO characters
                (user_id, 
                name, 
                class_name, 
                experience, 
                gold, 
                achievements,
                date_created,
                date_updated)
            VALUES 
                (@user_id, 
                @name, 
                @class_name, 
                0, 
                0, 
                0,
                @date_created,
                @date_updated)
            RETURNING id
            """
            [
                p "user_id" uId
                p "name" character.Name
                p "class_name" (character.ClassName.ToString())
                p "date_created" DateTime.Now
                p "date_updated" DateTime.Now
            ]

    let updateCharacter (character : CharacterUpdate) = 
        let (CharacterId charId) = character.Id
        let (UserId uId) = character.UserId

        sql
            """
            UPDATE characters 
            SET name         = @name, 
                experience   = @experience, 
                gold         = @gold, 
                achievements = @achievements, 
                date_updated = @date_updated
            WHERE id = @id 
                AND user_id = @user_id
            """
            [
                p "id" charId
                p "user_id" uId
                p "name" character.Name
                p "experience" character.Experience
                p "gold" character.Gold
                p "achievements" character.Achievements
                p "date_updated" DateTime.Now
            ]

    let deleteCharacter characterId userId = 
        let (CharacterId charId) = characterId
        let (UserId uId) = userId

        sql
            """
            DELETE FROM characters
            WHERE user_id = @user_id
                AND id = @character_id;

            DELETE FROM character_perks
            WHERE character_id = @character_id;
            """
            [ 
                p "user_id" uId 
                p "character_id" charId 
            ]

    let deletePerks characterId = 
        let (CharacterId charId) = characterId

        sql
            """
            DELETE FROM character_perks
            WHERE character_id = @id;
            """
            [ p "id" charId ]

    let insertPerk characterId (perk : PerkUpdate) = 
        let (CharacterId charId) = characterId

        sql
            """
            INSERT INTO character_perks
                (character_id, perk_id, quantity)
            VALUES 
                (@character_id, @perk_id, @quantity)
            RETURNING id
            """
            [
                p "character_id" charId
                p "perk_id" perk.Id
                p "quantity" perk.Quantity
            ]
    let insertPerks characterId (perks : PerkUpdate list) = 
        let (CharacterId charId) = characterId
        let queryParams = 
            perks
            |> List.map (fun perk ->
                [
                    p "character_id" charId
                    p "perk_id" perk.Id
                    p "quantity" perk.Quantity
                ])
            |> List.toArray

        sqlMulti
            """
            INSERT INTO character_perks
                (character_id, perk_id, quantity)
            VALUES 
                (@character_id, @perk_id, @quantity)
            """
            queryParams

[<RequireQualifiedAccess>]
module CharactersEditRepository = 

    let private insertPerk (dbContext : IDbContext) characterId perk = 
        CharactersEditSql.insertPerk characterId perk
        |> dbContext.Execute
        |> ignore

    let private updatePerks (dbContext : IDbContext) characterId perks = 
        CharactersEditSql.deletePerks characterId
        |> dbContext.Execute
        |> ignore

        CharactersEditSql.insertPerks characterId perks
        |> dbContext.ExecuteMulti
        |> ignore

    let insertNewCharacter (dbContext : IDbContext) (newCharacter : NewCharacter) = 
        CharactersEditSql.insertNewCharacter newCharacter
        |> dbContext.TryExecuteScalar
        |> function 
        | Success id -> 
            Ok id
        | UniqueConstraintError _ ->
            Error "You already have a character with that name and class." 

    let updateCharacter (dbContext : IDbContext) (character : CharacterUpdate) = 
        CharactersEditSql.updateCharacter character
        |> dbContext.TryExecute
        |> function 
        | Success count -> 
            updatePerks dbContext character.Id character.Perks
            Ok count
        | UniqueConstraintError _ ->
            Error "You already have a character with that name and class." 

    let deleteCharacter (dbContext : IDbContext) characterId userId = 
        CharactersEditSql.deleteCharacter characterId userId
        |> dbContext.Execute


