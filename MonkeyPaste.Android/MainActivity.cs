using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Essentials;
using FFImageLoading.Forms.Platform;
using System.Reflection;
using Android.Support.V4.App;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using Android.Media;

namespace MonkeyPaste.Droid {
    [Activity(
        Label = "MonkeyPaste",
        Icon = "@drawable/icon",
        Theme = "@style/MainTheme",
        MainLauncher = false,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
        public static MainActivity Current;
        static readonly int NOTIFICATION_ID = 1000;
        static readonly string CHANNEL_ID = "location_notification";
        internal static readonly string COUNT_KEY = "count";
        int count = 0;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            AndroidEnvironment.UnhandledExceptionRaiser += delegate (object sender, RaiseThrowableEventArgs args)
            {
                typeof(System.Exception).GetField("stack_trace", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(args.Exception, null);
                throw args.Exception;
            };

            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                args.Handled = true;
            };

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            CachedImageRenderer.Init(true);
            CachedImageRenderer.InitImageViewHandler();

            Current = this;

            LoadApplication(new App());

            LoadSelectedTextAsync();

            //CreateNotificationChannel();

            //PublishNotification();

            //var intent = new Intent(Android.App.Application.Context, typeof(MyForegroundService));

            //// start foreground service.
            //if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O) {
            //    StartForegroundService(intent);
            //}
        }

        void CreateNotificationChannel() {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var name = Resources.GetString(Resource.String.channel_name);
            var description = GetString(Resource.String.channel_description);
            var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Default) {
                Description = description
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        void PublishNotification() {
            // Pass the current button press count value to the next activity:
            //var valuesForActivity = new Bundle();
            //valuesForActivity.PutInt(COUNT_KEY, count);

            // When the user clicks the notification, SecondActivity will start up.
            var resultIntent = new Intent(this, typeof(MpCopyClipboardNotificationActivity));
            //resultIntent.SetAction("COPY_CLIPBOARD");
            // Pass some values to SecondActivity:
            //resultIntent.PutExtras(valuesForActivity);

            // Construct a back stack for cross-task navigation:
            var stackBuilder = TaskStackBuilder.Create(this);
            stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MpCopyClipboardNotificationActivity)));
            stackBuilder.AddNextIntent(resultIntent);

            // Create the PendingIntent with the back stack:
            var resultPendingIntent = stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);

            // Build the notification:
            var builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                          .SetAutoCancel(false) // Dismiss the notification from the notification area when the user clicks on it
                          .SetContentIntent(resultPendingIntent) // Start up this activity when the user clicks the intent.
                          .SetContentTitle("Tap to store clipboard") // Set the title
                          //.SetNumber(count) // Display the count in the Content Info
                          .SetSmallIcon(Resources.GetIdentifier("icon", "drawable", PackageName)) // This is the icon to display
                          .SetContentText(string.Empty)
                          .SetDefaults((int)(NotificationDefaults.Sound | NotificationDefaults.Vibrate))
                          .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Alarm))
                          .SetColor(Resource.Color.colorPrimary)
                          .SetColorized(true)
                          .SetStyle(new NotificationCompat.DecoratedCustomViewStyle());

            //builder.AddAction(Resources.GetIdentifier("icon", "drawable", PackageName), "Copy Title", resultPendingIntent);

            // Finally, publish the notification:
            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(NOTIFICATION_ID, builder.Build());

            // Increment the button press count:
            count++;
        }

        private async void LoadSelectedTextAsync() {
            var selectedText = Intent!.GetStringExtra("SelectedText");// ?? string.Empty;
            var hostPackageName = Intent!.GetStringExtra("HostPackageName") ?? string.Empty;
            var hostAppName = Intent!.GetStringExtra("HostAppName") ?? string.Empty;
            var hostAppIcon = Intent!.GetByteArrayExtra("HostIconByteArray") ?? null;
            if (!string.IsNullOrWhiteSpace(selectedText)) {
                await Clipboard.SetTextAsync(selectedText);

                await MonkeyPaste.MpClip.Create(new object[] { hostPackageName, selectedText, hostAppName, hostAppIcon });
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //if (requestCode == 33)
            //{
            //    var importer = (MpPhotoImporter)MpResolver.Resolve<MpIPhotoImporter>();
            //    importer.ContinueWithPermission(true);// grantResults == null || grantResults.Length == 0 || (Permission)grantResults[0] == Permission.Granted);
            //}
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }
}