 using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

using System;

namespace MonkeyPaste.Avalonia
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

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
                            //.With(new AvaloniaNativePlatformOptions { UseGpu = true })
                            .UsePlatformDetect()
                            .LogToTrace();
    }
}
