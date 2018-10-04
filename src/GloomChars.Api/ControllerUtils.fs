namespace GloomChars.Api

[<RequireQualifiedAccess>]
module ControllerUtils = 
    open System
    open Giraffe
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.ContextInsensitive
    open Microsoft.Extensions.Configuration

    let get (ctrlrFun : HttpHandler) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let config = ctx.GetService<IConfiguration>()
                let rsp = ctrlrFun
                return! rsp next ctx
            }

    let getWithArgs (ctrlrFun : 'T -> HttpHandler) (args : 'T) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let config = ctx.GetService<IConfiguration>()
                let rsp = ctrlrFun args
                return! rsp next ctx
            }

    let post (ctrlrFun : 'TJson -> HttpHandler) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! jsonContent = ctx.BindJsonAsync<'TJson>()
                let rsp = ctrlrFun jsonContent 
                return! rsp next ctx
            }

    let postWithArgs (ctrlrFun : 'TJson -> 'TArgs -> HttpHandler) (args : 'TArgs) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! jsonContent = ctx.BindJsonAsync<'TJson>()
                let rsp = ctrlrFun jsonContent args 
                return! rsp next ctx
            }

    let delete (ctrlrFun : 'TJson -> HttpHandler) : HttpHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! jsonContent = ctx.BindJsonAsync<'TJson>()
                let rsp = ctrlrFun jsonContent 
                return! rsp next ctx
            }

    let getCi path routeHandler = routeCi path >=> (get routeHandler)

    let getCif path routeHandler = routeCif path  (getWithArgs routeHandler)

    let postCi path routeHandler = routeCi path >=> (post routeHandler)

    let postCif path routeHandler = routeCif path  (postWithArgs routeHandler)

    let deleteCi path routeHandler = routeCi path >=> (delete routeHandler)