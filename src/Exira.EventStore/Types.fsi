namespace Exira.EventStore

open System.Net

module ServerPort =
    // encapsulated type
    type T

    // create with continuation
    val createWithCont: success: (T -> 'a) -> failure: (string -> 'a) -> value: int -> 'a

    // create directly
    val create: value: int -> T option

    // unwrap with continuation
    val apply: f: (int -> 'a) -> T -> 'a

    // unwrap directly
    val value: e: T -> int

type Configuration = {
    Address: IPAddress
    Port: ServerPort.T
    Username: string
    Password: string
}

type StreamId = StreamId of string