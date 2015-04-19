namespace Exira.IIS.Domain

module ErrorHandling =
    open Exira.Railway

    type Error =
        | ConstructionError of string * string
        | InvalidState of string
        | InvalidStateTransition of string

    let private constructionSuccess value =
        succeed value

    let private constructionError ``type`` error =
        fail [ConstructionError (``type``, error)]

    let construct ctor ``type`` value =
        value
        |> ctor constructionSuccess (constructionError ``type``)

    // TODO: Can get rid of this when we refactor applicative validation
    type ErrorStateBuilder() =
        member this.Return(x) = Success x
        member this.ReturnFrom(m) = m

        member this.Bind((m: Result<'a, Error list>), f) =
            match m with
            | Failure a' -> Failure a'
            | Success a' -> f a'

        member this.Bind((m: Result<'a, Error list> * Result<'b, Error list>), f) =
            match m with
            | Failure a', Success b' -> Failure a'
            | Success a', Failure b' -> Failure b'
            | Failure a', Failure b' -> Failure (a' @ b')
            | Success a', Success b' -> f (a', b')

        member this.Bind((m: Result<'a, Error list> * Result<'b, Error list> * Result<'c, Error list>), f) =
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