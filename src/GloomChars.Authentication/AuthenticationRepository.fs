namespace GloomChars.Authentication

open System
open GloomChars.Common
open GloomChars.Common.QueryUtils

[<RequireQualifiedAccess>]
module internal AuthenticationSql = 

    let getUserByEmail email = 
        sql
            """
            SELECT id as Id,
                email as Email, 
                password_hash as PasswordHash, 
                is_locked_out as IsLockedOut, 
                login_attempt_number as LoginAttemptNumber, 
                date_created as DateCreated, 
                date_updated as DateUpdated,
                date_locked_out as DateLockedOut
            FROM users
            WHERE email = @email
            """
            [ p "email" email ]

    let insertNewLogin (newLogin : NewLogin) = 
        let (AccessToken token) = newLogin.AccessToken

        sql
            """
            INSERT INTO auth_tokens
                (user_id, 
                access_token, 
                is_revoked,  
                date_created, 
                date_expires)
            VALUES 
                (@user_id, 
                @access_token, 
                false, 
                @date_created, 
                @date_expires)
            RETURNING id
            """
            [
                p "user_id" newLogin.UserId
                p "access_token" token
                p "date_expires" newLogin.AccessTokenExpiresAt
            ]

    let updateLoginDetails (statusUpdate : LoginStatusUpdate) = 
        sql
            """
            UPDATE users 
            SET is_locked_out = @is_locked_out,
                login_attempt_number = @login_attempt_number,
                date_locked_out = @date_locked_out
            WHERE id = @id
            """
            [
                p "id" statusUpdate.UserId
                p "is_locked_out" statusUpdate.IsLockedOut
                p "login_attempt_number" statusUpdate.AttemptNumber
                p "date_locked_out" statusUpdate.DateLockedOut
            ]

    let getAuthenticatedUser accessToken = 
        sql
            """
            SELECT users.id as Id,
                users.email as Email, 
                tokens.access_token as AccessToken, 
                tokens.date_expires as AccessTokenExpiresAt,
                users.is_system_admin AS IsSystemAdmin
            FROM users INNER JOIN auth_tokens AS tokens     
                ON users.id = tokens.user_id
            WHERE tokens.access_token = @access_token
                AND tokens.is_revoked = false
            """
            [ 
                p "access_token" accessToken 
            ]

    let revoke accessToken = 
        sql
            """
            UPDATE auth_tokens
            SET is_revoked = true,
                date_revoked = current_timestamp
            WHERE access_token = @access_token
            """
            [
                p "access_token" accessToken
            ]

[<RequireQualifiedAccess>]
module AuthenticationRepository = 

    let private mapToUser (dbUser : DbUser) = 
        let lockedOutStatus =
            match (dbUser.IsLockedOut, dbUser.DateLockedOut) with
            | (true, Some dt) -> 
                LockedOut dt
            | (true, None) ->
                //This would mean that the db is in an invalid state (perhaps the db design can change to make this impossible?).
                //Nevertheless, have to deal it because F# forces the code to account for all possible states.
                //Could add code to update the db in case this happens, but that would be overegineering at this point.
                LockedOut DateTime.UtcNow 
            | (false, _) ->
                NotLockedOut

        { 
            Id = dbUser.Id
            Email = dbUser.Email
            PasswordHash = dbUser.PasswordHash
            LoginAttemptNumber = dbUser.LoginAttemptNumber
            DateCreated = dbUser.DateCreated
            DateUpdated = dbUser.DateUpdated
            LockedOutStatus = lockedOutStatus
        }

    let getUserByEmail (dbContext : IDbContext) (email : string) : User option = 
        AuthenticationSql.getUserByEmail email
        |> dbContext.Query<DbUser>
        |> Array.tryHead
        |> Option.map mapToUser

    let insertNewLogin (dbContext : IDbContext) (newLogin : NewLogin) = 
        let result =
            AuthenticationSql.insertNewLogin newLogin
            |> dbContext.TryExecuteScalar

        match result with 
        | Success loginId -> 
            Ok loginId
        | UniqueConstraintError _ ->
            Error "Access token already exists." //Guid collision... hmm...

    let updateLoginStatus (dbContext : IDbContext) statusUpdate = 
        AuthenticationSql.updateLoginDetails statusUpdate
        |> dbContext.Execute
        |> ignore

    let getAuthenticatedUser (dbContext : IDbContext) accessToken = 
        let (AccessToken token) = accessToken
        AuthenticationSql.getAuthenticatedUser token
        |> dbContext.Query<AuthenticatedUser>
        |> Array.tryHead

    let revokeToken (dbContext : IDbContext) accessToken = 
        let (AccessToken token) = accessToken
        AuthenticationSql.revoke token
        |> dbContext.Execute
        |> ignore