module OctoPrint
open System.Text
open FSharp.Data
open Newtonsoft.Json
open Util

// XXX: mutable global. And the functions will throw exceptions if it's not set.
// this is an anti-functional variable
let mutable octoUri = ""

let makeOctoUri sub =
    let builder = System.UriBuilder octoUri
    builder.Path <- sub
    builder.Uri.AbsoluteUri

let _octoRequest method data path =
    Http.AsyncRequest (
        url = (makeOctoUri path),
        ?body = data,
        headers = [ ("Accept", "application/json"); ("Content-Type", HttpContentTypes.Json) ],
        httpMethod = method
        )

let octoGet = _octoRequest HttpMethod.Get None
let octoPost req = _octoRequest HttpMethod.Post (Some req)

let getWorkflowSupported () =
    async {
        let! res = octoGet "/plugin/appkeys/probe"
        return (int res.StatusCode) = 204
    }

type KeyRequest = { app: string; user: string }

type KeyRequestResp = JsonProvider<"""{ "app_token": "abcdef" }""">

/// <summary>Start authorization process</summary>
/// <param name="appName">Human readable identifier to use for the application requesting access</param>
/// <param name="userId">Username of the OctoPrint user this request is for</param>
/// <returns>AppToken to use in pollDecision</returns>
/// <seealso cref="pollDecision" />
let keyRequest appName userId =
    async {
        let req = JsonConvert.SerializeObject {app=appName; user=userId}
        let! resp = octoPost (TextRequest req) "/plugin/appkeys/request"
        match resp.Body with
        | Text(t) -> return Some (KeyRequestResp.Parse t).AppToken
        | Binary(_) -> return None
    }

type AppKeyGrantedResp = JsonProvider<"""{ "api_key": "apikey" }""">

type AppKeyDecision =
| Granted of string
| PollAgain
| Failed

/// <summary>Check if authorization has completed yet. Call every <5 seconds.</summary>
/// <param name="appToken">AppToken from <see cref="keyRequest">keyRequest</see></param>
let pollDecision appToken =
    async {
        let! res = octoGet (sprintf "/plugin/appkeys/request/%s" appToken)
        match int res.StatusCode with
        | 200 ->
            match res.Body with
            | Text(t) ->
                let appkey = AppKeyGrantedResp.Parse t
                return Granted(appkey.ApiKey)
            | Binary(_) -> return Failed
        | 202 -> return PollAgain
        | _ -> return Failed
    }


let rec waitForDecision appToken =
    async {
        let! decision = pollDecision appToken
        match decision with
        | PollAgain ->
                do! (Async.Sleep 2)
                return! (waitForDecision appToken)
        | Granted(s) -> return Some s
        | Failed -> return None
    }


let requestApiKey appName userId =
    // XXX: there isn't anything wrong with this code but we could make it less readable
    //      using functional constructs
    async {
        match! keyRequest appName userId with
        | None -> return None
        | Some tok -> return! waitForDecision tok
    }
