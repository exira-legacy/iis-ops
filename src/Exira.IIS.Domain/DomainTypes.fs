namespace Exira.IIS.Domain

module DomainTypes =
    open System.Text.RegularExpressions

    type GuidError =
    | Missing
    | MustNotBeEmpty

    type StringError =
    | Missing
    | DoesntMatchPattern of string

    let private (|Match|_|) pattern input =
        let m = Regex.Match(input, pattern) in
        if m.Success then Some (List.tail [ for g in m.Groups -> g.Value ]) else None

    module ServerId =
        open System

        type T = ServerId of Guid

        let newServerId() = ServerId (Guid.NewGuid())

        // create with continuation
        let createWithCont success failure value =
            if value <> Guid.Empty
                then success (ServerId value)
                else failure MustNotBeEmpty // "ServerId cannot be an empty Guid"

        // create directly
        let create value =
            let success e = Some e
            let failure _  = None
            createWithCont success failure value

        // unwrap with continuation
        let apply f (ServerId e) = f e

        // unwrap directly
        let value e = apply id e

    module Hostname =
        type T = Hostname of string

        let validHostnameRegex = "^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$"

        // create with continuation
        let createWithCont success failure value =
            match value with
            | null -> failure StringError.Missing
            | Match validHostnameRegex _ -> success (Hostname value)
            | _ -> failure (DoesntMatchPattern validHostnameRegex)  //"Invalid hostname according to RFC 1123"

        // create directly
        let create value =
            let success e = Some e
            let failure _  = None
            createWithCont success failure value

        // unwrap with continuation
        let apply f (Hostname e) = f e

        // unwrap directly
        let value e = apply id e