namespace GloomChars.Api

module DefaultView =
    open Giraffe.GiraffeViewEngine

    let layout (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText "src" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] content
        ]

    let partial () =
        h1 [] [ encodedText "GloomChars API" ]

    let index () =
        [
            partial()
            p [] [ encodedText "TODO: documentation" ]
        ] |> layout

module DefaultController = 
    open Giraffe
    open FSharp.Control.Tasks.ContextInsensitive

    let indexHandler () =
        DefaultView.index()
        |> htmlView 

