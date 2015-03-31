module Async
    let map f workflow =
        async {
            let! res = workflow
            return f res
        }
