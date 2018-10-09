namespace GloomChars.Core

type DbModifierCard = 
    {
        Id           : int
        Damage       : int
        DrawAnother  : bool 
        Reshuffle    : bool 
        Action       : string 
        ActionAmount : int option
    }

type DbPerk = 
    {
        Id             : int
        Quantity       : int 
        ClassId        : int
        SortOrder      : int
        PerkAction     : string 
        NumCards       : int
        ModifierCardId : int option
    }

type DbGloomClass = 
    {
        Id         : int
        ClassName  : string
        Name       : string
        Symbol     : string
        IsStarting : bool
    }

[<RequireQualifiedAccess>]
module GameSql = 
    open GloomChars.Common.QueryUtils

    let getClasses =
        sql
            """
            SELECT id       AS Id, 
                class_name  AS ClassName, 
                name        AS Name, 
                symbol      AS Symbol,
                is_starting AS IsStarting
            FROM gloom_classes
            ORDER BY is_starting, class_name;

            SELECT  
                perks.id                 AS Id,
                perks.quantity           AS Quantity, 
                perks.class_id           AS ClassId,
                perks.sort_order         AS SortOrder,
                actions.action           AS PerkAction, 
                actions.num_cards        AS NumCards,
                actions.modifier_card_id AS ModifierCardId,
                actions.sort_order       AS SortOrder
            FROM perks 
                INNER JOIN perk_actions AS actions
                ON perks.id = actions.perk_id;

            SELECT id              AS Id, 
                damage             AS Damage, 
                draw_another       AS DrawAnother, 
                reshuffle          AS Reshuffle, 
                card_action        AS Action, 
                card_action_amount AS ActionAmount
            FROM modifier_cards;
            """
            []

[<RequireQualifiedAccess>]
module GameRepository = 
    open System
    open System.Collections.Generic
    open GloomChars.Core
    open GloomChars.Common
    open GloomChars.Common.QueryUtils

    let private mapToPerk (dbPerk : DbPerk) (modCards : IDictionary<int, ModifierCard>) = 
        match dbPerk.PerkAction.ToUpper() with
        | "IGNORESCENARIOEFFECTS" ->
            IgnoreScenarioEffects
        | "IGNOREITEMOEFFECTS" ->
            IgnoreItemEffects
        | "REMOVECARD" ->
            let cardAction = 
                {
                    NumCards = dbPerk.NumCards
                    Card     = modCards.Item 0
                }
            RemoveCard cardAction
        | "ADDACTION" -> 
            let cardAction = 
                {
                    NumCards = dbPerk.NumCards
                    Card     = modCards.Item 0
                }
            AddCard cardAction 

    let private mapToActions dbPerk modCards = 
        dbPerk 
        |> List.filter(fun p -> p.ClassId = classId)
        |> List.sortBy(fun p -> p.SortOrder)
        |> List.map(fun p -> mapToPerk p modCards)

    let private mapToPerk (dbPerk : DbPerk) modCards = 
        {
            Id       = dbPerk.Id
            Quantity = dbPerk.Quantity
            Actions  = mapToActions dbPerk modCards
        }

    let private mapToPerks classId dbPerks modCards = 
        dbPerks 
        |> List.filter(fun p -> p.ClassId = classId)
        |> List.sortBy(fun p -> p.SortOrder)
        |> List.map(fun p -> mapToPerk p modCards)

    let mapToModifierCard dbModCard = 
        let cardAction = 
            match (dbModCard.Action.ToUpper(), dbModCard.ActionAmount) with
            | ("MISS", _)                       -> Miss  
            | ("DAMAGE", _)                     -> Damage 
            | ("DISARM", _)                     -> Disarm 
            | ("STUN", _)                       -> Stun 
            | ("POISON", _)                     -> Poison 
            | ("WOUND", _)                      -> Wound 
            | ("MUDDLE", _)                     -> Muddle 
            | ("ADDTARGET", _)                  -> AddTarget 
            | ("IMMOBILISE", _)                 -> Immobilise 
            | ("INVISIBLE", _)                  -> Invisible 
            | ("FIRE", _)                       -> Fire 
            | ("ICE", _)                        -> Ice 
            | ("LIGHT", _)                      -> Light 
            | ("AIR", _)                        -> Air 
            | ("DARK", _)                       -> Dark 
            | ("EARTH", _)                      -> Earth 
            | ("CURSE", _)                      -> Curse 
            | ("REFRESHITEM", _)                -> RefreshItem 
            | ("MULTIPLYDAMAGE", Some amount)   -> MultiplyDamage (DamageMultiplier amount)
            | ("PUSH", Some amount)             -> Push (PushAmount amount)
            | ("PULL", Some amount)             -> Pull (PullAmount amount)
            | ("PIERCE", Some amount)           -> Pierce (PierceAmount amount)
            | ("HEAL", Some amount)             -> Heal (HealAmount amount)
            | ("SHIELD", Some amount)           -> Shield (ShieldAmount amount)
            | _ -> raise (new Exception("Invalid Gloomhaven mod card action")) //Bad data... need to manually fix

        {
            DrawAnother = dbModCard.DrawAnother
            Reshuffle   = dbModCard.Reshuffle
            Action      = cardAction
            Damage      = dbModCard.Damage
        }

    let private mapToGloomClass (dbClass : DbGloomClass) (dbPerks : DbPerk list) (dbModCards : DbModifierCard list) = 
        //Put the modifier cards into a dictionary
        let modCards = 
            dbModCards
            |> List.map(fun c -> c.Id, mapToModifierCard c)
            |> dict

        {
            ClassName  = GloomClassName.fromString dbClass.ClassName
            Name       = dbClass.Name
            Symbol     = dbClass.Symbol
            IsStarting = dbClass.IsStarting
            Perks      = mapToPerks dbClass.Id dbPerks modCards
        }

    let getClasses (db : IDbContext) () = 

        let (dbClasses, dbPerks, dbModCards) = 
            GameSql.getClasses  
            |> dbContext.QueryMulti3<DbGloomClass list, DbPerk list, DbModifierCard list>

        
           