﻿namespace Exira.IIS.Domain

module DomainTypes =
    open System
    open Exira.Railway
    open ErrorHandling

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

    val constructServerId: (Guid -> Result<ServerId.T, Error list>)
    val constructHostname: (string -> Result<Hostname.T, Error list>)