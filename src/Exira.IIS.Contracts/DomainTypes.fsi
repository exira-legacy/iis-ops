namespace Exira.IIS.Contracts

module DomainTypes =
    open System

    module ServerId =
        // encapsulated type
        type T

        // new valid type
        val newServerId : unit -> T

        // create with continuation
        val createWithCont: success: (T -> 'a) -> failure: (string -> 'a) -> value: Guid -> 'a

        // create directly
        val create: value: Guid -> T option

        // unwrap with continuation
        val apply: f: (Guid -> 'a) -> T -> 'a

        // unwrap directly
        val value: e: T -> Guid

    module Hostname =
        // encapsulated type
        type T

        // create with continuation
        val createWithCont: success: (T -> 'a) -> failure: (string -> 'a) -> value: string -> 'a

        // create directly
        val create: value: string -> T option

        // unwrap with continuation
        val apply: f: (string -> 'a) -> T -> 'a

        // unwrap directly
        val value: e: T -> string
