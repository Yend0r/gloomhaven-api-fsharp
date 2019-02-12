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

    type CreatedResult<'a> = 
        {
            Uri : string
            Item : 'a
        }

    let optionToAppResultOrNotFound opt = 
        match opt with
        | Some c -> Ok c
        | None -> Error NotFound

    let optionToAppResultOrBadRequest msg opt = 
        match opt with
        | Some c -> Ok c
        | None -> Error (Msg msg)

    let toAppResult (result : Result<'a,string>) : Result<'a,AppError> = 
        result 
        |> Result.mapError Msg

    let toMessage msg = { Message = msg }

    let SUCCESS msg = json (toMessage msg)

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

    let CREATED uri obj = 
        setHttpHeader "Location" uri
        >=> Successful.CREATED obj

    //--------

    let toSuccess value = json value

    let toSuccessList value = json { Data = value }

    //204
    let toSuccessNoContent _ = 
        Successful.NO_CONTENT 

    //201
    let toCreated createdResult : HttpHandler = 
        setHttpHeader "Location" createdResult.Uri 
        >=> Successful.CREATED createdResult.Item

    let toCreatedX uri : obj -> HttpHandler = 
        fun item -> 
            setHttpHeader "Location" uri            
            >=> Successful.CREATED item


    let toError errorMsg appError = 
        match appError with
        | Msg err -> BAD_REQUEST errorMsg err
        | NotFound -> NOT_FOUND errorMsg 
        | Unauthorized err -> UNAUTHORIZED err 

