using MonkeyPaste;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace MpProcessHelper {
    public class MpAppBuilder : MonkeyPaste.MpIAppBuilder {

        public async Task<MpApp> Build(object handleInfo, MpIProcessIconBuilder pib) {
            object result = await Build(new object[] { handleInfo, pib });
            return result == null ? null : result as MpApp;
        }

        public async Task<MpApp> Build(object args) {
            object result = await Build(args as object[]);
            return result == null ? null : result as MpApp;
        }

        public async Task<object> Build(object[] args) {
            object handleInfo = args[0];
            MpIProcessIconBuilder pib = args[1] as MpIProcessIconBuilder;

            if (handleInfo == null || handleInfo.GetType() != typeof(IntPtr)) {
                return null;
            }
            IntPtr hWnd = (IntPtr)handleInfo;
            string appPath = MpProcessManager.GetProcessPath(hWnd);
            string appName = GetProcessApplicationName(appPath);

            var iconStr = pib.GetBase64BitmapFromFilePath(appPath);
            var icon = await MpIcon.Create(iconStr);
            var app = await MpApp.Create(appPath, appName, icon);

            return app;
        }

        public string GetProcessApplicationName(object handleInfo) {
            if (handleInfo == null || handleInfo.GetType() != typeof(IntPtr)) {
                return null;
            }
            IntPtr hWnd = (IntPtr)handleInfo;
            string mwt = MpProcessManager.GetProcessMainWindowTitle(hWnd);
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
    }

    public class MpProcessManager  {

        #region Private Variables
        private System.Timers.Timer _timer;

        private string fallback;
        private ObservableCollection<string> _knownAppPaths;
        private MpProcessIconBuilder _iconLoader;
        #endregion

        #region Properties

        public ConcurrentDictionary<string, List<IntPtr>> CurrentProcessWindowHandleStackDictionary { get; set; } = new ConcurrentDictionary<string, List<IntPtr>>();

        public ConcurrentDictionary<IntPtr, WinApi.ShowWindowCommands> LastWindowStateHandleDictionary = new ConcurrentDictionary<IntPtr, WinApi.ShowWindowCommands>();

        public string ActiveProcessPath { get; set; } = string.Empty;

        public string LastTitle { get; set; }

        public IntPtr LastHandle { get; private set; }

        #endregion

        #region Events

              
        #endregion

        public MpProcessManager(MpIIconBuilder ib) {
            Task.Run(async () => {
                var al = await MpDb.Instance.GetItemsAsync<MpApp>();
                Start(
                    @"C:\WINDOWS\Explorer.EXE",
                    al.Select(x => x.AppPath).ToArray(),
                    ib);
            });
        }

        public async void Start(string fallbackProcessPath, string[] knownAppPaths, MpIIconBuilder iconBuilder) {
            //fallback is used when cannot find path from handle
            _iconLoader = new MpProcessIconBuilder(iconBuilder);

            fallback = fallbackProcessPath;
            _knownAppPaths = new ObservableCollection<string>(knownAppPaths);

            LastHandle = IntPtr.Zero;
            RefreshHandleStack();

            //this loop is needed at app start so new/unknown apps are stored in db
            var handleLookup = CurrentProcessWindowHandleStackDictionary.ToArray();
            foreach (var kvp in CurrentProcessWindowHandleStackDictionary) {
                if (!_knownAppPaths.Contains(kvp.Key)) {
                    //var iconBmpSrc = MpHelpers.Instance.GetIconImage(kvp.Key);
                    //var icon = await MpIcon.Create(iconBmpSrc.ToBase64String());
                    //string appName = MpHelpers.Instance.GetProcessApplicationName(kvp.Value[0]);
                    //var app = await MpApp.Create(kvp.Key, appName, icon);
                    _knownAppPaths.Add(kvp.Key);

                    // this will notify main application of new app found
                    await new MpAppBuilder().Build(kvp.Value[0], _iconLoader);
                }
            }

            if(_timer == null) {
                _timer = new System.Timers.Timer(500);
                _timer.Elapsed += Timer_Elapsed;
            } else {
                _timer.Stop();
            }
            _timer.Start();
        }

        public void Stop() {
            _timer?.Stop();
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            IntPtr currentHandle = WinApi.GetForegroundWindow();

            RefreshHandleStack();

            LastHandle = currentHandle;
            LastTitle = GetProcessMainWindowTitle(LastHandle);

            UpdateHandleStack(LastHandle);

            string processName = GetProcessPath(LastHandle);
            if (processName.ToLower().Contains("powershell")) {
                Debugger.Break();
            }
            if (!_knownAppPaths.Contains(processName)) {
                //var iconBmpSrc = _iconLoader.GetIconImage(processName);
                //var icon = await MpIcon.Create(iconBmpSrc.ToBase64String());
                //var app = await MpApp.Create(processName, MpHelpers.Instance.GetProcessApplicationName(LastHandle), icon);
                _knownAppPaths.Add(processName);
                // this will notify main application of new app found
                await new MpAppBuilder().Build(LastHandle, _iconLoader);
            }

            MonkeyPaste.MpConsole.WriteLine(string.Format(@"Last Window: {0} ({1})", GetProcessMainWindowTitle(LastHandle), LastHandle));
        }

        

        private void RefreshHandleStack() {
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

        private void UpdateHandleStack(IntPtr fgHandle) {
            lock (CurrentProcessWindowHandleStackDictionary) {
                //check if this handle is already be tracked
                string processName = GetKnownProcessPath(fgHandle);
                if (string.IsNullOrEmpty(processName)) {
                    //if it is not resolve its process path
                    processName = GetProcessPath(fgHandle);
                }
                //if (processName == GetApplicationProcessPath()) {
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

        private string GetKnownProcessPath(IntPtr handle) {
            foreach (var kvp in CurrentProcessWindowHandleStackDictionary) {
                if (kvp.Value.Contains(handle)) {
                    return kvp.Key;
                }
            }
            return null;
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

        public static string GetProcessPath(IntPtr hwnd, string fallback = @"C:\WINDOWS\Explorer.EXE") {
            try {
                if (hwnd == null || hwnd == IntPtr.Zero) {
                    return fallback; //GetApplicationProcessPath();
                }

                WinApi.GetWindowThreadProcessId(hwnd, out uint pid);
                using (Process proc = Process.GetProcessById((int)pid)) {
                    // TODO when user clicks eye (to hide it) icon on running apps it should add to a string[] pref
                    // and if it contains proc.ProcessName return fallback (so choice persists
                    if (proc.ProcessName == @"csrss") {
                        //occurs with messageboxes and dialogs
                        return fallback; //GetApplicationProcessPath();
                    }
                    if (proc.MainWindowHandle == IntPtr.Zero) {
                        return fallback; //GetApplicationProcessPath();
                    }
                    return proc.MainModule.FileName.ToString();
                }
            }
            catch (Exception e) {
                MonkeyPaste.MpConsole.WriteLine("MpHelpers.Instance.GetProcessPath error (likely) cannot find process path (w/ Handle " + hwnd.ToString() + ") : " + e.ToString());
                //return GetExecutablePathAboveVista(hwnd);
                return fallback; //GetApplicationProcessPath();
            }
        }

        
    }

    public class MpProcessExtensions {

    }

}
