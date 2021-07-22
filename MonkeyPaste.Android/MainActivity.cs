using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Util;
using Xamarin.Essentials;
using FFImageLoading.Forms.Platform;
using System.Reflection;
using Android.Support.V4.App;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using Android.Media;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.Graphics;
using static Android.Graphics.Paint;
using static Android.Provider.Settings;
using static Java.Util.Jar.Attributes;
using Xamarin.Forms;
using Plugin.CurrentActivity;
using System.Net;
using Java.Security;
using Android.Widget;
using Android.Gms.Common;
using Firebase.Iid;
using Firebase;
using Firebase.Messaging;
using Firebase.Installations;
using Java.Interop;
using Android.Gms.Extensions;
using Android.Gms.Tasks;

namespace MonkeyPaste.Droid {
    [Activity(
        Label = "MonkeyPaste",
        Icon = "@drawable/icon",
        Theme = "@style/MainTheme",
        MainLauncher = false,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
        public static MainActivity Current;

        static readonly int CB_NOTIFICATION_ID = 1000;
        static readonly string CB_CHANNEL_ID = "location_notification";
        internal static readonly string COUNT_KEY = "count";


        static readonly string TAG = "MainActivity";
        // google play services
        internal static readonly string GPS_CHANNEL_ID = "my_notification_channel";
        internal static readonly int GPS_NOTIFICATION_ID = 100;

        TextView msgText;
        int count = 0;
                
        public MpINativeInterfaceWrapper AndroidInterfaceWrapper { get; set; }

        protected override async void OnCreate(Bundle savedInstanceState) {
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

            

            UserDialogs.Init(this);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            ServicePointManager.ServerCertificateValidationCallback += (o, cert, chain, errors) => true;
           
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            

            CachedImageRenderer.Init(true);
            CachedImageRenderer.InitImageViewHandler();
            Current = this;

            AndroidInterfaceWrapper = new MpAndroidInterfaceWrapper() {
                KeyboardService = new MpKeyboardInteractionService(),
                StorageService = new MpLocalStorage_Android()
            };


            //if (IsPlayServicesAvailable()) {
            //    CreateGpsNotificationChannel();
            //    FirebaseApp.InitializeApp(ApplicationContext);
            //    MpConsole.WriteLine("FCM InstanceID token: " + FirebaseInstanceId.Instance.Token);
            //} else {
            //    MpConsole.WriteTraceLine(@"Error w/ Google play services");
            //}

            // Required for push notifications so check if Play services are available on the device or navigate to store to install them
            CreateGpsNotificationChannel();

            if (!IsPlayServicesAvailable()) {
                GoogleApiAvailability.Instance.MakeGooglePlayServicesAvailable(this);
            }
            //FirebaseApp.InitializeApp(this.Application.ApplicationContext);
            //var firebaseListener = new FirebaseListener();
            //FirebaseMessaging.Instance.GetToken().AddOnCompleteListener(firebaseListener);
            //var token = await firebaseListener.GetToken();

            //Task.Run(async () => {
            //    await FirebaseInstallations.Instance.GetToken(true).AddOnCompleteListener(firebaseListener);
            //    var token = await firebaseListener.GetToken();

            //    //await FirebaseMessaging.Instance.GetToken().AddOnCompleteListener(firebaseListener);
            //    //var token = await firebaseListener.GetToken();
            //    Log.Debug(TAG, $"FCM push token:'{token}'");
            //});

            LoadApplication(new App(AndroidInterfaceWrapper));
            LoadSelectedTextAsync();


            //CreateNotificationChannel();

            //PublishNotification();

            //var intent = new Intent(Android.App.Application.Context, typeof(MyForegroundService));

            //// start foreground service.
            //if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O) {
            //    StartForegroundService(intent);
            //}
        }
        
        public bool IsPlayServicesAvailable() {
            string logStr = string.Empty;
            bool isAvailable = false;
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success) {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode)) {
                    logStr = GoogleApiAvailability.Instance.GetErrorString(resultCode);
                    isAvailable = true;
                } else {
                    isAvailable = false;
                    logStr = "This device is not supported";
                    Finish();
                }
            } else {
                logStr = "Google Play Services is available.";
                isAvailable = true;
            }
            MpConsole.WriteLine(logStr);
            return isAvailable;
        }

        void CreateGpsNotificationChannel() {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel(GPS_CHANNEL_ID,
                                                  "FCM Notifications",
                                                  NotificationImportance.Default) {

                Description = "Firebase Cloud Messages appear in this channel"
            };

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
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

            // Increment the button press count:
            count++;
        }

        private async void LoadSelectedTextAsync() {
            var selectedText = Intent!.GetStringExtra("SelectedText");// ?? string.Empty;
            var hostPackageName = Intent!.GetStringExtra("HostPackageName") ?? string.Empty;
            var hostAppName = Intent!.GetStringExtra("HostAppName") ?? string.Empty;
            var hostAppIcon = Intent!.GetByteArrayExtra("HostIconByteArray") ?? null;
            var hostAppIconBase64 = Intent!.GetStringExtra("HostIconBase64") ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(selectedText)) {
                await Clipboard.SetTextAsync(selectedText);

                await MonkeyPaste.MpCopyItem.Create(new object[] { hostPackageName, selectedText, hostAppName, hostAppIcon, hostAppIconBase64 });
            }
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

    class TaskCompleteListener : Java.Lang.Object, IOnCompleteListener {
        private readonly TaskCompletionSource<Java.Lang.Object> taskCompletionSource;

        public TaskCompleteListener(TaskCompletionSource<Java.Lang.Object> tcs) {
            this.taskCompletionSource = tcs;
        }

        public void OnComplete(Android.Gms.Tasks.Task task) {
            if (task.IsCanceled) {
                this.taskCompletionSource.SetCanceled();
            } else if (task.IsSuccessful) {
                this.taskCompletionSource.SetResult(task.Result);
            } else {
                this.taskCompletionSource.SetException(task.Exception);
            }
        }
    }

    public class FirebaseListener : Java.Lang.Object, Android.Gms.Tasks.IOnCompleteListener {
        private TaskCompletionSource<String> tcs = new TaskCompletionSource<String>(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<String> GetToken() {
            return tcs.Task;
        }

        public void Disposed() {
        }

        public void DisposeUnlessReferenced() {
        }

        public void Finalized() {
        }

        public void OnComplete(Android.Gms.Tasks.Task task) {
            if (task.IsSuccessful) {
                string theToken = task.Result.ToString();
                tcs.SetResult(theToken);
            } else {
                tcs.SetResult(null);
            }
        }

        public void SetJniIdentityHashCode(int value) {
        }

        public void SetJniManagedPeerState(JniManagedPeerStates value) {
        }

        public void SetPeerReference(JniObjectReference reference) {
        }
    }

}