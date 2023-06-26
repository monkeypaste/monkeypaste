using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Management;
using System.Linq;
using System.ComponentModel;
using System.Security.Principal;
#if WINDOWS
using MonkeyPaste.Common.Wpf;
using static MonkeyPaste.Common.Wpf.WinApi;

#endif

namespace MonkeyPaste.Avalonia {
    public class MpAvWin32ProcessWatcher : MpAvProcessWatcherBase {
        private string[] _ignoredProcessNames = new string[] {
            "csrss",
            "dwm",
            "mmc"
        };

        private string _FallbackProcessPath = @"C:\WINDOWS\Explorer.EXE";
        public bool IsThisAppAdmin { get; private set; } = false;

        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public override IntPtr GetParentHandleAtPoint(MpPoint p) {
#if WINDOWS
            // Get the window/control that the mouse is hovering over...
            IntPtr hwnd = WinApi.WindowFromPoint(new System.Drawing.Point((int)p.X, (int)p.Y));
            if (hwnd == IntPtr.Zero) {
                return IntPtr.Zero;
            }
            // Continue to get the parent until we reach the top-level window (with parent of NULL)...
            while (true) {
                IntPtr p_hwnd = WinApi.GetParent(hwnd);
                if (p_hwnd == IntPtr.Zero) {
                    return hwnd;
                }
                hwnd = p_hwnd;
            }
#else
            return IntPtr.Zero;
#endif
        }

        public override IntPtr SetActiveProcess(IntPtr handle) {
#if WINDOWS
            if (handle == IntPtr.Zero) {
                MpConsole.WriteLine("Warning cannot set active process to IntPtr.Zero, ignoring");
                return IntPtr.Zero;
            }
            //if (!MpAvMainView.Instance.IsVisible) {
            //    MpConsole.WriteLine("Warning cannot set active process mw is not visible");
            //    return IntPtr.Zero;
            //}
            IntPtr lastActive = WinApi.SetActiveWindow(ThisAppHandle);
            bool success = WinApi.SetForegroundWindow(handle);
            MpConsole.WriteLine($"SetForegroundWindow to '{handle}' from '{lastActive}' was {(success ? "SUCCESSFUL" : "FAILED")}");
            return lastActive;
#else
            return IntPtr.Zero;
#endif
        }
        public override IntPtr SetActiveProcess(IntPtr handle, ProcessWindowStyle windowStyle) {
#if WINDOWS
            if (!WinApi.ShowWindowAsync(handle, GetShowWindowState(windowStyle))) {
                MpConsole.WriteLine($"ShowWindowAsync failed for handle '{handle}' with window state '{windowStyle}'");
            }
            return SetActiveProcess(handle);
#else
            return IntPtr.Zero;
#endif
        }

