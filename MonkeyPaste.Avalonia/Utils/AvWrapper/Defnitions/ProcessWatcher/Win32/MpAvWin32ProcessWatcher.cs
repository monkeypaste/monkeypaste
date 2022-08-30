using System;
using System.Collections.Concurrent;
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
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvWin32ProcessWatcher : MpIProcessWatcher {
        #region Private Variables
        private DispatcherTimer _timer;

        private IntPtr _thisAppHandle;
        public IntPtr ThisAppHandle {
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

        //private string thisAppExe = Path.Combine(System.Windows.Forms.Application.StartupPath, "MpWpfApp.exe");
        //private MpIconBuilder _ib;

        private string _FallbackProcessPath = @"C:\WINDOWS\Explorer.EXE";

        public string[] _ignoredProcessNames = new string[] {
            "csrss",
            "dwm",
            "mmc"
        };

        #endregion

        #region Properties

        public ConcurrentDictionary<string, ObservableCollection<IntPtr>> RunningProcessLookup { get; private set; } = new ConcurrentDictionary<string, ObservableCollection<IntPtr>>();

        public ConcurrentDictionary<IntPtr, WinApi.ShowWindowCommands> RunningProcessWindowStateLookup { get; private set; } = new ConcurrentDictionary<IntPtr, WinApi.ShowWindowCommands>();

        public string ActiveProcessPath { get; set; } = string.Empty;
        public string LastMainWindowTitle { get; private set; }
        public string LastTitle { get; set; }

        public IntPtr LastHandle { get; private set; }

        public string LastProcessPath => GetProcessPath(LastHandle);

        public bool IsThisAppAdmin { get; private set; } = false;
        #endregion

        #region Events

        public event EventHandler<MpProcessActivatedEventArgs> OnAppActivated;

        #endregion

        #region Public Methods

        public MpAvWin32ProcessWatcher() {
            LastHandle = IntPtr.Zero;
            var pkvp = GetOpenWindows();
            foreach (var kvp in pkvp) {
                if (RunningProcessLookup.ContainsKey(kvp.Key)) {
                    RunningProcessLookup[kvp.Key].Add(kvp.Value);
                } else {
                    RunningProcessLookup.TryAdd(kvp.Key, new ObservableCollection<IntPtr> { kvp.Value });
                }
                RunningProcessWindowStateLookup.TryAdd(kvp.Value, WinApi.GetPlacement(kvp.Value).showCmd);
            }
            RefreshHandleStack();

            if (_timer == null) {
                _timer = new DispatcherTimer() {
                    Interval = TimeSpan.FromMilliseconds(500)
                };
                _timer.Tick += Timer_Elapsed;
            } else {
                _timer.Stop();
            }
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        public void SetActiveProcess(IntPtr handle) {
            WinApi.SetActiveWindow(handle);
        }
        public IntPtr GetParentHandleAtPoint(MpPoint point) {
            //System.Drawing.Point p = new System.Drawing.Point((int)point.X, (int)point.Y);
            // Get the window/control that the mouse is hovering over...
            IntPtr hwnd = IntPtr.Zero; //WinApi.WindowFromPoint(p);
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
        }

        public void Stop() {
            _timer?.Stop();
        }

        public string GetProcessMainWindowTitle(IntPtr hWnd) {
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

        public int GetShowWindowValue(WinApi.ShowWindowCommands cmd) {
            int winType = 0;
            switch (cmd) {
                case WinApi.ShowWindowCommands.Normal:
                    winType = WinApi.Windows.NORMAL;
                    break;
                case WinApi.ShowWindowCommands.Maximized:
                    winType = WinApi.Windows.MAXIMIXED;
                    break;
                case WinApi.ShowWindowCommands.Minimized:
                case WinApi.ShowWindowCommands.Hide:
                    winType = WinApi.Windows.HIDE;
                    break;
                default:
                    winType = WinApi.Windows.NORMAL;
                    break;
            }
            return winType;
        }

        public string GetProcessPath(IntPtr hwnd) {
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

        public Process GetProcessByHandle(IntPtr handle) {
            if (handle == null || handle == IntPtr.Zero) {
                return null;
            }
            try {
                WinApi.GetWindowThreadProcessId(handle, out uint pid);
                return Process.GetProcessById((int)pid);
            }
            catch (Exception e) {
                MpConsole.WriteTraceLine("Cannot find process path (w/ Handle " + handle.ToString() + ") : " + e.ToString(), e);
                //return GetExecutablePathAboveVista(hwnd);
                return null; //fallback;
            }
        }

        public bool IsHandleChildOfMainWindow(IntPtr handle) {
            if (handle == IntPtr.Zero) {
                return false;
            }
            if (handle == ThisAppHandle) {
                return true;
            }

            var childProcess = GetProcessByHandle(handle);
            if (childProcess == null) {
                return false;
            }
            IntPtr parentHandle = WinApi.GetParent(handle);
            while (parentHandle != null && parentHandle != IntPtr.Zero) {
                if (parentHandle == ThisAppHandle) {
                    return true;
                }
                parentHandle = WinApi.GetParent(parentHandle);
            }
            return false;
        }

        public string GetProcessApplicationName(string windowTitle) {
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

        public string GetProcessApplicationName(IntPtr hWnd) {
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

        public IntPtr GetThisApplicationMainWindowHandle() {
            return ThisAppHandle;
            //if(!CurrentProcessWindowHandleStackDictionary.ContainsKey(thisAppExe) ||
            //    CurrentProcessWindowHandleStackDictionary[thisAppExe].Count == 0) {
            //    return IntPtr.Zero;
            //} 
            //return CurrentProcessWindowHandleStackDictionary[thisAppExe][0];
        }

        public IntPtr GetLastActiveInstance(string processPath) {
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

        public bool IsHandleRunningProcess(IntPtr handle) {
            if (handle == null || handle == IntPtr.Zero) {
                return false;
            }
            return RunningProcessLookup.Any(x => x.Value.Contains(handle));
        }

        public bool IsProcessRunning(string processPath) {
            return GetLastActiveInstance(processPath) != IntPtr.Zero;
        }

        public bool Is64Bit(Process process) {
            if (!Environment.Is64BitOperatingSystem)
                return false;
            // if this method is not available in your version of .NET, use GetNativeSystemInfo via P/Invoke instead

            bool isWow64;
            if (!WinApi.IsWow64Process(process.Handle, out isWow64))
                throw new Win32Exception();
            return !isWow64;
        }



        #endregion

        #region Private Methods

        private string GetExecutablePathAboveVista(IntPtr dwProcessId) {
            StringBuilder buffer = new StringBuilder(1024);
            IntPtr hprocess = WinApi.OpenProcess(WinApi.ProcessAccessFlags.QueryLimitedInformation, false, (int)dwProcessId);
            if (hprocess != IntPtr.Zero) {
                try {
                    int size = buffer.Capacity;
                    if (WinApi.QueryFullProcessImageName(hprocess, 0, buffer, ref size)) {
                        return buffer.ToString(0, size).ToLower();
                    }
                }
                finally {
                    WinApi.CloseHandle(hprocess);
                }
            }
            return string.Empty;
        }

        private void Timer_Elapsed(object sender, EventArgs e) {
            IntPtr currentHandle = WinApi.GetForegroundWindow();
            if (currentHandle == ThisAppHandle) {
                return;
            }
            RefreshHandleStack();
            bool hasChanged = LastHandle != currentHandle;

            LastHandle = currentHandle;
            LastTitle = GetProcessMainWindowTitle(LastHandle);

            UpdateHandleStack(LastHandle);

            string processPath = GetProcessPath(LastHandle);

            if (hasChanged) {
                MpConsole.WriteLine(string.Format(@"Last Window: {0} '{1}' ({2})", LastTitle, LastProcessPath, LastHandle));

                OnAppActivated?.Invoke(
                    nameof(MpAvWin32ProcessWatcher),
                    new MpProcessActivatedEventArgs() {
                        ProcessPath = processPath,
                        ApplicationName = GetProcessApplicationName(LastTitle),
                        Handle = LastHandle
                    });
            }
        }

        private void RefreshHandleStack() {
            lock (RunningProcessLookup) {
                //called in LastWindowWatcher's timer to remove closed window handles and processes
                var toRemoveProcessNameList = new List<string>();
                var toRemoveHandleKeyValueList = new List<KeyValuePair<string, IntPtr>>();
                foreach (var processStack in RunningProcessLookup) {
                    //loop through all known processes
                    bool isProcessTerminated = true;
                    foreach (var handle in processStack.Value) {
                        //loop through all known handles to that process
                        if (WinApi.IsWindow(handle)) {
                            //verify that the processes window handle is still running
                            isProcessTerminated = false;

                            var placement = WinApi.GetPlacement(handle);
                            if (placement.showCmd == WinApi.ShowWindowCommands.Minimized || placement.showCmd == WinApi.ShowWindowCommands.Hide) {
                                //return;
                                continue;
                            }
                            if (RunningProcessWindowStateLookup.ContainsKey(handle)) {
                                RunningProcessWindowStateLookup.TryRemove(handle, out _);
                            }
                            RunningProcessWindowStateLookup.TryAdd(handle, placement.showCmd);
                            //MpConsole.WriteLine(@"Last Window State for " + processStack.Key + " was " + Enum.GetName(typeof(WinApi.ShowWindowCommands), placement.showCmd));
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
                    if (RunningProcessWindowStateLookup.ContainsKey(handleToRemove.Value)) {
                        RunningProcessWindowStateLookup.TryRemove(handleToRemove.Value, out _);
                    }
                }
                if (wasStackChanged) {
                    //OnPropertyChanged(nameof(CurrentProcessWindowHandleStackDictionary));

                }
            }
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
                bool wasStackChanged = false;
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
                    wasStackChanged = true;
                    ActiveProcessPath = processName;
                    //MpConsole.WriteLine(string.Format(@"(Known) Process: {0} Handle:{1} ACTIVE", processName, fgHandle));
                } else {
                    //if its a new process create a new list with this handle as its element
                    RunningProcessLookup.TryAdd(processName, new ObservableCollection<IntPtr> { fgHandle });
                    wasStackChanged = true;
                    ActiveProcessPath = processName;

                    //MpConsole.WriteLine(string.Format(@"(New) Process: {0} Handle:{1} ACTIVE", processName, fgHandle));
                }
                //}

                if (wasStackChanged) {
                    //OnPropertyChanged(nameof(CurrentProcessWindowHandleStackDictionary));

                    var placement = WinApi.GetPlacement(fgHandle);
                    if (placement.showCmd == WinApi.ShowWindowCommands.Minimized || placement.showCmd == WinApi.ShowWindowCommands.Hide) {
                        return;
                    }
                    if (RunningProcessWindowStateLookup.ContainsKey(fgHandle)) {
                        RunningProcessWindowStateLookup.TryRemove(fgHandle, out _);
                    }
                    try {
                        RunningProcessWindowStateLookup.TryAdd(fgHandle, placement.showCmd);
                    }
                    catch (Exception ex) {
                        //intermittenly fgHandle is still in dictionary so hopefully this swallows exception
                        MpConsole.WriteLine($"FgHandle: {fgHandle} already exists...ignoring " + ex);
                    }
                    //MpConsole.WriteLine(@"Last Window State for " + processName + " was " + Enum.GetName(typeof(WinApi.ShowWindowCommands), placement.showCmd));
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

        #endregion
    }
}
