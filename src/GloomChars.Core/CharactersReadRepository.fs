namespace GloomChars.Core

open System
open GloomChars.Common
open GloomChars.Common.QueryUtils
open FSharpPlus

[<RequireQualifiedAccess>]
module internal CharactersReadSql = 

    let getCharacters userId = 
        let (UserId uId) = userId

        sql
            """
            SELECT id        As Id,
                name         AS Name,
                class_name   AS ClassName,
                experience   AS Experience,
                gold         AS Gold
            FROM characters
            WHERE user_id = @user_id
            ORDER BY name 
            """
            [ p "user_id" uId ]

    let getCharacter characterId userId = 
        let (CharacterId charId) = characterId
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
            WHERE user_id = @user_id
                AND id = @id;

            SELECT perk_id AS PerkId,
                quantity   AS Quantity
            FROM character_perks
            WHERE character_id = @id
            ORDER BY perk_id
                
            """
            [ 
                p "user_id" uId 
                p "id" charId 
            ]


[<RequireQualifiedAccess>]
module CharactersReadRepository = 

    let private getGloomClassName characterId className = 
        match GloomClassName.FromString className with
        | Some name -> name
        | None -> 
            //Bad data... need to manually fix
            let error = sprintf "Invalid Gloomhaven class name for character %i" characterId
            raise (new Exception(error)) 

    let private toPerk (allPerks : Perk list) (dbPerk : DbCharacterPerk) : Perk option = 
        allPerks 
        |> List.tryFind(fun p -> p.Id.ToUpper() = dbPerk.PerkId.ToUpper())
        |> function
        | Some p -> Some { p with Quantity = dbPerk.Quantity }
        | None -> None

    let private toCharacter (dbPerks : DbCharacterPerk []) (dbCharacter : DbCharacter) : Character = 

        let className = getGloomClassName dbCharacter.Id dbCharacter.ClassName

        let allPerks = (GameData.gloomClass className).Perks

        let perks : Perk list = 
            dbPerks 
            |> Array.toList 
            |> List.choose (fun p -> toPerk allPerks p) 

        { 
            Id = CharacterId dbCharacter.Id
            UserId = UserId dbCharacter.UserId
            Name = dbCharacter.Name
            ClassName = getGloomClassName dbCharacter.Id dbCharacter.ClassName
            Experience = dbCharacter.Experience
            Gold = dbCharacter.Gold
            Achievements = dbCharacter.Achievements
            Perks = perks
        }

    let private toCharacterListItem (dbCharater : DbCharacterListItem) : CharacterListItem = 
        { 
            Id = CharacterId dbCharater.Id
            Name = dbCharater.Name
            ClassName = getGloomClassName dbCharater.Id dbCharater.ClassName
            Experience = dbCharater.Experience
            Gold = dbCharater.Gold
        }

    let getCharacter (dbContext : IDbContext) characterId userId : Character option = 

        let (dbCharacter, dbPerks) = 
            CharactersReadSql.getCharacter characterId userId
            |> dbContext.QueryMulti2<DbCharacter, DbCharacterPerk>

        dbCharacter
        |> Array.tryHead
        |> map (toCharacter dbPerks)

    let getCharacters (dbContext : IDbContext) userId : CharacterListItem list = 
        CharactersReadSql.getCharacters userId
        |> dbContext.Query<DbCharacterListItem>
        |> map toCharacterListItem
        |> Array.toList
