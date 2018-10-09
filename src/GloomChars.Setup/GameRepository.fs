namespace GloomChars.Setup

[<RequireQualifiedAccess>]
module GameSql = 
    open GloomChars.Core
    open GloomChars.Common
    open GloomChars.Common.QueryUtils

    let insertClassSQL (gloomClass : NewGloomClass) =
        let className = gloomClass.ClassName.ToString()

        sql
            """
            INSERT INTO gloom_classes
                (is_starting, class_name, name, symbol)
            VALUES 
                (@is_starting, @class_name, @name, @symbol)
            RETURNING id
            """
            [
                p "is_starting" gloomClass.IsStarting
                p "class_name" className
                p "name" gloomClass.Name
                p "symbol" gloomClass.Symbol
            ]

    let insertPerkSQL sortOrder classId qty =
        sql
            """
            INSERT INTO perks
                (class_id, sort_order, quantity)
            VALUES 
                (@class_id, @sort_order, @quantity)
            RETURNING id
            """
            [
                p "class_id" classId
                p "sort_order" sortOrder
                p "quantity" qty
            ]

    let insertCardSQL (card : ModifierCard) amnt =
        sql
            """
            INSERT INTO modifier_cards
                (damage, draw_another, reshuffle, card_action, card_action_amount)
            VALUES 
                (@damage, @draw_another, @reshuffle, @card_action, @card_action_amount)
            RETURNING id
            """
            [
                p "damage" card.Damage
                p "draw_another" card.DrawAnother
                p "reshuffle" card.Reshuffle
                p "card_action" (card.Action.ToString())
                p "card_action_amount" amnt
            ]

    let insertActionSQL action numCards perkId modCardId =
        sql
            """
            INSERT INTO perk_actions
                (action, num_cards, perk_id, modifier_card_id)
            VALUES 
                (@action, @num_cards, @perk_id, @modifier_card_id)
            """
            [
                p "action" action
                p "num_cards" numCards
                p "perk_id" perkId
                p "modifier_card_id" modCardId
            ]


[<RequireQualifiedAccess>]
module GameRepository = 
    open GloomChars.Core
    open GloomChars.Common
    open GloomChars.Common.QueryUtils

    let insertClass (db : IDbContext) (gloomClass : NewGloomClass) : int = 

        GameSql.insertClassSQL gloomClass 
        |> db.ExecuteScalar


    let insertPerk (db : IDbContext) sortOrder classId (perk : NewPerk) : int = 

        GameSql.insertPerkSQL sortOrder classId perk.Quantity 
        |> db.ExecuteScalar

    let insertModifierCard (db : IDbContext) (modCard : ModifierCard) : int option = 
        let amount = 
            match modCard.Action with 
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

        GameSql.insertCardSQL modCard amount 
        |> db.ExecuteScalar 
        |> Some

    let insertPerkAction (db : IDbContext) perkId action numCards modCardId = 

        GameSql.insertActionSQL action numCards perkId modCardId
        |> db.ExecuteScalar 
        |> ignore

    let insertPerkActions db (perkActions : PerkAction list) perkId = 
        perkActions
        |> List.iter(fun perkAction -> 
                match perkAction with
                | RemoveCard action | AddCard action -> 
                    insertModifierCard db action.Card
                    |> insertPerkAction db perkId (perkAction.ToString()) action.NumCards
                | _ -> 
                    insertPerkAction db perkId (perkAction.ToString()) 1 None
            )

    let insertPerks db (gloomClass : NewGloomClass) classId = 
        gloomClass.Perks
        |> List.iteri(fun sortOrder perk -> 
                insertPerk db sortOrder classId perk
                |> insertPerkActions db perk.Actions
            )

    let insertClasses db (gloomClasses : NewGloomClass list) = 
        gloomClasses
        |> List.iter(fun gloomClass -> 
                insertClass db gloomClass
                |> insertPerks db gloomClass
            )


(*

To reset and reseed the db:

delete from perk_actions;
delete from modifier_cards;
delete from perks;
delete from gloom_classes;

ALTER SEQUENCE gloom_classes_id_seq RESTART WITH 1;
UPDATE gloom_classes SET id=nextval('gloom_classes_id_seq');

ALTER SEQUENCE perks_id_seq RESTART WITH 1;
UPDATE perks SET id=nextval('perks_id_seq');

ALTER SEQUENCE modifier_cards_id_seq RESTART WITH 1;
UPDATE modifier_cards SET id=nextval('modifier_cards_id_seq');

ALTER SEQUENCE perk_actions_id_seq RESTART WITH 1;
UPDATE perk_actions SET id=nextval('perk_actions_id_seq');

*)