namespace GloomChars.Api

module ResponseHandlers = 
    open Giraffe

    type AppError = 
        | Msg of string
        | NotFound
        | Unauthorized of string

    type ApiError =
        {
            Message : string
            Detail  : string
        }
        
    let createApiError message detail = 
        { 
            Message = message
            Detail = detail
        }

    type SuccessMessage =
        {
            Message : string
        }

    type DataWrapper<'T> = 
        {
            Data : 'T
        }

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

    let UNAUTHORIZED detail = 
        setStatusCode 401 
        >=> json (createApiError "Access Denied" detail)

    let FORBIDDEN detail = 
        setStatusCode 403 
        >=> json (createApiError "Forbidden" detail)

    let NOT_FOUND detail = 
        setStatusCode 404 
        >=> json (createApiError "Resource Not Found" detail)

    let INTERNAL_ERROR title detail = 
        setStatusCode 500 
        >=> json (createApiError title detail)

    //--------

    let toSuccess value = json value

    let toSuccessList value = json { Data = value }

    let toSuccessNoContent _ = SUCCESS_204

    let toCreated location = SUCCESS_201 location

    let toError errorMsg appError = 
        match appError with
        | Msg err -> BAD_REQUEST errorMsg err
        | NotFound -> NOT_FOUND errorMsg 
        | Unauthorized err -> UNAUTHORIZED err 

