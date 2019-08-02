module OctoPrint
open System.Net.Http
open System.Runtime.Serialization.Json
open System.Text

let _client = new HttpClient ()
let mutable octoUri = ""

let _request msg =
    async {
        return! _client.SendAsync msg
                |> Async.AwaitTask
    } |> Async.RunSynchronously

let makeOctoUri sub =
    let builder = System.UriBuilder octoUri
    builder.Path <- sub
    builder.Uri

let _octoRequest method data sub =
    let msg = new HttpRequestMessage ()
    msg.RequestUri <- makeOctoUri sub
    msg.Method <- method
    data |> Option.map (fun d -> (msg.Content <- d)) |> ignore
    _request msg

let octoGet = _octoRequest HttpMethod.Get None
let octoPost data = _octoRequest HttpMethod.Post (Some(data))
let octoPut = _octoRequest HttpMethod.Put None


let serialize (serializer: DataContractJsonSerializer) obj =
    let stream = new System.IO.MemoryStream ()
    serializer.WriteObject(stream, obj)
    System.Text.Encoding.UTF8.GetString(stream.ToArray())

let unserialize (serializer: DataContractJsonSerializer) (str: string) =
    let stream = new System.IO.MemoryStream (Encoding.UTF8.GetBytes (str))
    serializer.ReadObject stream

let getWorkflowSupported () =
    let res = octoGet "/plugin/appkeys/probe"
    (int res.StatusCode) = 204

type KeyRequest = {
    app: string
    user: string
}

let keyRequestSerializer = serialize (DataContractJsonSerializer typeof<KeyRequest>)

let keyRequest appName userId =
    let req = new StringContent (keyRequestSerializer {app=appName; user=userId})
    octoPost req "/plugin/appkeys/request"

type AppKeyGrantedResp = {
    api_key: string
}
let appKeyUnserialize = unserialize (DataContractJsonSerializer typeof<AppKeyGrantedResp>)


type AppKeyDecision =
    | Granted of string
    | PollAgain
    | Failed

let pollDecision userId =
    let res = octoGet (sprintf "/plugin/appkeys/request/%s" userId)
    match int res.StatusCode with
    | 200 ->
        let appkey = appKeyUnserialize (res.Content.ToString()) :?> AppKeyGrantedResp
        Granted(appkey.api_key)
    | 202 -> PollAgain
    | _ -> Failed
