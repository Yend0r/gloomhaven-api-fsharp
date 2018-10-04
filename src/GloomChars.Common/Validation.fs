namespace GloomChars.Common

module Validation = 
    open System
    open System.Net.Mail

    type ValidationError = 
        {
            Property : string
            Message : string
        }

    let private lengthDiff (p1: string) (p2: string) = 
        p1.Length - p2.Length

    // Try to catch common weak passwords
    // I'm dubious that this is a good idea... there too many other ways to make weak passwords
    let private isCommonPassword (password : string) = 

        let pwd = password.ToLower()

        let commonPasswords = 
            [
                "123456";
                "password";
                "qwerty";
                "admin";
                "football";
                "letmein";
                "iloveyou";
                "monkey";
                "welcome";
                "login";
                "abc123";
                "starwars";
                "123123";
                "qazwsx";
                "trustno1"
            ]

        // Weak if password equals a weak one or it starts with a common one and is less than 5 chars longer
        // Eg: password2018
        commonPasswords 
        |> List.tryFind (fun p ->   
            (pwd = p || (pwd.StartsWith(p) && (lengthDiff pwd p) <= 4)))

    let checkPasswordRules (password : string) validationErrors : ValidationError list = 
        if password.Length < 6 then
            { Property = "Password"; Message = "Password must be greater than 6 characters." } :: validationErrors
        elif (isCommonPassword password).IsSome then
            { Property = "Password"; Message = "Password is too weak." } :: validationErrors
        else 
            validationErrors

    let validateRequiredString (str : string, propName : string) validationErrors : ValidationError list =
        match String.IsNullOrWhiteSpace(str) with
        | true -> { Property = propName; Message = (sprintf "%s is required." propName) } :: validationErrors 
        | false -> validationErrors

    let validatePassword (password : string) validationErrors : ValidationError list = 
        validateRequiredString (password, "Password") validationErrors
        |> checkPasswordRules password
             
    let validateEmail email validationErrors : ValidationError list =  
        try
            let _mailTest = new MailAddress(email)
            validationErrors
        with _ -> { Property = "Email"; Message = (sprintf "Invalid email address: %s" email) } :: validationErrors

    let errorsToString (validationErrors : ValidationError list) = 
        List.map (fun e -> e.Message) validationErrors |> (String.concat " ")