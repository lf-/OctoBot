
namespace OctoBot

open System
open System.Collections.Generic
open System.Linq
open System.Text

open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Util
open Android.Views
open Android.Widget
open Android.Preferences


type SettingsFragment () =
  inherit PreferenceFragment ()

  override this.OnCreate (savedInstanceState) =
    base.OnCreate (savedInstanceState)
    base.AddPreferencesFromResource(R.Xml.preferences)
    //let pref = base.FindPreference("pref_octoprintHost")
    

