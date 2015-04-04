namespace Exira.Jil

open System.IO
open System.Text
open System.Net.Http.Formatting
open System.Net.Http.Headers
open System.Threading.Tasks
open ExtCore
open Jil

type JilMediaTypeFormatter(options) =
    inherit MediaTypeFormatter()

    let applicationJson = MediaTypeHeaderValue("application/json")
    let textJson = MediaTypeHeaderValue("text/json")
    let utf8 = UTF8Encoding(encoderShouldEmitUTF8Identifier = false, throwOnInvalidBytes = true)
    let unicode = UnicodeEncoding(bigEndian = false, byteOrderMark = true, throwOnInvalidBytes = true)

    // https://github.com/dotnet/coreclr/blob/cbf46fb0b6a0b209ed1caf4a680910b383e68cba/src/mscorlib/src/System/IO/StreamWriter.cs#L43
    // https://github.com/dotnet/coreclr/blob/cbf46fb0b6a0b209ed1caf4a680910b383e68cba/src/mscorlib/src/System/IO/StreamReader.cs#L54
    let defaultBufferSize = 1024

    do
        base.SupportedMediaTypes.Add applicationJson
        base.SupportedMediaTypes.Add textJson
        base.SupportedEncodings.Add utf8
        base.SupportedEncodings.Add unicode

    new() = JilMediaTypeFormatter(Options.Default)

    override this.CanReadType ``type`` =
        checkNonNull "type" ``type``
        true

    override this.CanWriteType ``type`` =
        checkNonNull "type" ``type``
        true

    override this.ReadFromStreamAsync(``type``, stream, _, _) =
        async {
            use reader =
                new StreamReader(
                    stream = stream,
                    encoding = utf8,
                    detectEncodingFromByteOrderMarks = true,
                    bufferSize = defaultBufferSize,
                    leaveOpen = true)

            let methodInfo = typedefof<JSON>.GetMethod("Deserialize", [| typedefof<TextReader>; typedefof<Options> |])
            let generic = methodInfo.MakeGenericMethod ``type``
            return generic.Invoke(this, [| reader; options |])
        } |> Async.StartAsTask

    override this.WriteToStreamAsync(_, value, stream, _, _) =
        async {
            use streamWriter =
                new StreamWriter(
                    stream = stream,
                    encoding = utf8,
                    bufferSize = defaultBufferSize,
                    leaveOpen = true)

            JSON.Serialize(value, streamWriter, options)
            streamWriter.Flush()
        } |> Async.StartAsTask :> Task