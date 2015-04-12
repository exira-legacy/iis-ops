namespace Exira.IIS.Domain

module Railway =
    type Error =
        | ErrorCollection of Error list
        | ConstructionError of string
        | UnknownDto of string
        | UnknownCommand of string
        | InvalidState of string
        | InvalidStateTransition of string

    type Result<'T> =
        | Success of 'T
        | Failure of Error

    let bind switchFunction = function
        | Success s -> switchFunction s
        | Failure f -> Failure f

    let bindAsync switchFunction = function
        | Success s -> switchFunction s
        | Failure f -> async { return Failure f }

    let constructionSuccess value =
        Success value

    let constructionError error =
        Failure (ConstructionError (error))

    let (>>=) input switchFunction = bind switchFunction input
    let (>>=!) input switchFunction = bindAsync switchFunction input