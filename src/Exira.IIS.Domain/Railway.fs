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

    // map the messages to a different error type
    let mapMessages f result =
        match result with
        | Success x -> succeed x
        | Failure errors ->
            let errors' = List.map f errors
            fail errors'

    // TODO: Can get rid of this when we refactor applicative validation
    type ErrorStateBuilder() =
        member this.Return(x) = Success x
        member this.ReturnFrom(m) = m

        member this.Bind((m: Result<'a, 'error list>), f) =
            match m with
            | Failure a' -> Failure a'
            | Success a' -> f a'

        member this.Bind((m: Result<'a, 'error list> * Result<'b, 'error  list>), f) =
            match m with
            | Failure a', Success b' -> Failure a'
            | Success a', Failure b' -> Failure b'
            | Failure a', Failure b' -> Failure (a' @ b')
            | Success a', Success b' -> f (a', b')

        member this.Bind((m: Result<'a, 'error list> * Result<'b, 'error list> * Result<'c, 'error list>), f) =
            match m with
            | Failure a', Failure b', Failure c' -> Failure (a' @ b' @ c')
            | Failure a', Failure b', Success c' -> Failure (a' @ b')
            | Failure a', Success b', Failure c' -> Failure (a' @ c')
            | Failure a', Success b', Success c' -> Failure a'
            | Success a', Failure b', Failure c' -> Failure (b' @ c')
            | Success a', Failure b', Success c' -> Failure b'
            | Success a', Success b', Failure c' -> Failure c'
            | Success a', Success b', Success c' -> f (a', b', c')

        member this.Delay(f) = f()

    let errorState = new ErrorStateBuilder()