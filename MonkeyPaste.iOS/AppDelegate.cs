using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using FFImageLoading.Forms.Platform;
using Foundation;
using UIKit;
using Xamarin.Essentials;

namespace MonkeyPaste.iOS {
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate {
        public MpINativeInterfaceWrapper IosInterfaceWrapper { get; set; }
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options) {
            Rg.Plugins.Popup.Popup.Init();

            global::Xamarin.Forms.Forms.Init();

            CachedImageRenderer.Init();
            CachedImageRenderer.InitImageSourceHandler();
            IosInterfaceWrapper = new MpIosInterfaceWrapper() {
                KeyboardService = new MpKeyboardInteractionService()
            };
            LoadApplication(new App(IosInterfaceWrapper));

            return base.FinishedLaunching(app, options);
        }
    }
}
