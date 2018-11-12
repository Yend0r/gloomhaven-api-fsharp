namespace GloomChars.Authentication

open System
open System.Text
open GloomChars.Common
open FSharpPlus

[<RequireQualifiedAccess>]
module LoginCreator =

    let private createAccessToken() = 
        (Guid.NewGuid()) |> string |> AccessToken 

    let private makeNewLogin userId tokenDuration = 
        {
            UserId = userId
            AccessToken = createAccessToken()
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(float tokenDuration) 
        }

    let private getUser dbGetAuthenticatedUser newLogin = 
        match dbGetAuthenticatedUser newLogin.AccessToken with 
        | Some user -> Ok user
        | None -> Error AuthUserNotFound //This would be very weird

    let create
        (config : AuthenticationConfig) 
        (dbInsertNewLogin : NewLogin -> Result<NewLogin, string>)
        (dbGetAuthenticatedUser : AccessToken -> AuthenticatedUser option) 
        (user : PreAuthUser) = 
        
        //Log them in
        makeNewLogin user.Id config.AccessTokenDurationInMins
        |> dbInsertNewLogin 
        |> Result.mapError (fun _ -> ErrorSavingToken)
        >>= (getUser dbGetAuthenticatedUser)

[<RequireQualifiedAccess>]
module PasswordVerifier =

    let verify password (user : PreAuthUser) =         
        let passwordVerified = 
            PasswordHasher.verifyHashedPassword(user.Email, user.PasswordHash, password)

        match passwordVerified with 
        | true  -> Ok user
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
        ()

    let private onSuccess clearAttempts (user : AuthenticatedUser) = 
        //Clear any failed lockout attempts
        clearAttempts user.Id 
        ()

    let private onFailure logFailure authError = 
        match authError with
        | PasswordMismatch user -> logFailure user 
        | _                     -> ()

    let saveAuthAttempt config dbUpdateLoginStatus (authResult : Result<AuthenticatedUser, AuthFailure>) = 
        let clearAttempts = clearLoginAttempts dbUpdateLoginStatus 
        let logFailure = logFailedAttempt config dbUpdateLoginStatus 

        authResult |> either (onSuccess clearAttempts) (onFailure logFailure)

        authResult

[<RequireQualifiedAccess>]
module AuthUserService =
    let private getPreAuthUser dbGetUserForAuth email = 
        match dbGetUserForAuth email with 
        | Some user -> Ok user
        | None -> 
            //Do a fake password check (to hamper time based attacks). 
            PasswordHasher.hashFakePassword()
            Error EmailNotInSystem

    let getUserForAuth 
        (dbGetPreAuthUser : string -> PreAuthUser option)
        (verifyPassword : string -> PreAuthUser -> Result<PreAuthUser, AuthFailure>)
        (lockoutChecker : PreAuthUser -> Result<PreAuthUser, AuthFailure>)
        (email : string) 
        (password : string) =

        getPreAuthUser dbGetPreAuthUser email
        >>= verifyPassword password 
        >>= lockoutChecker

[<RequireQualifiedAccess>]
module AuthenticationService =

    let authenticate 
        (getPreAuthUser   : string -> string -> Result<PreAuthUser, AuthFailure>)
        (createLogin      : PreAuthUser -> Result<AuthenticatedUser, AuthFailure>)
        (saveLoginAttempt : Result<AuthenticatedUser, AuthFailure> -> Result<AuthenticatedUser, AuthFailure>)
        (email : string)
        (password : string) = 

        (email, password)
        ||> getPreAuthUser 
        >>= createLogin
        |> saveLoginAttempt 

    let getAuthenticatedUser 
        (dbGetAuthUser : AccessToken -> AuthenticatedUser option)
        accessToken = 

        match dbGetAuthUser accessToken with 
        | None -> Error "Invalid access token."
        | Some user -> Ok user

    let revokeToken (dbRevoke : AccessToken -> unit) accessToken = 
        dbRevoke accessToken

