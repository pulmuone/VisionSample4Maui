using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;

namespace VisionSample4Maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        static string[] PERMISSIONS = {
            Manifest.Permission.Camera
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);


            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS, 0);
            }

        }
    }
}
