namespace GloomChars.Authentication

open System

type AccessToken = AccessToken of string

type AuthenticationConfig = 
    {
        AccessTokenDurationInMins   : int
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
    static member fromDb(isLockedOut, dateLockedOut) = 
        match (isLockedOut, dateLockedOut) with
        | (true, Some dt) -> 
            LockedOut dt
        | (true, None) ->
            //This would mean that the db is in an invalid state (perhaps the db design can change to make this impossible?).
            //Nevertheless, have to deal it because F# forces the code to account for all possible states.
            //Could add code to update the db in case this happens, but that would be overegineering at this point.
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
