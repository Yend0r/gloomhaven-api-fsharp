namespace GloomChars.Api

module AuthenticationModels =
    open System
    open GloomChars.Users 

    [<CLIMutable>]
    type LoginRequest =
        {
            Email    : string
            Password : string
        }

    [<CLIMutable>]
    type ChangePasswordRequest =
        {
            OldPassword : string
            NewPassword : string
        }

    type LoginResponse =
        {
            Email                : string
            AccessToken          : string
            AccessTokenExpiresAt : DateTime
        }

    let toLoginResponse (user : AuthenticatedUser) : LoginResponse = 
        let (AccessToken token) = user.AccessToken

        {
            Email                = user.Email
            AccessToken          = token
            AccessTokenExpiresAt = user.AccessTokenExpiresAt
        }