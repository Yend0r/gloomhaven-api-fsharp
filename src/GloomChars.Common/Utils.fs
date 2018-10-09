namespace GloomChars.Common

[<RequireQualifiedAccess>]
module Utils = 
    open Microsoft.FSharp.Reflection

    let unionToString (x : 'a) = 
        match FSharpValue.GetUnionFields(x, typeof<'a>) with
        | case, _ -> case.Name

    let unionFromString<'a> (s : string) =
        match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
        | [|case|] -> Some(FSharpValue.MakeUnion(case, [||]) :?> 'a)
        | _ -> None