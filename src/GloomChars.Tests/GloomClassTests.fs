namespace GloomChars.Tests

open System
open Xunit
open FsUnit
open GloomChars.Core

module GloomClassTests = 

    [<Fact>]
    let ``There should be 17 classes`` () =
        GameData.gloomClasses
        |> List.length
        |> should equal 17

    [<Fact>]
    let ``There should be 6 starting classes`` () =
        GameData.gloomClasses
        |> List.filter (fun c -> c.IsStarting)
        |> List.length
        |> should equal 6

    [<Fact>]
    let ``Brute perks`` () =
        let perks = [
            "Remove two -1 cards" 
            "Remove one -1 card and add one +1 card"                
            "Add two +1 cards"       
            "Add one +3 card" 
            "Add three PUSH1 DRAW cards" 
            "Add two PIERCE3 DRAW cards"
            "Add one STUN DRAW card"
            "Add one DISARM DRAW card and add one MUDDLE DRAW card"
            "Add one ADD TARGET DRAW card"
            "Add one +1 SHIELD1 card"
            "Ignore negative item effects and add one +1 card"
        ]

        let ghClass = GameData.gloomClass Brute   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Tinkerer perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove one -2 card and add one +0 card"
            "Add two +1 cards"
            "Add one +3 card"
            "Add two FIRE DRAW cards"
            "Add three MUDDLE DRAW cards"
            "Add one +1 WOUND card"
            "Add one +1 IMMOBILISE card"
            "Add one +1 HEAL2 card"
            "Add one ADD TARGET card"
            "Ignore negative scenario effects"
        ]

        let ghClass = GameData.gloomClass Tinkerer   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Spellweaver perks`` () =
        let perks = [
            "Remove four +0 cards"
            "Remove one -1 card and add one +1 card"
            "Add two +1 cards"
            "Add one STUN card"
            "Add one +1 WOUND card"
            "Add one +1 IMMOBILISE card"
            "Add one +1 CURSE card"
            "Add one +2 FIRE card"
            "Add one +2 ICE card"
            "Add one EARTH DRAW card and add one AIR DRAW card"
            "Add one LIGHT DRAW card and add one DARK DRAW card"
        ]

        let ghClass = GameData.gloomClass Spellweaver   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Scoundrel perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove four +0 cards"
            "Remove one -2 card and add one +0 card"
            "Remove one -1 card and add one +1 card"
            "Remove one +0 card and add one +2 card"
            "Add two +1 DRAW cards"
            "Add two PIERCE3 DRAW cards"
            "Add two POISON DRAW cards"
            "Add two MUDDLE DRAW cards"
            "Add one INVISIBLE DRAW card"
            "Ignore negative scenario effects"
        ]

        let ghClass = GameData.gloomClass Scoundrel   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Cragheart perks`` () =
        let perks = [
            "Remove four +0 cards"
            "Remove one -1 card and add one +1 card"
            "Add one -2 card and add two +2 cards"
            "Add one +1 IMMOBILISE card"
            "Add one +2 MUDDLE card"
            "Add two PUSH2 DRAW cards"
            "Add two AIR DRAW cards"
            "Add two EARTH DRAW cards"
            "Ignore negative item effects"
            "Ignore negative scenario effects"
        ]

        let ghClass = GameData.gloomClass Cragheart   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Mindthief perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove four +0 cards"
            "Remove two +1 cards and add two +2 cards"
            "Remove one -2 card and add one +0 card"
            "Add one +2 ICE card"
            "Add two +1 DRAW cards"
            "Add three PULL1 DRAW cards"
            "Add three MUDDLE DRAW cards"
            "Add two IMMOBILISE DRAW cards"
            "Add one STUN DRAW card"
            "Add one DISARM DRAW card and add one MUDDLE DRAW card"
            "Ignore negative scenario effects"
        ]

        let ghClass = GameData.gloomClass Mindthief   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Sunkeeper perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove four +0 cards"
            "Remove one -2 card and add one +0 card"
            "Remove one +0 card and add one +2 card"
            "Add two +1 cards"
            "Add two HEAL1 DRAW cards"
            "Add one STUN DRAW card"
            "Add two LIGHT DRAW cards"
            "Add one SHIELD1 DRAW card"
            "Ignore negative item effects and add two +1 cards"
            "Ignore negative scenario effects"
        ]

        let ghClass = GameData.gloomClass Sunkeeper   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Quartermaster perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove four +0 cards"
            "Remove one +0 card and add one +2 card"
            "Add two +1 DRAW cards"
            "Add three MUDDLE DRAW cards"
            "Add two PIERCE3 DRAW cards"
            "Add one STUN DRAW card"
            "Add one ADD TARGET DRAW card"
            "Add one REFRESH AN ITEM card"
            "Ignore negative item effects and add two +1 cards"
        ]

        let ghClass = GameData.gloomClass Quartermaster   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Summoner perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove one -2 card and add one +0 card"
            "Remove one -1 card and add one +1 card"
            "Add one +2 card" 
            "Add two WOUND DRAW cards"
            "Add two POISON DRAW cards"
            "Add two HEAL1 DRAW cards"
            "Add one FIRE DRAW card and add one AIR DRAW card"
            "Add one DARK DRAW card and add one EARTH DRAW card"
            "Ignore negative item effects and add two +1 cards"
        ]

        let ghClass = GameData.gloomClass Summoner   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Nightshroud perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove four +0 cards"
            "Add one -1 DARK card"
            "Remove one -1 DARK card and add one +1 DARK card"
            "Add one +1 INVISIBLE card"
            "Add three MUDDLE DRAW cards"
            "Add two HEAL1 DRAW cards"
            "Add two CURSE DRAW cards"
            "Add one ADD TARGET DRAW card"
            "Ignore negative item effects and add two +1 cards"
        ]

        let ghClass = GameData.gloomClass Nightshroud   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Plagueherald perks`` () =
        let perks = [
            "Remove one -2 card and add one +0 card"
            "Remove one -1 card and add one +1 card"
            "Remove one +0 card and add one +2 card"
            "Add two +1 cards"
            "Add one +1 AIR card"
            "Add three POISON DRAW cards"
            "Add two CURSE DRAW cards"
            "Add two IMMOBILISE DRAW cards"
            "Add one STUN DRAW card"
            "Ignore negative scenario effects and add one +1 card"
        ]

        let ghClass = GameData.gloomClass Plagueherald   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Berserker perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove four +0 cards"
            "Remove one -1 card and add one +1 card"
            "Remove one +0 card and add one +2 DRAW card"
            "Add two WOUND DRAW cards"
            "Add one STUN DRAW card"
            "Add one +1 DISARM DRAW card"
            "Add two HEAL1 DRAW cards"
            "Add one +2 FIRE card"
            "Ignore negative item effects"
        ]

        let ghClass = GameData.gloomClass Berserker   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Soothsinger perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove one -2 card"
            "Remove two +1 cards and add one +4 card"
            "Remove one +0 card and add one +1 IMMOBILISE card"
            "Remove one +0 card and add one +1 DISARM card"
            "Remove one +0 card and add one +2 WOUND card"
            "Remove one +0 card and add one +2 POISON card"
            "Remove one +0 card and add one +2 CURSE card"
            "Remove one +0 card and add one +3 MUDDLE card"
            "Remove one -1 card and add one STUN card"
            "Add three +1 DRAW cards"
            "Add two CURSE DRAW cards"    
        ]

        let ghClass = GameData.gloomClass Soothsinger   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])

    [<Fact>]
    let ``Doomstalker perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove two +0 cards and add two +1 cards"
            "Add two +1 DRAW cards"
            "Add one +2 MUDDLE card"
            "Add one +1 POISON card"
            "Add one +1 WOUND card"
            "Add one +1 IMMOBILISE card"
            "Add one STUN card"
            "Add one ADD TARGET DRAW card"
            "Ignore negative scenario effects"  
        ]

        let ghClass = GameData.gloomClass Doomstalker   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])                   

    [<Fact>]
    let ``Sawbones perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove four +0 cards"
            "Remove one +0 card and add one +2 card"
            "Add one +2 DRAW card"
            "Add one +1 IMMOBILISE card"
            "Add two WOUND DRAW cards"
            "Add one STUN DRAW card"
            "Add one HEAL3 DRAW card"
            "Add one REFRESH AN ITEM card"     
        ]

        let ghClass = GameData.gloomClass Sawbones   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])  

    [<Fact>]
    let ``Elementalist perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove one -1 card and add one +1 card"
            "Remove one +0 card and add one +2 card"
            "Add three FIRE cards"
            "Add three ICE cards"
            "Add three AIR cards"
            "Add three EARTH cards"                
            "Remove two +0 cards and add one FIRE card and add one EARTH card"
            "Remove two +0 cards and add one ICE card and add one AIR card"   
            "Add two +1 PUSH1 cards"
            "Add one +1 WOUND card"
            "Add one STUN card"
            "Add one ADD TARGET card"
        ]

        let ghClass = GameData.gloomClass Elementalist   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])  

    [<Fact>]
    let ``BeastTyrant perks`` () =
        let perks = [
            "Remove two -1 cards"
            "Remove one -1 card and add one +1 card"
            "Remove one +0 card and add one +2 card"
            "Add one +1 WOUND card"
            "Add one +1 IMMOBILISE card"
            "Add two HEAL1 DRAW cards"
            "Add two EARTH DRAW cards"
            "Ignore negative scenario effects"
        ]

        let ghClass = GameData.gloomClass BeastTyrant   

        ghClass.Perks
        |> List.map(fun perk -> PerkService.getText perk.Actions)
        |> List.mapi(fun idx perkText -> perkText |> should equal perks.[idx])  


                
