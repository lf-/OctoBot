namespace OctoBot

open System

open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Views
open Android.Widget
open Android.Support.V7.App

type Resources = OctoBot.Resource

[<Activity (Label = "OctoBot", MainLauncher = true, Icon = "@mipmap/icon")>]
type MainActivity () =
    inherit AppCompatActivity ()

    let mutable count:int = 1

    override this.OnCreateOptionsMenu (menu) =
        let inflater = this.MenuInflater
        inflater.Inflate(R.Menu.mainMenu, menu)
        true

    override this.OnOptionsItemSelected (item) =
        match item.ItemId with
        | R.Id.action_settings ->
            let act = typeof<SettingsActivity>
            this.StartActivity(act)
            ()
        | _ -> ()
        true

    override this.OnCreate (bundle) =

        base.OnCreate (bundle)

        // Set our view from the "main" layout resource
        this.SetContentView (Resources.Layout.Main)
        this.SetSupportActionBar(this.FindViewById(R.Id.top_toolbar).JavaCast())

        // Get our button from the layout resource, and attach an event to it
        let button = this.FindViewById<Button>(Resources.Id.myButton)
        button.Click.Add (fun args -> 
            button.Text <- sprintf "%d clicks!" count
            count <- count + 1
        )

