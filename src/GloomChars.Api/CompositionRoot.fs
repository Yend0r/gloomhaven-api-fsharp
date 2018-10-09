namespace GloomChars.Api

module CompositionRoot = 
    open GloomChars.Common 
    open GloomChars.Authentication

    let private config = Config.config
    let private db = PostgresDbContext(config.Database.ConnectionString)

    module AuthenticationSvc = 

        let dbGetUser               = AuthenticationRepository.getUserByEmail db
        let dbGetAuthenticatedUser  = AuthenticationRepository.getAuthenticatedUser db
        let dbUpdateLoginStatus     = AuthenticationRepository.updateLoginStatus db
        let dbInsertNewLogin        = AuthenticationRepository.insertNewLogin db
        let dbRevoke                = AuthenticationRepository.revokeToken db
        let dbInsertNewUser         = UserRepository.insertNewUser db

        let addUser = UserService.addUser //dbGetUser dbInsertNewUser

        let authenticate = AuthenticationService.authenticate 
                                config.Authentication 
                                dbGetUser
                                dbUpdateLoginStatus
                                dbInsertNewLogin

        let getAuthenticatedUser = AuthenticationService.getAuthenticatedUser dbGetAuthenticatedUser

        let revokeToken = AuthenticationService.revokeToken dbRevoke



