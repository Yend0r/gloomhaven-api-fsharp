namespace GloomChars.Api

open GloomChars.Common 
open Microsoft.Extensions.Configuration

type DbContext(connStr) = 

    interface IDbContext with
        member __.Query<'T>(sqlQuery) = PostgresDb.query<'T> connStr sqlQuery
        member __.QueryMulti2<'T1, 'T2>(sqlQuery) = PostgresDb.queryMulti2<'T1, 'T2> connStr sqlQuery
        member __.QueryMulti3<'T1, 'T2, 'T3>(sqlQuery) = PostgresDb.queryMulti3<'T1, 'T2, 'T3> connStr sqlQuery
        member __.Execute(sqlQuery) = PostgresDb.execute connStr sqlQuery
        member __.TryExecute(sqlQuery) = PostgresDb.tryExecute connStr sqlQuery
        member __.ExecuteMulti(sqlMultiQuery) = PostgresDb.executeMulti connStr sqlMultiQuery
        member __.TryExecuteMulti(sqlMultiQuery) = PostgresDb.tryExecuteMulti connStr sqlMultiQuery
        member __.ExecuteScalar<'T>(sqlQuery) = PostgresDb.executeScalar<'T> connStr sqlQuery
        member __.TryExecuteScalar<'T>(sqlQuery) = PostgresDb.tryExecuteScalar<'T> connStr sqlQuery

module CompositionRoot = 

    let private config    = Config.config
    let private db = DbContext(config.Database.ConnectionString)

    module AuthenticationSvc = 
        open GloomChars.Authentication

        let dbGetUser               = AuthenticationRepository.getUserByEmail db
        let dbGetAuthenticatedUser  = AuthenticationRepository.getAuthenticatedUser db
        let dbUpdateLoginStatus     = AuthenticationRepository.updateLoginStatus db
        let dbInsertNewLogin        = AuthenticationRepository.insertNewLogin db
        let dbRevoke                = AuthenticationRepository.revokeToken db

        let authenticate = AuthenticationService.authenticate 
                                config.Authentication 
                                dbGetUser
                                dbUpdateLoginStatus
                                dbInsertNewLogin

        let getAuthenticatedUser = AuthenticationService.getAuthenticatedUser dbGetAuthenticatedUser

        let revokeToken = AuthenticationService.revokeToken dbRevoke