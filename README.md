# GloomHaven Api (F#)

A [Giraffe](https://github.com/giraffe-fsharp/Giraffe) web application, which is an API to persist characters for the GloomHaven board game.

## Tech Stack

- ASP.NET Core 2.1
- [Giraffe](https://github.com/giraffe-fsharp/Giraffe) : a native functional ASP.NET Core web framework.
- [PostgreSQL](https://www.postgresql.org/) : a relational database
- [Dapper](https://github.com/StackExchange/Dapper) : a micro-orm also used by StackOverflow 

### Architecture

The architecture is a typical layered app that attempts to separate concerns via layering as well as by code modularity. 

Web Layer -> Internal "Services" Layer -> Persistence Layer

## TODO: document the api and learn to write readme's better.