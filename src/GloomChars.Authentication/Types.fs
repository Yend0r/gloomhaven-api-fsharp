namespace GloomChars.Authentication

open System

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

type DbUser = 
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

type LockedOutStatus = 
    | NotLockedOut
    | LockedOut of DateTime

type User = 
    { 
        Id                  : int
        Email               : string
        PasswordHash        : string
        LoginAttemptNumber  : int
        DateCreated         : DateTime
        DateUpdated         : DateTime
        LockedOutStatus     : LockedOutStatus
    }

type AuthenticatedUser = 
    {
        Id                   : int
        Email                : string
        AccessToken          : string 
        AccessTokenExpiresAt : DateTime
        IsSystemAdmin        : bool
    }

type LoginStatusUpdate = 
    {
        UserId          : int
        AttemptNumber   : int
        IsLockedOut     : bool
        DateLockedOut   : DateTime option
    }
