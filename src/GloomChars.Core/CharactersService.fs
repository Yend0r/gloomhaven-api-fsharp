namespace GloomChars.Core

module CharactersService = 

    let getCharacter (dbGetCharacter : int -> int -> Character option) characterId userId : Character option = 
        dbGetCharacter characterId userId

    let getCharacters (dbGetCharacters : int -> Character list) userId : Character list = 
        dbGetCharacters userId

    let addCharacter (dbInsertNewCharacter : NewCharacter -> Result<int, string>) (newCharacter : NewCharacter) = 
        dbInsertNewCharacter newCharacter

    let updateCharacter (dbUpdateCharacter : CharacterUpdate -> Result<int, string>) (character : CharacterUpdate) = 
        dbUpdateCharacter character

    let deleteCharacter (dbDeleteCharacter : int -> int -> unit) characterId userId = 
        dbDeleteCharacter characterId userId