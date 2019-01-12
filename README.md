# GloomHaven Api (F#)

A [Giraffe](https://github.com/giraffe-fsharp/Giraffe) rest API used to to persist characters and associated modifier decks for the GloomHaven board game. This is only used by me and some friends so it has a simple authentication system. 

## Tech Stack

- ASP.NET Core 2.2
- [Giraffe](https://github.com/giraffe-fsharp/Giraffe) : a native functional ASP.NET Core web framework.
- [PostgreSQL](https://www.postgresql.org/) : a relational database
- [Dapper](https://github.com/StackExchange/Dapper) : a micro-orm also used by StackOverflow 

### Architecture

The architecture is a typical layered app that attempts to separate concerns via layering as well as by code modularity. 

Web Layer -> Internal "Services" Layer -> Persistence Layer

## Web Layer

Most of the code in the `GloomHaven.Api` project is utility/helper code. The app logic is mostly subfolders that contain the models and the controllers. The `Routing.fs` file contains the routes for the API. 

Authentication is a done via a simple bearer token. The token is stored in a server side cookie and is verified on every request. The number of requests is always going to be small enough that this never presents a scalability problem. 

The controllers are using some functional programming operators. At first this may look a little confusing but it is limited to the conventional operators and it removes boilerplate.

For example, a naive version of the controller might look like this:

```c#
let updateCharacter (ctx : HttpContext) (character : CharacterUpdateRequest) (characterId : int) = 
    let response = 
        match WebAuthentication.getLoggedInUserId ctx with
        | Error err -> Error err
        | Ok userId ->
            match validateCharacterUpdate character with
            | Error err -> Error err
            | Ok validCharacter ->
                let update = mapToCharacterUpdate characterId validCharacter userId 
                CharactersSvc.updateCharacter update  

    match response with
    | Error err -> toError "Failed to update character."
    | Ok _ -> toSuccessNoContent
```

This is terrible code with lots of boilerplate, but the good thing about F# is that most boilerplate can be removed. So simply including the `FSharpPlus` library allows it to be rewritten to this:

```c#
let updateCharacter (ctx : HttpContext) (character : CharacterUpdateRequest) (characterId : int) = 
    Ok (mapToCharacterUpdate characterId)
    <*> validateCharacterUpdate character
    <*> WebAuthentication.getLoggedInUserId ctx
    >>= CharactersSvc.updateCharacter 
    |> either toSuccessNoContent (toError "Failed to patch character.")
```

Which uses the spaceship operator `<*>` (appplicative functor) and the bind operator `>>=` to remove the boilerplate around the error handling.

Or it can be rewritten using a computation expression to this:

```c#
let updateCharacter (ctx : HttpContext) (character : CharacterUpdateRequest) (characterId : int) = 
    result {
        let! userId = WebAuthentication.getLoggedInUserId ctx
        let! validCharacter = validateCharacterUpdate character
        let update = toCharacterUpdate characterId validCharacter userId
        return! CharactersSvc.updateCharacter update
    }
    |> either toSuccessNoContent (toError "Failed to update character.")
```

This is probably a bit more idiomatic F#. It also is less scary to anyone not familiar with functional operators.

# Service Layer

This provides an internal api that is called by the UI layers (eg: web, console app, etc). It encapsulates all business logic and contains no web logic (or console app logic) so that it is reusable for different UI 'clients'. It has no state and all dependencies must passed into the functions. This makes the functions look a little clumsy but all the dependencies are explicit. This makes testing really easy. This is different to a typical C# app where dependencies are passed into the constructor which means that your methods get extra dependencies that they do not use. This makes testing more work... not necessarily harder, just more code and more work. So far, neither approach is a silver bullet in my opinion... you can also write F# in exactly the same style as C# and use constructor injection for dependencies and this ends up just as maintainable. 

# Repository

This uses Dapper to access a Postgres database. Postgres exclusively uses lowercase table and field names, so naming is not the same as .Net conventions. Dapper can handle this if you are using mutable classes (records) because it can instantiate the class and then use a case-sensitive comparison to set the properties. However using mutable records is not functional and Dapper lacks the ability to call constructors in a case-insensitive manner, so for this reason my SQL renames the database fields to camel case fields. This makes the SQL more verbose but the SQL is generally simple and easy to maintain.  