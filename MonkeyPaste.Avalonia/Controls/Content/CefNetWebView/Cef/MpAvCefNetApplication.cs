using Avalonia.Threading;
using CefNet;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using CefNet.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvCefNetApplication : CefNetApplication {
        #region Private Variables

        private MpAvCefNetMessageHub _messageHub;
        private Timer messagePump;
        private const int messagePumpDelay = 10;
        #endregion

        #region Constants

        public const string JS_REF_ERROR = "JS_REF_ERROR";

        public static bool UseCefNet { get; private set; } = true;
        #endregion

        #region Statics


        #endregion

        #region Properties



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
        public static void InitCefNet() {
            _ = new MpAvCefNetApplication();
        }

        public static void ShutdownCefNet() {
            if(Instance == null) {
                return;
            }
            Instance.Shutdown();
            MpConsole.WriteLine("CefNet Successfully shutdown");
        }
        public static string GetEditorPath() {
            string solution_path = MpCommonHelpers.GetSolutionDir();
            return Path.Combine(solution_path, "MonkeyPaste", "Resources", "Html", "Editor", "index.html");
        }

        private MpAvCefNetApplication() : base() {
            string datFileName = "icudtl.dat";
            //string cefRootDir = @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Avalonia\cef";
            string solution_dir = MpCommonHelpers.GetSolutionDir();
            string cefRootDir = Path.Combine(solution_dir,"MonkeyPaste.Avalonia", "cef");

            string localDirPath = string.Empty;
            string resourceDirPath = string.Empty;
            string releaseDir = string.Empty;
            string datFileSourcePath = string.Empty;
            string datFileTargetPath = string.Empty;

            if (OperatingSystem.IsWindows()) {
                cefRootDir = Path.Combine(cefRootDir, "win");
                localDirPath = Path.Combine(cefRootDir, "Resources", "locales");
                resourceDirPath = Path.Combine(cefRootDir, "Resources");
                releaseDir = Path.Combine(cefRootDir, "Release");
                datFileSourcePath = Path.Combine(resourceDirPath, datFileName);
                datFileTargetPath = Path.Combine(releaseDir, datFileName);
            } else if (OperatingSystem.IsLinux()) {
                cefRootDir = Path.Combine(cefRootDir, "linux");
                localDirPath = Path.Combine(cefRootDir, "Resources", "locales");
                resourceDirPath = Path.Combine(cefRootDir, "Resources");
                releaseDir = Path.Combine(cefRootDir, "Release");
                datFileSourcePath = Path.Combine(resourceDirPath, datFileName);
                datFileTargetPath = Path.Combine(releaseDir, datFileName);
            } else if(OperatingSystem.IsMacOS()) {
                cefRootDir = Path.Combine(cefRootDir, "mac");
            }  else {
                throw new Exception("No cef implementation found for this architecture");
            }

            if(!File.Exists(datFileTargetPath)) {
                // NOTE this would/will occur when a new cef version is installed
                if(!File.Exists(datFileSourcePath)) {
                    throw new Exception($"'CefNet cannot initialize, '{datFileSourcePath}' cannot be found");
                }
                try {
                    File.Copy(datFileSourcePath, datFileTargetPath);
                }catch(Exception ex) {
                    throw new Exception($"'CefNet cannot initialize, '{datFileSourcePath}' cannot be written to '{datFileTargetPath}'"+Environment.NewLine,ex);
                }
            }
            

            var settings = new CefSettings();
            if(_useExternalMessagePump) {
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

            if(OperatingSystem.IsLinux()) {
                settings.NoSandbox = true;
                //settings.CommandLineArgsDisabled = true;
                settings.WindowlessRenderingEnabled = true;
            } else if(OperatingSystem.IsWindows()) {
                settings.NoSandbox = true;
                //settings.CommandLineArgsDisabled = true;
                settings.WindowlessRenderingEnabled = true;
            }
            
            settings.LocalesDirPath = localDirPath;
            settings.ResourcesDirPath = resourceDirPath;
            settings.LogSeverity = CefLogSeverity.Verbose;
        
            App.FrameworkShutdown += App_FrameworkShutdown;

            _messageHub = new MpAvCefNetMessageHub(this);

            Initialize(Path.Combine(cefRootDir, "Release"), settings);
            MpConsole.WriteLine("CefNet Initialized.");
            return;
        }
        
        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            base.OnBeforeCommandLineProcessing(processType, commandLine);
            //return;
            // Console.WriteLine("ChromiumWebBrowser_OnBeforeCommandLineProcessing");
            // Console.WriteLine(commandLine.CommandLineString);
            //
             //commandLine.AppendSwitchWithValue("proxy-server", "127.0.0.1:8888");
            
            // commandLine.AppendSwitch("ignore-certificate-errors");
            // commandLine.AppendSwitchWithValue("remote-debugging-port", "9222");
			         
            // //enable-devtools-experiments
            // commandLine.AppendSwitch("enable-devtools-experiments");
            
            // //e.CommandLine.AppendSwitchWithValue("user-agent", "Mozilla/5.0 (Windows 10.0) WebKa/" + DateTime.UtcNow.Ticks);
            
            // //("force-device-scale-factor", "1");
            
            // //commandLine.AppendSwitch("disable-gpu");
            // //commandLine.AppendSwitch("disable-gpu-compositing");
            // //commandLine.AppendSwitch("disable-gpu-vsync");
            
            // commandLine.AppendSwitch("enable-begin-frame-scheduling");
            // commandLine.AppendSwitch("enable-media-stream");
            
            // commandLine.AppendSwitchWithValue("enable-blink-features", "CSSPseudoHas");
            
            commandLine.AppendSwitch("disable-component-update");
            if (OperatingSystem.IsLinux())
            {
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


    }

    
}
