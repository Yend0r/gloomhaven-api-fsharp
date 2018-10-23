namespace GloomChars.Core

open System

type DbModifierCard = 
    {
        Id           : int
        Damage       : int
        DrawAnother  : bool 
        Reshuffle    : bool 
        Action       : string 
        ActionAmount : int option
    }

type DbNewModifierCard = 
    {
        Damage        : int
        DrawAnother   : bool 
        Reshuffle     : bool 
        Action        : string 
        ActionAmount  : int option
        CharacterId   : int
        DateDiscarded : DateTime
    }

[<RequireQualifiedAccess>]
module DeckSql = 
    open GloomChars.Common.QueryUtils

    let getDiscards characterId =
        let (CharacterId charId) = characterId

        sql
            """
                SELECT id          AS Id, 
                damage             AS Damage, 
                draw_another       AS DrawAnother, 
                reshuffle          AS Reshuffle, 
                card_action        AS Action, 
                card_action_amount AS ActionAmount
            FROM modifier_discards 
            WHERE character_id = @character_id
            ORDER BY date_discarded DESC
            """
            [ p "character_id" charId ]

    let insertDiscard (card : DbNewModifierCard) = 
        sql
            """
                INSERT INTO modifier_discards
                    (damage, 
                    draw_another, 
                    reshuffle, 
                    card_action, 
                    card_action_amount, 
                    character_id, 
                    date_discarded)
                VALUES 
                    (@damage, 
                    @draw_another, 
                    @reshuffle, 
                    @card_action, 
                    @card_action_amount, 
                    @character_id, 
                    @date_discarded)
            """
            [
                p "damage" card.Damage
                p "draw_another" card.DrawAnother 
                p "reshuffle" card.Reshuffle
                p "card_action" card.Action
                p "card_action_amount" card.ActionAmount
                p "character_id" card.CharacterId
                p "date_discarded" card.DateDiscarded
            ]

    let deleteDiscards characterId =
        let (CharacterId charId) = characterId

        sql
            """
            DELETE FROM modifier_discards 
            WHERE character_id = @character_id
            """
            [ p "character_id" charId ]


[<RequireQualifiedAccess>]
module DeckRepository = 
    open System
    open System.Collections.Generic
    open GloomChars.Core
    open GloomChars.Common
    open GloomChars.Common.QueryUtils
    open FSharpPlus

    let toModifierCard (dbModCard : DbModifierCard) = 
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

    let private getActionAmount (cardAction : CardAction) = 
        match cardAction with 
        | MultiplyDamage damageMultiplier ->
            let (DamageMultiplier amount) = damageMultiplier
            Some amount
        | Push pushAmount->
            let (PushAmount amount) = pushAmount
            Some amount
        | Pull pullAmount->
            let (PullAmount amount) = pullAmount
            Some amount
        | Pierce pierceAmount->
            let (PierceAmount amount) = pierceAmount
            Some amount
        | Heal healAmount->
            let (HealAmount amount) = healAmount
            Some amount
        | Shield shieldAmount->
            let (ShieldAmount amount) = shieldAmount
            Some amount
        | _ -> None

    let toDbNewModifierCard characterId (card : ModifierCard) = 
        let (CharacterId charId) = characterId

        {
            Damage        = card.Damage
            DrawAnother   = card.DrawAnother
            Reshuffle     = card.Reshuffle
            Action        = card.Action.ToString()
            ActionAmount  = getActionAmount card.Action
            CharacterId   = charId
            DateDiscarded = DateTime.Now
        }

    let getDiscards (dbContext : IDbContext) characterId = 
        DeckSql.getDiscards characterId
        |> dbContext.Query<DbModifierCard>
        |> map toModifierCard
        |> Array.toList

    let insertDiscard (dbContext : IDbContext) characterId card = 
        toDbNewModifierCard characterId card
        |> DeckSql.insertDiscard 
        |> dbContext.Execute

    let deleteDiscards (dbContext : IDbContext) characterId = 
        DeckSql.deleteDiscards characterId 
        |> dbContext.Execute