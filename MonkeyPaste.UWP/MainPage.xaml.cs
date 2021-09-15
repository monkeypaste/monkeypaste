using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Diagnostics;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MonkeyPaste.UWP {
    public sealed partial class MainPage {
        public MpLastWindowWatcher LastWindowWatcher { get; set; }
        public MainPage() {
            this.InitializeComponent();

            var nw = new MpNativeWrapper() {
                UiLocationFetcher = new MpUiLocationFetcher(),
                TouchService = new MpGlobalTouch(),
                DbInfo = new MpDbFilePath_Uwp(),
                KeyboardService = new MpKeyboardInteractionService()
            };

            this.Loaded += MainPage_Loaded;

            MpClipboardListener.Instance.OnClipboardChanged += Instance_OnClipboardChanged;

            MpClipboardListener.Instance.Start();

            LoadApplication(new MonkeyPaste.App(nw));
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e) {

            var proc = Process.GetCurrentProcess();
            Task.Run(async () => {
                //DiagnosticAccessStatus diagnosticAccessStatus = await AppDiagnosticInfo.RequestAccessAsync();
                //switch (diagnosticAccessStatus) {
                //    case DiagnosticAccessStatus.Allowed:
                //        IReadOnlyList<ProcessDiagnosticInfo> processes = ProcessDiagnosticInfo.GetForProcesses();
                //        var p = processes.Where(x => x.ExecutableFileName == "MonkeyPaste.UWP.exe").FirstOrDefault();
                //        if (p != null) {
                //            LastWindowWatcher = new MpLastWindowWatcher(p.GetAppDiagnosticInfos().)
                //        }
                //        break;
                //    case DiagnosticAccessStatus.Limited:
                //        break;
                //}
                proc.Refresh();
                var hwnd = proc.Handle;
                while (hwnd == IntPtr.Zero) {
                    await Task.Delay(100);
                    proc.Refresh();
                    hwnd = proc.Handle;
                }
                LastWindowWatcher = new MpLastWindowWatcher(hwnd);
            });
        }

        private void Instance_OnClipboardChanged(object sender, object e) {
            if(LastWindowWatcher == null) {
                return;
            }
            var cboDict = e as Dictionary<string, object>;

            var processHandle = LastWindowWatcher.LastHandle;
            if (processHandle == IntPtr.Zero) {
                // since source is unknown set to this app
                processHandle = LastWindowWatcher.ThisAppHandle;
            }
            string processPath = MpProcessHelper.Instance.GetProcessPath(processHandle);
            string appName = MpProcessHelper.Instance.GetProcessApplicationName(processHandle);
            var processIconImg = MpImageHelper.Instance.GetIconImage(processPath);

            string processIconImg64 = string.Empty;// MpImageHelper.Instance.GetIconImage(processPath);
            
            MpApp app = MpApp.GetAppByPath(processPath);
            if (app == null) {
                var icon = MpIcon.GetIconByImageStr(processIconImg64);
                if (icon == null) {
                    icon = MpIcon.Create(processIconImg64);
                }
                app = MpApp.Create(processPath, appName, icon);
            }

            MpUrl url = null;
            foreach (var cbe in cboDict) {
                Debug.WriteLine(cbe.Key + ": " + cbe.Value.ToString());
            }
        }
    }
}
