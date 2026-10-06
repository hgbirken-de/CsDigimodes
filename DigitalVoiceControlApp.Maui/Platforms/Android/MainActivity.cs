using Android.App;
using Android.Content.PM;
using Android.OS;
using DigitalVoiceControlApp.Maui.Services;

namespace DigitalVoiceControlApp.Maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        // OnPause kommt sofort, wenn die App den Vordergrund verliert (Home, Übersicht/Viereck, Bildschirm aus,
        // anderer Task). Hier wird gemeldet, damit der DMR-Client unverzüglich gestoppt werden kann.
        protected override void OnPause()
        {
            AppLifecycle.RaisePausing();
            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            AppLifecycle.RaiseResumed();
        }
    }
}
