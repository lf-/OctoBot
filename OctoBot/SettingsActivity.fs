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

[<Activity (Label = "SettingsActivity")>]
type SettingsActivity () =
  inherit AppCompatActivity ()

  override this.OnOptionsItemSelected (item) =
    match item.ItemId with
    | Android.Resource.Id.Home ->
        this.OnBackPressed ()
        true
    | _ -> base.OnOptionsItemSelected item

  override this.OnCreate(bundle) =
    base.OnCreate (bundle)
    this.SetContentView (R.Layout.Settings)
    this.SetSupportActionBar(this.FindViewById(R.Id.top_toolbar).JavaCast())
    this.SupportActionBar.SetDisplayHomeAsUpEnabled true
    this.SupportActionBar.SetDisplayShowHomeEnabled true
    this.FragmentManager.BeginTransaction().Replace(R.Id.settingsContentPanel, (new SettingsFragment())).Commit() |> ignore


