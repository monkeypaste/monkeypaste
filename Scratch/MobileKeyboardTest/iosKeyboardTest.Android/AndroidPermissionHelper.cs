using Android.Content;
using Android.Provider;
//using Xamarin.Essentials;

namespace iosKeyboardTest.Android {
    public class AndroidPermissionHelper : IKeyboardPermissionHelper {
        public void ShowKeyboardActivator() {
            if(MainActivity.Instance is not { } ma) {
                return;
            }
            ma.StartActivity(new Intent(Settings.ActionInputMethodSettings));
        }
    }
}
