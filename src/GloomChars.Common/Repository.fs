namespace GloomChars.Common

[<Struct>]
type ConnectionString = ConnectionString of string

type DatabaseConfig = 
    {
        ConnectionString : ConnectionString
    }

// At the moment, this code only detects unique constraint errors
// All other errors are exceptions that cannot be handled by code
type NonQueryResult<'T> =
    | Success of 'T
    | UniqueConstraintError of string

type SqlQuery =
    {
        Query : string
        Parameters : (string * obj) list
    }

type SqlMultiQuery =
    {
        Query : string
        Parameters : (string * obj) list array
    }

type IDbContext = 
    abstract member Query<'T> : SqlQuery -> 'T[]
    abstract member QueryMulti2<'T1, 'T2> : SqlQuery -> 'T1[] * 'T2[]
    abstract member QueryMulti3<'T1, 'T2, 'T3> : SqlQuery -> 'T1[] * 'T2[] * 'T3[]
    abstract member Execute : SqlQuery -> int
    abstract member TryExecute : SqlQuery -> NonQueryResult<int>
    abstract member ExecuteMulti : SqlMultiQuery -> int
    abstract member TryExecuteMulti : SqlMultiQuery -> NonQueryResult<int>
    abstract member ExecuteScalar<'T> : SqlQuery -> 'T
    abstract member TryExecuteScalar<'T> : SqlQuery -> NonQueryResult<'T>

module QueryUtils =

    let p name value = 
        // It may seem strange to cast to obj, but that is going to happen 
        // anyway when the sql statement is processed in Dapper
        ( name, value :> obj )

    let sql query parameters : SqlQuery =
        {
            Query = query
            Parameters = parameters
        }

    let sqlMulti query parameters : SqlMultiQuery =
        {
            Query = query
            Parameters = parameters
        }

[<RequireQualifiedAccess>]
module internal FsDapper = 

    open System
    open System.Data
    open Dapper

    type OptionHandler<'T>() =
        inherit SqlMapper.TypeHandler<option<'T>>()

        override __.SetValue(param, value) = 
            let valueOrNull = 
                match value with
                | Some x -> box x
                | None -> null

            param.Value <- valueOrNull    

        override __.Parse value =
            if isNull value || value = box DBNull.Value 
            then None
            else Some (value :?> 'T)

    let init() = 
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores <- true
        // Need to add handling for Option types
        SqlMapper.AddTypeHandler (OptionHandler<string>())
        SqlMapper.AddTypeHandler (OptionHandler<int>())
        SqlMapper.AddTypeHandler (OptionHandler<double>())
        SqlMapper.AddTypeHandler (OptionHandler<DateTime>())
        SqlMapper.AddTypeHandler (OptionHandler<bool>())

    let query<'T> (connection : IDbConnection) (q : SqlQuery) =
        connection.Query<'T>(q.Query, dict q.Parameters)
        |> Array.ofSeq

    let queryMulti2<'T1, 'T2> (connection : IDbConnection) (q : SqlQuery) = 
        use multi = connection.QueryMultiple(q.Query, dict q.Parameters)
        (
            multi.Read<'T1>() |> Array.ofSeq,
            multi.Read<'T2>() |> Array.ofSeq
        )

    let queryMulti3<'T1, 'T2, 'T3> (connection : IDbConnection) (q : SqlQuery) = 
        use multi = connection.QueryMultiple(q.Query, dict q.Parameters)
        (
            multi.Read<'T1>() |> Array.ofSeq,
            multi.Read<'T2>() |> Array.ofSeq,
            multi.Read<'T3>() |> Array.ofSeq
        )

    let execute (connection : IDbConnection) (q : SqlQuery) = 
        connection.Execute(q.Query, dict q.Parameters)

    let executeMulti (connection : IDbConnection) (q : SqlMultiQuery) = 
        let queryParams = Array.map (fun p -> dict p) q.Parameters
        connection.Execute(q.Query, queryParams)

    let executeScalar<'T> (connection : IDbConnection) (q : SqlQuery) = 
        connection.ExecuteScalar(q.Query, dict q.Parameters) :?> 'T


[<RequireQualifiedAccess>]
module PostgresDb = 

    open Npgsql

    let private isUniqueErr (ex : Npgsql.PostgresException) = 
        ex.Message.ToLower().Contains("unique constraint")

    // These are the Postgres wrappers around the Dapper calls
    let private callDb dapperFn (connectionString : ConnectionString) q = 
        let (ConnectionString connString) = connectionString
        use connection = new NpgsqlConnection(connString) 
        dapperFn connection q

    // These are the Postgres wrappers around the Dapper calls
    let private tryCallDb dapperFn (connectionString : ConnectionString) q = 
        try
            Success (callDb dapperFn connectionString q)
        with
            | :? Npgsql.PostgresException as ex when (isUniqueErr ex) -> 
                UniqueConstraintError ex.Message
            | _ -> 
                reraise()

    let init() = 
        FsDapper.init()

    let query<'T> (connectionString : ConnectionString) (q : SqlQuery) =
        callDb FsDapper.query<'T> connectionString q

    let queryMulti2<'T1, 'T2> (connectionString : ConnectionString) (q : SqlQuery) =
        callDb FsDapper.queryMulti2<'T1, 'T2> connectionString q

    let queryMulti3<'T1, 'T2, 'T3> (connectionString : ConnectionString) (q : SqlQuery) =
        callDb FsDapper.queryMulti3<'T1, 'T2, 'T3> connectionString q

    let execute (connectionString : ConnectionString) (q : SqlQuery) = 
        callDb FsDapper.execute connectionString q

    let tryExecute (connectionString : ConnectionString) (q : SqlQuery) = 
        tryCallDb FsDapper.execute connectionString q

    let executeMulti (connectionString : ConnectionString) (q : SqlMultiQuery) = 
        callDb FsDapper.executeMulti connectionString q

    let tryExecuteMulti (connectionString : ConnectionString) (q : SqlMultiQuery) = 
        tryCallDb FsDapper.executeMulti connectionString q

    let executeScalar<'T> (connectionString : ConnectionString) (q : SqlQuery) = 
        callDb FsDapper.executeScalar<'T> connectionString q

    let tryExecuteScalar<'T> (connectionString : ConnectionString) (q : SqlQuery) = 
        tryCallDb FsDapper.executeScalar<'T> connectionString q
        

