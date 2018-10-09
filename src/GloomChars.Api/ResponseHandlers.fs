namespace GloomChars.Api

module ResponseHandlers = 
    open Giraffe

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

    let wrapData data = { Data = data }

    let jsonList data = json (wrapData data)

    let badRequestError title detail = createApiError title detail

    let toMessage msg = { Message = msg }

    let SUCCESS msg = json (toMessage msg)

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

    let mapResultToJson result f errorMsg = 
        match result with 
        | Ok x -> json (f x)
        | Error error -> BAD_REQUEST errorMsg error

    let resultToJson errorMsg result = 
        match result with 
        | Ok x -> json x
        | Error error -> BAD_REQUEST errorMsg error

    let resultToSuccess successMsg errorMsg result = 
        match result with 
        | Ok x -> SUCCESS successMsg
        | Error error -> BAD_REQUEST errorMsg error
        
    let mapOptionToJson opt f errorMsg = 
        match opt with 
        | Some x -> json (f x)
        | None -> NOT_FOUND errorMsg 

    let optionToJson opt errorMsg = 
        match opt with 
        | Some x -> json x
        | None -> NOT_FOUND errorMsg 