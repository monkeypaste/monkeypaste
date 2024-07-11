using Avalonia;
using Avalonia.iOS;
using Avalonia.ReactiveUI;
using Foundation;

namespace iosKeyboardTest.iOS;

// The UIApplicationDelegate for the application. This class is responsible for launching the 
// User Interface of the application, as well as listening (and optionally responding) to 
// application events from iOS.
[Register("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public partial class AppDelegate : AvaloniaAppDelegate<App>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    { 
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI()
            //.With(new iOSPlatformOptions { RenderingMode = [iOSRenderingMode.Metal] })
            //.UseMaui<MauiApplication>(this)
            .AfterSetup(_ => {
                //KeyboardExtHelper.
                //KeyboardViewController.InitAv();
                //if (MainView.IsMainViewLoaded) {
                //    Debug.WriteLine("Already loaded");
                //    //KeyboardViewController.CreateKeyboardView();
                //} else {
                //    MainView.OnMainViewLoaded += (s, e) => {
                //        //KeyboardViewController.CreateKeyboardView();
                //        //iosExtAvaloniaViewLoader.AvViewObj = new AvaloniaView() {
                //        //    Content = new Avalonia.Controls.Border() {
                //        //        Width = 600,
                //        //        Height = 600,
                //        //        Background = Brushes.Brown
                //        //    }
                //        //};
                //        //iosExtAvaloniaViewLoader.AvViewObj = MauiApplication.Instance.Handler.MauiContext;

                //        //DispatchQueue.MainQueue.DispatchAsync(() => {
                //        //    var alert = UIAlertController.Create("Test title", "Test msg", UIAlertControllerStyle.Alert);
                //        //    var ok = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (x) => {
                //        //        Debug.WriteLine("whatever");
                //        //    });
                //        //    alert.AddAction(ok);
                //        //    this.Window.RootViewController.PresentViewController(alert, true, null);
                //        //});
                //    };
                //}

            })
            ;
    }


}