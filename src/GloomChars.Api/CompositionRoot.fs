namespace GloomChars.Api

module CompositionRoot = 
    open GloomChars.Core
    open GloomChars.Common 
    open GloomChars.Authentication
    open ResponseHandlers

    let private config = Config.config
    let private db = PostgresDbContext(config.Database.ConnectionString)

    let private optionToAppResult opt = 
        match opt with
        | Some c -> Ok c
        | None -> Error NotFound

    let private toAppResult (result : Result<'a,string>) : Result<'a,AppError> = 
        result 
        |> Result.mapError Msg

    module AuthenticationSvc = 

        let private dbGetPreAuthUser        = AuthenticationRepository.getUserForAuth db
        let private dbGetAuthenticatedUser  = AuthenticationRepository.getAuthenticatedUser db
        let private dbUpdateLoginStatus     = AuthenticationRepository.updateLoginStatus db
        let private dbInsertNewLogin        = AuthenticationRepository.insertNewLogin db
        let private dbRevoke                = AuthenticationRepository.revokeToken db

        let private getPreAuthUser = 
            AuthUserService.getUserForAuth 
                dbGetPreAuthUser
                PasswordVerifier.verify 
                (LockoutChecker.check config.Authentication)
                
        let private authFailureToString authError = 
            match authError with
            | IsLockedOut msg -> msg
            | _ -> "Invalid email/password."

        let authenticate email password = 
            AuthenticationService.authenticate 
                getPreAuthUser
                (LoginCreator.create config.Authentication dbInsertNewLogin dbGetAuthenticatedUser)
                (AuthenticationAttempts.saveAuthAttempt config.Authentication dbUpdateLoginStatus)
                email
                password
            |> Result.mapError authFailureToString //Change any errors to a descriptive string
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

        let private dbGetCharacter        = CharactersReadRepository.getCharacter db
        let private dbGetCharacters       = CharactersReadRepository.getCharacters db
        let private dbInsertNewCharacter  = CharactersEditRepository.insertNewCharacter db
        let private dbUpdateCharacter     = CharactersEditRepository.updateCharacter db
        let private dbDeleteCharacter     = CharactersEditRepository.deleteCharacter db

        let getCharacter characterId userId = 
            CharactersService.getCharacter dbGetCharacter characterId userId
            |> optionToAppResult

        let getCharacters = CharactersService.getCharacters dbGetCharacters

        let addCharacter newCharacter = 
            CharactersService.addCharacter dbInsertNewCharacter newCharacter
            |> toAppResult

        let updateCharacter characterUpdate = 
            CharactersService.updateCharacter dbGetCharacter dbUpdateCharacter characterUpdate
            |> toAppResult

        let deleteCharacter characterId userId = 
            CharactersService.deleteCharacter dbGetCharacter dbDeleteCharacter characterId userId
            |> toAppResult

    module DeckSvc = 

        let private dbGetDiscards        = DeckRepository.getDiscards db
        let private dbInsertDiscard      = DeckRepository.insertDiscard db
        let private dbDeleteDiscards     = DeckRepository.deleteDiscards db

        let getDeck = DeckService.getDeck dbGetDiscards 

        let drawCard = DeckService.drawCard dbGetDiscards dbInsertDiscard

        let reshuffle = DeckService.reshuffle dbDeleteDiscards
