namespace GloomChars.Api

module ScenarioModels = 
    open GloomChars.Core
    open FSharpPlus
    open System
    open GloomChars.Common.Validation
    open ResponseHandlers

    [<CLIMutable>]
    type NewScenarioRequest = 
        {
            Name : string
        }

    [<CLIMutable>]
    type EventRequest = 
        {
            Event : string
            Amount : int 
        }

    [<CLIMutable>]
    type DeckActionRequest = 
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

    type ScenarioViewModel = 
        {
            Id            : int
            CharacterId   : int
            Name          : string
            Health        : int 
            MaxHealth     : int
            Experience    : int  
            DateStarted   : DateTime
            DateLastEvent : DateTime
            ModifierDeck  : DeckViewModel    
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

    let toScenarioViewModel (scenario : ScenarioState) : ScenarioViewModel = 
        let (CharacterId charId) = scenario.Info.CharacterId

        {
            Id            = scenario.Info.Id
            CharacterId   = charId
            Name          = scenario.Info.Name
            Health        = scenario.CharacterStats.Health
            MaxHealth     = scenario.Info.MaxHealth
            Experience    = scenario.CharacterStats.Experience
            DateStarted   = scenario.Info.DateStarted
            DateLastEvent = scenario.Info.DateLastEvent
            ModifierDeck  = toDeckViewModel scenario.ModifierDeck
        }

    let validateNewScenario (scenario : NewScenarioRequest) = 
        validateRequiredString (scenario.Name, "name") []
        |> toValidationResult scenario
        |> Result.mapError Msg