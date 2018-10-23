namespace GloomChars.Api

module DeckModels = 
    open GloomChars.Core
    open FSharpPlus

    [<CLIMutable>]
    type NewDeckRequest = 
        {
            Action : string
        }

    type CardViewModel = 
        {
            Damage       : int
            DrawAnother  : bool 
            Reshuffle    : bool 
            Action       : string 
            ActionAmount : int option
        }

    type DeckViewModel = 
        {
            TotalCards  : int
            CurrentCard : CardViewModel option
            Discards    : CardViewModel list 
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

    let toCardViewModel (card : ModifierCard) : CardViewModel = 
        {
            Damage        = card.Damage
            DrawAnother   = card.DrawAnother
            Reshuffle     = card.Reshuffle
            Action        = card.Action.ToString()
            ActionAmount  = getActionAmount card.Action
        }

    let toDeckViewModel (deck : ModifierDeck) : DeckViewModel = 
        {
            TotalCards    = deck.TotalCards
            CurrentCard   = map toCardViewModel deck.CurrentCard
            Discards      = deck.Discards |> map toCardViewModel
        }