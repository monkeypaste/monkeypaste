using MonkeyPaste;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MpProcessHelper {
    public static class MpProcessManager {
        #region Private Variables
        private static System.Timers.Timer _timer;

        //private static MpIconBuilder _ib;

        private static string fallback;
        private static ObservableCollection<string> _knownAppPaths;
        #endregion

        #region Properties

        public static ConcurrentDictionary<string, List<IntPtr>> CurrentProcessWindowHandleStackDictionary { get; set; } = new ConcurrentDictionary<string, List<IntPtr>>();

        public static ConcurrentDictionary<IntPtr, WinApi.ShowWindowCommands> LastWindowStateHandleDictionary = new ConcurrentDictionary<IntPtr, WinApi.ShowWindowCommands>();

        public static string ActiveProcessPath { get; set; } = string.Empty;

        public static string LastTitle { get; set; }
                

        public static IntPtr LastHandle { get; private set; }

        public static string LastProcessPath => GetProcessPath(LastHandle);

        #endregion

        #region Public Methods

        public static void Init(string fallbackProcessPath, string[] knownAppPaths, MpIconBuilderBase iconBuilder) {
            Task.Run(async () => {
                fallback = fallbackProcessPath;
                _knownAppPaths = new ObservableCollection<string>(knownAppPaths);

                LastHandle = IntPtr.Zero;
                RefreshHandleStack();

                //this loop is needed at app start so new/unknown apps are stored in db
                var handleLookup = CurrentProcessWindowHandleStackDictionary.ToArray();
                foreach (var kvp in CurrentProcessWindowHandleStackDictionary) {
                    if (!_knownAppPaths.Contains(kvp.Key)) {
                        //var iconBmpSrc = MpHelpers.GetIconImage(kvp.Key);
                        //var icon = await MpIcon.Create(iconBmpSrc.ToBase64String());
                        //string appName = MpHelpers.GetProcessApplicationName(kvp.Value[0]);
                        //var app = await MpApp.Create(kvp.Key, appName, icon);
                        _knownAppPaths.Add(kvp.Key);

                        // this will notify main application of new app found
                        await MpAppBuilder.Build(kvp.Value[0]);
                    }
                }

                if (_timer == null) {
                    _timer = new System.Timers.Timer(500);
                    _timer.Elapsed += Timer_Elapsed;
                } else {
                    _timer.Stop();
                }
                _timer.Start();
            });
        }

        public static void Stop() {
            _timer?.Stop();
        }

        public static string GetProcessMainWindowTitle(IntPtr hWnd) {
            try {
                if (hWnd == null || hWnd == IntPtr.Zero) {
                    return "Unknown Application";
                }
                //uint processId;
                //WinApi.GetWindowThreadProcessId(hWnd, out processId);
                //using (Process proc = Process.GetProcessById((int)processId)) {
                //    return proc.MainWindowTitle;
                //}
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

        public static bool IsProcessAdmin(IntPtr handle) {
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
                MonkeyPaste.MpConsole.WriteLine("IsProcessAdmin error: " + ex.ToString());
                return true;
            }
        }

        public static int GetShowWindowValue(WinApi.ShowWindowCommands cmd) {
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

        public static string GetProcessPath(IntPtr hwnd, string fallback = @"C:\WINDOWS\Explorer.EXE") {
            try {
                if (hwnd == null || hwnd == IntPtr.Zero) {
                    return fallback; //fallback;
                }

                WinApi.GetWindowThreadProcessId(hwnd, out uint pid);
                using (Process proc = Process.GetProcessById((int)pid)) {
                    // TODO when user clicks eye (to hide it) icon on running apps it should add to a string[] pref
                    // and if it contains proc.ProcessName return fallback (so choice persists
                    if (proc.ProcessName == @"csrss") {
                        //occurs with messageboxes and dialogs
                        return fallback; //fallback;
                    }
                    if (proc.MainWindowHandle == IntPtr.Zero) {
                        return fallback; //fallback;
                    }
                    return proc.MainModule.FileName.ToString();
                }
            }
            catch (Exception e) {
                MonkeyPaste.MpConsole.WriteLine("MpHelpers.GetProcessPath error (likely) cannot find process path (w/ Handle " + hwnd.ToString() + ") : " + e.ToString());
                //return GetExecutablePathAboveVista(hwnd);
                return fallback; //fallback;
            }
        }

        public static string GetProcessPath(IntPtr hwnd) {
            try {
                if (hwnd == null || hwnd == IntPtr.Zero) {
                    return fallback;
                }

                WinApi.GetWindowThreadProcessId(hwnd, out uint pid);
                using (Process proc = Process.GetProcessById((int)pid)) {
                    if (proc.ProcessName == @"csrss") {
                        //occurs with messageboxes and dialogs
                        return fallback;
                    }
                    if (proc.MainWindowHandle == IntPtr.Zero) {
                        return fallback;
                    }
                    return proc.MainModule.FileName.ToString();
                }
            }
            catch (Exception e) {
                MonkeyPaste.MpConsole.WriteLine("GetProcessPath error (likely) cannot find process path (w/ Handle " + hwnd.ToString() + ") : " + e.ToString());
                //return GetExecutablePathAboveVista(hwnd);
                return fallback;
            }
        }

        public static string GetMainModuleFilepath(int processId) {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString)) {
                using (var results = searcher.Get()) {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null) {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }


        public static string GetProcessApplicationName(IntPtr hWnd) {
            string mwt = GetProcessMainWindowTitle(hWnd);
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

        #endregion

        #region Private Methods

        private static string GetExecutablePathAboveVista(IntPtr dwProcessId) {
            StringBuilder buffer = new StringBuilder(1024);
            IntPtr hprocess = WinApi.OpenProcess(WinApi.ProcessAccessFlags.QueryLimitedInformation, false, (int)dwProcessId);
            if (hprocess != IntPtr.Zero) {
                try {
                    int size = buffer.Capacity;
                    if (WinApi.QueryFullProcessImageName(hprocess, 0, buffer, ref size)) {
                        return buffer.ToString(0, size);
                    }
                }
                finally {
                    WinApi.CloseHandle(hprocess);
                }
            }
            return string.Empty;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            IntPtr currentHandle = WinApi.GetForegroundWindow();

            RefreshHandleStack();
            bool hasChanged = LastHandle != currentHandle;

            LastHandle = currentHandle;
            LastTitle = GetProcessMainWindowTitle(LastHandle);

            UpdateHandleStack(LastHandle);

            string processName = GetProcessPath(LastHandle);
            if (!_knownAppPaths.Contains(processName)) {
                //var iconBmpSrc = _iconLoader.GetIconImage(processName);
                //var icon = await MpIcon.Create(iconBmpSrc.ToBase64String());
                //var app = await MpApp.Create(processName, MpHelpers.GetProcessApplicationName(LastHandle), icon);
                _knownAppPaths.Add(processName);
                // this will notify main application of new app found
                Task.Run(async () => {
                    await MpAppBuilder.Build(LastHandle);
                });
            }

            if (hasChanged) {
                MonkeyPaste.MpConsole.WriteLine(string.Format(@"Last Window: {0} ({1})", GetProcessMainWindowTitle(LastHandle), LastHandle));
            }
        }

        private static void RefreshHandleStack() {
            lock (CurrentProcessWindowHandleStackDictionary) {
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

                            var placement = WinApi.GetPlacement(handle);
                            if (placement.showCmd == WinApi.ShowWindowCommands.Minimized || placement.showCmd == WinApi.ShowWindowCommands.Hide) {
                                return;
                            }
                            if (LastWindowStateHandleDictionary.ContainsKey(handle)) {
                                LastWindowStateHandleDictionary.TryRemove(handle, out _);
                            }
                            LastWindowStateHandleDictionary.TryAdd(handle, placement.showCmd);
                            //MonkeyPaste.MpConsole.WriteLine(@"Last Window State for " + processStack.Key + " was " + Enum.GetName(typeof(WinApi.ShowWindowCommands), placement.showCmd));
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
                    CurrentProcessWindowHandleStackDictionary.TryRemove(processToRemove, out _);
                    wasStackChanged = true;

                    //MonkeyPaste.MpConsole.WriteLine(string.Format(@"Process: {0} REMOVED", processToRemove));
                }
                foreach (var handleToRemove in toRemoveHandleKeyValueList) {
                    if (CurrentProcessWindowHandleStackDictionary.ContainsKey(handleToRemove.Key)) {
                        //remove individual window handles that were flagged
                        CurrentProcessWindowHandleStackDictionary[handleToRemove.Key].Remove(handleToRemove.Value);
                        wasStackChanged = true;
                        //MonkeyPaste.MpConsole.WriteLine(string.Format(@"Process: {0} Handle: {1} REMOVED", handleToRemove.Key, handleToRemove.Value));
                    }
                    if (LastWindowStateHandleDictionary.ContainsKey(handleToRemove.Value)) {
                        LastWindowStateHandleDictionary.TryRemove(handleToRemove.Value, out _);
                    }
                }
                if (wasStackChanged) {
                    //OnPropertyChanged(nameof(CurrentProcessWindowHandleStackDictionary));

                }
            }
        }

        private static void UpdateHandleStack(IntPtr fgHandle) {
            lock (CurrentProcessWindowHandleStackDictionary) {
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
                if (CurrentProcessWindowHandleStackDictionary.ContainsKey(processName)) {
                    //if process is already being tracked 
                    if (CurrentProcessWindowHandleStackDictionary[processName].Contains(fgHandle)) {
                        //remove the handle if it is also being tracked
                        CurrentProcessWindowHandleStackDictionary[processName].Remove(fgHandle);
                    }
                    //set fg handle to the top of its process list
                    CurrentProcessWindowHandleStackDictionary[processName].Insert(0, fgHandle);
                    wasStackChanged = true;
                    ActiveProcessPath = processName;
                    //MonkeyPaste.MpConsole.WriteLine(string.Format(@"(Known) Process: {0} Handle:{1} ACTIVE", processName, fgHandle));
                } else {
                    //if its a new process create a new list with this handle as its element
                    CurrentProcessWindowHandleStackDictionary.TryAdd(processName, new List<IntPtr> { fgHandle });
                    wasStackChanged = true;
                    ActiveProcessPath = processName;

                    //MonkeyPaste.MpConsole.WriteLine(string.Format(@"(New) Process: {0} Handle:{1} ACTIVE", processName, fgHandle));
                }
                //}

                if (wasStackChanged) {
                    //OnPropertyChanged(nameof(CurrentProcessWindowHandleStackDictionary));

                    var placement = WinApi.GetPlacement(fgHandle);
                    if (placement.showCmd == WinApi.ShowWindowCommands.Minimized || placement.showCmd == WinApi.ShowWindowCommands.Hide) {
                        return;
                    }
                    if (LastWindowStateHandleDictionary.ContainsKey(fgHandle)) {
                        LastWindowStateHandleDictionary.TryRemove(fgHandle, out _);
                    }
                    try {
                        LastWindowStateHandleDictionary.TryAdd(fgHandle, placement.showCmd);
                    }
                    catch (Exception ex) {
                        //intermittenly fgHandle is still in dictionary so hopefully this swallows exception
                        MonkeyPaste.MpConsole.WriteTraceLine($"FgHandle: {fgHandle} already exists...ignoring", ex);
                    }
                    //MonkeyPaste.MpConsole.WriteLine(@"Last Window State for " + processName + " was " + Enum.GetName(typeof(WinApi.ShowWindowCommands), placement.showCmd));
                }
            }
        }

        private static string GetKnownProcessPath(IntPtr handle) {
            foreach (var kvp in CurrentProcessWindowHandleStackDictionary) {
                if (kvp.Value.Contains(handle)) {
                    return kvp.Key;
                }
            }
            return null;
        }

        #endregion
    }
}
