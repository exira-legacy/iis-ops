namespace Exira

module Railway =
    type Result<'TSuccess, 'TFailure> =
        | Success of 'TSuccess
        | Failure of 'TFailure

    // convert a single value into a two-track result
    let succeed x =
        Success x

    // convert a single value into a two-track result
    let fail x =
        Failure x

    let failAsync x =
        async { return Failure x }

    // apply either a success function or failure function
    let either successFunc failureFunc twoTrackInput =
        match twoTrackInput with
        | Success s -> successFunc s
        | Failure f -> failureFunc f

    // convert a switch function into a two-track function
    let bind f = either f fail

    // convert a one-track function into a switch
    let switch f = f >> succeed

    // convert a one-track function into a two-track function
    let map f = either (f >> succeed) fail

    // convert a dead-end function into a one-track function
    let tee f x = f x; x

    // convert a switch function into a two-track function
    let bindAsync f = either f failAsync