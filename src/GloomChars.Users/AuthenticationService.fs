﻿namespace GloomChars.Users

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

    let private getUser dbGetAuthenticatedUser (newLogin : NewLogin) = 
        match dbGetAuthenticatedUser newLogin.AccessToken with 
        | Some user -> Ok user
        | None -> Error InvalidAccessToken //This would be very weird

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

    let verify (plainPassword : PlainPassword) (user : PreAuthUser) = 
        match PasswordUtils.verifyPassword user.Email user.PasswordHash plainPassword with 
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

    let getUserForAuth 
        (dbGetPreAuthUser : string -> PreAuthUser option)
        (email : string) =

        dbGetPreAuthUser email

[<RequireQualifiedAccess>]
module AuthenticationService =

    let private getPreAuthUser dbGetUserForAuth email = 
        match dbGetUserForAuth email with 
        | Some user -> 
            Ok user
        | None -> 
            //Do a fake password check (to hamper time based attacks). 
            PasswordUtils.hashFakePassword()
            Error EmailNotInSystem

    let authenticate 
        (config : AuthenticationConfig)
        (repo : IAuthenticationRepository)
        (email : string)
        (password : PlainPassword) = 

        let getUserByEmail = getPreAuthUser repo.GetUserForAuth 
        let verifyPassword = PasswordVerifier.verify password
        let checkIfLockedOut = LockoutChecker.check config
        let createNewLogin = LoginCreator.create config repo.InsertNewLogin repo.GetAuthenticatedUser
        let saveLoginAttempt = AuthenticationAttempts.saveAuthAttempt config repo.UpdateLoginStatus

        email
        |> getUserByEmail
        >>= verifyPassword
        >>= checkIfLockedOut
        >>= createNewLogin
        |> saveLoginAttempt 

    let getAuthenticatedUser 
        (dbGetAuthUser : AccessToken -> AuthenticatedUser option) 
        (accessToken : AccessToken)= 

        match dbGetAuthUser accessToken with 
        | None -> Error "Invalid access token."
        | Some user -> Ok user

    let revokeToken 
        (dbRevoke : AccessToken -> unit)
        (accessToken : AccessToken) = 
        dbRevoke accessToken

    let changePassword 
        (dbGetUserByToken : AccessToken -> PreAuthUser option)
        (dbUpdatePassword : NewPasswordInfo -> int)
        (passwordUpdate : PasswordUpdate) = 

        let getPreAuthUserByToken token = 
            dbGetUserByToken token
            |> function 
            | Some user -> Ok user
            | None -> Error "Invalid access token"

        let toNewPasswordInfo (newPassword : NewPassword) (user : PreAuthUser) = 
            let (NewPassword newPwd) = newPassword
            PasswordUtils.hashPassword user.Email (PlainPassword newPwd)
            |> map (fun p -> { UserId = user.Id; PasswordHash = p }) 

        let verifyOldPassword (oldPassword : OldPassword) = 
            let (OldPassword oldPwd) = passwordUpdate.OldPassword
            PasswordVerifier.verify (PlainPassword oldPwd)
            >> Result.mapError (fun _ -> "Invalid current password")

        let hashPassword plainPassword (user : PreAuthUser) = 
            PasswordUtils.hashPassword user.Email plainPassword

        passwordUpdate.AccessToken
        |> getPreAuthUserByToken 
        >>= verifyOldPassword passwordUpdate.OldPassword
        >>= toNewPasswordInfo passwordUpdate.NewPassword
        |> map dbUpdatePassword 

    let create authConfig db = 

        let authRepo = AuthenticationRepository.create db

        let auth email password = 
            authenticate authConfig authRepo email password

        let getAuthUser = 
            getAuthenticatedUser authRepo.GetAuthenticatedUser

        let revoke = 
            revokeToken authRepo.RevokeToken 

        let changePwd =
            (authRepo.GetUserForAuthByToken, authRepo.UpdatePassword)
            ||> changePassword

        { new IAuthenticationService with 
            member __.Authenticate email password   = auth email password
            member __.GetAuthenticatedUser token    = getAuthUser token
            member __.RevokeToken token             = revoke token
            member __.ChangePassword passwordUpdate = changePwd passwordUpdate }

