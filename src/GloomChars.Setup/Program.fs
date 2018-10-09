namespace GloomChars.Setup

open System
open GloomChars.Common

module App = 
    [<EntryPoint>]
    let main argv =

        printfn "Starting game data import..."

        PostgresDb.init()

        let connStr = ConnectionString "Server=localhost;Port=5432;User Id=postgres;Password=condign-nucleate-chuff-firmware;Database=gloomchars;"
        let dbContext = PostgresDbContext(connStr) 

        GameData.gloomClasses |> GameRepository.insertClasses dbContext

        printfn "Complete"
        0 // return an integer exit code
