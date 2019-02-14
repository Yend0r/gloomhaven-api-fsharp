namespace GloomChars.Api

module CompositionRoot = 
    open GloomChars.Core
    open GloomChars.Common 
    open GloomChars.Users
    open ResponseHandlers

    let private config = Config.config
    let private db = PostgresDbContext(config.Database.ConnectionString)

    module AuthenticationSvc = 

        let private authConfig = config.Authentication
        let private authSvc = AuthenticationService.create authConfig db

        let authFailureToAppError authFailure = 
            match authFailure with
            | IsLockedOut msg -> Msg msg
            | _ -> Msg "Invalid email/password." // Always return same error... TODO: log other error

        let authenticate email password = 
            authSvc.Authenticate email password 
            |> Result.mapError authFailureToAppError 

        let revokeToken = authSvc.RevokeToken
        let changePassword = authSvc.ChangePassword >> toAppResult
        let getAuthenticatedUser = authSvc.GetAuthenticatedUser >> toAppResult

    module UsersSvc = 

        let private userSvc = UserService.create db

        let add = userSvc.Add >> toAppResult

        let list () = userSvc.List ()

        let get userId = 
            userSvc.Get userId
            |> optionToAppResultOrNotFound

    module GameDataSvc = 

        let gloomClasses = GameData.gloomClasses

        let getGlClass glClassName = 
            GameData.gloomClass glClassName 

        let getGloomClass className = 
            GameData.getGloomClass className 
            |> optionToAppResultOrNotFound

    module CharactersSvc = 

        let private charSvc = CharactersService.create db

        let get characterId userId = 
            charSvc.Get characterId userId
            |> optionToAppResultOrNotFound

        let list = charSvc.List 

        let add = charSvc.Add >> toAppResult

        let update = charSvc.Update >> toAppResult

        let delete characterId userId = 
            charSvc.Delete characterId userId
            |> toAppResult

    module ScenarioSvc = 

        let private deckSvc = DeckService.create db
        let private scenarioSvc = ScenarioService.create db deckSvc
        
        let newScenario character name = 
            scenarioSvc.NewScenario character name
            |> toAppResult

        let completeScenario = scenarioSvc.Complete >> toAppResult

        let getScenario = scenarioSvc.Get  >> toAppResult

        let updateStats character statsUpdate = 
            scenarioSvc.UpdateStats character statsUpdate
            |> toAppResult

        let drawCard = scenarioSvc.DrawCard >> toAppResult

        let reshuffle = scenarioSvc.Reshuffle >> toAppResult

