﻿namespace GloomChars.Core

open System
open GloomChars.Common

type UserId = UserId of int
type CharacterId = CharacterId of int

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
    | MultiplyDamage of DamageMultiplier
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
        Damage      : int //Not an option type because cards can have "+0" dmg 
    }

type ModifierDeck = 
    {
        TotalCards  : int
        CurrentCard : ModifierCard option
        Discards    : ModifierCard list
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
        static member FromString s = Utils.unionFromString<GloomClassName> s 

type GloomClass = 
    {
        ClassName   : GloomClassName
        Name        : string
        Symbol      : string
        IsStarting  : bool
        Perks       : Perk list
        HPLevels    : int list
        PetHPLevels : int list option
    }

type Character = 
    { 
        Id           : CharacterId
        UserId       : UserId
        Name         : string
        ClassName    : GloomClassName
        Experience   : int
        Gold         : int
        Achievements : int
        ClaimedPerks : Perk list
    }

type CharacterListItem = 
    { 
        Id         : CharacterId
        Name       : string
        ClassName  : GloomClassName
        Experience : int
        Gold       : int
        ScenarioId : int option
    }

type NewCharacter = 
    {
        UserId       : UserId
        Name         : string
        ClassName    : GloomClassName
    }

type PerkUpdate = 
    { 
        Id       : string
        Quantity : int
    }

type CharacterUpdate = 
    { 
        Id           : CharacterId
        UserId       : UserId
        Name         : string
        Experience   : int
        Gold         : int
        Achievements : int
        Perks        : PerkUpdate list
    }

type ScenarioDeckAction = 
    | DrawCard
    | Reshuffle
    with        
        static member FromString s = Utils.unionFromString<ScenarioDeckAction> s 

type ScenarioInfo = 
    {
        Id            : int
        CharacterId   : CharacterId
        Name          : string
        MaxHealth     : int
        DateStarted   : DateTime
        DateLastEvent : DateTime  
    }

type ScenarioCharacterStats = 
    {      
        Health      : int 
        Experience  : int  
    }

type ScenarioState = 
    {
        Info           : ScenarioInfo
        CharacterStats : ScenarioCharacterStats
        ModifierDeck   : ModifierDeck    
    }

type StatsUpdate = 
    {      
        Health      : int option
        Experience  : int option 
    }

// Interfaces for services - makes the calling code do less work

type ICharactersService = 
    abstract member Get : CharacterId -> UserId -> Character option
    abstract member List : UserId -> CharacterListItem list
    abstract member Add : NewCharacter -> Result<int, string>
    abstract member Update : CharacterUpdate -> Result<int, string>
    abstract member Delete : CharacterId -> UserId -> Result<int, string>

type IDeckService = 
    abstract member Get : Character -> ModifierDeck
    abstract member Draw : Character -> ModifierDeck
    abstract member Reshuffle : Character -> ModifierDeck

type IScenarioService = 
    abstract member Get : Character -> Result<ScenarioState, string>
    abstract member NewScenario : Character -> string -> Result<int, string>
    abstract member Complete : Character -> Result<int, string>
    abstract member UpdateStats : Character -> StatsUpdate -> Result<ScenarioState, string>
    abstract member DrawCard : Character -> Result<ScenarioState, string>
    abstract member Reshuffle : Character -> Result<ScenarioState, string>