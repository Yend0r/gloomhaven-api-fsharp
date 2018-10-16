namespace GloomChars.Tests

open System
open Xunit
open FsUnit
open GloomChars.Authentication

module AuthenticationTests = 

    let config = 
        {
            AccessTokenDurationInMins  = 10
            LoginAttemptsBeforeLockout = 2
            LockoutDurationInMins      = 5
        }

    type Password = 
        {
            PlainText : string
            Hash : string
        }

    let validPwd = 
        { 
            PlainText = "test1234"
            Hash = "AQAAAAEAACcQAAAAEFAz9Z0mJChsqre81ZLf9pnGVReDv8iR6Udw/p9PMtWQUWGSGHt4jCSUBd+6R1M3dQ==" 
        }

    let validToken = "valid_token"
    let validEmail = "test@example.com"
    let validPreAuthUser =  
        { 
            Id                  = 1
            Email               = validEmail
            PasswordHash        = validPwd.Hash
            LoginAttemptNumber  = 0
            DateCreated         = DateTime.Now
            DateUpdated         = DateTime.Now
            LockedOutStatus     = NotLockedOut
        }

    let validCredentialsText = sprintf "AccessToken %s" validToken
    let invalidCredentialsText = "Invalid email/password."

    let lockedOutText = 
        sprintf 
            "Account is locked out. Please wait %i mins or contact an administrator." 
            config.LockoutDurationInMins

    type TestResult =
        | Pass
        | Fail of obj

    let toResultString (result : Result<AccessToken, string>) : string = 
        match result with
        | Ok accessToken -> 
            let (AccessToken token) = accessToken
            sprintf "AccessToken %s" token
        | Error err -> 
            err

    let toTestResult result = 
        match result with
        | Ok _ -> Pass
        | Error err -> Fail err



    let getValidUser (email : string) = Some validPreAuthUser

    let dbUpdateLoginStatus status = ()

    let dbInsertNewLogin newLogin = Ok (AccessToken "valid_token")
    
    [<Fact>]
    let ``Valid password should pass`` () =

        let result = 
            AuthenticationService.authenticate
                config 
                getValidUser
                dbUpdateLoginStatus 
                dbInsertNewLogin
                validEmail
                validPwd.PlainText
                |> toResultString

        result |> should equal validCredentialsText

    [<Fact>]
    let ``Invalid password should fail`` () =

        let result = 
            AuthenticationService.authenticate
                config 
                getValidUser 
                dbUpdateLoginStatus 
                dbInsertNewLogin
                validEmail
                "NOT_VALID_PASSWORD"
                |> toResultString

        result |> should equal invalidCredentialsText

    [<Fact>]
    let ``Invalid email (no user returned) should fail`` () =

        let getNoUser (email : string) = None

        let result = 
            AuthenticationService.authenticate
                config 
                getNoUser 
                dbUpdateLoginStatus 
                dbInsertNewLogin
                "NOT_VALID_EMAIL"
                validPwd.PlainText
                |> toResultString

        result |> should equal invalidCredentialsText

    [<Fact>]
    let ``Locked out user (within lockout period) should fail`` () =

        let lockedOutUser = { validPreAuthUser with LockedOutStatus = LockedOut (DateTime.UtcNow.AddMinutes(-1.0)) }
        let getLockedOutUser (email : string) = Some lockedOutUser
        
        let result = 
            AuthenticationService.authenticate
                config 
                getLockedOutUser 
                dbUpdateLoginStatus 
                dbInsertNewLogin
                validEmail
                validPwd.PlainText
                |> toResultString

        result |> should equal lockedOutText

    [<Fact>]
    let ``Locked out user (after lockout period) should pass`` () =

        let lockedOutUser = { validPreAuthUser with LockedOutStatus = LockedOut (DateTime.UtcNow.AddMinutes(-8.0)) }
        let getLockedOutUser (email : string) = Some lockedOutUser
        
        let result = 
            AuthenticationService.authenticate
                config 
                getLockedOutUser 
                dbUpdateLoginStatus 
                dbInsertNewLogin
                validEmail
                validPwd.PlainText
                |> toResultString

        result |> should equal validCredentialsText

    [<Fact>]
    let ``Locked out user (if config lockout duration set to 0) should pass`` () =

        let noLockoutConfig = { config with LockoutDurationInMins = 0 }
        let lockedOutUser = { validPreAuthUser with LockedOutStatus = LockedOut (DateTime.UtcNow.AddMinutes(-1.0)) }
        let getLockedOutUser (email : string) = Some lockedOutUser
        
        let result = 
            AuthenticationService.authenticate
                noLockoutConfig 
                getLockedOutUser 
                dbUpdateLoginStatus 
                dbInsertNewLogin
                validEmail
                validPwd.PlainText
                |> toResultString

        result |> should equal validCredentialsText