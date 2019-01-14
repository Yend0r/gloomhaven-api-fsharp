namespace GloomChars.Api

module AuthenticationModels =
    open System
    open GloomChars.Users 
    open GloomChars.Common.Validation
    open ResponseHandlers

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
            AccessToken          = "Bearer " + token
            AccessTokenExpiresAt = user.AccessTokenExpiresAt
        }

    let toPasswordUpdate (changePasswordRequest : ChangePasswordRequest) accessToken : PasswordUpdate = 
       {
            AccessToken = accessToken
            OldPassword = OldPassword changePasswordRequest.OldPassword
            NewPassword = NewPassword changePasswordRequest.NewPassword
        }

    let validateChangePasswordRequest (changePasswordRequest : ChangePasswordRequest) = 
        validateRequiredString (changePasswordRequest.OldPassword, "oldPassword") []
        |> validateRequiredString (changePasswordRequest.NewPassword, "newPassword") 
        |> validatePassword changePasswordRequest.NewPassword
        |> toValidationResult changePasswordRequest
        |> Result.mapError Msg