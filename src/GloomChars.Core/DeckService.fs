namespace GloomChars.Core

open System

module DeckService = 
    open GameData 

    let private makeReshuffleCard action dmg (drawAction : DrawAction) = 
        modCard action dmg drawAction true

    let private makeDmgCard dmg =
        modCard Damage dmg NoDraw false

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
        [ makeReshuffleCard (MultiplyDamage (DamageMultiplier 2)) 0 NoDraw ]
        @ [ makeReshuffleCard Miss 0 NoDraw ]
        @ [ makeDmgCard -2 ]
        @ [ makeDmgCard 2 ]
        @ [ for i in 1..5 -> makeDmgCard -1 ]
        @ [ for i in 1..6 -> makeDmgCard 0 ]
        @ [ for i in 1..5 -> makeDmgCard 1 ]

    let private addCardsToDeck (action : PerkCardAction) deck = 
        [1..action.NumCards]
        |> List.map (fun _ -> action.Card) //Create a list of cards to add
        |> List.append deck //Append the lists

    //Two possible methods... recursion (can short circuit so it's faster) 
    //or higher order functions (easier to understand?)... go with HOF for now
    let removeCard (card : ModifierCard) deck = 
        deck
        |> List.fold (
                fun (newList, notFound) c ->
                    if notFound && c = card 
                    then (newList, false) 
                    else (c :: newList, notFound)
                ) 
                ([], true)
        |> fst

    let rec private removeCards deck cardsToRemove  =
        match cardsToRemove with
        | [] -> deck
        | head :: tail -> 
            let newDeck = removeCard head deck
            removeCards newDeck tail

    let private removeCardsFromDeck (action : PerkCardAction) deck = 
        [1..action.NumCards] 
        |> List.map (fun _ -> action.Card) //Create a list of cards to remove
        |> removeCards deck

    let private applyPerkAction deck (action : PerkAction) = 
        match action with 
        | AddCard addAction -> addCardsToDeck addAction deck
        | RemoveCard removeAction -> removeCardsFromDeck removeAction deck
        | _ -> deck

    let private applyPerks (perks : Perk list) deck = 
        seq {
                for perk in perks do
                    for i in [1..perk.Quantity] do
                        for action in perk.Actions do
                            yield action

        }
        |> List.ofSeq
        |> List.fold (fun newDeck action -> applyPerkAction newDeck action) deck

    let private removeDiscards discards deck = 
        removeCards deck discards

    let private getRandomCard deck =
        let rnd = System.Random()  
        let len = List.length deck
        deck |> List.item (rnd.Next(len)) 

    let private getFullDeck perks = 
        startingModifierDeck |> applyPerks perks

    let drawCard 
        (dbGetDiscards : CharacterId -> ModifierCard list) 
        (dbSaveDiscard : CharacterId -> ModifierCard -> int) 
        (character : Character) : ModifierDeck = 
        
        let discards = dbGetDiscards character.Id

        let fullDeck = getFullDeck character.ClaimedPerks

        //Select random card 
        let deckWithoutDiscards = 
            fullDeck
            |> removeDiscards discards

        //Select random card 
        let cardOpt = 
            match deckWithoutDiscards with
            | [] -> None
            | _ ->
                let card = getRandomCard deckWithoutDiscards
                //Save as discard
                dbSaveDiscard character.Id card |> ignore
                Some card

        //Return deck 
        {
            TotalCards = fullDeck.Length
            CurrentCard = cardOpt
            Discards = discards 
        }

    let getDeck 
        (dbGetDiscards : CharacterId -> ModifierCard list) 
        (character : Character) : ModifierDeck = 

        let discards = dbGetDiscards character.Id

        let fullDeck = getFullDeck character.ClaimedPerks

        match discards with
        | [] ->
            {
                TotalCards = fullDeck.Length
                CurrentCard = None
                Discards = []
            }
        | head :: tail ->
            {
                TotalCards = fullDeck.Length
                CurrentCard = Some head
                Discards = tail 
            }

    let reshuffle 
        (dbDeleteDiscards : CharacterId -> int) 
        (character : Character) : ModifierDeck = 

        dbDeleteDiscards character.Id |> ignore

        let fullDeck = getFullDeck character.ClaimedPerks

        {
            TotalCards = fullDeck.Length
            CurrentCard = None
            Discards = []
        }