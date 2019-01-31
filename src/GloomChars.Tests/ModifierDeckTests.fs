namespace GloomChars.Tests

open System
open Xunit
open FsUnit
open GloomChars.Core

module ModifierDeckTests = 

    let numCardsInStartingDeck = DeckService.startingModifierDeck |> List.length

    let private makeCard action dmg drawAnother reshuffle = 
        { 
            DrawAnother = drawAnother
            Reshuffle = reshuffle
            Action = action
            Damage = dmg
        }

    let private makeDmgCard dmg =
        makeCard Damage dmg false false

    let private isDamage dmg (card : ModifierCard) = 
        card = (makeCard Damage dmg false false)

    let testCharacter = 
        { 
            Id           = CharacterId 42
            UserId       = UserId 42
            Name         = "Test"
            ClassName    = Soothsinger
            Experience   = 5
            Level        = 1
            HP           = 6
            PetHP        = None
            Gold         = 5
            Achievements = 5
            ClaimedPerks = []
        }

    let character perks = { testCharacter with ClaimedPerks = perks }

    let dbGetDiscards testDiscards characterId = testDiscards

    let dbSaveDiscard characterId card = 42

    let dbDeleteDiscards characterId = 42

    [<Fact>]
    let ``Character with no perks and no draws should have the same num of cards as the starting deck`` () =

        let getDiscards = dbGetDiscards []

        let deck = DeckService.getDeck getDiscards testCharacter

        deck.TotalCards |> should equal numCardsInStartingDeck
        deck.CurrentCard |> should equal None
        deck.Discards |> List.length |> should equal 0

    [<Fact>]
    let ``Drawing once should have result in no discards`` () =

        let getDiscards = dbGetDiscards []

        let deck = DeckService.drawCard getDiscards dbSaveDiscard testCharacter

        deck.TotalCards |> should equal (DeckService.startingModifierDeck |> List.length)
        deck.CurrentCard.IsSome |> should equal true
        deck.Discards |> List.length |> should equal 0

    [<Fact>]
    let ``Drawing should never produce a card that has been discarded`` () =

        //This discard list will leave only 2 cards in the deck
        let discards = 
            [ makeDmgCard -2 ]
            @ [ makeDmgCard 2 ]
            @ [ for i in 1..5 -> makeDmgCard -1 ]
            @ [ for i in 1..6 -> makeDmgCard 0 ]
            @ [ for i in 1..5 -> makeDmgCard 1 ]

        let getDiscards = dbGetDiscards discards

        let deck = DeckService.drawCard getDiscards dbSaveDiscard testCharacter

        let expected1 = makeCard (MultiplyDamage (DamageMultiplier 2)) 0 false true
        let expected2 = makeCard Miss 0 false true

        let cardResult = 
            match deck.CurrentCard with
            | Some c when c = expected1 || c = expected2 -> true
            | _ -> false

        deck.TotalCards |> should equal (DeckService.startingModifierDeck |> List.length)
        cardResult |> should equal true
        deck.Discards |> List.length |> should equal 18



    //
    // Starting Deck Tests -----------------
    //

    [<Fact>]
    let ``There should be 20 cards in the starting deck`` () =
        DeckService.startingModifierDeck
        |> List.length
        |> should equal 20 //Not using the calculated value because this is checking the calculated value

    [<Fact>]
    let ``There should be 1 MISS card (and shuffle) in the starting deck`` () =
        let expected = makeCard Miss 0 false true

        DeckService.startingModifierDeck
        |> List.filter (fun c -> c = expected)
        |> List.length
        |> should equal 1

    [<Fact>]
    let ``There should be 1 damage x2 card (and shuffle) in the starting deck`` () =
        let expected = makeCard (MultiplyDamage (DamageMultiplier 2)) 0 false true

        DeckService.startingModifierDeck
        |> List.filter (fun c -> c = expected)
        |> List.length
        |> should equal 1

    let isEqual card1 card2 = 
        card1 = card2

    [<Fact>]
    let ``There should be 1 damage +2 card in the starting deck`` () =
        let expectedCard = makeCard Damage 2 false false

        DeckService.startingModifierDeck
        |> List.filter (fun card -> isEqual card expectedCard)
        |> List.length
        |> should equal 1

    [<Fact>]
    let ``There should be 1 damage -2 card in the starting deck`` () =
        DeckService.startingModifierDeck
        |> List.filter (fun card -> isDamage -2 card)
        |> List.length
        |> should equal 1


    [<Fact>]
    let ``There should be 6 damage +0 cards in the starting deck`` () =
        DeckService.startingModifierDeck
        |> List.filter (fun card -> isDamage 0 card)
        |> List.length
        |> should equal 6

    [<Fact>]
    let ``There should be 5 damage -1 cards in the starting deck`` () =
        DeckService.startingModifierDeck
        |> List.filter (fun card -> isDamage -1 card)
        |> List.length
        |> should equal 5

    [<Fact>]
    let ``There should be 6 damage +1 cards in the starting modifier deck`` () =
        DeckService.startingModifierDeck
        |> List.filter (fun card -> isDamage 1 card)
        |> List.length
        |> should equal 5