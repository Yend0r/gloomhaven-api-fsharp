﻿namespace GloomChars.Users

open System

type AccessToken = AccessToken of string

type AuthenticationConfig = 
    {
        AccessTokenDurationInMins   : int
        UseLockout                  : bool
        LoginAttemptsBeforeLockout  : int
        LockoutDurationInMins       : int
    }

type NewUser = 
    {
        Email    : string
        Password : string
    }

type LockedOutStatus = 
    | NotLockedOut
    | LockedOut of DateTime
    with 
    static member FromDb(isLockedOut, dateLockedOut) = 
        match (isLockedOut, dateLockedOut) with
        | (true, Some dt) -> 
            LockedOut dt
        | (true, None) ->
            //This would mean that the db is in an invalid state (perhaps the db design can change to make this impossible?).
            //Nevertheless, have to deal it because F# forces the code to account for all possible states.
            LockedOut DateTime.UtcNow 
        | (false, _) ->
            NotLockedOut

//Used to check auth credentials
type DbPreAuthUser = 
    { 
        Id                  : int
        Email               : string
        PasswordHash        : string
        IsLockedOut         : bool
        LoginAttemptNumber  : int
        DateCreated         : DateTime
        DateUpdated         : DateTime
        DateLockedOut       : DateTime option
    }

type PreAuthUser = 
    { 
        Id                  : int
        Email               : string
        PasswordHash        : string
        LoginAttemptNumber  : int
        DateCreated         : DateTime
        DateUpdated         : DateTime
        LockedOutStatus     : LockedOutStatus
    }

type AuthFailure =
    | EmailNotInSystem
    | PasswordMismatch of PreAuthUser
    | IsLockedOut of string
    | ErrorSavingToken
    | InvalidAccessToken

type DbAuthenticatedUser = 
    {
        Id                   : int
        Email                : string
        AccessToken          : string 
        AccessTokenExpiresAt : DateTime
        IsSystemAdmin        : bool
    }

type AuthenticatedUser = 
    {
        Id                   : int
        Email                : string
        AccessToken          : AccessToken 
        AccessTokenExpiresAt : DateTime
        IsSystemAdmin        : bool
    }

type DbUser = 
    { 
        Id                  : int
        Email               : string
        IsLockedOut         : bool
        DateCreated         : DateTime
        DateLockedOut       : DateTime option
    }

type User = 
    { 
        Id                  : int
        Email               : string
        DateCreated         : DateTime
        LockedOutStatus     : LockedOutStatus
    }
    
type NewLogin = 
    {
        UserId               : int
        AccessToken          : AccessToken 
        AccessTokenExpiresAt : DateTime
    }

type LoginStatusUpdate = 
    {
        UserId          : int
        AttemptNumber   : int
        IsLockedOut     : bool
        DateLockedOut   : DateTime option
    }

type PasswordUpdate = 
    {
        AccessToken : AccessToken
        OldPassword : string
        NewPassword : string
    }

type NewPassword = 
    {
        UserId       : int
        PasswordHash : string
    }

// Using an interface here ends up nicer... practicality is good
type IAuthenticationRepository = 
    abstract member GetAuthenticatedUser : AccessToken -> AuthenticatedUser option
    abstract member GetUserForAuth : string -> PreAuthUser option
    abstract member InsertNewLogin : NewLogin -> Result<NewLogin, string>
    abstract member UpdateLoginStatus : LoginStatusUpdate -> unit
