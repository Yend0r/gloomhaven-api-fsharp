namespace GloomChars.Api

module RequestHandlers = 
    open System
    open Giraffe
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.ContextInsensitive
    open Microsoft.Extensions.Configuration

    let get (ctrlrFun : HttpContext -> HttpHandler) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let config = ctx.GetService<IConfiguration>()
                let rsp = ctrlrFun ctx
                return! rsp next ctx
            }

    let getWithArgs (ctrlrFun : HttpContext -> 'T -> HttpHandler) (args : 'T) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let config = ctx.GetService<IConfiguration>()
                let rsp = ctrlrFun ctx args
                return! rsp next ctx
            }

    let post (ctrlrFun : HttpContext -> 'TJson -> HttpHandler) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! jsonContent = ctx.BindJsonAsync<'TJson>()
                let rsp = ctrlrFun ctx jsonContent 
                return! rsp next ctx
            }

    let postWithArgs (ctrlrFun : HttpContext -> 'TJson -> 'TArgs -> HttpHandler) (args : 'TArgs) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! jsonContent = ctx.BindJsonAsync<'TJson>()
                let rsp = ctrlrFun ctx jsonContent args 
                return! rsp next ctx
            }

    let delete (ctrlrFun : HttpContext -> HttpHandler) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let rsp = ctrlrFun ctx 
                return! rsp next ctx
            }

    let deleteWithArgs (ctrlrFun : HttpContext -> 'TArgs -> HttpHandler) (args : 'TArgs) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let rsp = ctrlrFun ctx args
                return! rsp next ctx
            }

    let getCi path routeHandler = routeCi path >=> (get routeHandler)

    let getCif path routeHandler = routeCif path  (getWithArgs routeHandler)

    let postCi path routeHandler = routeCi path >=> (post routeHandler)

    let postCif path routeHandler = routeCif path  (postWithArgs routeHandler)

    let deleteCi path routeHandler = routeCi path >=> (delete routeHandler)

    let deleteCif path routeHandler = routeCif path (deleteWithArgs routeHandler)