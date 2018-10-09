namespace GloomChars.Authentication

open System
open System.Text
open GloomChars.Common
open FSharpPlus

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

    let private clearLockout updateStatus userId = 
        updateStatus (userId, 0, false)

    let private updateLockoutStatus updateStatus (attemptsBeforeLockout : int) user = 
        //Update details with the login attempt
        let attemptNumber = user.LoginAttemptNumber + 1
        let isLockedOut = (attemptNumber > attemptsBeforeLockout)
        updateStatus (user.Id, attemptNumber, isLockedOut)

    let private shouldUnlock (dateLockedOut : DateTime) lockoutDuration = 
        lockoutDuration = 0 || dateLockedOut.AddMinutes(float lockoutDuration) < DateTime.UtcNow

    let private checkLockout (lockoutDuration : int) (user : User) = 
        match user.LockedOutStatus with
        | LockedOut dateLockedOut -> 
            if shouldUnlock dateLockedOut lockoutDuration then
                Ok { user with LockedOutStatus = NotLockedOut }
            else
                let dateLockedOut = dateLockedOut.AddMinutes(float lockoutDuration)
                let msg = sprintf "Locked out: %i mins (%A - %A)" lockoutDuration dateLockedOut DateTime.UtcNow
                //Error (sprintf "Account is locked out. Please wait %i mins or contact an administrator." lockoutDuration)
                Error msg

        | NotLockedOut -> 
            Ok user

    let private checkPassword (onInvalidPwd : User -> unit) password (user : User) = 
        //If we got a user then always do the password check to hamper time based attacks
        let passwordVerified = PasswordHasher.verifyHashedPassword(user.Email, user.PasswordHash, password)

        match passwordVerified with 
        | true -> 
            Ok user
        | false -> 
            //Increment the number of login attempts... may result in user being locked out 
            onInvalidPwd(user) 
            Error "Invalid email/password"

    let private createLogin
        (config : AuthenticationConfig) 
        (dbInsertNewLogin : NewLogin -> Result<int, string>)
        (onSuccessfulLogin : int -> unit)
        (user : User) = 

        //Log them in
        let newLogin = 
            {
                UserId = user.Id
                AccessToken = AccessToken(string (Guid.NewGuid())) 
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(float config.AccessTokenDurationInMins) 
            }

        dbInsertNewLogin newLogin
        |> function 
        | Ok _ -> 
            //Update user details - reset login attempts to zero
            onSuccessfulLogin(user.Id)
            Ok newLogin.AccessToken
        | Error err -> 
            Error err

    let private hashFakePassword() = 
        let fakePwd = Guid.NewGuid().ToString() |> Encoding.UTF8.GetBytes |> Convert.ToBase64String
        PasswordHasher.verifyHashedPassword("", fakePwd, "") |> ignore

    let private getUserByEmail dbGetUser email = 
        match dbGetUser email with 
        | None -> 
            //No such user... do a fake password check (to take the same time as a real email) 
            //so attackers cannot tell that the email doesn't exist. Although this is probably 
            //pointless because they can use the password reset form to check emails... maybe it defeats dumb attackers.
            hashFakePassword()
            Error "Invalid email/password."
        | Some user ->
            Ok user

    let authenticate 
        (config : AuthenticationConfig)
        (dbGetUser : string -> User option)
        (dbUpdateLoginStatus : LoginStatusUpdate -> unit)
        (dbInsertNewLogin : NewLogin -> Result<int, string>)
        (email : string)
        (password : string) = 

        //Partially apply some fns to make the final logic clearer
        let getUser = dbGetUser |> getUserByEmail 
        let updateStatus = dbUpdateLoginStatus |> updateLoginStatus  
        let verifyPassword = (updateStatus, config.LoginAttemptsBeforeLockout) ||> updateLockoutStatus |> checkPassword 
        let checkIfLockedOut = config.LockoutDurationInMins |> checkLockout 
        let onSuccessfulLogin = updateStatus |> clearLockout 
        let createAccessToken = (config, dbInsertNewLogin, onSuccessfulLogin) |||> createLogin 

        getUser email
        >>= verifyPassword password 
        >>= checkIfLockedOut
        >>= createAccessToken

    let getAuthenticatedUser (dbGetAuthenticatedUser : AccessToken -> AuthenticatedUser option) accessToken = 
        match dbGetAuthenticatedUser accessToken with 
        | None -> Error "Invalid access token."
        | Some user -> Ok user

    let revokeToken (dbRevoke : AccessToken -> unit) accessToken = 
        dbRevoke accessToken