namespace GloomChars.Core

open System
open GloomChars.Common
open GloomChars.Common.QueryUtils
open FSharpPlus

type DbCharacter = 
    { 
        Id           : int
        UserId       : int
        Name         : string
        ClassName    : string
        Experience   : int
        Gold         : int
        Achievements : int
    }

type DbCharacterListItem = 
    { 
        Id         : int
        Name       : string
        ClassName  : string
        Experience : int
        Gold       : int
        ScenarioId : int option
    }

type DbCharacterPerk = 
    {
        PerkId   : string
        Quantity : int
    }

[<RequireQualifiedAccess>]
module internal CharactersReadSql = 

    let getCharacters userId = 
        let (UserId uId) = userId

        sql
            """
            SELECT characters.id      AS Id,
                characters.name       AS Name,
                characters.class_name AS ClassName,
                characters.experience AS Experience,
                characters.gold       AS Gold,
                scenarios.id          AS ScenarioId
            FROM characters LEFT JOIN scenarios 
                ON characters.id = scenarios.character_id
                AND scenarios.is_active = true
            WHERE characters.user_id = @user_id                
            ORDER BY characters.name 
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
                AND id = @character_id;

            SELECT perk_id AS PerkId,
                quantity   AS Quantity
            FROM character_perks
            WHERE character_id = @character_id
            ORDER BY perk_id                
            """
            [ 
                p "user_id" uId 
                p "character_id" charId 
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
        |> List.tryFind(fun p -> String.Equals(p.Id, dbPerk.PerkId, StringComparison.OrdinalIgnoreCase))
        |> function
        | Some p -> Some { p with Quantity = dbPerk.Quantity }
        | None -> None

    let private toCharacter (dbPerks : DbCharacterPerk []) (dbCharacter : DbCharacter) : Character = 

        let className = getGloomClassName dbCharacter.Id dbCharacter.ClassName
        let gloomClass = GameData.gloomClass className
        let allPerks = gloomClass.Perks

        let claimedPerks : Perk list = 
            dbPerks 
            |> Array.toList 
            |> List.choose (fun p -> toPerk allPerks p) 

        { 
            Id           = CharacterId dbCharacter.Id
            UserId       = UserId dbCharacter.UserId
            Name         = dbCharacter.Name
            ClassName    = className
            Experience   = dbCharacter.Experience            
            Gold         = dbCharacter.Gold
            Achievements = dbCharacter.Achievements
            ClaimedPerks = claimedPerks
        }

    let private toCharacterListItem (dbCharacter : DbCharacterListItem) : CharacterListItem = 
        { 
            Id         = CharacterId dbCharacter.Id
            Name       = dbCharacter.Name
            ClassName  = getGloomClassName dbCharacter.Id dbCharacter.ClassName
            Experience = dbCharacter.Experience
            Gold       = dbCharacter.Gold
            ScenarioId = dbCharacter.ScenarioId
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
