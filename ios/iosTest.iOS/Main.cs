using MonkeyPaste.Avalonia;
using UIKit;

namespace iosTest.iOS {
    public class Application {
        // This is the main entry point of the application.
        static void Main(string[] args) {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
#if DEBUG
            //App.WaitForDebug(new string[] {App.WAIT_FOR_DEBUG_ARG});
#endif
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}
