namespace GloomChars.Core

module CharactersService = 

    let getCharacter (dbGetCharacter : CharacterId -> UserId -> Character option) characterId userId : Character option = 
        dbGetCharacter characterId userId

    let getCharacters (dbGetCharacters : UserId -> Character list) userId : Character list = 
        dbGetCharacters userId

    let addCharacter (dbInsertNewCharacter : NewCharacter -> Result<int, string>) (newCharacter : NewCharacter) = 
        dbInsertNewCharacter newCharacter

    let updateCharacter (dbUpdateCharacter : CharacterUpdate -> Result<int, string>) (character : CharacterUpdate) = 
        dbUpdateCharacter character

    let deleteCharacter (dbDeleteCharacter : CharacterId -> UserId -> unit) characterId userId = 
        dbDeleteCharacter characterId userId