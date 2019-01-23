namespace GloomChars.Api

module AdminModels = 
    open System
    open GloomChars.Users
    open GloomChars.Common.Validation
    open ResponseHandlers

    [<CLIMutable>]
    type AddUserRequest =
        {
            Email    : string
            Name     : string
            Password : string
        }

    type UserViewModel =
        {
            Id            : int
            Email         : string 
            Name          : string
            DateCreated   : DateTime
            IsLockedOut   : bool
        }

    let toUserViewModel (user : User) = 
        {
            Id           = user.Id
            Email        = user.Email
            Name         = user.Name
            DateCreated  = user.DateCreated
            IsLockedOut  = match user.LockedOutStatus with | LockedOut _ -> true | _ -> false 
        }

    let toNewUser (user : AddUserRequest) : NewUser = 
        { 
            Email = user.Email
            Name = user.Name
            Password = PlainPassword user.Password 
        }

    let validateNewUser (user : AddUserRequest) = 
        validateRequiredString (user.Email, "email") []
        |> validateRequiredString (user.Password, "password") 
        |> validateRequiredString (user.Name, "name") 
        |> validateEmail user.Email 
        |> validatePassword user.Password
        |> toValidationResult user
        |> Result.mapError Msg