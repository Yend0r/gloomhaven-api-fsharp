namespace GloomChars.Authentication

open GloomChars.Common

[<RequireQualifiedAccess>]
module RegistrationService =  

    let private validateInput
        (newUser : NewUser) = 

        let errors = 
            Validation.validateEmail newUser.Email []
            |> Validation.validatePassword newUser.Password
        match errors with
        | [] -> Ok newUser
        | _ -> Error (Validation.errorsToString errors)

    let private checkIfEmailExists dbGetUser (newUser : NewUser) = 

        match (dbGetUser newUser.Email) with
        | Some _ -> Error "A user account with that email already exists."
        | None -> Ok newUser

    let private hashPassword (newUser : NewUser) = 
        PasswordHasher.hashPassword(newUser.Email, newUser.Password)

    let register db 
        (dbGetUser : string -> User option)
        (dbInsertNewUser : string -> string -> Result<int, string>)
        (newUser : NewUser) = 

        validateInput newUser
        |> Result.bind (checkIfEmailExists dbGetUser)
        |> Result.bind hashPassword 
        |> Result.bind (dbInsertNewUser newUser.Email) 