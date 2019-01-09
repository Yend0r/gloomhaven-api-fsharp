namespace GloomChars.Tests

open System
open Xunit
open FsUnit
open GloomChars.Users

module AuthenticationTests = 

    let errorString = "ERROR"

    let config = 
        {
            AccessTokenDurationInMins  = 10
            UseLockout                 = true
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

    let validUserId = 42
    let invalidUserId = 88888888
    let validToken = "valid_token"
    let invalidToken = "invalid_token_error"
    let validEmail = "test@example.com"
    let validPreAuthUser =  
        { 
            Id                  = validUserId
            Email               = validEmail
            PasswordHash        = validPwd.Hash
            LoginAttemptNumber  = 0
            DateCreated         = DateTime.Now
            DateUpdated         = DateTime.Now
            LockedOutStatus     = NotLockedOut
        }

    let validAuthUser : AuthenticatedUser = 
        {
            Id                   = validUserId
            Email                = validEmail
            AccessToken          = AccessToken validToken
            AccessTokenExpiresAt = DateTime.UtcNow.AddDays(5.0)
            IsSystemAdmin        = false
        }

    // These are weird values so the tests can make sure that they never occur
    let testLoginStatus : LoginStatusUpdate = 
        {
            UserId          = 0 
            AttemptNumber   = 8888
            IsLockedOut     = true
            DateLockedOut   = Some (DateTime.UtcNow.AddDays(5.0))
        }

    // These are weird values so the tests can make sure that they never occur
    let testNewLogin : NewLogin = 
        {
            UserId               = 0 
            AccessToken          = AccessToken "invalid_token_error"
            AccessTokenExpiresAt = DateTime.UtcNow.AddDays(5.0)
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

    let toTestResult result = 
        match result with
        | Ok _ -> Pass
        | Error err -> Fail err

    let getValidUser (email : string) = Some validPreAuthUser

    let dbUpdateLoginStatus status = ()

    let dbInsertNewLogin newLogin : Result<NewLogin, string> = Ok newLogin

    let fail_dbInsertNewLogin _ : Result<NewLogin, string> = Error "Fail"

    let dbGetAuthenticatedUser token = Some { validAuthUser with AccessToken = token }

    let authFailureToString authFailure = 
        match authFailure with
        | IsLockedOut _ -> "IsLockedOut"
        | PasswordMismatch _ -> "PasswordMismatch"
        | ErrorSavingToken -> "ErrorSavingToken" 
        | EmailNotInSystem -> "EmailNotInSystem"
        | InvalidAccessToken -> "InvalidAccessToken"

    //
    // LoginCreator tests ----------------------------------
    //

    let newLoginToTuple (result : Result<NewLogin, AuthFailure>) = 
        match result with
        | Ok newLogin -> (newLogin.UserId, newLogin.AccessTokenExpiresAt, "")
        | Error authFailure -> (0, DateTime.UtcNow, authFailureToString authFailure)

    let authUserToTuple (result : Result<AuthenticatedUser, AuthFailure>) = 
        match result with
        | Ok authUser -> 
            let (AccessToken token) = authUser.AccessToken
            (authUser.Id, token, authUser.AccessTokenExpiresAt, "")
        | Error authFailure -> 
            (0, invalidToken, DateTime.UtcNow, authFailureToString authFailure)

    [<Fact>]
    let ``Login creator should create an access token with an expiry in the future`` () =

        let mutable newLogin = testNewLogin

        let insertNewLogin data = 
            newLogin <- data //Use a mutable field to see what is being passed to this method
            Ok newLogin

        let (userId, authUsertoken, expiry, error) = 
            LoginCreator.create
                config 
                insertNewLogin
                dbGetAuthenticatedUser
                validPreAuthUser
            |> authUserToTuple

        let (newLoginUserId, AccessToken newLoginToken) = (newLogin.UserId, newLogin.AccessToken)

        newLoginUserId |> should equal validUserId
        newLoginToken  |> should equal authUsertoken
        userId |> should equal validUserId
        expiry |> should be (greaterThan DateTime.UtcNow)
        error  |> should equal ""

    [<Fact>]
    let ``Login creator should return an error if saving the login fails`` () =

        let (_, _, _, error) = 
            LoginCreator.create
                config 
                fail_dbInsertNewLogin
                dbGetAuthenticatedUser
                validPreAuthUser
            |> authUserToTuple

        error |> should equal "ErrorSavingToken"

    //
    // LockoutChecker tests ----------------------------------
    //

    let lockoutCheckToTuple (result : Result<PreAuthUser, AuthFailure>) = 
        match result with
        | Ok user -> (user.Id, user.LockedOutStatus, user.LoginAttemptNumber, "")
        | Error authFailure -> (0, NotLockedOut, 0, authFailureToString authFailure)

    [<Fact>]
    let ``Login checker should not block a non-lockedout user`` () =

        let (userId, lockedOutStatus, attemptNum, error) = 
            LockoutChecker.check 
                config 
                validPreAuthUser
            |> lockoutCheckToTuple

        userId          |> should equal validUserId
        lockedOutStatus |> should equal NotLockedOut
        attemptNum      |> should equal 0
        error           |> should equal ""

    [<Fact>]
    let ``Login checker should block a locked out user within the lockout duration`` () =

        let lockedOutUser = { validPreAuthUser with LockedOutStatus = LockedOut DateTime.UtcNow }

        let (userId, _, _, error) = 
            LockoutChecker.check 
                config 
                lockedOutUser
            |> lockoutCheckToTuple

        userId |> should equal 0
        error  |> should equal "IsLockedOut"

    [<Fact>]
    let ``Login checker should unlock a locked out user outside the lockout duration`` () =

        let lockedOutUser = { validPreAuthUser with LockedOutStatus = LockedOut (DateTime.UtcNow.AddDays(-1.0)) }

        let (userId, lockedOutStatus, attemptNum, error) = 
            LockoutChecker.check 
                config 
                lockedOutUser
            |> lockoutCheckToTuple

        userId          |> should equal validUserId
        lockedOutStatus |> should equal NotLockedOut
        attemptNum      |> should equal 0
        error           |> should equal ""

    [<Fact>]
    let ``Login checker should unlock a locked out user if lockout is not enabled`` () =

        let noLockoutConfig = { config with UseLockout = false }
        let lockedOutUser = { validPreAuthUser with LockedOutStatus = LockedOut DateTime.UtcNow }

        let (userId, lockedOutStatus, attemptNum, error) = 
            LockoutChecker.check 
                noLockoutConfig 
                lockedOutUser
            |> lockoutCheckToTuple

        userId          |> should equal validUserId
        lockedOutStatus |> should equal NotLockedOut
        attemptNum      |> should equal 0
        error           |> should equal ""

    //
    // AuthenticationAttempts tests ----------------------------------
    //

    let authAttemptToTuple (result : Result<AuthenticatedUser, AuthFailure>) = 
        match result with
        | Ok user -> 
            let (AccessToken token) = user.AccessToken
            (token, "")
        | Error authFailure -> ("", authFailureToString authFailure)

    [<Fact>]
    let ``Successful login should clear the number of attempts and any lockout`` () =

        let successLogin : Result<AuthenticatedUser, AuthFailure> = 
            Ok validAuthUser

        let mutable loginStatus = testLoginStatus

        let updateLoginStatus status = 
            loginStatus <- status //Use a mutable field to see what is being passed to this method
            ()

        let (token, error) = 
            AuthenticationAttempts.saveAuthAttempt 
                config 
                updateLoginStatus
                successLogin
            |> authAttemptToTuple

        token                     |> should equal validToken
        loginStatus.UserId        |> should equal validUserId
        loginStatus.AttemptNumber |> should equal 0
        loginStatus.IsLockedOut   |> should equal false
        loginStatus.DateLockedOut |> should equal None
        error                     |> should equal ""

    [<Fact>]
    let ``Failed password mismatch login should increment the number of attempts`` () =

        let failedLogin : Result<AuthenticatedUser, AuthFailure> = Error (PasswordMismatch validPreAuthUser)

        let mutable loginStatus = testLoginStatus

        let updateLoginStatus status = 
            loginStatus <- status //Use a mutable field to see what is being passed to this method
            ()

        let (token, error) = 
            AuthenticationAttempts.saveAuthAttempt 
                config 
                updateLoginStatus
                failedLogin
            |> authAttemptToTuple

        token                     |> should equal ""
        loginStatus.UserId        |> should equal validUserId
        loginStatus.AttemptNumber |> should equal 1
        loginStatus.IsLockedOut   |> should equal false
        loginStatus.DateLockedOut |> should equal None
        error                     |> should equal "PasswordMismatch"

    [<Fact>]
    let ``Failed password mismatch login should increment the number of attempts and lockout user if required`` () =

        let user = { validPreAuthUser with LoginAttemptNumber = config.LoginAttemptsBeforeLockout }

        let failedLogin : Result<AuthenticatedUser, AuthFailure> = Error (PasswordMismatch user)

        let mutable loginStatus = testLoginStatus

        let updateLoginStatus status = 
            loginStatus <- status //Use a mutable field to see what is being passed to this method
            ()

        let (token, error) = 
            AuthenticationAttempts.saveAuthAttempt 
                config 
                updateLoginStatus
                failedLogin
            |> authAttemptToTuple

        token                            |> should equal ""
        loginStatus.UserId               |> should equal validUserId
        loginStatus.AttemptNumber        |> should equal (user.LoginAttemptNumber + 1)
        loginStatus.IsLockedOut          |> should equal true
        loginStatus.DateLockedOut.IsSome |> should equal true
        error                            |> should equal "PasswordMismatch"

    [<Fact>]
    let ``If lockout not enabled, a failed password mismatch login clear the number of attempts and any lockout`` () =

        let noLockoutConfig = { config with UseLockout = false }
        let user = { validPreAuthUser with LoginAttemptNumber = config.LoginAttemptsBeforeLockout }

        let failedLogin : Result<AuthenticatedUser, AuthFailure> = Error (PasswordMismatch user)

        let mutable loginStatus = testLoginStatus

        let updateLoginStatus status = 
            loginStatus <- status //Use a mutable field to see what is being passed to this method
            ()

        let (token, error) = 
            AuthenticationAttempts.saveAuthAttempt 
                noLockoutConfig 
                updateLoginStatus
                failedLogin
            |> authAttemptToTuple

        token                     |> should equal ""
        loginStatus.UserId        |> should equal validUserId
        loginStatus.AttemptNumber |> should equal 0
        loginStatus.IsLockedOut   |> should equal false
        loginStatus.DateLockedOut |> should equal None
        error                     |> should equal "PasswordMismatch"

    [<Fact>]
    let ``Failed login due to 'IsLockedOut' (not password mismatch) should not call status update`` () =

        let failedLogin : Result<AuthenticatedUser, AuthFailure> = Error (IsLockedOut "blah")

        let mutable loginStatus = testLoginStatus

        let updateLoginStatus status = 
            loginStatus <- status //Use a mutable field to see what is being passed to this method
            ()

        let (token, error) = 
            AuthenticationAttempts.saveAuthAttempt 
                config 
                updateLoginStatus
                failedLogin
            |> authAttemptToTuple

        token                     |> should equal ""
        loginStatus.UserId        |> should equal testLoginStatus.UserId
        loginStatus.AttemptNumber |> should equal testLoginStatus.AttemptNumber
        loginStatus.IsLockedOut   |> should equal testLoginStatus.IsLockedOut
        loginStatus.DateLockedOut |> should equal testLoginStatus.DateLockedOut
        error                     |> should equal "IsLockedOut"

    [<Fact>]
    let ``Failed login due to 'ErrorSavingToken' (not password mismatch) should not call status update`` () =

        let failedLogin : Result<AuthenticatedUser, AuthFailure> = Error ErrorSavingToken

        let mutable loginStatus = testLoginStatus

        let updateLoginStatus status = 
            loginStatus <- status //Use a mutable field to see what is being passed to this method
            ()

        let (token, error) = 
            AuthenticationAttempts.saveAuthAttempt 
                config 
                updateLoginStatus
                failedLogin
            |> authAttemptToTuple

        token                     |> should equal ""
        loginStatus.UserId        |> should equal testLoginStatus.UserId
        loginStatus.AttemptNumber |> should equal testLoginStatus.AttemptNumber
        loginStatus.IsLockedOut   |> should equal testLoginStatus.IsLockedOut
        loginStatus.DateLockedOut |> should equal testLoginStatus.DateLockedOut
        error                     |> should equal "ErrorSavingToken"

    [<Fact>]
    let ``Failed login due to 'EmailNotInSystem' (not password mismatch) should not call status update`` () =

        let failedLogin : Result<AuthenticatedUser, AuthFailure> = Error EmailNotInSystem

        let mutable loginStatus = testLoginStatus

        let updateLoginStatus status = 
            loginStatus <- status //Use a mutable field to see what is being passed to this method
            ()

        let (token, error) = 
            AuthenticationAttempts.saveAuthAttempt 
                config 
                updateLoginStatus
                failedLogin
            |> authAttemptToTuple

        token                     |> should equal ""
        loginStatus.UserId        |> should equal testLoginStatus.UserId
        loginStatus.AttemptNumber |> should equal testLoginStatus.AttemptNumber
        loginStatus.IsLockedOut   |> should equal testLoginStatus.IsLockedOut
        loginStatus.DateLockedOut |> should equal testLoginStatus.DateLockedOut
        error                     |> should equal "EmailNotInSystem"
     