namespace Exira.EventStore

open System.Net

module ServerPort =
    type T = ServerPort of int

    // create with continuation
    let createWithCont success failure value =
        if value < 1 || value > 65535
            then failure "ServerPort must be between 1 and 65535."
            else success (ServerPort value)

    // create directly
    let create value =
        let success e = Some e
        let failure _  = None
        createWithCont success failure value

    // unwrap with continuation
    let apply f (ServerPort e) = f e

    // unwrap directly
    let value e = apply id e

// TODO: How do I wrap this to make sure you can only create a valid configuration?
type Configuration = {
    Address: IPAddress
    Port: ServerPort.T
    Username: string
    Password: string
}

// TODO: Not sure if there is a max streamid length?
type StreamId = StreamId of string