module OctoPrint
open System.Text
open FSharp.Data
open Newtonsoft.Json

let mutable octoUri = ""

let makeOctoUri sub =
    let builder = System.UriBuilder octoUri
    builder.Path <- sub
    builder.Uri.AbsoluteUri

let _octoRequest method data path =
    Http.Request (
        url = (makeOctoUri path),
        ?body = data,
        headers = [ ("Accept", "application/json"); ("Content-Type", HttpContentTypes.Json) ],
        httpMethod = method
        )

let octoGet = _octoRequest HttpMethod.Get None
let octoPost req = _octoRequest HttpMethod.Post (Some req)

let getWorkflowSupported () =
    let res = octoGet "/plugin/appkeys/probe"
    (int res.StatusCode) = 204

type KeyRequest = { app: string; user: string }

type KeyRequestResp = JsonProvider<"""{ "app_token": "abcdef" }""">

/// <summary>Start authorization process</summary>
/// <param name="appName">Human readable identifier to use for the application requesting access</param>
/// <param name="userId">Username of the OctoPrint user this request is for</param>
/// <returns>AppToken to use in pollDecision</returns>
/// <seealso cref="pollDecision" />
let keyRequest appName userId =
    let req = JsonConvert.SerializeObject {app=appName; user=userId}
    let resp = octoPost (TextRequest req) "/plugin/appkeys/request"
    match resp.Body with
    | Text(t) -> Some (KeyRequestResp.Parse t).AppToken
    | Binary(_) -> None

type AppKeyGrantedResp = JsonProvider<"""{ "api_key": "apikey" }""">

type AppKeyDecision =
| Granted of string
| PollAgain
| Failed

/// <summary>Check if authorization has completed yet. Call every <5 seconds.</summary>
/// <param name="appToken">AppToken from <see cref="keyRequest">keyRequest</see></param>
let pollDecision appToken =
    let res = octoGet (sprintf "/plugin/appkeys/request/%s" appToken)
    match int res.StatusCode with
    | 200 ->
        match res.Body with
        | Text(t) ->
            let appkey = AppKeyGrantedResp.Parse t
            Granted(appkey.ApiKey)
        | Binary(_) -> Failed
    | 202 -> PollAgain
    | _ -> Failed


let rec waitForDecision appToken pollInterval =
    let pollInt = defaultArg pollInterval 2
    match pollDecision appToken with
    | PollAgain ->
        System.Threading.Thread.Sleep pollInt
        waitForDecision appToken pollInterval
    | Granted(s) -> Some s
    | Failed -> None


let testReqProcess () =
    keyRequest "app" "lf" |> Option.map waitForDecision
