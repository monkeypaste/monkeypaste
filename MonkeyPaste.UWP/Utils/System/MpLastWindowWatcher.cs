﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Runtime.InteropServices;

namespace MonkeyPaste.UWP {
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
            ThisAppPath = MpProcessHelper.Instance.GetProcessPath(ThisAppHandle);
            
           // MpRunningApplicationManager.Instance.Init();

            MonkeyPaste.MpConsole.WriteLine("This app's exe: " + ThisAppPath);
            
            Timer timer = new Timer(100);
            timer.Elapsed += (s, e) => {
                IntPtr currentHandle = WinApi.GetForegroundWindow();
                
               // MpRunningApplicationManager.Instance.RefreshHandleStack();

                if (ThisAppHandle == IntPtr.Zero) { 
                    ThisAppHandle = Process.GetCurrentProcess().MainWindowHandle;
                }
                if (currentHandle != LastHandle && 
                    currentHandle != ThisAppHandle && 
                    ThisAppHandle != IntPtr.Zero && 
                    currentHandle != IntPtr.Zero// &&
                    //!MpSettingsWindowViewModel.IsOpen &&
                   // !MpAssignShortcutModalWindowViewModel.IsOpen &&
                    /*!MpLoadingWindow.IsOpen*/) {
                    LastHandle = currentHandle;
                    LastTitle = MpProcessHelper.Instance.GetProcessMainWindowTitle(LastHandle);

                   // MpRunningApplicationManager.Instance.UpdateHandleStack(LastHandle);

                    MonkeyPaste.MpConsole.WriteLine(string.Format(@"Last Window: {0} ({1})",MpProcessHelper.Instance.GetProcessMainWindowTitle(_lastHandle), _lastHandle));
                }
            };
            timer.Start();
        }

    }   
}