using Android.App;
using Android.Content.PM;
using Android.OS;

namespace DCCPanelController;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState) {
        RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
        base.OnCreate(savedInstanceState);
    }
}
