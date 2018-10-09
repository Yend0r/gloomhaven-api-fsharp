namespace GloomChars.Authentication

[<RequireQualifiedAccess>]
module internal UserSql = 
    open GloomChars.Common.QueryUtils

    let insertNewUser email passwordHash = 
        sql
            """
            INSERT INTO users
                (email, 
                password_hash, 
                is_locked_out, 
                login_attempt_number, 
                date_created, 
                date_updated)
            VALUES 
                (@email, 
                @password_hash, 
                false, 
                0, 
                current_timestamp, 
                current_timestamp)
            RETURNING id
            """
            [
                p "email" email
                p "password_hash" passwordHash
            ]

[<RequireQualifiedAccess>]
module UserRepository = 
    open GloomChars.Common

    let insertNewUser 
        (dbContext : IDbContext)
        (email : string) 
        (hashedPassword : string) : Result<int,string> = 
        
        let result =
            UserSql.insertNewUser email hashedPassword
            |> dbContext.TryExecuteScalar

        match result with 
        | Success id -> 
            Ok id
        | UniqueConstraintError _ ->
            Error "A user account with that email already exists."