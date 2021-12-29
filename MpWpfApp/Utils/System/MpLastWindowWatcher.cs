using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpLastWindowWatcher {
        #region Private Variables
        private bool _isFirstTick = false;
        //private Dictionary<string, List<IntPtr>> _currentProcessWindowHandleStackDictionary = new Dictionary<string, List<IntPtr>>();
        #endregion

        public string ThisAppPath { get; set; }
        
        public IntPtr ThisAppHandle { get; set; }

        private IntPtr _lastHandle = IntPtr.Zero;
        public IntPtr LastHandle {
            get {
                return _lastHandle;
            }
            set {
                _lastHandle = value;
            }
        }

        private string _lastTitle = string.Empty;
        public string LastTitle {
            get {
                return _lastTitle;
            }
            set {
                _lastTitle = value;
            }
        }

        public MpLastWindowWatcher() {
            HwndSource hwnd = (HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow);
            //Process.GetCurrentProcess().Invalidate();
            ThisAppHandle = hwnd.Handle;
            LastHandle = IntPtr.Zero;
            ThisAppPath = MpHelpers.Instance.GetProcessPath(ThisAppHandle);
            
            MpRunningApplicationManager.Instance.Init();

            MpRunningApplicationManager.Instance.RefreshHandleStack();

            Task.Run(async () => {
                while (MpClipboardManager.Instance.LastWindowWatcher == null) {
                    await Task.Delay(100);
                }
                await MpAppCollectionViewModel.Instance.Refresh();
                var handleLookup = MpRunningApplicationManager.Instance.CurrentProcessWindowHandleStackDictionary.ToArray();
                foreach (var kvp in handleLookup) {
                    var dupCheck = MpAppCollectionViewModel.Instance.GetAppViewModelByProcessPath(kvp.Key);
                    if (dupCheck == null) {
                        var iconBmpSrc = MpHelpers.Instance.GetIconImage(kvp.Key);
                        var icon = await MpIcon.Create(iconBmpSrc.ToBase64String());
                        string appName = MpHelpers.Instance.GetProcessApplicationName(kvp.Value[0]);
                        var app = await MpApp.Create(kvp.Key, appName, icon);
                    }
                }
                MpConsole.WriteLine("This app's exe: " + ThisAppPath);

                MpHelpers.Instance.RunOnMainThread(() => {
                    Timer timer = new Timer(500);
                    timer.Elapsed += Timer_Elapsed;

                    timer.Start();
                });
            });

            
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            IntPtr currentHandle = WinApi.GetForegroundWindow();

            MpRunningApplicationManager.Instance.RefreshHandleStack();

            if (ThisAppHandle == IntPtr.Zero) {
                ThisAppHandle = Process.GetCurrentProcess().MainWindowHandle;
            }
            if (currentHandle != LastHandle &&
                currentHandle != ThisAppHandle &&
                ThisAppHandle != IntPtr.Zero &&
                currentHandle != IntPtr.Zero &&
                !MpMainWindowViewModel.Instance.IsShowingDialog &&
                !MpMainWindowViewModel.Instance.IsShowingDialog) {
                LastHandle = currentHandle;
                LastTitle = MpHelpers.Instance.GetProcessMainWindowTitle(LastHandle);

                MpRunningApplicationManager.Instance.UpdateHandleStack(LastHandle);

                string processName = MpHelpers.Instance.GetProcessPath(LastHandle);
                if (processName.ToLower().Contains("powershell")) {
                    Debugger.Break();
                }
                var dupCheck = MpAppCollectionViewModel.Instance.GetAppViewModelByProcessPath(processName);
                if (dupCheck == null) {
                    var iconBmpSrc = MpHelpers.Instance.GetIconImage(processName);
                    var icon = await MpIcon.Create(iconBmpSrc.ToBase64String());
                    var app = await MpApp.Create(processName, MpHelpers.Instance.GetProcessApplicationName(LastHandle), icon);

                }

                MonkeyPaste.MpConsole.WriteLine(string.Format(@"Last Window: {0} ({1})", MpHelpers.Instance.GetProcessMainWindowTitle(_lastHandle), _lastHandle));
            }
        }
    }   
}
