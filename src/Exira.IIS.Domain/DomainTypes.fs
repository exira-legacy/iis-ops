namespace Exira.IIS.Domain

module DomainTypes =
    open System.Text.RegularExpressions

    type GuidError =
    | Missing
    | MustNotBeEmpty

    type StringError =
    | Missing
    | DoesntMatchPattern of string

    type UriError =
    | Missing
    | Unknown

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
        open System

        type T = Hostname of string

        // create with continuation
        let createWithCont success failure value =
            match value, Uri.CheckHostName value with
            | null, _ -> failure UriError.Missing
            | _, UriHostNameType.Unknown
            | _, UriHostNameType.Basic -> failure UriError.Unknown
            | _ -> success (Hostname value)

        // create directly
        let create value =
            let success e = Some e
            let failure _  = None
            createWithCont success failure value

        // unwrap with continuation
        let apply f (Hostname e) = f e

        // unwrap directly
        let value e = apply id e