namespace GloomChars.Common

[<RequireQualifiedAccess>]
module Parse =
    open System
    open System.Globalization

    let private dateFormat = "yyyy-MM-dd"
    let private iso8601Format = "yyyy-MM-ddTHH:mm:ss"
    let private auDateTimeFormat = "d/M/yyyy h:mm tt"
    let private dateTimeCulture = new CultureInfo("en-AU")

    let private parseUsing<'T> (parser : string -> bool * 'T) (s : string) : Option<'T> =
        match parser s with
        | true, i -> Some i
        | false, _ -> None // (sprintf "Cound not parse '%s' to %s" s typeof<'T>.Name)

    let private parseListUsing<'T> (parser : string -> bool * 'T) (strList : string list) : Option<'T list> = 
        let result = List.fold (fun newList str -> 
            match newList with 
            | Some xList -> 
                match parser str with
                | true, i -> Some(i :: xList)
                | false, _ -> None
            | None -> None) (Some(List.empty)) strList

        match result with 
        | Some t -> Some(List.rev t)
        | None -> None

    let int32 = parseUsing Int32.TryParse
    let uint32 = parseUsing UInt32.TryParse
    let int64 = parseUsing Int64.TryParse
    let uint64 = parseUsing UInt64.TryParse
    let uri = parseUsing (fun s -> Uri.TryCreate(s, UriKind.RelativeOrAbsolute))
    let dateTime = parseUsing (fun s -> DateTime.TryParseExact(s, iso8601Format, dateTimeCulture, DateTimeStyles.None))
    let auDateTime = parseUsing (fun s -> DateTime.TryParseExact(s, auDateTimeFormat, dateTimeCulture, DateTimeStyles.None))
    let date = parseUsing (fun s -> DateTime.TryParseExact(s, dateFormat, dateTimeCulture, DateTimeStyles.None))
    let decimal = parseUsing (fun s -> Decimal.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture))

    let int32List = parseListUsing Int32.TryParse
    let uint32List = parseListUsing UInt32.TryParse
    let int64List = parseListUsing Int64.TryParse
    let uint64List = parseListUsing UInt64.TryParse
    let uriList = parseListUsing (fun s -> Uri.TryCreate(s, UriKind.RelativeOrAbsolute))
    let dateTimeList = parseListUsing (fun s -> DateTime.TryParseExact(s, iso8601Format, dateTimeCulture, DateTimeStyles.None))
    let auDateTimeList = parseListUsing (fun s -> DateTime.TryParseExact(s, auDateTimeFormat, dateTimeCulture, DateTimeStyles.None))
    let dateList = parseListUsing (fun s -> DateTime.TryParseExact(s, dateFormat, dateTimeCulture, DateTimeStyles.None))
    let decimalList = parseListUsing (fun s -> Decimal.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture))

