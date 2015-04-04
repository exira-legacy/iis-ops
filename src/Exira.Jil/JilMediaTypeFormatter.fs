namespace Exira.Jil

open System
open System.IO
open System.Text
open System.Net.Http.Formatting
open System.Net.Http.Headers
open System.Threading.Tasks
open ExtCore
open Jil

type JilMediaTypeFormatter() =
    inherit MediaTypeFormatter()

    let jilOptions = Options(prettyPrint = true, dateFormat = DateTimeFormat.ISO8601)
    let applicationJson = MediaTypeHeaderValue("application/json")
    let textJson = MediaTypeHeaderValue("text/json")
    let utf8 = UTF8Encoding(encoderShouldEmitUTF8Identifier = false, throwOnInvalidBytes = true)
    let unicode = UnicodeEncoding(bigEndian = false, byteOrderMark = true, throwOnInvalidBytes = true)

    do
        base.SupportedMediaTypes.Add applicationJson
        base.SupportedMediaTypes.Add textJson
        base.SupportedEncodings.Add utf8
        base.SupportedEncodings.Add unicode

    override this.CanReadType ``type`` =
        checkNonNull "type" ``type``
        true

    override this.CanWriteType ``type`` =
        checkNonNull "type" ``type``
        true

    member private this.DeserializeTypeFromStream (``type``: Type) readStream =
        use reader = new StreamReader(stream = readStream)
        let methodInfo = typedefof<JSON>.GetMethod("Deserialize", [| typedefof<TextReader>; typedefof<Options> |])
        let generic = methodInfo.MakeGenericMethod ``type``
        generic.Invoke(this, [| reader; jilOptions |])

    override this.ReadFromStreamAsync(``type``, inputStream, _, _) =
        Task.Factory.StartNew (fun () ->
            this.DeserializeTypeFromStream ``type`` inputStream)

    override this.WriteToStreamAsync(_, value, outputStream, _, _) =
        Task.Factory.StartNew (fun () ->
            let streamWriter = new StreamWriter(stream = outputStream)
            JSON.Serialize(value, streamWriter, jilOptions)
            streamWriter.Flush())

//    override this.WriteToStreamAsync(_, value, outputStream, _, _) =
//        Task.Factory.StartNew (fun () ->
//            use streamWriter = new StreamWriter(stream = outputStream)
//            JSON.Serialize(value, streamWriter, jilOptions)
//            streamWriter.Flush())

//    override this.WriteToStreamAsync(_, value, outputStream, _, _) =
//        async {
//            use streamWriter = new StreamWriter(stream = outputStream)
//            JSON.Serialize(value, streamWriter, jilOptions)
//            streamWriter.Flush()
//        } |> Async.StartAsTask :> Task

//    override this.WriteToStreamAsync(_, value, outputStream, _, _) =
//        Task.Factory.StartNew (fun () ->
//            let streamWriter = new StreamWriter(stream = outputStream)
//            try
//                JSON.Serialize(value, streamWriter, jilOptions)
//                streamWriter.Flush()
//            finally
//                streamWriter.Dispose())