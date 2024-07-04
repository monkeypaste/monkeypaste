using Avalonia;
using Avalonia.Controls;
using Avalonia.iOS;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Foundation;
//using iosKeyboardTest.iOS.KeyboardExt;
using System;
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
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    { 
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI()
            //.AfterSetup(_ => {
            //    //if(MainView.IsMainViewLoaded) {
            //    //    Console.WriteLine("Already loaded");
            //    //    KeyboardViewController.CreateKeyboardView();
            //    //} else {
            //    //    MainView.OnMainViewLoaded += (s, e) => {
            //    //        KeyboardViewController.CreateKeyboardView();
            //    //    };
            //    //}
            //})
            ;
    }
}
