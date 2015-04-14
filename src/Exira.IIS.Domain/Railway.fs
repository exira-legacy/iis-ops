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

    let combine f = function
        | Success left, Success right -> f left right
        | Failure e, Success _
        | Success _, Failure e -> Failure e
        | Failure left, Failure right ->
            match left, right with
            | ErrorCollection e1, ErrorCollection e2 -> Failure <| ErrorCollection(e1 @ e2)
            | ErrorCollection e1, _ -> Failure <| ErrorCollection(e1 @ [right])
            | _, ErrorCollection e2 -> Failure <| ErrorCollection(e2 @ [left])
            | _, _ -> Failure <| ErrorCollection [left; right]