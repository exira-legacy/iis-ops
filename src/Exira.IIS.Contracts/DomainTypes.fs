namespace Exira.IIS.Contracts

module DomainTypes =

    module ServerId =
        open System

        type T = ServerId of Guid

        let newServerId() = ServerId (Guid.NewGuid())

        // create with continuation
        let createWithCont success failure value =
            if value <> Guid.Empty
                then success (ServerId value)
                else failure "ServerId cannot be an empty Guid"

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
        open System.Text.RegularExpressions

        type T = Hostname of string

        let validHostnameRegex = "^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$"

        // create with continuation
        let createWithCont success failure value =
            if Regex.IsMatch(value, validHostnameRegex)
                then success (Hostname value)
                else failure "Invalid hostname according to RFC 1123"

        // create directly
        let create value =
            let success e = Some e
            let failure _  = None
            createWithCont success failure value

        // unwrap with continuation
        let apply f (Hostname e) = f e

        // unwrap directly
        let value e = apply id e