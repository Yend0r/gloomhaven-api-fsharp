namespace GloomChars.Core

module CharactersService = 

    let private perksContain (allPerks : Perk list) (perkId : string) : bool = 
        allPerks 
        |> List.exists(fun p -> p.Id.ToUpper() = perkId.ToUpper())

    let private toValidPerks (perkUpdates : PerkUpdate list) className = 
        //Check that the perk ids are valid for this class
        let perkExists = perksContain (GameData.gloomClass className).Perks
        perkUpdates |> List.choose (fun p -> if perkExists p.Id then Some p else None) 

    let get (dbGetCharacter : CharacterId -> UserId -> Character option) characterId userId : Character option = 
        dbGetCharacter characterId userId

    let list (dbGetCharacters : UserId -> CharacterListItem list) userId : CharacterListItem list = 
        dbGetCharacters userId

    let add (dbInsertNewCharacter : NewCharacter -> Result<int, string>) (newCharacter : NewCharacter) = 
        dbInsertNewCharacter newCharacter

    let update
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

    let delete 
        (dbGetCharacter : CharacterId -> UserId -> Character option)
        (dbDeleteCharacter : CharacterId -> UserId -> int) 
        characterId 
        userId = 

        //First must get the character to make sure that this user owns the character
        dbGetCharacter characterId userId
        |> function 
        | Some _ -> Ok (dbDeleteCharacter characterId userId)
        | None -> Error "Character not found."

    let create db = 

        let dbGetCharacter       = CharactersReadRepository.getCharacter db
        let dbGetCharacters      = CharactersReadRepository.getCharacters db
        let dbInsertNewCharacter = CharactersEditRepository.insertNewCharacter db
        let dbUpdateCharacter    = CharactersEditRepository.updateCharacter db
        let dbDeleteCharacter    = CharactersEditRepository.deleteCharacter db

        { new ICharactersService with 
            member __.Get characterId userId = 
                get dbGetCharacter characterId userId

            member __.List userId = 
                list dbGetCharacters userId

            member __.Add newCharacter = 
                add dbInsertNewCharacter newCharacter

            member __.Update characterUpdate = 
                update dbGetCharacter dbUpdateCharacter  characterUpdate

            member __.Delete characterId userId = 
                delete dbGetCharacter dbDeleteCharacter characterId userId
        }