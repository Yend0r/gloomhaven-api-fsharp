namespace GloomChars.Common

open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.Serialization
open Microsoft.FSharp.Reflection
open System
open System.IO

[<RequireQualifiedAccess>]
module JsonUtils =
    type OptionConverter() =
        inherit JsonConverter()
        
        override _x.CanConvert(t) = 
            t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>
     
        override _x.WriteJson(writer, value, serializer) =
            let value = 
                if isNull value then 
                    null
                else 
                    let _,fields = FSharpValue.GetUnionFields(value, value.GetType())
                    fields.[0]  
            serializer.Serialize(writer, value)
     
        override _x.ReadJson(reader, t, _existingValue, serializer) =        
            let innerType = t.GetGenericArguments().[0]
            let innerType = 
                if innerType.IsValueType then 
                    typedefof<Nullable<_>>.MakeGenericType([|innerType|])
                else 
                    innerType        
            let value = serializer.Deserialize(reader, innerType)
            let cases = FSharpType.GetUnionCases(t)
            if isNull value then 
                FSharpValue.MakeUnion(cases.[0], [||])
            else 
                FSharpValue.MakeUnion(cases.[1], [|value|])

    let private jsonConverters : JsonConverter[] = [| OptionConverter() |]

    let jsonSerializerSettings = 
        let mutable serializerSettings = JsonSerializerSettings();
        serializerSettings.Converters.Add(OptionConverter())
        serializerSettings.NullValueHandling <- NullValueHandling.Ignore
        serializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver() 
        serializerSettings

    let deserialize<'T> (json : string) =
        JsonConvert.DeserializeObject<'T>(json, converters = jsonConverters)

    let deserializeOption<'T> (json : Option<string>) : Option<'T> =
        match json with
        | Some s -> Some (JsonConvert.DeserializeObject<'T>(s, converters = jsonConverters))
        | None -> None

    let tryDeserialize<'T> (json : string) : Result<'T, exn> =
        try
            JsonConvert.DeserializeObject(json, typeof<'T>, converters = jsonConverters) :?> 'T |> Ok
        with e -> e |> Error

    let tryDeserializeFile<'T> (filepath : string) : Result<'T, exn> =
        try
            JsonConvert.DeserializeObject(File.ReadAllText(filepath), typeof<'T>, converters = jsonConverters) :?> 'T |> Ok
        with e -> e |> Error

    let trySerialize<'T> (data : 'T) : Result<string, exn> =
        try
            JsonConvert.SerializeObject(data, jsonSerializerSettings) |> Ok
        with e -> e |> Error

    let serializeOption<'T> (data : Option<'T>) : Option<string> =
        match data with
        | Some obj -> Some (JsonConvert.SerializeObject(obj, jsonSerializerSettings))
        | None -> None

    let serialize<'T> (data : 'T) =
        JsonConvert.SerializeObject(data, jsonSerializerSettings)

    let tryParseFile (filepath : string) : Result<JObject, exn> =
        try
            JObject.Parse(File.ReadAllText(filepath)) |> Ok
        with e -> e |> Error
    
    let tryMerge (jobj1 : JObject) (jobj2 : JObject) = 
        try
            let mutable mergeSettings = JsonMergeSettings();
            mergeSettings.MergeArrayHandling <- MergeArrayHandling.Union
            jobj1.Merge(jobj2, mergeSettings)
            Ok jobj1
        with e -> e |> Error

    let tryDeserializeJObject<'T> (jobj : JObject) : Result<'T, exn> =
        try
            let serializer = JsonSerializer.Create(jsonSerializerSettings)
            jobj.ToObject(typeof<'T>, serializer) :?> 'T |> Ok
        with e -> e |> Error