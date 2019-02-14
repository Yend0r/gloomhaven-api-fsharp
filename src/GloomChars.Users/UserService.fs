namespace GloomChars.Users

[<RequireQualifiedAccess>]
module UserService =  
    open FSharpPlus
    open GloomChars.Common

    let private validateInput (newUser : NewUser) = 
        let (PlainPassword plainPwd) = newUser.Password

        Validation.validateEmail newUser.Email []
        |> Validation.validatePassword plainPwd
        |> function
        | [] -> Ok newUser
        | errors -> Error (Validation.errorsToString errors)

    let private checkIfEmailExists dbGetUser (newUser : NewUser) = 
        match (dbGetUser newUser.Email) with
        | Some _ -> Error "A user account with that email already exists."
        | None -> Ok newUser

    let private hashPassword (newUser : NewUser) = 
        PasswordUtils.hashPassword newUser.Email newUser.Password

    let private toValidatedNewUser email name (passwordHash : HashedPassword) = 
        {
            Email = email
            Name = name
            PasswordHash = passwordHash
        }

    let add 
        (dbGetUser : string -> User option)
        (dbInsertNewUser : ValidatedNewUser -> Result<int, string>)
        (newUser : NewUser) = 

        validateInput newUser
        >>= checkIfEmailExists dbGetUser
        >>= hashPassword 
        |> map (toValidatedNewUser newUser.Email newUser.Name)
        >>= dbInsertNewUser 

    let list (dbGetUsers : unit -> User list) () : User list = 
        dbGetUsers()

    let get (dbGetUser : int -> User option) id : User option = 
        dbGetUser id

    let create db = 

        let dbGetUserByEmail = UserRepository.getUserByEmail db
        let dbGetUsers       = UserRepository.getUsers db
        let dbGetUser        = UserRepository.getUser db
        let dbInsertNewUser  = UserRepository.insertNewUser db

        { new IUserService with 
            member __.Get userId = 
                get dbGetUser userId

            member __.List () = 
                list dbGetUsers ()

            member __.Add newUser = 
                add dbGetUserByEmail dbInsertNewUser newUser
        }
