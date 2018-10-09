namespace GloomChars.Core

open System
open GloomChars.Common

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
    with override this.ToString() = Utils.unionToString this

type ModifierCard = 
    {
        DrawAnother : bool
        Reshuffle   : bool
        Action      : CardAction 
        Damage      : int //Not an option type because the game docs often have text that says "+0" dmg
    }

type ModifierDeck = 
    {
        Cards      : ModifierCard list
        DrawnCards : ModifierCard list
    }

type PerkCardAction = 
    {
        NumCards : int
        Card     : ModifierCard
    }

type PerkAction =
    | RemoveCard of PerkCardAction 
    | AddCard of PerkCardAction 
    | IgnoreScenarioEffects 
    | IgnoreItemEffects 
    with override this.ToString() = Utils.unionToString this

type Perk = 
    {
        Id       : string //Unique id for the perk
        Quantity : int //The number of times this perk can be claimed
        Actions  : PerkAction list
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
    with
        override this.ToString() = Utils.unionToString this
        static member fromString s = 
            match Utils.unionFromString<GloomClassName> s with
            | Some name -> name
            | None -> raise (new Exception("Invalid Gloomhaven class name")) //Bad data... need to manually fix

type GloomClass = 
    {
        ClassName  : GloomClassName
        Name       : string
        Symbol     : string
        IsStarting : bool
        Perks      : Perk list
    }

type Character = 
    { 
        Id           : int
        UserId       : int
        Name         : string
        ClassName    : GloomClassName
        Experience   : int
        Gold         : int
        Achievements : int
        Perks        : Perk list
    }

type NewCharacter = 
    {
        UserId       : int
        Name         : string
        ClassName    : GloomClassName
        Experience   : int
        Gold         : int
        Achievements : int
    }

type CharacterUpdate = 
    { 
        Id           : int
        UserId       : int
        Name         : string
        ClassName    : GloomClassName
        Experience   : int
        Gold         : int
        Achievements : int
        PerkIds      : int list
    }

//--------------------------------------------
// Database classes
//--------------------------------------------
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