namespace GloomChars.Core

open System
open GloomChars.Common
open GloomChars.Common.QueryUtils
open FSharpPlus

[<RequireQualifiedAccess>]
module internal CharacterSql = 

    let getCharacters userId = 
        sql
            """
            SELECT id as Id,
                user_id AS UserId,
                name AS Name,
                class_name AS ClassName,
                experience AS Experience,
                gold AS Gold,
                achievements AS Achievements
            FROM characters
            WHERE user_id = @userId
            ORDER BY name 
            """
            [ p "userId" userId ]

    let getCharacter characterId userId = 
        sql
            """
            SELECT id as Id,
                user_id AS UserId,
                name AS Name,
                class_name AS ClassName,
                experience AS Experience,
                gold AS Gold,
                achievements AS Achievements
            FROM characters
            WHERE user_id = @userId
                AND id = @characterId
            """
            [ 
                p "userId" userId 
                p "id" characterId 
            ]

    let insertNewCharacter (character : NewCharacter) = 
        sql
            """
            INSERT INTO characters
                (user_id, 
                access_token, 
                is_revoked,  
                date_created, 
                date_expires)
            VALUES 
                (@user_id, 
                @access_token, 
                false, 
                @date_created, 
                @date_expires)
            RETURNING id
            """
            [
                p "user_id" character.UserId
                p "name" character.Name
                p "experience" character.Experience
            ]

    let updateCharacter (character : CharacterUpdate) = 
        sql
            """
            UPDATE characters 
            SET is_locked_out = @is_locked_out,
                login_attempt_number = @login_attempt_number,
                date_locked_out = @date_locked_out
            WHERE id = @id
            """
            [
                p "user_id" character.UserId
                p "name" character.Name
                p "experience" character.Experience
            ]

    let delete characterId userId = 
        sql
            """
            DELETE FROM characters
            WHERE user_id = @userId
                AND id = @characterId
            """
            [ 
                p "userId" userId 
                p "id" characterId 
            ]

[<RequireQualifiedAccess>]
module CharaterRepository = 
    
    let private mapToCharacter perks (dbCharater : DbCharacter) = 
        { 
            Id = dbCharater.Id
            UserId = dbCharater.UserId
            Name = dbCharater.Name
            ClassName = GloomClassName.fromString dbCharater.ClassName
            Experience = dbCharater.Experience
            Gold = dbCharater.Gold
            Achievements = dbCharater.Achievements
            Perks = perks
        }

    let getCharacter (dbContext : IDbContext) characterId userId : Character option = 
        CharacterSql.getCharacter characterId userId
        |> dbContext.Query<DbCharacter>
        |> Array.tryHead
        |> Option.map(mapToCharacter [])

    let getCharacters (dbContext : IDbContext) userId : Character list = 
        CharacterSql.getCharacters userId
        |> dbContext.Query<DbCharacter>
        |> Array.map (fun c -> mapToCharacter [] c)
        |> Array.toList

    let insertNewCharacter (dbContext : IDbContext) (newCharacter : NewCharacter) = 
        let result =
            CharacterSql.insertNewCharacter newCharacter
            |> dbContext.TryExecuteScalar

        match result with 
        | Success loginId -> 
            Ok loginId
        | UniqueConstraintError _ ->
            Error "You already have a character with that name and class." 

    let updateCharacter (dbContext : IDbContext) (character : CharacterUpdate) = 
        CharacterSql.updateCharacter character
        |> dbContext.Execute
        |> ignore

    let delete (dbContext : IDbContext) characterId userId = 
        CharacterSql.delete characterId userId
        |> dbContext.Execute
        |> ignore        