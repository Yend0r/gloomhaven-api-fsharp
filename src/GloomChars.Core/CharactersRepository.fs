namespace GloomChars.Core

open System
open GloomChars.Common
open GloomChars.Common.QueryUtils
open FSharpPlus

[<RequireQualifiedAccess>]
module internal CharactersSql = 

    let getCharacters userId = 
        let (UserId uId) = userId

        sql
            """
            SELECT id        As Id,
                user_id      AS UserId,
                name         AS Name,
                class_name   AS ClassName,
                experience   AS Experience,
                gold         AS Gold,
                achievements AS Achievements
            FROM characters
            WHERE user_id = @userId
            ORDER BY name 
            """
            [ p "userId" uId ]

    let getCharacter characterId userId = 
        let (CharacterId cId) = characterId
        let (UserId uId) = userId

        sql
            """
            SELECT id        AS Id,
                user_id      AS UserId,
                name         AS Name,
                class_name   AS ClassName,
                experience   AS Experience,
                gold         AS Gold,
                achievements AS Achievements
            FROM characters
            WHERE user_id = @userId
                AND id = @id
            """
            [ 
                p "userId" uId 
                p "id" cId 
            ]

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
        let (CharacterId cId) = character.Id
        let (UserId uId) = character.UserId

        sql
            """
            UPDATE characters 
            SET user_id = @user_id, 
                name         = @name, 
                experience   = @experience, 
                gold         = @gold, 
                achievements = @achievements, 
                date_updated = @date_updated
            WHERE id = @id AND user_id = @user_id
            """
            [
                p "id" cId
                p "user_id" uId
                p "name" character.Name
                p "experience" character.Experience
                p "gold" character.Gold
                p "achievements" character.Achievements
                p "date_updated" DateTime.Now
            ]

    let deleteCharacter characterId userId = 
        let (CharacterId cId) = characterId
        let (UserId uId) = userId

        sql
            """
            DELETE FROM characters
            WHERE user_id = @userId
                AND id = @characterId
            """
            [ 
                p "userId" cId 
                p "id" uId 
            ]

[<RequireQualifiedAccess>]
module CharactersRepository = 

    let private mapToCharacter perks (dbCharater : DbCharacter) = 
        let gloomClassName = 
            match GloomClassName.fromString dbCharater.ClassName with
            | Some name -> name
            | None -> 
                //Bad data... need to manually fix
                let error = sprintf "Invalid Gloomhaven class name for character %i" dbCharater.Id
                raise (new Exception(error)) 

        { 
            Id = CharacterId dbCharater.Id
            UserId = UserId dbCharater.UserId
            Name = dbCharater.Name
            ClassName = gloomClassName
            Experience = dbCharater.Experience
            Gold = dbCharater.Gold
            Achievements = dbCharater.Achievements
            Perks = perks
        }

    let getCharacter (dbContext : IDbContext) characterId userId : Character option = 
        CharactersSql.getCharacter characterId userId
        |> dbContext.Query<DbCharacter>
        |> Array.tryHead
        |> map (mapToCharacter [])
         
    let getCharacters (dbContext : IDbContext) userId : Character list = 
        CharactersSql.getCharacters userId
        |> dbContext.Query<DbCharacter>
        |> map (fun c -> mapToCharacter [] c)
        |> Array.toList

    let insertNewCharacter (dbContext : IDbContext) (newCharacter : NewCharacter) = 
        CharactersSql.insertNewCharacter newCharacter
        |> dbContext.TryExecuteScalar
        |> function 
        | Success id -> 
            Ok id
        | UniqueConstraintError _ ->
            Error "You already have a character with that name and class." 

    let updateCharacter (dbContext : IDbContext) (character : CharacterUpdate) = 
        CharactersSql.updateCharacter character
        |> dbContext.TryExecuteScalar
        |> function 
        | Success count -> 
            Ok count
        | UniqueConstraintError _ ->
            Error "You already have a character with that name and class." 

    let deleteCharacter (dbContext : IDbContext) characterId userId = 
        CharactersSql.deleteCharacter characterId userId
        |> dbContext.Execute
        |> ignore        