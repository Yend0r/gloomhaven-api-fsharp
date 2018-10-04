namespace GloomChars.Tests

open System
open Xunit
open FsUnit
open GloomChars.Core

module ModifierDeckTests = 

    let private isDamage dmg (card : ModifierCard) = 
        card.Action = Damage && card.Damage = dmg && not card.DrawAnother && not card.Reshuffle

    [<Fact>]
    let ``There should be 20 cards in the starting deck`` () =
        DeckService.startingModifierDeck
        |> List.length
        |> should equal 20

    [<Fact>]
    let ``There should be 1 MISS card (and shuffle) in the starting deck`` () =
        DeckService.startingModifierDeck
        |> List.filter (fun c -> c.Action = Miss && not c.DrawAnother && c.Reshuffle)
        |> List.length
        |> should equal 1

    [<Fact>]
    let ``There should be 1 damage x2 card (and shuffle) in the starting deck`` () =
        DeckService.startingModifierDeck
        |> List.filter (fun c -> c.Action = (MultiplyDamage (DamageMultiplier 2)) && not c.DrawAnother && c.Reshuffle)
        |> List.length
        |> should equal 1

    [<Fact>]
    let ``There should be 1 damage +2 card in the starting deck`` () =
        DeckService.startingModifierDeck
        |> List.filter (fun card -> isDamage 2 card)
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