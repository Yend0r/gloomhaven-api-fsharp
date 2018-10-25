namespace GloomChars.Authentication

open System
open System.Text
open GloomChars.Common
open FSharpPlus

type AuthFailure =
    | EmailNotInSystem
    | PasswordMismatch of PreAuthUser
    | IsLockedOut of string
    | ErrorSavingToken

[<RequireQualifiedAccess>]
module AuthenticationService =

    let private updateLoginStatus dbUpdateLoginStatus (userId, attemptNumber, isLockedOut) = 
        let dateLockedOut = 
            match isLockedOut with
            | true -> Some DateTime.UtcNow
            | false -> None

        { 
            UserId = userId
            AttemptNumber = 0
            IsLockedOut = false
            DateLockedOut = dateLockedOut 
        }
        |> dbUpdateLoginStatus

    let private logAuthFailure updateStatus (attemptsBeforeLockout : int) user = 
        //Update details with the login attempt
        let attemptNumber = user.LoginAttemptNumber + 1
        let isLockedOut = (attemptNumber > attemptsBeforeLockout)
        updateStatus (user.Id, attemptNumber, isLockedOut)

    let private shouldUnlock (dateLockedOut : DateTime) lockoutDuration = 
        lockoutDuration = 0 || dateLockedOut.AddMinutes(float lockoutDuration) < DateTime.UtcNow

    let private checkLockout (lockoutDuration : int) (user : PreAuthUser) = 
        match user.LockedOutStatus with
        | LockedOut dateLockedOut -> 
            if shouldUnlock dateLockedOut lockoutDuration then
                Ok { user with LockedOutStatus = NotLockedOut }
            else
                //let dateLockedOut = dateLockedOut.AddMinutes(float lockoutDuration)
                //let msg = sprintf "Locked out: %i mins (%A - %A)" lockoutDuration dateLockedOut DateTime.UtcNow
                let msg = sprintf "Account is locked out. Please wait %i mins or contact an administrator." lockoutDuration
                Error (IsLockedOut msg)

        | NotLockedOut -> 
            Ok user

    let private verifyPassword password (user : PreAuthUser) = 
        //If we got a user then always do the password check to hamper time based attacks
        let passwordVerified = 
            PasswordHasher.verifyHashedPassword(user.Email, user.PasswordHash, password)

        match passwordVerified with 
        | true -> Ok user
        | false -> Error (PasswordMismatch user)

    let private createAccessToken() = 
        AccessToken(string (Guid.NewGuid())) 

    let private makeNewLogin userId tokenDuration = 
        {
            UserId = userId
            AccessToken = createAccessToken() 
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(float tokenDuration) 
        }

    let private createLogin
        (config : AuthenticationConfig) 
        (dbInsertNewLogin : NewLogin -> Result<NewLogin, string>)
        (user : PreAuthUser) = 

        //Log them in
        makeNewLogin user.Id config.AccessTokenDurationInMins
        |> dbInsertNewLogin 
        |> Result.mapError (fun _ -> ErrorSavingToken)

    let private hashFakePassword() = 
        let fakePwd = Guid.NewGuid().ToString() |> Encoding.UTF8.GetBytes |> Convert.ToBase64String
        PasswordHasher.verifyHashedPassword("", fakePwd, "") |> ignore

    let private getUserByEmail dbGetUserForAuth email = 
        match dbGetUserForAuth email with 
        | None -> 
            //No such user... do a fake password check (to take the same time as a real email) 
            //so attackers cannot tell that the email doesn't exist. 
            hashFakePassword()
            Error EmailNotInSystem
        | Some user ->
            Ok user

    let private onSuccess updateStatus (newLogin : NewLogin) = 
        //Clear any failed lockout attempts
        updateStatus (newLogin.UserId, 0, false) 
        newLogin.AccessToken

    let private onFailure updateStatus config authError = 
        match authError with
        | EmailNotInSystem -> 
            "Invalid email/password."
        | IsLockedOut msg -> 
            msg
        | PasswordMismatch user ->
            logAuthFailure updateStatus config.LoginAttemptsBeforeLockout user
            "Invalid email/password."
        | ErrorSavingToken -> 
            //Caused by token collision? If so then making the user try again should fix it
            "Invalid email/password." 

    let authenticate 
        (config : AuthenticationConfig)
        (dbGetUserForAuth : string -> PreAuthUser option)
        (dbUpdateLoginStatus : LoginStatusUpdate -> unit)
        (dbInsertNewLogin : NewLogin -> Result<NewLogin, string>)
        (email : string)
        (password : string) = 

        //Partially apply some fns to make the final logic clearer
        let getUser = dbGetUserForAuth |> getUserByEmail 
        let updateStatus = dbUpdateLoginStatus |> updateLoginStatus  
        let checkIfLockedOut = config.LockoutDurationInMins |> checkLockout 
        let createAccessToken = (config, dbInsertNewLogin) ||> createLogin 

        getUser email
        >>= verifyPassword password 
        >>= checkIfLockedOut
        >>= createAccessToken
        |> map (onSuccess updateStatus)
        |> Result.mapError (onFailure updateStatus config)
 
    let getAuthenticatedUser 
        (dbGetAuthenticatedUser : AccessToken -> AuthenticatedUser option) 
        accessToken = 
        
        match dbGetAuthenticatedUser accessToken with 
        | None -> Error "Invalid access token."
        | Some user -> Ok user

    let revokeToken (dbRevoke : AccessToken -> unit) accessToken = 
        dbRevoke accessToken