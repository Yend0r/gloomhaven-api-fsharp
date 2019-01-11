﻿namespace GloomChars.Users

[<RequireQualifiedAccess>]
module internal UserSql = 
    open GloomChars.Common.QueryUtils

    let insertNewUser email passwordHash = 
        let (HashedPassword hashedPwd) = passwordHash

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
                p "password_hash" hashedPwd
            ]

    let getUserByEmail email = 
        sql
            """
            SELECT id as Id,
                email as Email, 
                is_locked_out as IsLockedOut,  
                date_created as DateCreated, 
                date_locked_out as DateLockedOut
            FROM users
            WHERE email = @email
            """
            [ p "email" email ]

    let getUser id = 
        sql
            """
            SELECT id as Id,
                email as Email, 
                is_locked_out as IsLockedOut,  
                date_created as DateCreated, 
                date_locked_out as DateLockedOut
            FROM users
            WHERE id = @id
            """
            [ p "id" id ]

    let getUserList = 
        sql
            """
            SELECT id as Id,
                email as Email, 
                is_locked_out as IsLockedOut,  
                date_created as DateCreated, 
                date_locked_out as DateLockedOut
            FROM users
            ORDER BY email
            """
            [ ]

[<RequireQualifiedAccess>]
module UserRepository = 
    open GloomChars.Common
    open FSharpPlus

    let private mapToUser (dbUser : DbUser) = 
        { 
            Id = dbUser.Id
            Email = dbUser.Email
            DateCreated = dbUser.DateCreated
            LockedOutStatus = LockedOutStatus.FromDb(dbUser.IsLockedOut, dbUser.DateLockedOut)
        }

    let insertNewUser 
        (dbContext : IDbContext)
        (email : string) 
        (hashedPassword : HashedPassword) = 
        
        UserSql.insertNewUser email hashedPassword
        |> dbContext.TryExecuteScalar
        |> function
        | Success id -> 
            Ok id
        | UniqueConstraintError _ ->
            Error "A user account with that email already exists."

    let getUserByEmail (dbContext : IDbContext) (email : string) : User option = 
        UserSql.getUserByEmail email
        |> dbContext.Query<DbUser>
        |> Array.tryHead
        |> map mapToUser

    let getUser (dbContext : IDbContext) (id : int) : User option = 
        UserSql.getUser id
        |> dbContext.Query<DbUser>
        |> Array.tryHead
        |> map mapToUser

    let getUsers (dbContext : IDbContext) () : User list = 
        UserSql.getUserList 
        |> dbContext.Query<DbUser>
        |> Array.toList
        |> map mapToUser