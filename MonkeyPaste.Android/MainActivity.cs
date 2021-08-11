using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Essentials;
using FFImageLoading.Forms.Platform;
using Android.Support.V4.App;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using Android.Media;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Plugin.CurrentActivity;
using System.Net;
using Android.Widget;
using Java.Interop;
using Android.Views;
using Xamarin.Forms;
using Rg.Plugins.Popup.Services;

namespace MonkeyPaste.Droid {
    [Activity(
        Label = "MonkeyPaste",
        Icon = "@drawable/icon",
        Theme = "@style/MainTheme",
        MainLauncher = true, 
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
        public static MainActivity Current;

        static readonly int CB_NOTIFICATION_ID = 1000;
        static readonly string CB_CHANNEL_ID = "location_notification";
        internal static readonly string COUNT_KEY = "count";

        private Point _touchDownLoc;

        public MpINativeInterfaceWrapper AndroidInterfaceWrapper { get; set; }

        public event EventHandler GlobalTouchHandler;

        public override bool DispatchTouchEvent(MotionEvent ev) {
            var loc = new Point(ev.GetX(), ev.GetY());
            if (ev.Action == MotionEventActions.Down) {
                _touchDownLoc = loc;
            }else if (ev.Action == MotionEventActions.Up && loc.Distance(_touchDownLoc) < 10) {
                GlobalTouchHandler?.Invoke(null, new MpTouchEventArgs<Point>(loc));
            } 

            return base.DispatchTouchEvent(ev);
        }


        protected override void OnCreate(Bundle savedInstanceState) {
            Current = this;

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            if (Intent.Extras != null) {
                MpConsole.WriteLine(@"Main activity contains the following extras: ");
                foreach (var key in Intent.Extras.KeySet()) {
                    var value = Intent.Extras.GetString(key);
                    MpConsole.WriteLine("Key: {0} Value: {1}", key, value);
                }
            }

            Rg.Plugins.Popup.Popup.Init(this);
            UserDialogs.Init(this);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            ServicePointManager.ServerCertificateValidationCallback += (o, cert, chain, errors) => true;
           
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            CachedImageRenderer.Init(true);
            CachedImageRenderer.InitImageViewHandler();

            AndroidInterfaceWrapper = new MpAndroidInterfaceWrapper() {
                KeyboardService = new MpKeyboardInteractionService(),
                StorageService = new MpLocalStorage_Android(),
                TouchService = new MpGlobalTouch(),
                UiLocationFetcher = new MpUiLocationFetcher(),
                Screenshot = new MpScreenshot()
            };
            //MpNativeWrapper.Instance.Register<MpKeyboardInteractionService>();
            //MpNativeWrapper.Instance.Register<MpLocalStorage_Android>();
            //MpNativeWrapper.Instance.Register<MpGlobalTouch>();
            //MpNativeWrapper.Instance.Register<MpUiLocationFetcher>();
            //MpNativeWrapper.Instance.Register<MpScreenshot>();

            Task.Run(async () => {
                while(true) {
                    var ss =AndroidInterfaceWrapper.GetScreenshot().Capture(Window);
                    if(ss != null) {
                        string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);// @"/storage/emulated/0/Download/"
                        string path = System.IO.Path.Combine(folder, string.Format(@"screen.png"));
                        MpHelpers.Instance.WriteByteArrayToFile(path, ss,true);
                        var imgSrc = MpHelpers.Instance.ReadImageFromFile(path);

                        var ss64 = new MpImageConverter().Convert(imgSrc, typeof(string)) as string;
                    }
                    await Task.Delay(1000);
                }
            });
            LoadApplication(new App(AndroidInterfaceWrapper));
            //LoadSelectedTextAsync();


            //CreateNotificationChannel();

            //PublishNotification();

            //var intent = new Intent(Android.App.Application.Context, typeof(MyForegroundService));

            //// start foreground service.
            //if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O) {
            //    StartForegroundService(intent);
            //}
        }


        void CreateCbNotificationChannel() {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var name = Resources.GetString(Resource.String.channel_name);
            var description = GetString(Resource.String.channel_description);
            var channel = new NotificationChannel(CB_CHANNEL_ID, name, NotificationImportance.Default) {
                Description = description
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        void PublishCbNotification() {
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
            var builder = new NotificationCompat.Builder(this, CB_CHANNEL_ID)
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
            notificationManager.Notify(CB_NOTIFICATION_ID, builder.Build());
        }       

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
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