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
            Password : string
        }

    type UserViewModel =
        {
            Id            : int
            Email         : string 
            DateCreated   : DateTime
            IsLockedOut   : bool
        }

    let toUserViewModel (user : User) = 
        {
            Id           = user.Id
            Email        = user.Email
            DateCreated  = user.DateCreated
            IsLockedOut  = match user.LockedOutStatus with | LockedOut _ -> true | _ -> false 
        }

    let toNewUser (user : AddUserRequest) : NewUser = 
        { 
            Email = user.Email
            Password = user.Password 
        }

    let validateNewUser (user : AddUserRequest) = 
        validateRequiredString (user.Email, "email") []
        |> validateRequiredString (user.Password, "password") 
        |> validateEmail user.Email 
        |> validatePassword user.Password
        |> function 
        | [] -> Ok user
        | errors -> Error (Msg (errorsToString errors))