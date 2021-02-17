using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HWND = System.IntPtr;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace MpWpfApp {
    public class MpRunningApplicationManager : INotifyPropertyChanged {
        private static readonly Lazy<MpRunningApplicationManager> _Lazy = new Lazy<MpRunningApplicationManager>(() => new MpRunningApplicationManager());
        public static MpRunningApplicationManager Instance { get { return _Lazy.Value; } }

        #region Private Variables

        #endregion

        #region Properties
        private Dictionary<string, List<IntPtr>> _currentProcessWindowHandleStackDictionary = new Dictionary<string, List<IntPtr>>();
        public Dictionary<string, List<IntPtr>> CurrentProcessWindowHandleStackDictionary {
            get {
                return _currentProcessWindowHandleStackDictionary;
            }
        }
        #endregion

        #region Public Methods
        public void Init() {
            //empty to initialize singleton
        }

        public void RefreshHandleStack() {
            //called in LastWindowWatcher's timer to remove closed window handles and processes
            var toRemoveProcessNameList = new List<string>();
            var toRemoveHandleKeyValueList = new List<KeyValuePair<string, IntPtr>>();
            foreach (var processStack in CurrentProcessWindowHandleStackDictionary) {
                //loop through all known processes
                bool isProcessTerminated = true;
                foreach (var handle in processStack.Value) {
                    //loop through all known handles to that process
                    if (WinApi.IsWindow(handle)) {
                        //verify that the processes window handle is still running
                        isProcessTerminated = false;
                    } else {
                        //if handle gone mark it to be removed from its handle stack
                        toRemoveHandleKeyValueList.Add(new KeyValuePair<string, IntPtr>(processStack.Key, handle));
                    }
                }
                if (isProcessTerminated) {
                    toRemoveProcessNameList.Add(processStack.Key);
                }
            }
            bool wasStackChanged = false;
            foreach (var processToRemove in toRemoveProcessNameList) {
                //remove any processes w/o active handles
                CurrentProcessWindowHandleStackDictionary.Remove(processToRemove);
                wasStackChanged = true;

                Console.WriteLine(string.Format(@"Process: {0} REMOVED", processToRemove));
            }
            foreach(var handleToRemove in toRemoveHandleKeyValueList) {                
                if(CurrentProcessWindowHandleStackDictionary.ContainsKey(handleToRemove.Key)) {
                    //remove individual window handles that were flagged
                    CurrentProcessWindowHandleStackDictionary[handleToRemove.Key].Remove(handleToRemove.Value);
                    wasStackChanged = true;
                    Console.WriteLine(string.Format(@"Process: {0} Handle: {1} REMOVED", handleToRemove.Key, handleToRemove.Value));
                }
            }
            if(wasStackChanged) {
                OnPropertyChanged(nameof(CurrentProcessWindowHandleStackDictionary));
            }
        }

        public void UpdateHandleStack(IntPtr fgHandle) {
            //check if this handle is already be tracked
            string processName = GetKnownProcessPath(fgHandle);
            if (string.IsNullOrEmpty(processName)) {
                //if it is not resolve its process path
                processName = MpHelpers.Instance.GetProcessPath(fgHandle);
            }
            bool wasStackChanged = false;
            processName = processName.ToLower();
            if (CurrentProcessWindowHandleStackDictionary.ContainsKey(processName)) {
                //if process is already being tracked 
                if (CurrentProcessWindowHandleStackDictionary[processName].Contains(fgHandle)) {
                    //remove the handle if it is also being tracked
                    CurrentProcessWindowHandleStackDictionary[processName].Remove(fgHandle);
                }
                //set fg handle to the top of its process list
                CurrentProcessWindowHandleStackDictionary[processName].Insert(0, fgHandle);
                wasStackChanged = true;

                Console.WriteLine(string.Format(@"(Known) Process: {0} Handle:{1} ACTIVE", processName, fgHandle));
            } else {
                //if its a new process create a new list with this handle as its element
                CurrentProcessWindowHandleStackDictionary.Add(processName, new List<IntPtr> { fgHandle });
                wasStackChanged = true;

                Console.WriteLine(string.Format(@"(New) Process: {0} Handle:{1} ACTIVE", processName, fgHandle));
            }
            if (wasStackChanged) {
                OnPropertyChanged(nameof(CurrentProcessWindowHandleStackDictionary));
            }
        }

        public IntPtr SetActiveProcess(string processPath, bool isAdmin, object forceHandle = null) {
            try {
                if (string.IsNullOrEmpty(processPath)) {
                    return IntPtr.Zero;
                }
                if(processPath[0] == '%') {
                    //only occurs for hardcoded %windir%\cmd.exe
                    processPath = string.Format(
                        @"{0}\System32\cmd.exe",
                        Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName);
                }
                processPath = processPath.Replace(@"\\", @"\").ToLower();
                Console.WriteLine(processPath);
                IntPtr handle = IntPtr.Zero;
                if (!CurrentProcessWindowHandleStackDictionary.ContainsKey(processPath)) {
                    //if process is not running start it 
                    handle = MpHelpers.Instance.StartProcess(string.Empty, processPath, isAdmin, false);
                } else {
                    //ensure the process has a handle matching isAdmin, if not it needs to be created
                    var handleList = CurrentProcessWindowHandleStackDictionary[processPath];
                    foreach (var h in handleList) {
                        if (isAdmin == MpHelpers.Instance.IsProcessAdmin(h)) {
                            handle = h;
                            break;
                        }
                    }
                    if (handle == IntPtr.Zero) {
                        //no handle found matching admin rights
                        handle = MpHelpers.Instance.StartProcess(string.Empty,processPath, isAdmin, false);
                    }
                }
                WinApi.SetActiveWindow(handle);
                return handle;
            }
            catch (Exception ex) {
                Console.WriteLine("MpRunningApplicationManager.SetActiveApplication error: " + ex.ToString());
                return IntPtr.Zero;
            }
        }
        #endregion

        #region Private Methods
        private MpRunningApplicationManager() {
            foreach (KeyValuePair<IntPtr, string> window in OpenWindowGetter.GetOpenWindows()) {                
                UpdateHandleStack(window.Key);
            }
            Console.WriteLine("RunningApplicationManager Initialized w/ contents: ");
            Console.WriteLine(this.ToString());
        }

        private string GetKnownProcessPath(IntPtr handle) {
            foreach (var kvp in CurrentProcessWindowHandleStackDictionary) {
                if (kvp.Value.Contains(handle)) {
                    return kvp.Key;
                }
            }
            return null;
        }
        #endregion

        #region Overrides
        public override string ToString() {
            var outStr = string.Empty;
            foreach (var handleStack in CurrentProcessWindowHandleStackDictionary) {
                outStr += handleStack.Key + Environment.NewLine;
                foreach (var handle in handleStack.Value) {
                    outStr += "\t" + handle.ToInt32() + Environment.NewLine;
                }
            }
            return outStr;
        }
        #endregion

        #region Commands

        #endregion

        #region INotifyPropertyChanged 
        public bool ThrowOnInvalidPropertyName { get; private set; }

        private event PropertyChangedEventHandler _propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }

        public virtual void OnPropertyChanged(string propertyName) {
            this.VerifyPropertyName(propertyName);
            PropertyChangedEventHandler handler = _propertyChanged;
            if (handler != null) {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName) {
            // Verify that the property name matches a real, 
            // public, instance property on this object. 
            if (TypeDescriptor.GetProperties(this)[propertyName] == null) {
                string msg = "Invalid property name: " + propertyName;
                if (this.ThrowOnInvalidPropertyName) {
                    throw new Exception(msg);
                } else {
                    Debug.Fail(msg);
                }
            }
        }
        #endregion
    }

    /// <summary>Contains functionality to get all the open windows.</summary>
    public static class OpenWindowGetter {
        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static IDictionary<HWND, string> GetOpenWindows() {
            HWND shellWindow = GetShellWindow();
            Dictionary<HWND, string> windows = new Dictionary<HWND, string>();

            EnumWindows(delegate (HWND hWnd, int lParam) {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }

        private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();
    }
}
