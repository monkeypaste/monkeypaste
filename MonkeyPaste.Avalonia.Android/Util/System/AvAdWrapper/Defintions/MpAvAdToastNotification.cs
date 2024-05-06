using Android.Widget;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdToastNotification : MpIPlatformToastNotification {
        public void ShowToast(string title, string text, object icon, string accentHexColor) {
            // TODO create custom toast like this https://stackoverflow.com/a/76724589/105028

            ToastLength tl = text.Length < 30 ? ToastLength.Short : ToastLength.Long;
            Toast.MakeText(MainActivity.Instance, text, tl).Show();
        }
    }
}
