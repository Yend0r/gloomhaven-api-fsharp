namespace GloomChars.Api

module CompositionRoot = 
    open GloomChars.Core
    open GloomChars.Common 
    open GloomChars.Users
    open ResponseHandlers

    let private config = Config.config
    let private db = PostgresDbContext(config.Database.ConnectionString)

    module AuthenticationSvc = 

        let private authConfig = config.Authentication

        // Using an interface here ends up nicer... implemented as an object expression
        let private authRepo = 
            { new IAuthenticationRepository with 
                member __.GetAuthenticatedUser token = AuthenticationRepository.getAuthenticatedUser db token
                member __.GetUserForAuth email       = AuthenticationRepository.getRegisteredUserByEmail db email
                member __.InsertNewLogin newLogin    = AuthenticationRepository.insertNewLogin db newLogin
                member __.UpdateLoginStatus status   = AuthenticationRepository.updateLoginStatus db status }

        let private authFailureToAppError authFailure = 
            match authFailure with
            | IsLockedOut msg -> Msg msg
            | _ -> Msg "Invalid email/password."

        let authenticate email password = 
            AuthenticationService.authenticate authConfig authRepo email (PlainPassword password)
            |> Result.mapError authFailureToAppError

        let getAuthenticatedUser = 
            AuthenticationRepository.getAuthenticatedUser db
            |> AuthenticationService.getAuthenticatedUser 
            >> toAppResult

        let revokeToken = 
            AuthenticationRepository.revokeToken db
            |> AuthenticationService.revokeToken 

        
        let private dbGetRegisteredUserByToken = AuthenticationRepository.getRegisteredUserByToken db 
        let private dbUpdatePassword = AuthenticationRepository.updatePassword db 

        let changePassword =
            (dbGetRegisteredUserByToken, dbUpdatePassword)
            ||> AuthenticationService.changePassword
            >> toAppResult

    module UsersSvc = 

        let private dbGetUserByEmail    = UserRepository.getUserByEmail db
        let private dbGetUsers          = UserRepository.getUsers db
        let private dbGetUser           = UserRepository.getUser db
        let private dbInsertNewUser     = UserRepository.insertNewUser db

        let addUser = 
            UserService.addUser dbGetUserByEmail dbInsertNewUser 
            >> toAppResult

        let getUsers = UserService.getUsers dbGetUsers

        let getUser userId = 
            UserService.getUser dbGetUser userId
            |> optionToAppResultOrNotFound

    module GameDataSvc = 

        let gloomClasses = GameData.gloomClasses

        let getGlClass glClassName = 
            GameData.gloomClass glClassName 

        let getGloomClass className = 
            GameData.getGloomClass className 
            |> optionToAppResultOrNotFound

    module CharactersSvc = 

        let private dbGetCharacter        = CharactersReadRepository.getCharacter db
        let private dbGetCharacters       = CharactersReadRepository.getCharacters db
        let private dbInsertNewCharacter  = CharactersEditRepository.insertNewCharacter db
        let private dbUpdateCharacter     = CharactersEditRepository.updateCharacter db
        let private dbDeleteCharacter     = CharactersEditRepository.deleteCharacter db

        let getCharacter characterId userId = 
            CharactersService.getCharacter dbGetCharacter characterId userId
            |> optionToAppResultOrNotFound

        let getCharacters = CharactersService.getCharacters dbGetCharacters

        let addCharacter  = 
            CharactersService.addCharacter dbInsertNewCharacter 
            >> toAppResult

        let updateCharacter  = 
            CharactersService.updateCharacter dbGetCharacter dbUpdateCharacter 
            >> toAppResult

        let deleteCharacter characterId userId = 
            CharactersService.deleteCharacter dbGetCharacter dbDeleteCharacter characterId userId
            |> toAppResult

    module ScenarioSvc = 

        let private dbGetDiscards    = DeckRepository.getDiscards db
        let private dbInsertDiscard  = DeckRepository.insertDiscard db
        let private dbDeleteDiscards = DeckRepository.deleteDiscards db

        let private getDeck = DeckService.getDeck dbGetDiscards 
        let private drawCard = DeckService.drawCard dbGetDiscards dbInsertDiscard
        let private reshuffle = DeckService.reshuffle dbDeleteDiscards

        let private dbInsertNewScenario = ScenarioRepository.insertNewScenario db
        let private dbCompleteScenario  = ScenarioRepository.completeActiveScenarios db
        let private dbGetScenario = ScenarioRepository.getScenario db
        let private dbUpdateCharacterStats = ScenarioRepository.updateCharacterStats db
        
        let newScenario = 
            ScenarioService.newScenario dbInsertNewScenario reshuffle
            >> toAppResult

        let completeScenario = 
            ScenarioService.completeScenario dbGetScenario dbCompleteScenario reshuffle
            >> toAppResult

        let getScenario character = 
            ScenarioService.getScenario dbGetScenario getDeck character
            |> optionToAppResultOrNotFound

        let processStatsEvent = ScenarioService.processStatsEvent dbUpdateCharacterStats

        let processDeckAction = ScenarioService.processDeckAction drawCard reshuffle


