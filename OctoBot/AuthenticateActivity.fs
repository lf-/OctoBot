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

[<Activity (Label = "AuthenticateActivity", Name = "ca.lfcode.OctoBot.AuthenticateActivity")>]
type AuthenticateActivity () =
  inherit AppCompatActivity()

  override this.OnCreate(bundle) =
    base.OnCreate (bundle)
    this.SetContentView (R.Layout.Authenticate)


