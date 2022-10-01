using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvWin32ProcessWatcher : MpAvProcessWatcherBase {
        private string[] _ignoredProcessNames = new string[] {
            "csrss",
            "dwm",
            "mmc"
        };

        private string _FallbackProcessPath = @"C:\WINDOWS\Explorer.EXE";

        private IntPtr _thisAppHandle;
        public override IntPtr ThisAppHandle {
            get => _thisAppHandle;
            set {
                if (_thisAppHandle != value) {
                    _thisAppHandle = value;
                    if (_thisAppHandle != IntPtr.Zero) {
                        IsThisAppAdmin = IsProcessAdmin(ThisAppHandle);
                    }
                }
            }
        }

        public bool IsThisAppAdmin { get; private set; } = false;
        public override IntPtr GetParentHandleAtPoint(MpPoint p) {
            // Get the window/control that the mouse is hovering over...
            IntPtr hwnd = WinApi.WindowFromPoint(new System.Drawing.Point((int)p.X,(int)p.Y));
            if (hwnd == null || hwnd == IntPtr.Zero) {
                return IntPtr.Zero;
            }
            // Continue to get the parent until we reach the top-level window (with parent of NULL)...
            while (true) {
                IntPtr p_hwnd = WinApi.GetParent(hwnd);
                if (p_hwnd == null || p_hwnd == IntPtr.Zero) {
                    return hwnd;
                }
                hwnd = p_hwnd;
            }
        }

        public override void SetActiveProcess(IntPtr handle) {
            //WinApi.SetActiveWindow(handle);
            bool success = WinApi.SetForegroundWindow(handle);
            MpConsole.WriteLine($"SetForegroundWindow to '{handle}' was {(success ? "SUCCESSFUL" : "FAILED")}");
        }

        protected override Tuple<string, string, IntPtr> RefreshRunningProcessLookup() {
            lock (RunningProcessLookup) {
                //called in LastWindowWatcher's timer to remove closed window handles and processes
                IntPtr active_handle = WinApi.GetForegroundWindow();
                Tuple<string, string, IntPtr>? activeAppTuple = null;

                var toRemoveProcessNameList = new List<string>();
                var toRemoveHandleKeyValueList = new List<KeyValuePair<string, IntPtr>>();
                foreach (var processStack in RunningProcessLookup) {
                    //loop through all known processes
                    bool isProcessTerminated = true;
                    foreach (var handle in processStack.Value) {
                        //loop through all known handles to that process
                        if (WinApi.IsWindow(handle)) {
                            var curTuple = new Tuple<string, string, IntPtr>(
                                    GetProcessPath(handle),
                                    GetProcessApplicationName(handle),
                                    handle);

                            if (handle == active_handle) {
                                if(activeAppTuple != null) {
                                    // should only be set once how come?
                                    Debugger.Break();
                                }
                                activeAppTuple = curTuple;
                            }
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
                    RunningProcessLookup.TryRemove(processToRemove, out _);
                    wasStackChanged = true;

                    //MpConsole.WriteLine(string.Format(@"Process: {0} REMOVED", processToRemove));
                }
                foreach (var handleToRemove in toRemoveHandleKeyValueList) {
                    if (RunningProcessLookup.ContainsKey(handleToRemove.Key)) {
                        //remove individual window handles that were flagged
                        RunningProcessLookup[handleToRemove.Key].Remove(handleToRemove.Value);
                        wasStackChanged = true;
                        //MpConsole.WriteLine(string.Format(@"Process: {0} Handle: {1} REMOVED", handleToRemove.Key, handleToRemove.Value));
                    }
                }
                if (wasStackChanged) {
                    //OnPropertyChanged(nameof(CurrentProcessWindowHandleStackDictionary));

                }
                UpdateHandleStack(active_handle);

                return activeAppTuple;
            }
        }

        public override string GetProcessMainWindowTitle(IntPtr hWnd) {
            try {
                if (hWnd == null || hWnd == IntPtr.Zero) {
                    return "Unknown Application";
                }
                int length = WinApi.GetWindowTextLength(hWnd);
                if (length == 0) {
                    return string.Empty;
                }

                StringBuilder builder = new StringBuilder(length);
                WinApi.GetWindowText(hWnd, builder, length + 1);
                return builder.ToString();
            }
            catch (Exception ex) {
                return "MpHelpers.GetProcessMainWindowTitle Exception: " + ex.ToString();
            }
        }

        public override string GetProcessApplicationName(IntPtr hWnd) {
            string mwTitle = GetProcessMainWindowTitle(hWnd);
            string appName = GetProcessApplicationName(mwTitle);

            if (string.IsNullOrWhiteSpace(appName) || appName.HasSpecialCharacters()) {
                // NOTE trying to enforce app name to not be empty or end up
                // being file name when window title is normal pattern
                string processPath = GetProcessPath(hWnd);
                return Path.GetFileName(processPath);
            }
            return appName;
        }

        public override string GetProcessPath(IntPtr hwnd) {
            string fallback = _FallbackProcessPath;
            try {
                if (hwnd == null || hwnd == IntPtr.Zero) {
                    return fallback; //fallback;
                }

                WinApi.GetWindowThreadProcessId(hwnd, out uint pid);
                using (Process proc = Process.GetProcessById((int)pid)) {
                    // TODO when user clicks eye (to hide it) icon on running apps it should add to a string[] pref
                    // and if it contains proc.ProcessName return fallback (so choice persists
                    if (_ignoredProcessNames.Contains(proc.ProcessName.ToLower())) {
                        //occurs with messageboxes and dialogs
                        MpConsole.WriteTraceLine($"Active process '{proc.ProcessName}' is on ignored list, using fallback '{fallback}'");
                        return fallback; //fallback;
                    }
                    if (proc.MainWindowHandle == IntPtr.Zero) {
                        return fallback; //fallback;
                    }


                    if (!Environment.Is64BitProcess && Is64Bit(proc)) {
                        return fallback;
                    }

                    bool isProcElevated = IsProcessAdmin(proc.MainWindowHandle);

                    if (!IsThisAppAdmin && isProcElevated) {
                        return fallback;
                    }

                    try {
                        return proc.MainModule.FileName.ToString().ToLower();
                    }
                    catch (InvalidOperationException) {
                        return fallback;
                    }

                }
            }
            catch (Exception e) {
                MpConsole.WriteTraceLine("Cannot find process path (w/ Handle " + hwnd.ToString() + ") : " + e.ToString(), e);
                //return GetExecutablePathAboveVista(hwnd);
                return fallback; //fallback;
            }
        }

        public override IntPtr GetLastActiveInstance(string processPath) {
            if (string.IsNullOrWhiteSpace(processPath)) {
                return IntPtr.Zero;
            }
            processPath = processPath.ToLower();
            if (RunningProcessLookup.ContainsKey(processPath) &&
               RunningProcessLookup[processPath].Count > 0) {
                return RunningProcessLookup[processPath][0];
            }
            return IntPtr.Zero;
        }

        public override bool IsHandleRunningProcess(IntPtr handle) {
            if (handle == null || handle == IntPtr.Zero) {
                return false;
            }
            return RunningProcessLookup.Any(x => x.Value.Contains(handle));
        }

        protected override void CreateRunningProcessLookup() {
            var pkvp = GetOpenWindows();
            foreach (var kvp in pkvp) {
                if (RunningProcessLookup.ContainsKey(kvp.Key)) {
                    RunningProcessLookup[kvp.Key].Add(kvp.Value);
                } else {
                    RunningProcessLookup.TryAdd(kvp.Key, new ObservableCollection<IntPtr> { kvp.Value });
                }
            }
            RefreshRunningProcessLookup();
        }

        #region Helpers

        private IDictionary<string, IntPtr> GetOpenWindows() {
            IntPtr shellWindow = WinApi.GetShellWindow();
            Dictionary<string, IntPtr> windows = new Dictionary<string, IntPtr>();

            WinApi.EnumWindows(delegate (IntPtr hWnd, int lParam) {
                if (hWnd == shellWindow) {
                    return true;
                }
                if (!WinApi.IsWindowVisible(hWnd)) {
                    return true;
                }

                try {
                    WinApi.GetWindowThreadProcessId(hWnd, out uint pid);
                    var process = Process.GetProcessById((int)pid);
                    if (process.MainWindowHandle == IntPtr.Zero) {
                        return true;
                    }

                    int length = WinApi.GetWindowTextLength(hWnd);
                    if (length == 0 || !WinApi.IsWindow(hWnd)) return true;

                    //if(MpHelpers.IsThisAppAdmin()) {
                    //    process.WaitForInputIdle(100);
                    //}

                    //StringBuilder builder = new StringBuilder(length);
                    //WinApi.GetWindowText(hWnd, builder, length + 1);

                    windows.AddOrReplace(GetProcessPath(hWnd), hWnd);
                }
                catch (InvalidOperationException ex) {
                    // no graphical interface
                    MpConsole.WriteLine("OpenWindowGetter, ignoring non GUI window w/ error: " + ex.ToString());
                }

                return true;

            }, 0);

            return windows;
        }

        private void UpdateHandleStack(IntPtr fgHandle) {
            lock (RunningProcessLookup) {
                //check if this handle is already be tracked
                string processName = GetKnownProcessPath(fgHandle);
                if (string.IsNullOrEmpty(processName)) {
                    //if it is not resolve its process path
                    processName = GetProcessPath(fgHandle);
                }
                //if (processName == fallback) {
                //    return;
                //}
                //bool wasStackChanged = false;
                processName = processName.ToLower();
                //lock (CurrentProcessWindowHandleStackDictionary) {
                if (RunningProcessLookup.ContainsKey(processName)) {
                    //if process is already being tracked 
                    if (RunningProcessLookup[processName].Contains(fgHandle)) {
                        //remove the handle if it is also being tracked
                        RunningProcessLookup[processName].Remove(fgHandle);
                    }
                    //set fg handle to the top of its process list
                    RunningProcessLookup[processName].Insert(0, fgHandle);
                    //wasStackChanged = true;
                    //MpConsole.WriteLine(string.Format(@"(Known) Process: {0} Handle:{1} ACTIVE", processName, fgHandle));
                } else {
                    //if its a new process create a new list with this handle as its element
                    RunningProcessLookup.TryAdd(processName, new ObservableCollection<IntPtr> { fgHandle });
                    //wasStackChanged = true;
                }
            }
        }

        private string GetKnownProcessPath(IntPtr handle) {
            foreach (var kvp in RunningProcessLookup) {
                if (kvp.Value.Contains(handle)) {
                    return kvp.Key.ToLower();
                }
            }
            return null;
        }

        private bool Is64Bit(Process process) {
            if (!Environment.Is64BitOperatingSystem)
                return false;
            // if this method is not available in your version of .NET, use GetNativeSystemInfo via P/Invoke instead

            bool isWow64;
            if (!WinApi.IsWow64Process(process.Handle, out isWow64))
                throw new Win32Exception();
            return !isWow64;
        }
        private string GetProcessApplicationName(string windowTitle) {
            string mwt = windowTitle;
            if (string.IsNullOrEmpty(mwt)) {
                return mwt;
            }
            var mwta = mwt.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if (mwta.Length == 1) {
                if (string.IsNullOrEmpty(mwta[0])) {
                    return "Explorer";
                }
                return mwta[0];
            }
            return mwta[mwta.Length - 1].Trim();
        }

        public bool IsProcessAdmin(IntPtr handle) {
            if (handle == null || handle == IntPtr.Zero) {
                return false;
            }
            try {
                WinApi.GetWindowThreadProcessId(handle, out uint pid);
                using (Process proc = Process.GetProcessById((int)pid)) {
                    IntPtr ph = IntPtr.Zero;
                    WinApi.OpenProcessToken(proc.Handle, WinApi.TOKEN_ALL_ACCESS, out ph);
                    WindowsIdentity iden = new WindowsIdentity(ph);
                    bool result = false;

                    foreach (IdentityReference role in iden.Groups) {
                        if (role.IsValidTargetType(typeof(SecurityIdentifier))) {
                            SecurityIdentifier sid = role as SecurityIdentifier;
                            if (sid.IsWellKnown(WellKnownSidType.AccountAdministratorSid) || sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid)) {
                                result = true;
                                break;
                            }
                        }
                    }
                    WinApi.CloseHandle(ph);
                    return result;
                }
            }
            catch (Exception ex) {
                //if app is started using "Run as" is if you get "Access Denied" error. 
                //That means that running app has rights that your app does not have. 
                //in this case ADMIN rights
                MpConsole.WriteLine("IsProcessAdmin error: " + ex.ToString());
                return true;
            }
        }

        #endregion
    }
}
