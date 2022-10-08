using Avalonia;
using System;
using System.Threading;
using MonkeyPaste.Common;
namespace MonkeyPaste.Avalonia
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        
        [STAThread]
        public static void Main(string[] args) {
//#if DEBUG
            // if(args.Length > 0 && args[0] == "waitfordebugger") {
            //     MpConsole.WriteLine("Waiting for debugger...");
            //     Thread.Sleep(10000); // Wait 10 Seconds
            // }
//#endif
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
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
