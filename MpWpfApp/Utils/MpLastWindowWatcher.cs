using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;

namespace MpWpfApp {
    public class MpLastWindowWatcher {
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

        public MpLastWindowWatcher(IntPtr appHandle) {
            //Process.GetCurrentProcess().Invalidate();
            ThisAppHandle = appHandle;
            LastHandle = IntPtr.Zero;
            ThisAppPath = MpHelpers.GetProcessPath(ThisAppHandle);
            Console.WriteLine("This app's exe: " + ThisAppPath);
            Timer timer = new Timer(100);
            timer.Elapsed += (s, e) => {
                IntPtr currentHandle = WinApi.GetForegroundWindow();
                //var mwvm = (MpMainWindowViewModel)Application.Current.MainWindow.DataContext;
                if (ThisAppHandle == IntPtr.Zero) {
                    ThisAppHandle = Process.GetCurrentProcess().MainWindowHandle;
                }
                if (currentHandle != LastHandle && 
                    currentHandle != ThisAppHandle && 
                    ThisAppHandle != IntPtr.Zero && 
                    currentHandle != IntPtr.Zero &&
                    !MpSettingsWindowViewModel.IsOpen &&
                    !MpAssignShortcutModalWindowViewModel.IsOpen &&
                    !MpTemplateTokenEditModalWindowViewModel.IsOpen && 
                    !MpTemplateTokenPasteModalWindowViewModel.IsOpen) {
                    LastHandle = currentHandle;
                    LastTitle = MpHelpers.GetProcessMainWindowTitle(LastHandle);
                    Console.WriteLine("Last Window: " + MpHelpers.GetProcessMainWindowTitle(_lastHandle));
                }
            };
            timer.Start();
        }        
    }
}