        public override bool IsAdmin(object handleIdOrTitle) {
            if (handleIdOrTitle is IntPtr handle) {
                return IsProcessAdmin(handle);
            }
            throw new NotImplementedException();
        }
        public override ProcessWindowStyle GetWindowStyle(object handleIdOrTitle) {
#if WINDOWS
            IntPtr handle = IntPtr.Zero;
            if (handleIdOrTitle is IntPtr) {
                handle = (IntPtr)handleIdOrTitle;
            } else {
                throw new NotImplementedException();
            }
            ShowWindowCommands swc = (ShowWindowCommands)GetWindowLong(handle, GWL_STYLE);
            switch (swc) {
                case ShowWindowCommands.Hide:
                    return ProcessWindowStyle.Hidden;

                case ShowWindowCommands.Minimized:
                    return ProcessWindowStyle.Minimized;

                case ShowWindowCommands.Maximized:
                    return ProcessWindowStyle.Maximized;

                default:
                    return ProcessWindowStyle.Normal;
            }
#else
            return ProcessWindowStyle.Normal;
#endif
        }
        public override MpPortableProcessInfo GetActiveProcessInfo() {
#if WINDOWS
            IntPtr active_handle = WinApi.GetForegroundWindow();
            var active_info = new MpPortableProcessInfo() {
                Handle = active_handle,
                ProcessPath = GetProcessPath(active_handle),
                MainWindowTitle = GetProcessApplicationName(active_handle)
            };
            return active_info;
#else
            return null;
#endif
        }
        public override string GetProcessTitle(IntPtr hWnd) {
#if WINDOWS
            try {
                if (hWnd == IntPtr.Zero) {
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
#else
            return null;
#endif
        }

        public override Process GetProcess(object handleIdOrTitle) {
#if WINDOWS
            if (handleIdOrTitle is IntPtr handle) {
                GetWindowThreadProcessId(handle, out uint pid);
                return base.GetProcess((int)pid);
            }
            return base.GetProcess(handleIdOrTitle);
#else
            return null;
#endif
        }
        public override string GetProcessPath(IntPtr hWnd) {

            string fallback = _FallbackProcessPath;

            if (hWnd == IntPtr.Zero) {
                return fallback; //fallback;
            }
            var getMeths = new Func<IntPtr, string>[] {
                GetProcessPath1,
                GetProcessPath2,
                GetProcessPath3,
                GetProcessPath4,
            };

            foreach (var getMeth in getMeths) {
                try {
                    string process_path = getMeth(hWnd);
                    if (!string.IsNullOrEmpty(process_path)) {
                        return process_path;
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"GetProcessPath error on method #{getMeths.IndexOf(getMeth)}.", ex);
                }
            }

            return fallback;

        }

        #endregion

        #region Protected Methods

        protected override MpPortableProcessInfo RefreshRunningProcessLookup() {
#if WINDOWS
            lock (RunningProcessLookup) {
                //called in LastWindowWatcher's timer to remove closed window handles and processes
                IntPtr active_handle = WinApi.GetForegroundWindow();
                MpPortableProcessInfo activeProcessInfo = null;

                var toRemoveProcessNameList = new List<string>();
                var toRemoveHandleKeyValueList = new List<KeyValuePair<string, IntPtr>>();
                foreach (var processStack in RunningProcessLookup) {
                    //loop through all known processes
                    bool isProcessTerminated = true;
                    foreach (var handle in processStack.Value) {
                        //loop through all known handles to that process
                        if (WinApi.IsWindow(handle)) {

                            if (handle == active_handle) {
                                if (activeProcessInfo != null) {
                                    // should only be set once how come?
                                    MpDebug.Break();
                                }
                                var cur_info = new MpPortableProcessInfo() {
                                    Handle = handle,
                                    ProcessPath = GetProcessPath(handle),
                                    ProcessName = GetProcessApplicationName(handle),
                                    MainWindowTitle = GetProcessTitle(handle),
                                };
                                activeProcessInfo = cur_info;
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

                return activeProcessInfo;
            }
#else
            return null;
#endif
        }
        protected override void CreateRunningProcessLookup() {
            // get lookup of all window handles by process path
            var pkvp = GetOpenWindows();
            foreach (var kvp in pkvp) {
                // migrate lookup to concurrent dictionary
                if (RunningProcessLookup.ContainsKey(kvp.Key)) {
                    RunningProcessLookup[kvp.Key].Add(kvp.Value);
                } else {
                    RunningProcessLookup.TryAdd(kvp.Key, new ObservableCollection<IntPtr> { kvp.Value });
                }
            }
            RefreshRunningProcessLookup();
        }
        #endregion

        #region Private Methods
        #region Helpers

        private string GetProcessPath1(IntPtr hWnd) {
#if WINDOWS
            StringBuilder sb = new StringBuilder(2000);

            /** Need to get the process ID from handle under cursor **/
            GetWindowThreadProcessId(hWnd, out uint pid);

            /** Hook into process **/
            IntPtr pic = OpenProcess(ProcessAccessFlags.All, true, (int)pid);

            /** This gets the filename of the process image. Path is in device format **/
            GetProcessImageFileName(pic, sb, 2000);

            return MpWpfDevicePathMapper.FromDevicePath(sb.ToString());
#else
            return null;
#endif
        }

        private string GetProcessPath2(IntPtr hwnd) {
#if WINDOWS
            GetWindowThreadProcessId(hwnd, out uint pid);
            string Query = "SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = " + pid;

            using (ManagementObjectSearcher mos = new ManagementObjectSearcher(Query)) {
                using (ManagementObjectCollection moc = mos.Get()) {
                    if (moc.Count == 0) {
                        return null;
                    }
                    var proc_record = (from mo in moc.Cast<ManagementObject>() select mo["ExecutablePath"]).FirstOrDefault();
                    if (proc_record is string path) {
                        return path;
                    }
                }
            }
#endif
            return null;

        }
        private string GetProcessPath3(IntPtr hwnd) {
#if WINDOWS
            GetWindowThreadProcessId(hwnd, out uint pid);
            int capacity = 1024;
            StringBuilder sb = new StringBuilder(capacity);
            IntPtr handle = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, pid);
            QueryFullProcessImageName(handle, 0, sb, ref capacity);
            string fullPath = sb.ToString();

            return fullPath;
#else
            return null;
#endif
        }
        private string GetProcessPath4(IntPtr hwnd) {
#if WINDOWS
            GetWindowThreadProcessId(hwnd, out uint pid);
            using (Process proc = Process.GetProcessById((int)pid)) {
                // TODO when user clicks eye (to hide it) icon on running apps it should add to a string[] pref
                // and if it contains proc.ProcessName return fallback (so choice persists
                if (_ignoredProcessNames.Contains(proc.ProcessName.ToLower())) {
                    //occurs with messageboxes and dialogs
                    //MpConsole.WriteTraceLine($"Active process '{proc.ProcessName}' is on ignored list, using fallback '{fallback}'");
                    return null;
                }
                if (proc.MainWindowHandle == IntPtr.Zero) {
                    return null; //null;
                }


                if (!Environment.Is64BitProcess && Is64Bit(proc)) {
                    return null;
                }

                bool isProcElevated = IsProcessAdmin(proc.MainWindowHandle);

                if (!IsThisAppAdmin && isProcElevated) {
                    return null;
                }

                return proc.MainModule.FileName.ToString().ToLower();
            }
#else
            return null;
#endif

        }

        private int GetShowWindowState(ProcessWindowStyle pws) {
#if WINDOWS
            switch (pws) {
                case ProcessWindowStyle.Hidden:
                    return (int)ShowWindowCommands.Hide;

                case ProcessWindowStyle.Minimized:
                    return (int)ShowWindowCommands.Minimized;

                case ProcessWindowStyle.Maximized:
                    return (int)ShowWindowCommands.Maximized;

                default:
                    return (int)ShowWindowCommands.Normal;
            }
#else
            return (int)ProcessWindowStyle.Normal;
#endif
        }

        private IDictionary<string, IntPtr> GetOpenWindows() {
#if WINDOWS
            IntPtr shellWindow = GetShellWindow();
            Dictionary<string, IntPtr> windows = new Dictionary<string, IntPtr>();

            EnumWindows(delegate (IntPtr hWnd, int lParam) {
                if (hWnd == shellWindow) {
                    return true;
                }
                if (!IsWindowVisible(hWnd)) {
                    return true;
                }

                try {
                    GetWindowThreadProcessId(hWnd, out uint pid);
                    var process = Process.GetProcessById((int)pid);
                    if (process.MainWindowHandle == IntPtr.Zero) {
                        return true;
                    }

                    int length = WinApi.GetWindowTextLength(hWnd);

                    if (length == 0 || !WinApi.IsWindow(hWnd)) {
                        return true;
                    }

                    //if(MpHelpers.IsThisAppAdmin()) {
                    //    process.WaitForInputIdle(100);
                    //}

                    //StringBuilder builder = new StringBuilder(length);
                    //WinApi.GetWindowText(hWnd, builder, length + 1);
                    string process_path = GetProcessPath(hWnd);
                    if (string.IsNullOrEmpty(process_path)) {
                        return true;
                    }
                    windows.AddOrReplace(process_path, hWnd);
                }
                catch (InvalidOperationException ex) {
                    // no graphical interface
                    MpConsole.WriteLine("OpenWindowGetter, ignoring non GUI window w/ error: " + ex.ToString());
                }

                return true;

            }, 0);

            return windows;
#else
            return null;
#endif
        }

        private void UpdateHandleStack(IntPtr fgHandle) {
            lock (RunningProcessLookup) {
                //check if this handle is already be tracked
                string processName = GetKnownProcessPath(fgHandle);
                if (string.IsNullOrEmpty(processName)) {
                    //if it is not resolve its process path
                    processName = GetProcessPath(fgHandle);
                }
                if (processName == null) {
                    return;
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
#if WINDOWS
            if (!Environment.Is64BitOperatingSystem) {
                return false;
            }
            // if this method is not available in your version of .NET, use GetNativeSystemInfo via P/Invoke instead

            bool isWow64;
            if (!WinApi.IsWow64Process(process.Handle, out isWow64))
                throw new Win32Exception();
            return !isWow64;
#else
            return false;
#endif

        }

        private bool IsProcessAdmin(IntPtr handle) {
#if WINDOWS
            if (handle == IntPtr.Zero) {
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
#else
            return false;
#endif
        }

        #endregion

        #endregion

        #region Commands
        #endregion
    }
}
