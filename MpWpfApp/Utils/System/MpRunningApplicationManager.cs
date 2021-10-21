using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;
using System.Collections.Concurrent;

namespace MpWpfApp {
    public class MpRunningApplicationManager : MpViewModelBase<object> {
        private static readonly Lazy<MpRunningApplicationManager> _Lazy = new Lazy<MpRunningApplicationManager>(() => new MpRunningApplicationManager());
        public static MpRunningApplicationManager Instance { get { return _Lazy.Value; } }

        #region Private Variables

        #endregion

        #region Properties
        private ConcurrentDictionary<string, List<IntPtr>> _currentProcessWindowHandleStackDictionary = new ConcurrentDictionary<string, List<IntPtr>>();
        public ConcurrentDictionary<string, List<IntPtr>> CurrentProcessWindowHandleStackDictionary {
            get {
                return _currentProcessWindowHandleStackDictionary;
            }
        }

        public ConcurrentDictionary<IntPtr,WinApi.ShowWindowCommands> LastWindowStateHandleDictionary = new ConcurrentDictionary<IntPtr, WinApi.ShowWindowCommands>();


        public string ActiveProcessPath { get; set; } = string.Empty;
        #endregion

        #region Constructors

        private MpRunningApplicationManager() : base(null) {
            foreach (KeyValuePair<IntPtr, string> window in MpOpenWindowGetter.GetOpenWindows()) {
                //MonkeyPaste.MpConsole.WriteLine("Window Title: " + window.Value);
                UpdateHandleStack(window.Key);
            }
            //MonkeyPaste.MpConsole.WriteLine("RunningApplicationManager Initialized w/ contents: ");
            //MonkeyPaste.MpConsole.WriteLine(this.ToString());
        }

        #endregion

        #region Public Methods
        public void Init() {
            //empty to initialize singleton
        }

        public void RefreshHandleStack() {
            lock(CurrentProcessWindowHandleStackDictionary) {
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

        public void UpdateHandleStack(IntPtr fgHandle) {
            lock(CurrentProcessWindowHandleStackDictionary) {
                //check if this handle is already be tracked
                string processName = GetKnownProcessPath(fgHandle);
                if (string.IsNullOrEmpty(processName)) {
                    //if it is not resolve its process path
                    processName = MpHelpers.Instance.GetProcessPath(fgHandle);
                }
                if (processName == MpHelpers.Instance.GetApplicationProcessPath()) {
                    return;
                }
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

        public IntPtr SetActiveProcess(
            string processPath, 
            bool isAdmin, 
            bool isSilent = false, 
            string args = "", 
            object forceHandle = null, 
            WinApi.ShowWindowCommands forceWindowState = WinApi.ShowWindowCommands.Maximized) {
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
                //MonkeyPaste.MpConsole.WriteLine(processPath);

                //forceHandle is only passed when its a running application
                IntPtr handle = forceHandle == null ? IntPtr.Zero : (IntPtr)forceHandle;
                if (handle != IntPtr.Zero || !CurrentProcessWindowHandleStackDictionary.ContainsKey(processPath)) {
                    //if process is not running anymore or needs to be started (custom pastetoapppath)
                    handle = MpHelpers.Instance.StartProcess(args, processPath, isAdmin, isSilent, forceWindowState);
                } else {
                    //ensure the process has a handle matching isAdmin, if not it needs to be created
                    var handleList = CurrentProcessWindowHandleStackDictionary[processPath];
                    foreach (var h in handleList) {
                        if (isAdmin == MpHelpers.Instance.IsProcessAdmin(h)) {
                            handle = h;
                            if (LastWindowStateHandleDictionary.ContainsKey(handle)) {
                                forceWindowState = LastWindowStateHandleDictionary[handle];
                            }
                            break;
                        }
                    }
                    if (handle == IntPtr.Zero) {
                        //no handle found matching admin rights
                        handle = MpHelpers.Instance.StartProcess(args,processPath, isAdmin, isSilent, forceWindowState);
                    } else {
                        //show running window with last known window state
                        WinApi.ShowWindowAsync(handle, MpHelpers.Instance.GetShowWindowValue(forceWindowState));
                    }
                }
                
                return handle;
            }
            catch (Exception) {
                //MonkeyPaste.MpConsole.WriteLine("MpRunningApplicationManager.SetActiveApplication error: " + ex.ToString());
                return IntPtr.Zero;
            }
        }
        #endregion

        #region Private Methods

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
    }

    
}
