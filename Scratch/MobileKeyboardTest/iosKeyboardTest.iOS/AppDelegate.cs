using Avalonia;
using Avalonia.iOS;
using Foundation;
using UIKit;

namespace iosKeyboardTest.iOS;

// The UIApplicationDelegate for the application. This class is responsible for launching the 
// User Interface of the application, as well as listening (and optionally responding) to 
// application events from iOS.
[Register("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public partial class AppDelegate : AvaloniaAppDelegate<App>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    //protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
    //    return base.CustomizeAppBuilder(builder)
    //        .WithInterFont()
    //        .UseReactiveUI()
    //        //.With(new iOSPlatformOptions { RenderingMode = [iOSRenderingMode.Metal] })
    //        .AfterSetup(_ => {

    //        });
    //        ;
    //}

    [Export("application:didFinishLaunchingWithOptions:")]
    public new bool FinishedLaunching(UIApplication application, NSDictionary launchOptions) {
        this.Window = new UIWindow(UIScreen.MainScreen.Bounds);
        var mkbvc = new MockKeyboardViewController();
        Window.RootViewController = mkbvc;
        Window.MakeKeyAndVisible();
        return true;
    }

}