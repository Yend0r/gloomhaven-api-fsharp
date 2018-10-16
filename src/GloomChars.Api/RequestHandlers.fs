namespace GloomChars.Api

module RequestHandlers = 
    open System
    open Giraffe
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.ContextInsensitive
    open Microsoft.Extensions.Configuration

    let handle (ctrlrFun : HttpContext -> HttpHandler) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let rsp = ctrlrFun ctx
                return! rsp next ctx
            }

    let handleWithArgs (ctrlrFun : HttpContext -> 'T -> HttpHandler) (args : 'T) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let rsp = ctrlrFun ctx args
                return! rsp next ctx
            }

    let handleBody (ctrlrFun : HttpContext -> 'TJson -> HttpHandler) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! jsonContent = ctx.BindJsonAsync<'TJson>()
                let rsp = ctrlrFun ctx jsonContent 
                return! rsp next ctx
            }

    let handleBodyWithArgs (ctrlrFun : HttpContext -> 'TJson -> 'TArgs -> HttpHandler) (args : 'TArgs) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! jsonContent = ctx.BindJsonAsync<'TJson>()
                let rsp = ctrlrFun ctx jsonContent args 
                return! rsp next ctx
            }

    let getCi path routeHandler = routeCi path >=> (handle routeHandler)

    let getCif path routeHandler = routeCif path  (handleWithArgs routeHandler)

    let postCi path routeHandler = routeCi path >=> (handleBody routeHandler)

    let postCif path routeHandler = routeCif path  (handleBodyWithArgs routeHandler)

    let deleteCi path routeHandler = routeCi path >=> (handle routeHandler)

    let deleteCif path routeHandler = routeCif path (handleWithArgs routeHandler)

    let patchCif path routeHandler = routeCif path (handleBodyWithArgs routeHandler)