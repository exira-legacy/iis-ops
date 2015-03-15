namespace Exira.IIS

module Application =
    open Exira.EventStore.EventStore
    open Exira.IIS.Contract.Commands

    let es = connect()

    let application controller command =
        command |> parseCommand |> ignore
        ()