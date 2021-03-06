﻿namespace GloomChars.Users

[<RequireQualifiedAccess>]
module PasswordUtils = 
    open System
    open System.Text
    open Microsoft.AspNetCore.Identity

    // The Microsoft.AspNetCore.Identity hasher requires a user of type 'TUser' 
    // passed in but as far as I can see, it doesn't use it... it is probably for
    // apps that override it and use the user info
    // https://github.com/aspnet/Identity/blob/rel/2.0.0/src/Microsoft.Extensions.Identity.Core/PasswordHasher.cs
    type UserForHasher = { Email : string }

    let hashPassword email (password : PlainPassword) = 
        let userForHasher = { Email = email }
        let hasher = PasswordHasher()
        let (PlainPassword plainPwd) = password
        try
            hasher.HashPassword(userForHasher, plainPwd) 
            |> HashedPassword
            |> Ok
        with _err -> Error "Error hashing password"

    let verifyPassword email (hashedPassword : HashedPassword) (plainPassword : PlainPassword) = 
        let hasher = PasswordHasher()
        let userForHasher = { Email = email }
        let (PlainPassword plainPwd) = plainPassword
        let (HashedPassword hashedPwd) = hashedPassword
        try
            let result = hasher.VerifyHashedPassword(userForHasher, hashedPwd, plainPwd)
            match result with 
            | PasswordVerificationResult.Success -> 
                true
            | _ -> 
                false //Ignoring the case of "Success - rehash required" for now
        with _exn -> 
            reraise()

    //Fake password check (to hamper time based attacks). 
    let hashFakePassword() = 
        let fakeHashedPwd =
            Guid.NewGuid().ToString() |> Encoding.UTF8.GetBytes |> Convert.ToBase64String |> HashedPassword
        let fakePlainPwd = "fake" |> PlainPassword
        verifyPassword "fake.email" fakeHashedPwd fakePlainPwd |> ignore