# GloomHaven Api (F#)

A [Giraffe](https://github.com/giraffe-fsharp/Giraffe) web application, which is an API to persist characters for the GloomHaven board game.

## Tech Stack

- ASP.NET Core 2.2
- [Giraffe](https://github.com/giraffe-fsharp/Giraffe) : a native functional ASP.NET Core web framework.
- [PostgreSQL](https://www.postgresql.org/) : a relational database
- [Dapper](https://github.com/StackExchange/Dapper) : a micro-orm also used by StackOverflow 

### Architecture

The architecture is a typical layered app that attempts to separate concerns via layering as well as by code modularity. 

Web Layer -> Internal "Services" Layer -> Persistence Layer

## Web Layer

Most of the code in the `GloomHaven.Api` project is utility/helper code. The app logic is mostly contained in the `Routing.fs` file and the subfolders that contain the models and the controllers. 

Authentication is a done via a simple bearer token. The token is stored in a server side cookie and is verified on every request. The number of requests is always going to be small enough that this never presents a scalability problem. 

The controllers are using lots of functional programming operators. At first this may look a little confusing but it is limited to the conventional operators and it removes boilerplate.

For example: 



# Service Layer

This provides an internal api that is called by the UI layers (eg: web, console app, etc). It encapsulates all business logic and contains zero web logic (or console app logic) so that it is reusable for different UI 'clients'. It has no state and all dependencies must passed into the functions. This makes the functions look a little clumsy but all the dependencies are explicit. This makes testing really easy. This is different to a typical C# app where dependencies are passed into the constructor which means that your methods get extra dependencies that they do not use. This makes testing more work... not necessarily harder, just more code and more work. So far, neither approach is a silver bullet in my opinion... you can also write F# in exactly the same style as C# and use constructor injection for dependencies and this ends up just as maintainable. 

# Repository

This uses Dapper to access a Postgres database. Postgres exclusively uses lowercase table and field names, so naming is not the same as .Net conventions. Dapper can handle this if you are using mutable classes (records) because it can instantiate the class and then use a case-sensitive comparison to set the properties. However using mutable records is not functional and Dapper lacks the ability to call constructors in a case-insensitive manner, so for this reason my SQL renames the database fields to camel case fields. This makes the SQL more verbose but the SQL is generally simple and easy to maintain.  