using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace MpProcessHelper {
    public class MpProcessActivatedEventArgs : EventArgs {
        public string ProcessPath { get; set; }
        public string ApplicationName { get; set; }
        public IntPtr Handle { get; set; }
    }

    public static class MpProcessManager {
        #region Private Variables
        private static System.Timers.Timer _timer;

        public static IntPtr ThisAppHandle { get; set; }

        //private static string thisAppExe = Path.Combine(System.Windows.Forms.Application.StartupPath, "MpWpfApp.exe");
        //private static MpIconBuilder _ib;

        private static string _FallbackProcessPath = @"C:\WINDOWS\Explorer.EXE";

        public static string[] _ignoredProcessNames;

        #endregion

        #region Properties

        public static ConcurrentDictionary<string, List<IntPtr>> CurrentProcessWindowHandleStackDictionary { get; private set; } = new ConcurrentDictionary<string, List<IntPtr>>();

        public static ConcurrentDictionary<IntPtr, WinApi.ShowWindowCommands> CurrentWindowStateHandleDictionary { get; private set; } = new ConcurrentDictionary<IntPtr, WinApi.ShowWindowCommands>();

        public static string ActiveProcessPath { get; set; } = string.Empty;

        public static string LastTitle { get; set; }
                
        public static IntPtr LastHandle { get; private set; }

        public static string LastProcessPath => GetProcessPath(LastHandle);

        
        #endregion

        #region Events

        public static event EventHandler<MpProcessActivatedEventArgs> OnAppActivated;

        #endregion

        #region Public Methods

        public static void Init(string ignoredProcessNames) {
            _ignoredProcessNames = ignoredProcessNames.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            LastHandle = IntPtr.Zero;
            var pkvp = GetOpenWindows();
            foreach(var kvp in pkvp) {
                if(CurrentProcessWindowHandleStackDictionary.ContainsKey(kvp.Key)) {
                    CurrentProcessWindowHandleStackDictionary[kvp.Key].Add(kvp.Value);
                } else {
                    CurrentProcessWindowHandleStackDictionary.TryAdd(kvp.Key, new List<IntPtr> { kvp.Value });
                }
                CurrentWindowStateHandleDictionary.TryAdd(kvp.Value, WinApi.GetPlacement(kvp.Value).showCmd);
            }
            RefreshHandleStack();

            if (_timer == null) {
                _timer = new System.Timers.Timer(500);
                _timer.Elapsed += Timer_Elapsed;
            } else {
                _timer.Stop();
            }
            _timer.Start();
        }

        public static IntPtr GetParentHandleAtPoint(System.Drawing.Point p) {
            // Get the window/control that the mouse is hovering over...
            IntPtr hwnd = WinApi.WindowFromPoint(p);
            if(hwnd == null || hwnd == IntPtr.Zero) {
                return IntPtr.Zero;
            }
            // Continue to get the parent until we reach the top-level window (with parent of NULL)...
            while (true) {
                IntPtr p_hwnd = WinApi.GetParent(hwnd); 
                if(p_hwnd == null || p_hwnd == IntPtr.Zero) {
                    return hwnd;
                }
                hwnd = p_hwnd;
            }
        }

        public static void Stop() {
            _timer?.Stop();
        }

        public static string GetProcessMainWindowTitle(IntPtr hWnd) {
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
                MpConsole.WriteLine("IsProcessAdmin error: " + ex.ToString());
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

        public static string GetProcessPath(IntPtr hwnd, string fallback = "") {
            fallback = string.IsNullOrEmpty(fallback) ? _FallbackProcessPath : fallback;
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

                    //bool isThisAppAdmin = UACHelper.UACHelper.IsAdministrator;
                    //bool isProcElevated = UACHelper.UACHelper.IsProcessElevated(proc);

                    //if (!isThisAppAdmin && isProcElevated) {
                    //    return fallback;
                    //}
                    try {
                        return proc.MainModule.FileName.ToString().ToLower();
                    }catch(InvalidOperationException) {
                        return fallback;
                    }
                    
                }
            }
            catch (Exception e) {
                MpConsole.WriteLine("MpHelpers.GetProcessPath error (likely) cannot find process path (w/ Handle " + hwnd.ToString() + ") : " + e.ToString());
                //return GetExecutablePathAboveVista(hwnd);
                return fallback; //fallback;
            }
        }

        public static Process GetProcessByHandle(IntPtr handle) {
            if(handle == null || handle == IntPtr.Zero) {
                return null;
            }
            try {
                WinApi.GetWindowThreadProcessId(handle, out uint pid);
                return Process.GetProcessById((int)pid);
            }
            catch (Exception e) {
                MpConsole.WriteLine("MpHelpers.GetProcessPath error (likely) cannot find process path (w/ Handle " + handle.ToString() + ") : " + e.ToString());
                //return GetExecutablePathAboveVista(hwnd);
                return null; //fallback;
            }
        }

        public static bool IsHandleChildOfMainWindow(IntPtr handle) {
            if(handle == IntPtr.Zero) {
                return false;
            }
            if(handle == ThisAppHandle) {
                return true;
            }

            var childProcess = GetProcessByHandle(handle);
            if(childProcess == null) {
                return false;
            }
            IntPtr parentHandle = WinApi.GetParent(handle);
            while(parentHandle != null && parentHandle != IntPtr.Zero) {
                if(parentHandle == ThisAppHandle) {
                    return true;
                }
                parentHandle = WinApi.GetParent(parentHandle);
            }
            return false;
        }

        public static string GetMainModuleFilepath(int processId) {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString)) {
                using (var results = searcher.Get()) {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null) {
                        return ((string)mo["ExecutablePath"]).ToLower();
                    }
                }
            }
            return null;
        }

        public static string GetProcessApplicationName(string windowTitle) {
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

        public static string GetProcessApplicationName(IntPtr hWnd) {
            string mwTitle = GetProcessMainWindowTitle(hWnd);
            string appName = GetProcessApplicationName(mwTitle);
            
            if(string.IsNullOrWhiteSpace(appName) || appName.HasSpecialCharacters()) {
                // NOTE trying to enforce app name to not be empty or end up
                // being file name when window title is normal pattern
                string processPath = GetProcessPath(hWnd);
                return Path.GetFileName(processPath);
            }
            return appName;
        }

        public static IntPtr GetThisApplicationMainWindowHandle() {
            return ThisAppHandle;
            //if(!CurrentProcessWindowHandleStackDictionary.ContainsKey(thisAppExe) ||
            //    CurrentProcessWindowHandleStackDictionary[thisAppExe].Count == 0) {
            //    return IntPtr.Zero;
            //} 
            //return CurrentProcessWindowHandleStackDictionary[thisAppExe][0];
        }

        public static IntPtr GetLastActiveInstance(string processPath) {
            if(string.IsNullOrWhiteSpace(processPath)) {
                return IntPtr.Zero;
            }
            processPath = processPath.ToLower();
            if(CurrentProcessWindowHandleStackDictionary.ContainsKey(processPath) &&
               CurrentProcessWindowHandleStackDictionary[processPath].Count > 0) {
                return CurrentProcessWindowHandleStackDictionary[processPath][0];
            }
            return IntPtr.Zero;
        }

        public static bool IsHandleRunningProcess(IntPtr handle) {
            if(handle == null || handle == IntPtr.Zero) {
                return false;
            }
            return CurrentProcessWindowHandleStackDictionary.Any(x => x.Value.Contains(handle));
        }

        public static bool IsProcessRunning(string processPath) {
            return GetLastActiveInstance(processPath) != IntPtr.Zero;
        }

        public static bool Is64Bit(Process process) {
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

        public static bool HasSpecialCharacters(this string str) {
            return !Regex.IsMatch(str, "[^a-zA-Z0-9_.]+", RegexOptions.Compiled);
        }
        private static string GetExecutablePathAboveVista(IntPtr dwProcessId) {
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

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            IntPtr currentHandle = WinApi.GetForegroundWindow();
            if(currentHandle == ThisAppHandle) {
                return;
            }
            RefreshHandleStack();
            bool hasChanged = LastHandle != currentHandle;

            LastHandle = currentHandle;
            LastTitle = GetProcessMainWindowTitle(LastHandle);

            UpdateHandleStack(LastHandle);

            string processPath = GetProcessPath(LastHandle);

            if (hasChanged) {
                MpConsole.WriteLine(string.Format(@"Last Window: {0} '{1}' ({2})", LastTitle,LastProcessPath, LastHandle));
                
                OnAppActivated?.Invoke(
                    nameof(MpProcessManager), 
                    new MpProcessActivatedEventArgs() {
                        ProcessPath = processPath,
                        ApplicationName = GetProcessApplicationName(LastTitle),
                        Handle = LastHandle
                    });
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
                                //return;
                                continue;
                            }
                            if (CurrentWindowStateHandleDictionary.ContainsKey(handle)) {
                                CurrentWindowStateHandleDictionary.TryRemove(handle, out _);
                            }
                            CurrentWindowStateHandleDictionary.TryAdd(handle, placement.showCmd);
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
                    CurrentProcessWindowHandleStackDictionary.TryRemove(processToRemove, out _);
                    wasStackChanged = true;

                    //MpConsole.WriteLine(string.Format(@"Process: {0} REMOVED", processToRemove));
                }
                foreach (var handleToRemove in toRemoveHandleKeyValueList) {
                    if (CurrentProcessWindowHandleStackDictionary.ContainsKey(handleToRemove.Key)) {
                        //remove individual window handles that were flagged
                        CurrentProcessWindowHandleStackDictionary[handleToRemove.Key].Remove(handleToRemove.Value);
                        wasStackChanged = true;
                        //MpConsole.WriteLine(string.Format(@"Process: {0} Handle: {1} REMOVED", handleToRemove.Key, handleToRemove.Value));
                    }
                    if (CurrentWindowStateHandleDictionary.ContainsKey(handleToRemove.Value)) {
                        CurrentWindowStateHandleDictionary.TryRemove(handleToRemove.Value, out _);
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
                    //MpConsole.WriteLine(string.Format(@"(Known) Process: {0} Handle:{1} ACTIVE", processName, fgHandle));
                } else {
                    //if its a new process create a new list with this handle as its element
                    CurrentProcessWindowHandleStackDictionary.TryAdd(processName, new List<IntPtr> { fgHandle });
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
                    if (CurrentWindowStateHandleDictionary.ContainsKey(fgHandle)) {
                        CurrentWindowStateHandleDictionary.TryRemove(fgHandle, out _);
                    }
                    try {
                        CurrentWindowStateHandleDictionary.TryAdd(fgHandle, placement.showCmd);
                    }
                    catch (Exception ex) {
                        //intermittenly fgHandle is still in dictionary so hopefully this swallows exception
                        MpConsole.WriteLine($"FgHandle: {fgHandle} already exists...ignoring " + ex);
                    }
                    //MpConsole.WriteLine(@"Last Window State for " + processName + " was " + Enum.GetName(typeof(WinApi.ShowWindowCommands), placement.showCmd));
                }
            }
        }

        private static string GetKnownProcessPath(IntPtr handle) {
            foreach (var kvp in CurrentProcessWindowHandleStackDictionary) {
                if (kvp.Value.Contains(handle)) {
                    return kvp.Key.ToLower();
                }
            }
            return null;
        }

        private static IDictionary<string, IntPtr> GetOpenWindows() {
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
