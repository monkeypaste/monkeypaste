using Avalonia;
using Avalonia.ReactiveUI;
using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MonkeyPaste.Common.Plugin;
#if CEFNET_WV
using CefNet;
#endif

namespace MonkeyPaste.Avalonia {
    internal class Program {
        const string THIS_APP_GUID = "252C6489-DFF3-4CFF-A419-7D3770461FFE";
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.

        [STAThread]
        public static void Main(string[] args) {
            TryForceHighPerformanceGpu();

            App.Args = args;

            WaitForDebug(args);
            HandleSingleInstanceLaunch(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        static AppBuilder BuildAvaloniaApp()
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
        static void HandleSingleInstanceLaunch(object[] args) {
#if CEFNET_WV
            // NOTE if implementing mutex this NEEDS to be beforehand or webviews never load
            MpAvCefNetApplication.Init();
#endif
            // from https://stackoverflow.com/a/19128246/105028

            // get application GUID as defined in AssemblyInfo.cs
            //string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
            //string appGuid = THIS_APP_GUID;

            //// unique id for global mutex - Global prefix means it is global to the machine
            //string mutexId = string.Format("Global\\{{{0}}}", appGuid);

            //using (var mutex = new Mutex(false, mutexId)) {
            //    try {
            //        if (!mutex.WaitOne(0, false)) {
            //            //signal existing app via named pipes

            //            MpNamedPipe<string>.Send(MpNamedPipeTypes.SourceRef, "test");
            //            //MpNamedPipe<string>.Send(MpNamedPipeTypes.SourceRef, args == null ? string.Empty : string.Join(Environment.NewLine,args));

            //            Environment.Exit(0);
            //        } else {
            // handle protocol with this instance   
            BuildAndLaunch(args);

            //        }
            //    }
            //    finally {
            //        mutex.ReleaseMutex();
            //    }
            //}
        }
        private static void BuildAndLaunch(object[] args) {
            Exception top_level_ex = null;
            try {

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
        private static void WaitForDebug(object[] args) {
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