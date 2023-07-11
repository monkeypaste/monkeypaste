#if DESKTOP

using Avalonia.Threading;
using CefNet;
using MonkeyPaste.Common;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvCefNetApplication : CefNetApplication {
        #region Private Variables

        private MpAvCefNetMessageHub _messageHub;
        private Timer messagePump;
        private const int messagePumpDelay = 10;
        #endregion

        #region Constants

        public const string JS_REF_ERROR = "JS_REF_ERROR";

        #endregion

        #region Statics

        #endregion

        #region Properties
        public string LogFilePath =>
            Path.Combine(Mp.Services.PlatformInfo.ExecutingDir, "debug.log");

        public string CefFrameworkPath =>
            Path.Combine(Mp.Services.PlatformInfo.ExecutingDir, "cef", $"cef_{Mp.Services.PlatformInfo.OsShortName}");

        #endregion

        #region Message Pump (unused, for testing)

        private bool _useExternalMessagePump = false;

        private async void OnScheduleMessagePumpWorAsync(long delayMs) {
            await Task.Delay((int)delayMs);
            Dispatcher.UIThread.Post(CefApi.DoMessageLoopWork);
        }


        private void App_FrameworkInitialized(object sender, EventArgs e) {
            if (Instance.UsesExternalMessageLoop) {
                messagePump = new Timer(_ => Dispatcher.UIThread.Post(CefApi.DoMessageLoopWork), null, messagePumpDelay, messagePumpDelay);
            }
        }

        protected override void OnScheduleMessagePumpWork(long delayMs) {
            ScheduleMessagePumpWorkCallback(delayMs);
        }
        public Action<long> ScheduleMessagePumpWorkCallback { get; set; }
        #endregion

        #region Events

        #endregion
        public static void Init() {
            _ = new MpAvCefNetApplication();
        }


        public static void ShutdownCefNet() {
            if (Instance == null) {
                return;
            }
            Instance.Shutdown();
            MpConsole.WriteLine("CefNet Successfully shutdown");
        }

        private MpAvCefNetApplication() : base() {
            ResetCefNetLogging();

            string datFileName = "icudtl.dat";
            string cefRootDir = CefFrameworkPath;

            string localDirPath = Path.Combine(cefRootDir, "Resources", "locales");
            string resourceDirPath = Path.Combine(cefRootDir, "Resources");
            string releaseDir = Path.Combine(cefRootDir, "Release");
            string datFileSourcePath = Path.Combine(resourceDirPath, datFileName);
            string datFileTargetPath = Path.Combine(releaseDir, datFileName);

            if (!File.Exists(datFileTargetPath)) {
                // NOTE this would/will occur when a new cef version is installed
                if (!File.Exists(datFileSourcePath)) {
                    throw new Exception($"'CefNet cannot initialize, '{datFileSourcePath}' cannot be found");
                }
                try {
                    File.Copy(datFileSourcePath, datFileTargetPath);
                }
                catch (Exception ex) {
                    throw new Exception($"'CefNet cannot initialize, '{datFileSourcePath}' cannot be written to '{datFileTargetPath}'" + Environment.NewLine, ex);
                }
            }

            var settings = new CefSettings();
            if (_useExternalMessagePump) {
                // when true will only work on windows or linux, also maybe pointless...trying to configure for single process
                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux()) {
                    settings.MultiThreadedMessageLoop = false;
                    settings.ExternalMessagePump = true;
                    ScheduleMessagePumpWorkCallback = OnScheduleMessagePumpWorAsync;
                    App.FrameworkInitialized += App_FrameworkInitialized;
                } else {
                    settings.MultiThreadedMessageLoop = true;
                    settings.ExternalMessagePump = false;
                }
            } else {
                settings.MultiThreadedMessageLoop = true;
                settings.ExternalMessagePump = false;
            }

            if (OperatingSystem.IsLinux()) {
                settings.NoSandbox = true;
                //settings.CommandLineArgsDisabled = true;
                settings.WindowlessRenderingEnabled = true;
            } else if (OperatingSystem.IsWindows()) {
                settings.NoSandbox = true;
                //settings.CommandLineArgsDisabled = true;
                settings.WindowlessRenderingEnabled = true;
            }

            settings.LocalesDirPath = localDirPath;
            settings.ResourcesDirPath = resourceDirPath;
            settings.LogSeverity = CefLogSeverity.Warning;

            App.FrameworkShutdown += App_FrameworkShutdown;

            _messageHub = new MpAvCefNetMessageHub(this);

            Initialize(Path.Combine(cefRootDir, "Release"), settings);
            MpConsole.WriteLine("CefNet Initialized.");
        }

        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine) {
            base.OnBeforeCommandLineProcessing(processType, commandLine);
            //return;
            // Console.WriteLine("ChromiumWebBrowser_OnBeforeCommandLineProcessing");
            // Console.WriteLine(commandLine.CommandLineString);
            //
            //commandLine.AppendSwitchWithValue("proxy-server", "127.0.0.1:8888");

            //commandLine.AppendSwitch("ignore-certificate-errors");
            //commandLine.AppendSwitchWithValue("remote-debugging-port", "9222");

            //enable-devtools-experiments
            //commandLine.AppendSwitch("enable-devtools-experiments");

            //e.CommandLine.AppendSwitchWithValue("user-agent", "Mozilla/5.0 (Windows 10.0) WebKa/" + DateTime.UtcNow.Ticks);

            //double scale;
            //if (App.Desktop == null || MpAvWindowManager.MainWindow == null) {
            //    scale = new Window().PlatformImpl.DesktopScaling;
            //} else {
            //    scale = MpAvWindowManager.MainWindow.PlatformImpl.DesktopScaling;
            //}
            //commandLine.AppendSwitchWithValue("force-device-scale-factor", scale.ToString());

            //commandLine.AppendSwitch("disable-gpu");
            //commandLine.AppendSwitch("disable-gpu-compositing");
            //commandLine.AppendSwitch("disable-gpu-vsync");

            //commandLine.AppendSwitch("enable-begin-frame-scheduling");
            //commandLine.AppendSwitch("enable-media-stream");

            //commandLine.AppendSwitchWithValue("enable-blink-features", "CSSPseudoHas");

            commandLine.AppendSwitch("disable-component-update");
            commandLine.AppendSwitch("process-per-site");
            if (OperatingSystem.IsLinux()) {
                commandLine.AppendSwitch("no-zygote");
                commandLine.AppendSwitch("no-sandbox");
            }
        }

        private void App_FrameworkShutdown(object sender, EventArgs e) {
            messagePump?.Dispose();
            ShutdownCefNet();
        }

        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context) {
            _messageHub.WindowBinder.CefNetApp_OnCefNetContextCreated(this, context);
        }


        public void ResetCefNetLogging() {
            if (!string.IsNullOrEmpty(LogFilePath)) {
                MpFileIo.WriteTextToFile(LogFilePath, string.Empty, false);
            }
        }
    }


}
#endif