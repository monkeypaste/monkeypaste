using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Runtime.InteropServices;

namespace MpWpfApp {
    public class MpLastWindowWatcher {
        #region Private Variables
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

        public MpLastWindowWatcher(IntPtr appHandle) {
            //Process.GetCurrentProcess().Invalidate();
            ThisAppHandle = appHandle;
            LastHandle = IntPtr.Zero;
            ThisAppPath = MpHelpers.Instance.GetProcessPath(ThisAppHandle);
            
            MpRunningApplicationManager.Instance.Init();

            Console.WriteLine("This app's exe: " + ThisAppPath);
            
            Timer timer = new Timer(100);
            timer.Elapsed += (s, e) => {
                IntPtr currentHandle = WinApi.GetForegroundWindow();
                
                MpRunningApplicationManager.Instance.RefreshHandleStack();

                if (ThisAppHandle == IntPtr.Zero) { 
                    ThisAppHandle = Process.GetCurrentProcess().MainWindowHandle;
                }
                if (currentHandle != LastHandle && 
                    currentHandle != ThisAppHandle && 
                    ThisAppHandle != IntPtr.Zero && 
                    currentHandle != IntPtr.Zero &&
                    !MpSettingsWindowViewModel.IsOpen &&
                    !MpAssignShortcutModalWindowViewModel.IsOpen) {
                    LastHandle = currentHandle;
                    LastTitle = MpHelpers.Instance.GetProcessMainWindowTitle(LastHandle);

                    MpRunningApplicationManager.Instance.UpdateHandleStack(LastHandle);

                    Console.WriteLine("Last Window: " + MpHelpers.Instance.GetProcessMainWindowTitle(_lastHandle));
                }
            };
            timer.Start();
        }

    }   
}
