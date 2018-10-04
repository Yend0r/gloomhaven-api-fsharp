namespace GloomChars.Core

open System

module DeckService = 

    let modCard action dmg (drawAction : DrawAction) = 
        let drawAnother = if drawAction=Draw then true else false
        { DrawAnother=drawAnother; Reshuffle=false; Action=action; Damage=dmg }

    let private shuffleModCard action dmg (drawAction : DrawAction) = 
        let drawAnother = if drawAction=Draw then true else false
        { DrawAnother=drawAnother; Reshuffle=true; Action=action; Damage=dmg }

    let private dmgModCard dmg 
        = modCard Damage dmg NoDraw 

    (*
        Starting decks consist of 
        1x “Miss”
        1x “2x”
        1x “+2”
        1x “-2”
        6x “+0”
        5x “+1”
        5x “-1”
   *)
    let startingModifierDeck = 
        [ shuffleModCard (MultiplyDamage (DamageMultiplier 2)) 0 NoDraw ]
        @ [ shuffleModCard Miss 0 NoDraw ]
        @ [ dmgModCard -2 ]
        @ [ dmgModCard 2 ]
        @ [ for i in 1..5 -> dmgModCard -1 ]
        @ [ for i in 1..6 -> dmgModCard 0 ]
        @ [ for i in 1..5 -> dmgModCard 1 ]