namespace GloomChars.Users

[<RequireQualifiedAccess>]
module UserService =  
    open FSharpPlus
    open GloomChars.Common

    let private validateInput (newUser : NewUser) = 
        Validation.validateEmail newUser.Email []
        |> Validation.validatePassword newUser.Password
        |> function
        | [] -> Ok newUser
        | errors -> Error (Validation.errorsToString errors)

    let private checkIfEmailExists dbGetUser (newUser : NewUser) = 
        match (dbGetUser newUser.Email) with
        | Some _ -> Error "A user account with that email already exists."
        | None -> Ok newUser
    
    let private hashPassword (newUser : NewUser) = 
        PasswordUtils.hashPassword newUser.Email newUser.Password

    let addUser 
        (dbGetUser : string -> User option)
        (dbInsertNewUser : string -> string -> Result<int, string>)
        (newUser : NewUser) = 

        validateInput newUser
        >>= checkIfEmailExists dbGetUser
        >>= hashPassword 
        >>= dbInsertNewUser newUser.Email

    let getUsers (dbGetUsers : unit -> User list) () : User list = 
        dbGetUsers()

    let getUser (dbGetUser : int -> User option) id : User option = 
        dbGetUser id
  