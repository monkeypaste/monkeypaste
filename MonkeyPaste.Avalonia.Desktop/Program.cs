using Avalonia;
using CefNet;
using MonkeyPaste.Common;
using System;

namespace MonkeyPaste.Avalonia {
    internal class Program {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.

        [STAThread]
        public static void Main(string[] args) {
            try {

                App.Args = args ?? new string[] { };
                BuildAvaloniaApp()
                //.StartWithClassicDesktopLifetime(args);
                .StartWithCefNetApplicationLifetime(args);
            }
            catch (Exception e) {
                // here we can work with the exception, for example add it to our log file
                MpConsole.WriteTraceLine("Something very bad happened", e);
            }
            finally {
                // This block is optional. 
                // Use the finally-block if you need to clean things up or similar
                //Log.CloseAndFlush();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                            //.With(new Win32PlatformOptions { UseWgl = true })
                            //.With(new AvaloniaNativePlatformOptions { UseGpu = !OperatingSystem.IsMacOS() })
                            //.With(new Win32PlatformOptions {
                            //    UseWgl = true,
                            //    AllowEglInitialization = true
                            //})
                            //.With(new Win32PlatformOptions { AllowEglInitialization = true, UseWgl = true })
                            //.With(new X11PlatformOptions { UseGpu = false, UseEGL = false, EnableSessionManagement = false })
                            //.With(new AvaloniaNativePlatformOptions { UseGpu = false })
                            .UsePlatformDetect()
                            .LogToTrace();
    }
}
