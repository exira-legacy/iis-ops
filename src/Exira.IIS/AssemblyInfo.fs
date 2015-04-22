namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.IIS")>]
[<assembly: AssemblyProductAttribute("Exira.IIS")>]
[<assembly: AssemblyDescriptionAttribute("Manage IIS through a REST API.")>]
[<assembly: AssemblyVersionAttribute("0.0.3")>]
[<assembly: AssemblyFileVersionAttribute("0.0.3")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.3"
