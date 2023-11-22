using Avalonia;
using Avalonia.ReactiveUI;
using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
#if CEFNET_WV
using CefNet;
#endif

namespace MonkeyPaste.Avalonia {
    internal class Program {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.

        [STAThread]
        public static void Main(string[] args) {
            if (args.Contains("--wait-for-attach")) {
                Console.WriteLine("Attach debugger and use 'Set next statement'");
                while (true) {
                    Thread.Sleep(100);
                    if (Debugger.IsAttached)
                        break;
                }
            }
            Exception top_level_ex = null;
            try {
                //if (MpFileIo.IsFileInUse(MpConsole.LogFilePath) && !MpConsole.HasInitialized) {
                //    return;
                //}
                //MpConsole.Init();

#if CEFNET_WV
                MpAvCefNetApplication.Init();
#endif
                //App.Args = args ?? new string[] { };
                BuildAvaloniaApp()
#if CEFNET_WV
        .StartWithCefNetApplicationLifetime(App.Args);
#else
        .StartWithClassicDesktopLifetime(args);
#endif
                // 
            }
            catch (Exception ex) {
                top_level_ex = ex;
                // here we can work with the exception, for example add it to our log file
                MpConsole.WriteTraceLine("Something very bad happened", ex);
                if (Mp.Services != null &&
                    Mp.Services.ShutdownHelper != null) {
                    Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.TopLevelException, top_level_ex == null ? "NONE" : top_level_ex.ToString());
                }
            }
            finally {
                // This block is optional. 
                // Use the finally-block if you need to clean things up or similar
                //Log.CloseAndFlush();
                if (Mp.Services != null &&
                    Mp.Services.ShutdownHelper != null) {
                    Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.TopLevelException, top_level_ex == null ? "NONE" : top_level_ex.ToString());
                }

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
            //.With(new AvaloniaNativePlatformOptions { UseGpu = false }

            .UsePlatformDetect()
            //.WithInterFont()
            .LogToTrace()
            .UseReactiveUI()
                .LogToTrace()// LogEventLevel.Verbose)
                ;


    }
}
