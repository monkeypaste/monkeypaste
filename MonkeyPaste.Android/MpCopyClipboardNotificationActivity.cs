using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Widget;
using Xamarin.Essentials;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using MonkeyPaste.Plugin;

namespace MonkeyPaste.Droid {
    [Service]
    [Obsolete]
    public class MpCopyClipboardNotificationService : IntentService {
        protected override void OnHandleIntent(Intent intent) {
            Task.Run(async () => {
                var cbText = await Clipboard.GetTextAsync();
                MpConsole.WriteLine($"Clipboard Text: {cbText}");
            });
        }
    }

    [Activity(
        Label = "CopyClipboardNotificationActivity")]
    public class MpCopyClipboardNotificationActivity : Activity {
        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var cbText = await Clipboard.GetTextAsync();
                MpConsole.WriteLine($"Clipboard Text: {cbText}");

                Finish();
            });
            
            //Task.Run(async () => {
            //    var cbText = await Clipboard.GetTextAsync();
            //    MpConsole.WriteLine($"Clipboard Text: {cbText}");

            //    Finish();
            //});

        }
    }
    [Service]
    public class MyForegroundService : Service {
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId) {
            CreateNotificationChannel();
            string messageBody = "service starting";


            Clipboard.ClipboardContentChanged += Clipboard_ClipboardContentChanged;

            // / Create an Intent for the activity you want to start
            Intent resultIntent = new Intent(this, typeof(MpCopyClipboardNotificationActivity));
            // Create the TaskStackBuilder and add the intent, which inflates the back stack
            var stackBuilder = TaskStackBuilder.Create(this);
            stackBuilder.AddNextIntentWithParentStack(resultIntent);
            // Get the PendingIntent containing the entire back stack
            PendingIntent resultPendingIntent = stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);
            var notification = new Notification.Builder(this, "10111")
             .SetContentIntent(resultPendingIntent)
             .SetContentTitle("Foreground")
             .SetContentText(messageBody)
             .SetSmallIcon(Resources.GetIdentifier("icon", "drawable", PackageName))
             .SetOngoing(true)
             .Build();
            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, notification);
            //do you work
            return StartCommandResult.Sticky;
        }

        private async void Clipboard_ClipboardContentChanged(object sender, EventArgs e) {
            //throw new NotImplementedException();

            var text = await Clipboard.GetTextAsync();
            Toast.MakeText(this, text, ToastLength.Long).Show();
            if (text.Contains("@")) {
                await Clipboard.SetTextAsync(text.Replace("@", ""));
            }
        }

        public override void OnDestroy() {
            base.OnDestroy();
            Clipboard.ClipboardContentChanged -= Clipboard_ClipboardContentChanged;

            StopForeground(true);
        }
        public override IBinder OnBind(Intent intent) {
            return null;
        }

        void CreateNotificationChannel() {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) {

                return;
            }

            var channelName = Resources.GetString(Resource.String.channel_name);
            var channelDescription = GetString(Resource.String.channel_description);
            var channel = new NotificationChannel("10111", channelName, NotificationImportance.Default) {
                Description = channelDescription
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

    }
}