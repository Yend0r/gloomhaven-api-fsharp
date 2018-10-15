namespace GloomChars.Api

module ResponseHandlers = 
    open Giraffe

    type AppError = 
        | Msg of string
        | NotFound
        | Unauthorized of string

    type ApiError =
        {
            Title  : string
            Detail : string
        }

    type ApiErrors = 
        {
            Errors : ApiError list
        }

    let createApiError title detail = 
        { 
            Errors = 
            [ 
                {
                    Title  = title
                    Detail = detail
                }
            ]
        }

    type SuccessMessage =
        {
            Message : string
        }

    type DataWrapper<'T> = 
        {
            Data : 'T
        }

    let optionToAppResult opt = 
        match opt with
        | Some c -> Ok c
        | None -> Error NotFound

    let toAppResult (result : Result<'a,string>) : Result<'a,AppError> = 
        result 
        |> Result.mapError Msg

    let wrapData data = { Data = data }

    let jsonList data = json (wrapData data)

    let badRequestError title detail = createApiError title detail

    let toMessage msg = { Message = msg }

    let SUCCESS msg = json (toMessage msg)

    let SUCCESS_201 location = 
        setHttpHeader "Location" location
        >=> setStatusCode 201

    let SUCCESS_204 = 
        Successful.NO_CONTENT

    let BAD_REQUEST title detail = 
        setStatusCode 400 
        >=> json (createApiError title detail)

    let badRequest (apiErros : ApiErrors) = 
        setStatusCode 400 
        >=> json apiErros

    let UNAUTHORIZED detail = 
        setStatusCode 401 
        >=> json (createApiError "Access Denied" detail)

    let NOT_FOUND detail = 
        setStatusCode 404 
        >=> json (createApiError "Resource Not Found" detail)

    let INTERNAL_ERROR title detail = 
        setStatusCode 500 
        >=> json (createApiError title detail)

    let private appErrorToResponse errorMsg appError = 
        match appError with
        | Msg err -> BAD_REQUEST errorMsg err
        | NotFound -> NOT_FOUND errorMsg 
        | Unauthorized err -> UNAUTHORIZED err 

    let resultToJson errorMsg result = 
        match result with 
        | Ok x -> json x
        | Error error -> error |> appErrorToResponse errorMsg

    let resultToJsonList result = 
        match result with 
        | Ok x -> jsonList x
        | Error error -> error |> appErrorToResponse ""

    let resultToSuccessNoContent errorMsg result = 
        match result with 
        | Ok location -> SUCCESS_204 
        | Error error -> error |> appErrorToResponse errorMsg

    let resultToResourceLocation errorMsg result = 
        match result with 
        | Ok location -> SUCCESS_201 location
        | Error error -> error |> appErrorToResponse errorMsg
