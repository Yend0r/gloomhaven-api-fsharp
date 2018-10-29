namespace GloomChars.Authentication

open System
open System.Text
open GloomChars.Common
open FSharpPlus

[<RequireQualifiedAccess>]
module LoginCreator =

    let private createAccessToken() = 
        AccessToken(string (Guid.NewGuid())) 

    let private makeNewLogin userId tokenDuration = 
        {
            UserId = userId
            AccessToken = createAccessToken() 
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(float tokenDuration) 
        }

    let create
        (config : AuthenticationConfig) 
        (dbInsertNewLogin : NewLogin -> Result<NewLogin, string>)
        (user : PreAuthUser) = 

        //Log them in
        makeNewLogin user.Id config.AccessTokenDurationInMins
        |> dbInsertNewLogin 
        |> Result.mapError (fun _ -> ErrorSavingToken)

[<RequireQualifiedAccess>]
module PasswordVerifier =

    let verify password (user : PreAuthUser) = 
        //If we got a user then always do the password check to hamper time based attacks
        let passwordVerified = 
            PasswordHasher.verifyHashedPassword(user.Email, user.PasswordHash, password)

        match passwordVerified with 
        | true -> Ok user
        | false -> Error (PasswordMismatch user)

[<RequireQualifiedAccess>]
module LockoutChecker =

    let private hasLockoutExpired useLockout lockoutDuration (dateLockedOut : DateTime) = 
        //lockoutDuration = 0 means that lockout is not enabled
        (not useLockout) || dateLockedOut.AddMinutes(float lockoutDuration) < DateTime.UtcNow

    let check config (user : PreAuthUser) = 
        let lockoutDuration = config.LockoutDurationInMins
        let lockoutExpired = (config.UseLockout, lockoutDuration) ||> hasLockoutExpired 

        match user.LockedOutStatus with
        | LockedOut dateLockedOut when not (lockoutExpired dateLockedOut) -> 
            //They are locked out  TODO: make this show the mins left until lockout expires
            let msg = sprintf "Account is locked out. Please wait %i mins or contact an administrator." lockoutDuration
            Error (IsLockedOut msg)
        | LockedOut dateLockedOut when (lockoutExpired dateLockedOut) -> 
            //Lockout has expired or is no longer being used, it's time to unlock...            
            Ok { user with LockedOutStatus = NotLockedOut }
        | _ -> 
            Ok user

[<RequireQualifiedAccess>]
module AuthenticationAttempts =

    let private clearLoginAttempts dbUpdateLoginStatus userId = 
        { 
            UserId = userId
            AttemptNumber = 0
            IsLockedOut = false
            DateLockedOut = None 
        }
        |> dbUpdateLoginStatus

    let private logFailedAttempt (config : AuthenticationConfig) dbUpdateLoginStatus (user : PreAuthUser) = 
        //Update details with the login attempt
        if not config.UseLockout then
            //Lockout is not enabled, so clear any failed attempts
            clearLoginAttempts dbUpdateLoginStatus user.Id
        else
            let attemptNumber = user.LoginAttemptNumber + 1
            let isLockedOut = (attemptNumber > config.LoginAttemptsBeforeLockout)

            { 
                UserId = user.Id
                AttemptNumber = attemptNumber
                IsLockedOut = isLockedOut
                DateLockedOut = if isLockedOut then Some DateTime.UtcNow else None
            }
            |> dbUpdateLoginStatus

    let private onSuccess clearAttempts (newLogin : NewLogin) = 
        //Clear any failed lockout attempts
        clearAttempts newLogin.UserId 
        newLogin.AccessToken

    let private onFailure logFailure authError = 
        match authError with
        | PasswordMismatch user -> logFailure user 
        | _ -> ()

        authError

    let saveAuthAttempt config dbUpdateLoginStatus (authResult : Result<NewLogin, AuthFailure>) = 
        let clearAttempts = clearLoginAttempts dbUpdateLoginStatus 
        let logFailure = logFailedAttempt config dbUpdateLoginStatus 

        match authResult with
        | Ok newLogin -> Ok (onSuccess clearAttempts newLogin)
        | Error authFailure -> Error (onFailure logFailure authFailure)

[<RequireQualifiedAccess>]
module AuthenticationService =

    let private hashFakePassword() = 
        let fakePwd = Guid.NewGuid().ToString() |> Encoding.UTF8.GetBytes |> Convert.ToBase64String
        PasswordHasher.verifyHashedPassword("", fakePwd, "") |> ignore

    let private getPreAuthUser dbGetUserForAuth email = 
        match dbGetUserForAuth email with 
        | None -> 
            //No such user... do a fake password check (to take the same time as a real email) 
            //so attackers cannot tell that the email doesn't exist. 
            hashFakePassword()
            Error EmailNotInSystem
        | Some user ->
            Ok user

    let private authFailureToString authError = 
        let genericError = "Invalid email/password."
        match authError with
        | IsLockedOut msg -> msg
        | _ -> genericError
        
    let authenticate 
        (dbGetPreAuthUser : string -> PreAuthUser option)
        (verifyPassword : string -> PreAuthUser -> Result<PreAuthUser, AuthFailure>)
        (lockoutChecker : PreAuthUser -> Result<PreAuthUser, AuthFailure>)
        (createLogin : PreAuthUser -> Result<NewLogin, AuthFailure>)
        (updateUserLoginAttempts : Result<NewLogin, AuthFailure> -> Result<AccessToken, AuthFailure>)
        (email : string)
        (password : string) = 

        getPreAuthUser dbGetPreAuthUser email
        >>= verifyPassword password 
        >>= lockoutChecker
        >>= createLogin
        |> updateUserLoginAttempts 
        |> Result.mapError (fun err -> authFailureToString err) //Change any errors to a descriptive string

    let getAuthenticatedUser 
        (dbGetAuthenticatedUser : AccessToken -> AuthenticatedUser option) 
        accessToken = 

        match dbGetAuthenticatedUser accessToken with 
        | None -> Error "Invalid access token."
        | Some user -> Ok user

    let revokeToken (dbRevoke : AccessToken -> unit) accessToken = 
        dbRevoke accessToken

  