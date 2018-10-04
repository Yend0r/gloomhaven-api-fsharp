namespace GloomChars.Core

open System

type DslText =
    | With
    | And 
    | AndAdd

type DrawAction = 
    | Draw
    | NoDraw

type HealAmount = HealAmount of int
type ShieldAmount = ShieldAmount of int
type PierceAmount = PierceAmount of int
type PushAmount = PushAmount of int
type PullAmount = PullAmount of int
type DamageMultiplier = DamageMultiplier of int

type CardAction = 
    | Miss 
    | Damage 
    | MultiplyDamage of DamageMultiplier
    | Disarm 
    | Stun 
    | Poison 
    | Wound 
    | Muddle 
    | AddTarget 
    | Immobilise 
    | Invisible 
    | Fire 
    | Ice 
    | Light 
    | Air 
    | Dark 
    | Earth 
    | Curse 
    | RefreshItem 
    | Push of PushAmount
    | Pull of PullAmount
    | Pierce of PierceAmount
    | Heal of HealAmount
    | Shield of ShieldAmount

type ModifierCard = 
    {
        DrawAnother : bool
        Reshuffle : bool
        Action : CardAction 
        Damage : int
    }

type ModifierDeck = 
    {
        Cards : ModifierCard list
        DrawnCards : ModifierCard list
    }

type PerkCardAction = 
    {
        NumCards : int
        Card : ModifierCard
    }

type PerkAction =
    | RemoveCard of PerkCardAction 
    | AddCard of PerkCardAction 
    | IgnoreScenarioEffects 
    | IgnoreItemEffects 

type Perk = 
    {
        Quantity : int //The number of times this perk is available
        Actions : PerkAction list
    }

type GloomClassName = 
    | Brute
    | Tinkerer
    | Spellweaver
    | Scoundrel
    | Cragheart
    | Mindthief
    | Sunkeeper
    | Quartermaster
    | Summoner
    | Nightshroud
    | Plagueherald
    | Berserker
    | Soothsinger
    | Doomstalker
    | Sawbones
    | Elementalist
    | BeastTyrant

type GloomClass = 
    {
        ClassName : GloomClassName
        Name : string
        Symbol : string
        IsStarting : bool
        Perks : Perk list
    }

type Character = 
    { 
        Id : int
        Name : string
        GloomClass : GloomClass
        Experience : int
        Gold : int
        Achievements : int
        Perks : Perk list
    }
