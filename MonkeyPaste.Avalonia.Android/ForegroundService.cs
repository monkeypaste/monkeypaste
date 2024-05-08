using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
namespace MonkeyPaste.Avalonia.Android {
    [Service]
    public class ForegroundService : Service {
        #region Private Variables
        private MyBroadcastReceiver _broadcastReceiver;
        private string foregroundChannelId = "9001";
        private string READ_CLIPBOARD_ACTION = "read clipboard";
        private Context context = global::Android.App.Application.Context;
        #endregion

        #region Constants
        const int ServiceRunningNotifID = 9000;
        #endregion

        #region Statics
        private static ForegroundService _instance;
        public static ForegroundService Instance => _instance ?? (_instance = new ForegroundService());
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        public bool IsRunning { get; private set; }
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public void Start() {
            var intent = new Intent(context, typeof(ForegroundService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O) {
                context.StartForegroundService(intent);
            } else {
                context.StartService(intent);
            }
            IsRunning = true;
        }

        public void Stop() {
            var intent = new Intent(context, typeof(ForegroundService));
            context.StopService(intent);
            IsRunning = false;   
        }

        public override IBinder OnBind(Intent intent) {
            return null;
        }

        
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) {
            Notification notif = ReturnNotif();
            StartForeground(ServiceRunningNotifID, notif);

            Clipboard.ClipboardContentChanged += Clipboard_ClipboardContentChanged;
            //_ = DoLongRunningOperationThings();

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy() {
            base.OnDestroy();
            Clipboard.ClipboardContentChanged -= Clipboard_ClipboardContentChanged;
            //StopForeground(true);
        }

        public override bool StopService(Intent name) {
            return base.StopService(name);
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private Notification ReturnNotif() {

            var intent_filter = new IntentFilter();
            intent_filter.AddCategory(Intent.CategoryDefault);
            intent_filter.AddAction(READ_CLIPBOARD_ACTION);

            _broadcastReceiver = new MyBroadcastReceiver();
            this.RegisterReceiver(_broadcastReceiver, intent_filter);

            //var intent = new Intent(context, typeof(MainActivity));
            //intent.AddFlags(ActivityFlags.SingleTop);
            //intent.PutExtra("Title", "Message");
            //var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            var intent = new Intent(READ_CLIPBOARD_ACTION);
            var pendingIntent = PendingIntent.GetBroadcast(context, 0, intent, 0);

            var notifBuilder = new NotificationCompat.Builder(context, foregroundChannelId)
                .SetContentTitle("Your Title")
                .SetContentText("Main Text Body")
                //.SetSmallIcon(Resource.Drawable.MetroIcon)
                .SetOngoing(true)
                .SetContentIntent(pendingIntent);

            // Building channel if API verion is 26 or above
            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O) {
                NotificationChannel notificationChannel = 
                    new NotificationChannel(foregroundChannelId, "Title", NotificationImportance.Low);
                //notificationChannel.Importance = NotificationImportance.High;
                //notificationChannel.EnableLights(true);
                //notificationChannel.EnableVibration(true);
                //notificationChannel.SetShowBadge(true);
                //notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300, 400, 500, 400, 300, 200, 400 });

                var notifManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
                if (notifManager != null) {
                    notifBuilder.SetChannelId(foregroundChannelId);
                    notifManager.CreateNotificationChannel(notificationChannel);
                }
            }

            return notifBuilder.Build();
        }
        private async void Clipboard_ClipboardContentChanged(object sender, EventArgs e) {
            //throw new NotImplementedException();

            var text = await Clipboard.GetTextAsync();
            Toast.MakeText(this, text, ToastLength.Long).Show();
            if (text.Contains("@")) {
                await Clipboard.SetTextAsync(text.Replace("@", ""));
            }
        }
        #endregion

        #region Commands
        #endregion
    }

    public class MyBroadcastReceiver : BroadcastReceiver {
        public override void OnReceive(Context context, Intent intent) {
            string action = intent.Action;
            MpConsole.WriteLine($"Action: {action}");
        }
    }
}
