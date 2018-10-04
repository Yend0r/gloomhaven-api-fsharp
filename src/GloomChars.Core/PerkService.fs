namespace GloomChars.Core

module PerkService = 
    open System

    let private numText num = 
        match num with
        | 1 -> "one"
        | 2 -> "two"
        | 3 -> "three"
        | 4 -> "four"
        | _ -> "INFINITE"

    let private dmgText (modCard : ModifierCard) = 
        match modCard.Damage with
        | x when x < 0 -> sprintf "%i" x
        | x when x = 0 && modCard.Action <> Damage -> String.Empty
        | x -> sprintf "+%i" x

    let private modCardText (modCard : ModifierCard) = 
        match modCard.Action with
        | Miss -> "Miss"
        | Damage -> String.Empty
        | MultiplyDamage _ -> String.Empty
        | Disarm -> "DISARM"
        | Stun -> "STUN"
        | Poison -> "POISON" 
        | Wound  -> "WOUND" 
        | Muddle -> "MUDDLE"
        | AddTarget -> "ADD TARGET"
        | Immobilise -> "IMMOBILISE"
        | Invisible -> "INVISIBLE"
        | Fire -> "FIRE"
        | Ice -> "ICE"
        | Light -> "LIGHT"
        | Air -> "AIR"
        | Dark -> "DARK"
        | Earth -> "EARTH"
        | Curse -> "CURSE"
        | RefreshItem -> "REFRESH AN ITEM"
        | Push amount ->
            let (PushAmount value) = amount
            sprintf "PUSH%i" value
        | Pull amount ->
            let (PullAmount value) = amount
            sprintf "PULL%i" value
        | Pierce amount ->
            let (PierceAmount value) = amount
            sprintf "PIERCE%i" value
        | Heal amount ->
            let (HealAmount value) = amount
            sprintf "HEAL%i" value
        | Shield amount ->
            let (ShieldAmount value) = amount
            sprintf "SHIELD%i" value

    let private drawText drawAnother = if drawAnother then "DRAW" else String.Empty

    let private cardText numCards = if numCards = 1 then "card" else "cards"

    let private actionText (pcard : PerkCardAction) = 
        let num = numText pcard.NumCards
        let dmg = dmgText pcard.Card
        let cards = cardText pcard.NumCards
        let action = modCardText pcard.Card
        let draw = drawText pcard.Card.DrawAnother

        [num; dmg; action; draw; cards] 
        |> List.filter(fun s -> not (String.IsNullOrWhiteSpace s)) 
        |> String.concat " " 

    let private addText idx (pcard : PerkCardAction) = 
        let startText = if idx = 0 then "Add" else "and add"
        sprintf "%s %s" startText (actionText pcard)

    let private removeText idx (pcard : PerkCardAction) = 
        let startText = if idx = 0 then "Remove" else "and remove"
        sprintf "%s %s" startText (actionText pcard)

    let private ignoreScenarioText idx = 
        if idx = 0 then 
             "Ignore negative scenario effects"
        else 
            "and ignore negative scenario effects"

    let private ignoreItemText idx = 
        if idx = 0 then 
             "Ignore negative item effects"
        else 
            "and ignore negative item effects"

    let private getActionText idx perkAction =
        match perkAction with
        | RemoveCard pcard -> removeText idx pcard
        | AddCard pcard -> addText idx pcard
        | IgnoreScenarioEffects -> ignoreScenarioText idx
        | IgnoreItemEffects -> ignoreItemText idx

    let getText (perkActions : PerkAction list) =
        perkActions
        |> List.mapi(fun idx action -> getActionText idx action)
        |> String.concat " " 


