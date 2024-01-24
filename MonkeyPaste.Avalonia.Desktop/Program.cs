using Avalonia;
using Avalonia.ReactiveUI;
using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.IO;
using MonkeyPaste.Common.Plugin;


#if CEFNET_WV
using CefNet;
#endif

namespace MonkeyPaste.Avalonia {
    internal class Program {
        static bool CLEAR_STORAGE = false;
        static bool LOCALIZE_ONLY = false;
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.

        [STAThread]
        public static void Main(string[] args) {
            bool is_debug =
#if DEBUG
                true;
#else
                false;
#endif

            if (args.Contains(App.WAIT_FOR_DEBUG_ARG) || (args.Contains(App.RESTART_ARG) && is_debug)) {
                Console.WriteLine("Attach debugger and use 'Set next statement'");
                while (true) {
                    Thread.Sleep(100);
                    if (Debugger.IsAttached)
                        break;
                }
            }
            if (CLEAR_STORAGE) {
                // NOTE use this when local storage folder won't go away
                string path1 = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MonkeyPaste");
                string path2 = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MonkeyPaste_DEBUG");
                bool success1 = MpFileIo.DeleteDirectory(path1);
                bool success2 = MpFileIo.DeleteDirectory(path2);
                Console.WriteLine($"Deleted '{path1}': {success1.ToTestResultLabel()}");
                Console.WriteLine($"Deleted '{path2}': {success2.ToTestResultLabel()}");
            }

            if (LOCALIZE_ONLY) {
                MpAvCurrentCultureViewModel.SetAllCultures(new System.Globalization.CultureInfo("zh-CN"));
                return;
            }

            Exception top_level_ex = null;
            try {

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
            .UseReactiveUI()
                .LogToTrace()// LogEventLevel.Verbose)
                ;


    }
}
