using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;

namespace MpWpfApp {
    public class MpLastWindowWatcher {
        #region Private Variables
        private Dictionary<string, List<IntPtr>> _currentProcessWindowHandleStackDictionary = new Dictionary<string, List<IntPtr>>();
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
            ThisAppPath = MpHelpers.GetProcessPath(ThisAppHandle);
            Console.WriteLine("This app's exe: " + ThisAppPath);
            Timer timer = new Timer(100);
            timer.Elapsed += (s, e) => {
                IntPtr currentHandle = WinApi.GetForegroundWindow();
                
                //RefreshHandleStack();

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
                    LastTitle = MpHelpers.GetProcessMainWindowTitle(LastHandle);

                    //UpdateHandleStack(LastHandle);
                    Console.WriteLine("Last Window: " + MpHelpers.GetProcessMainWindowTitle(_lastHandle));
                }
            };
            timer.Start();
        }
        private void RefreshHandleStack() {
            var toRemoveProcessNameList = new List<string>();
            foreach(var processStack in _currentProcessWindowHandleStackDictionary) {
                bool isProcessTerminated = true;
                foreach (var handle in processStack.Value) { 
                    if(WinApi.IsWindow(handle)) {
                        isProcessTerminated = false;
                    }
                }
                if(isProcessTerminated) {
                    toRemoveProcessNameList.Add(processStack.Key);
                }
            }
            foreach(var processToRemove in toRemoveProcessNameList) {
                _currentProcessWindowHandleStackDictionary.Remove(processToRemove);
            }
        }
        private void UpdateHandleStack(IntPtr fgHandle) {
            var processName = MpHelpers.GetProcessPath(fgHandle);
            if(_currentProcessWindowHandleStackDictionary.ContainsKey(processName)) {
                if(_currentProcessWindowHandleStackDictionary[processName].Contains(fgHandle)) {
                    _currentProcessWindowHandleStackDictionary[processName].Remove(fgHandle);
                }
                _currentProcessWindowHandleStackDictionary[processName].Insert(0, fgHandle);
            } else {
                _currentProcessWindowHandleStackDictionary.Add(processName, new List<IntPtr> { fgHandle });
            }
        }
        private void PrintHandleStack() {
            foreach(var handleStack in _currentProcessWindowHandleStackDictionary) {
                var outStr = handleStack.Key;
                foreach(var handle in handleStack.Value) {
                    outStr += " " + handle.ToInt32();
                }
                Console.WriteLine(outStr);
            }
        }
    }
}
