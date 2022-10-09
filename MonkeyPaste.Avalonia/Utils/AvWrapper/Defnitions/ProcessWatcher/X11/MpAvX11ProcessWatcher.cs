using System;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Threading;
//using Gio;
//using Gtk;
//using GLib;
//using Gdk;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
//using Avalonia.Gtk3;
using X11;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Avalonia {
    
    public class MpAvX11ProcessWatcher : MpAvProcessWatcherBase {
        #region Private Variables

        private string[] _requiredTools = new string[] {
            "xdotool"
        };
        
        private object _lockObj = new object();

        private Tuple<string, string, IntPtr> _lastActiveTuple;

        private List<string> _errorWindows = new List<string>();
        
        #endregion

        #region Properties

        public override bool CanWatchProcesses() {
            string xdotoolPath = "command -v xdotool".ShellExec();
            return !string.IsNullOrEmpty(xdotoolPath);
        }

        #endregion

        #region Constructors

        #endregion

        #region MpIProcessWatcher Overrides

        public override IntPtr GetParentHandleAtPoint(MpPoint poIntPtr) {
            return IntPtr.Zero;
        }

        public override void SetActiveProcess(IntPtr handle) {
            int handle_val = handle.ToInt32();
            $"xdotool windowactivate {handle_val}".ShellExecAsync().FireAndForgetSafeAsync();
        }
        
        protected override void CreateRunningProcessLookup() {
            var runningApps = GetRunningAppsWrapper();
            if (runningApps == null) {
                return;
            }
            foreach (var kvp in runningApps) {
                foreach (var handle in kvp.Value) {
                    IntPtr handlePtr = new IntPtr(int.Parse(handle));
                    if (!RunningProcessLookup.ContainsKey(kvp.Key)) {
                        RunningProcessLookup.TryAdd(kvp.Key, new() { handlePtr });
                    } else if (RunningProcessLookup.TryGetValue(kvp.Key, out var handles)) {
                        handles.Add(handlePtr);
                        RunningProcessLookup[kvp.Key] = handles;
                    }
                }
            }
        }

        protected override Tuple<string, string, IntPtr> RefreshRunningProcessLookup() {
            lock (_lockObj) {
                string activeWindow = "xdotool getactivewindow".ShellExec().Trim();
                if(_errorWindows.Contains(activeWindow)) {
                    // window reports error when accessing pid, i think this slows system down
                    return null;
                }

                string activePidStr = "xdotool getactivewindow getwindowpid".ShellExec().Trim();
                IntPtr activeIntPtr = IntPtr.Zero;
                string activeWindowTitle = null;
                string xdotool_error = "has no pid associated with it.";

                if (!activePidStr.IsStringNullOrWhiteSpace()) {
                    if(activePidStr.Contains(xdotool_error)) {
                        // window handle was found but no pid, note this to avoid rechecking since it errors
                        _errorWindows.Add(activeWindow);
                        return null;
                    }
                    try {
                        activeIntPtr = new IntPtr(int.Parse(activePidStr));
                        if (activeIntPtr != IntPtr.Zero) {
                            if(_lastActiveTuple != null && _lastActiveTuple.Item3 == activeIntPtr) {
                                return _lastActiveTuple;
                            }

                            activeWindowTitle = "xdotool getactivewindow getwindowname".ShellExec().Trim();
                            string activeWindowPath = $"ps -q {activePidStr} -o cmd=".ShellExec().Trim();
                            if(!string.IsNullOrEmpty(activeWindowPath)) {
                                _lastActiveTuple = new Tuple<string, string, IntPtr>(
                                    activeWindowPath,
                                    activeWindowTitle,
                                    activeIntPtr);
                                return _lastActiveTuple;
                            }
                        }
                    }
                    catch (Exception) {
                        //MpConsole.WriteTraceLine(ex);
                        activeIntPtr = IntPtr.Zero;
                    }
                }
                return null;

                // Tuple<string, string, IntPtr>? activeAppTuple = _lastProcessTuple;


                // var filteredApps = GetRunningAppsWrapper();
                // var refreshedPaths = new List<string>();

                // foreach (var runningApp in filteredApps) {
                //     foreach (var handle in runningApp.Value) {
                //         IntPtr handlePtr = new IntPtr(int.Parse(handle));

                //         ObservableCollection<IntPtr> handles = null;
                //         if (RunningProcessLookup.TryGetValue(runningApp.Key.ToLower(), out handles)) {
                //             // application is already known
                //             if (!handles.Contains(handlePtr)) {
                //                 //handle is new
                //                 handles.Add(handlePtr);
                //             }

                //         } else {
                //             // new process found
                //             handles = new() { handlePtr };
                //             RunningProcessLookup.TryAdd(runningApp.Key.ToLower(), handles);
                //         }
                //         if (handles == null) {
                //             Debugger.Break();
                //         }
                //         if (handlePtr == activeIntPtr) {
                //             // handle is active
                //             // if (activeAppTuple != null) {
                //             //     Debugger.Break();
                //             // } else {
                //             int runningAppIdx = handles.IndexOf(handlePtr);
                //             handles.Move(runningAppIdx, 0);

                //             activeAppTuple = new Tuple<string, string, IntPtr>(
                //                 runningApp.Key.ToLower(),
                //                 activeWindowTitle,
                //                 handlePtr);
                //         }

                //         RunningProcessLookup[runningApp.Key.ToLower()] = handles;

                //         if (!refreshedPaths.Contains(runningApp.Key.ToLower())) {
                //             refreshedPaths.Add(runningApp.Key.ToLower());
                //         }
                //     }

                // }

                // //remove apps that were terminated
                // var appsToRemove = RunningProcessLookup.Where(x => !refreshedPaths.Contains(x.Key.ToLower()));
                // for (int i = 0; i < appsToRemove.Count(); i++) {
                //     RunningProcessLookup.TryRemove(appsToRemove.ElementAt(i));
                // }

                // return activeAppTuple;
            }
        }
        
        #endregion

        #region Helpers

        private Dictionary<string, List<string>> GetRunningAppsWrapper() {
            // if(MpAvX11ProcessWatcher_xlib.IsXDisplayAvailable()) {
            //     return MpAvX11ProcessWatcher_xlib.GetRunningApps();
            // }
            return MpAvX11ProcessWatcher_shell.GetRunningApps();
        }
        #endregion
        
    }
}
