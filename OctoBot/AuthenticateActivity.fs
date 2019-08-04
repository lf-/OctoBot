namespace OctoBot

open System
open System.Collections.Generic
open System.Linq
open System.Text

open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Views
open Android.Widget
open Android.Support.V7.App
open Android.Support.Design.Widget
open System.Text


module AuthMachinery =
    let validateUri (uri: IEnumerable<char>) =
        // if you cause the URI parser to fall over in any way, your input is wrong
        try
            Uri (System.String.Concat uri) |> ignore
            Ok ()
        with
        | e -> Error (e.ToString ())
        

[<Activity (Label = "AuthenticateActivity", Name = "ca.lfcode.OctoBot.AuthenticateActivity")>]
type AuthenticateActivity () =
  inherit AppCompatActivity()

  override this.OnCreate(bundle) =
    base.OnCreate (bundle)
    this.SetContentView (R.Layout.Authenticate)
    let uriLayout = this.FindViewById<TextInputLayout> (R.Id.inputLayoutOctoprintHost)
    let uriBox = this.FindViewById<TextInputEditText> (R.Id.editTextOctoprintHost)
    uriBox.TextChanged.Add (fun arg ->
        uriLayout.Error <- match AuthMachinery.validateUri arg.Text with
                           | Ok(_) -> ""
                           | Error(_) -> "schema://host:port"
    )

    let authBtn = this.FindViewById<Button> (R.Id.buttonAuthenticate)
    let allowView = this.FindViewById<View> (R.Id.allowView)
    let validate () =
        let res = uriBox.Text |> AuthMachinery.validateUri
        match res with
        | Ok(_) ->
            allowView.Visibility <- ViewStates.Visible
            // return! 
        | Error(e) -> e
            
    authBtn.Click.Add (fun _ ->
        OctoPrint.octoUri <- uriBox.Text
    )


