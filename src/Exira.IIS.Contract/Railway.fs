﻿namespace Exira.IIS.Contract

module Railway =
    type Error =
        | UnknownDto of string

    type Result<'T> =
        | Success of 'T
        | Failure of Error

    let bind switchFunction = function
        | Success s -> switchFunction s
        | Failure f -> Failure f

    let (>>=) input switchFunction = bind switchFunction input