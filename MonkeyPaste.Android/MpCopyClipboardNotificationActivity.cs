using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Widget;
using Xamarin.Essentials;

namespace MonkeyPaste.Droid {
    [Activity(Label = "CopyClipboardNotificationActivity")]
    public class MpCopyClipboardNotificationActivity : Activity {
        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            // Get the count value passed to us from MainActivity:
            var count = Intent.Extras.GetInt(MainActivity.COUNT_KEY, -1);

            // No count was passed? Then just return.
            if (count <= 0) {
                return;
            }

            // Display the count sent from the first activity:

            //SetContentView(Resource.Layout.Second);
            //var txtView = FindViewById<TextView>(Resource.Id.textView1);
            //txtView.Text = $"You clicked the button {count} times in the previous activity.";

            MpConsole.WriteLine($"Count: {count}");

            Task.Run(async () => {
                var cbText = await Clipboard.GetTextAsync();
                MpConsole.WriteLine($"Clipboard Text: {cbText}");
            });
            
        }
    }
}