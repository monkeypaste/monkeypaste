using Avalonia;
using Avalonia.ReactiveUI;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Avalonia.Logging;

#if SUGAR_WV
using Avalonia.WebView.Desktop;
#endif

namespace MonkeyPaste.Avalonia {
    internal class Program {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.

        [STAThread]
        public static void Main(string[] args) {
//#if WINDOWS
            TryForceHighPerformanceGpu(); 
//#endif

            App.Args = args;

            WaitForDebug(args);
            HandleSingleInstanceLaunch(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
            .With(new AvaloniaNativePlatformOptions {
                OverlayPopups = true,
                //RenderingMode = [AvaloniaNativeRenderingMode.Software]
            })
            //.With(new Win32PlatformOptions {
            //    UseWgl = true,
            //    AllowEglInitialization = true
            //})
            //.With(new Win32PlatformOptions { AllowEglInitialization = true, UseWgl = true })
            //.With(new X11PlatformOptions { UseGpu = false, UseEGL = false, EnableSessionManagement = false })

            .UsePlatformDetect()
#if SUGAR_WV
            .UseDesktopWebView()
#endif
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace()// LogEventLevel.Verbose)
                ;
        static void HandleSingleInstanceLaunch(object[] args) {
#if CEFNET_WV
            // NOTE if implementing mutex this NEEDS to be beforehand or webviews never load
            MpAvCefNetApplication.Init();
#endif
            BuildAndLaunch(args);
        }
        private static void BuildAndLaunch(object[] args) {
            Exception top_level_ex = null;
            try {
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(App.Args);
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
                if (Mp.Services != null &&
                    Mp.Services.ShutdownHelper != null) {
                    Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.TopLevelException, top_level_ex == null ? "NONE" : top_level_ex.ToString());
                }

            }
        }
        private static void WaitForDebug(object[] args) {
            if (args.Contains(App.WAIT_FOR_DEBUG_ARG)) {
                Console.WriteLine("Attach debugger and use 'Set next statement'");
                while (true) {
                    Thread.Sleep(100);
                    if (Debugger.IsAttached)
                        break;
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("nvapi64.dll", EntryPoint = "fake")]
        private static extern int LoadNvApi64();

        [System.Runtime.InteropServices.DllImport("nvapi.dll", EntryPoint = "fake")]
        private static extern int LoadNvApi32();

        static void TryForceHighPerformanceGpu() {
            // from https://community.monogame.net/t/switchable-gpu-hell-sharpdx-exception-hresult-0x887a0005-the-gpu-device-instance-has-been-suspended/19249
            try {
                if (System.Environment.Is64BitProcess)
                    LoadNvApi64();
                else
                    LoadNvApi32();
            }
            catch { } // this will always be triggered, so just catch it and do nothing :P
        }

    }
}
