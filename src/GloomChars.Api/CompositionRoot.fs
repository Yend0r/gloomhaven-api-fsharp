namespace GloomChars.Api

module CompositionRoot = 
    open GloomChars.Core
    open GloomChars.Common 
    open GloomChars.Authentication
    open ResponseHandlers

    let private config = Config.config
    let private db = PostgresDbContext(config.Database.ConnectionString)

    module AuthenticationSvc = 

        let private dbGetPreAuthUser        = AuthenticationRepository.getUserForAuth db
        let private dbGetAuthenticatedUser  = AuthenticationRepository.getAuthenticatedUser db
        let private dbUpdateLoginStatus     = AuthenticationRepository.updateLoginStatus db
        let private dbInsertNewLogin        = AuthenticationRepository.insertNewLogin db
        let private dbRevoke                = AuthenticationRepository.revokeToken db

        let authenticate email password = 
            AuthenticationService.authenticate 
                config.Authentication 
                dbGetPreAuthUser
                dbUpdateLoginStatus
                dbInsertNewLogin
                email 
                password
            |> toAppResult

        let getAuthenticatedUser accessToken = 
            AuthenticationService.getAuthenticatedUser dbGetAuthenticatedUser accessToken
            |> toAppResult

        let revokeToken = AuthenticationService.revokeToken dbRevoke

    module UsersSvc = 

        let private dbGetUserByEmail    = UserRepository.getUserByEmail db
        let private dbGetUsers          = UserRepository.getUsers db
        let private dbInsertNewUser     = UserRepository.insertNewUser db

        let addUser newUser = 
            UserService.addUser dbGetUserByEmail dbInsertNewUser newUser
            |> toAppResult

        let getUsers = UserService.getUsers dbGetUsers

    module GameDataSvc = 

        let gloomClasses = GameData.gloomClasses

        let getGloomClass className = 
            GameData.getGloomClass className 
            |> optionToAppResult

    module CharactersSvc = 

        let private dbGetCharacter        = CharactersRepository.getCharacter db
        let private dbGetCharacters       = CharactersRepository.getCharacters db
        let private dbInsertNewCharacter  = CharactersRepository.insertNewCharacter db
        let private dbUpdateCharacter     = CharactersRepository.updateCharacter db
        let private dbDeleteCharacter     = CharactersRepository.deleteCharacter db


        let getCharacter characterId userId = 
            CharactersService.getCharacter dbGetCharacter characterId userId
            |> optionToAppResult

        let getCharacters = CharactersService.getCharacters dbGetCharacters

        let addCharacter newCharacter = 
            CharactersService.addCharacter dbInsertNewCharacter newCharacter
            |> toAppResult

        let updateCharacter characterUpdate = 
            CharactersService.updateCharacter dbUpdateCharacter characterUpdate
            |> toAppResult

        let deleteCharacter = CharactersService.deleteCharacter dbDeleteCharacter 