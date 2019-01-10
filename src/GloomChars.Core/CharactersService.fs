namespace GloomChars.Core

module CharactersService = 

    let private perksContain (allPerks : Perk list) (perkId : string) : bool = 
        allPerks 
        |> List.exists(fun p -> p.Id.ToUpper() = perkId.ToUpper())

    let private toValidPerks (perkUpdates : PerkUpdate list) className = 
        //Check that the perk ids are valid for this class
        let perkExists = perksContain (GameData.gloomClass className).Perks
        perkUpdates |> List.choose (fun p -> if perkExists p.Id then Some p else None) 

    let getCharacter (dbGetCharacter : CharacterId -> UserId -> Character option) characterId userId : Character option = 
        dbGetCharacter characterId userId

    let getCharacters (dbGetCharacters : UserId -> CharacterListItem list) userId : CharacterListItem list = 
        dbGetCharacters userId

    let addCharacter (dbInsertNewCharacter : NewCharacter -> Result<int, string>) (newCharacter : NewCharacter) = 
        dbInsertNewCharacter newCharacter

    let updateCharacter 
        (dbGetCharacter : CharacterId -> UserId -> Character option)
        (dbUpdateCharacter : CharacterUpdate -> Result<int, string>) 
        (character : CharacterUpdate) = 

        //First must get the character to make sure that this user owns the character
        dbGetCharacter character.Id character.UserId
        |> function 
        | Some c -> 
            dbUpdateCharacter { character with Perks = toValidPerks character.Perks c.ClassName }
        | None -> 
            Error "Character not found."

    let deleteCharacter 
        (dbGetCharacter : CharacterId -> UserId -> Character option)
        (dbDeleteCharacter : CharacterId -> UserId -> int) 
        characterId 
        userId = 

        //First must get the character to make sure that this user owns the character
        dbGetCharacter characterId userId
        |> function 
        | Some _ -> Ok (dbDeleteCharacter characterId userId)
        | None -> Error "Character not found."