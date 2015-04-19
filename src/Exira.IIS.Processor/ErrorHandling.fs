namespace Exira.IIS.Processor

module ErrorHandling =
    type Error =
        | UnknownEvent of string
        | DeserializeProblem of string
