namespace Exira.IIS

module WebStartup =
    open Exira.EventStore.EventStore

    let es = connect()