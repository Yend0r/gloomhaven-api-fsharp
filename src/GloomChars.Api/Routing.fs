namespace GloomChars.Api
open Giraffe
open RequestHandlers
open WebAuthentication

module AuthenticationRoutes =
    open AuthenticationController 

    let router : HttpHandler =  
        choose [
            POST >=>
                choose [
                    postCi "/authentication/login" login
                    //Must be authenticated to change your password
                    requiresAuthenticatedUser >=> postCi "/authentication/password" changePassword 
                ]

            //Must be authenticated to logout
            DELETE >=> requiresAuthenticatedUser >=> deleteCi "/authentication/logout" logout 
        ]

module CharactersRoutes = 
    open CharacterReadController 
    open CharacterEditController 
    open DeckController 

    let router : HttpHandler =  
        choose [
            GET >=>
                choose [
                    getCif "/characters/%i" getCharacter 
                    getCi "/characters" listCharacters 
                    getCif "/characters/%i/decks" getDeck 
                ]
            POST >=>
                choose [
                    postCif "/characters/%i" updateCharacter 
                    postCi "/characters" addCharacter
                    postCif "/characters/%i/decks" newDeck 
                ]
            PATCH >=> requiresAuthenticatedUser >=> patchCif "/characters/%i" patchCharacter 
            DELETE >=> requiresAuthenticatedUser >=> deleteCif "/characters/%i" deleteCharacter 
        ]

module GameDataRoutes = 
    open GameDataController 

    let router : HttpHandler =  
        choose [
            GET >=>
                choose [
                    getCif "/game/classes/%s" getClass 
                    getCi "/game/classes" listClasses 
                ]
        ]

module AdminRoutes = 
    open AdminController 

    let router : HttpHandler =  
        choose [
            GET >=>
                choose [
                    getCi "/admin/users" listUsers 
                ]
            POST >=>
                choose [
                    postCi "/admin/users" addUser 
                ]
        ]

module Routing = 

    // ---------------------------------
    // Main router
    // ---------------------------------

    let router : HttpHandler =
        choose [
            //Public access
            routeStartsWithCi "/authentication" >=> AuthenticationRoutes.router

            //Must be logged in
            routeStartsWithCi "/game" >=> requiresAuthenticatedUser >=> GameDataRoutes.router 
            routeStartsWithCi "/characters" >=> requiresAuthenticatedUser >=> CharactersRoutes.router 

            // System admin methods... requires a user with 'SystemAdmin' role
            routeStartsWithCi "/admin" >=> requiresSystemAdmin >=> AdminRoutes.router 
                
            GET >=>
                choose [
                    route "/" >=> DefaultController.indexHandler()
                ]
            setStatusCode 404 >=> text "Not Found" 
        ]