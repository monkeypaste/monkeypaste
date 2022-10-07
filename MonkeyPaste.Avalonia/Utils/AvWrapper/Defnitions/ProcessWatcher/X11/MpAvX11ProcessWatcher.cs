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

        private IntPtr _displayPtr;
        private Window _rootWindow;
        
        private object _lockObj = new object();

        #endregion

        public override void SetActiveProcess(IntPtr handle) {
            throw new NotImplementedException();
        }
        public override IntPtr GetParentHandleAtPoint(MpPoint poIntPtr) {
            throw new NotImplementedException();
        }


        public override bool IsHandleRunningProcess(IntPtr handle) {
            throw new NotImplementedException();
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

        private Dictionary<string, List<string>> GetRunningAppsWrapper() {
            var runningApps = new Dictionary<string, List<string>>();

            string winHandleStr = @"ps -o pid=".Bash();
            var winHandles = winHandleStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string winHandle in winHandles) {
                if(winHandle.IsStringNullOrWhiteSpace()) {
                    continue;
                }
                int handleInt = 0;

                try {
                    handleInt = int.Parse(winHandle);
                }catch(FormatException ex) {
                    MpConsole.WriteTraceLine($"Error parsing x11 handle: '{winHandle}'", ex);

                }
                if(handleInt == 0) {
                    continue;
                }
                //MpConsole.WriteLine("WindowHandleStr: " + winHandle + " Int: "+handleInt);
                string processPathsStr = $"ps -q {winHandle} -o cmd=".Bash();
                //MpConsole.WriteLine("Window Paths: " + processPathsStr);
                if (!processPathsStr.IsNullOrEmpty()) {
                    var processPaths = processPathsStr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (processPaths != null && processPaths.Length > 0) {
                        string processPath = processPaths[0];
                        //MpConsole.WriteLine("Found path: " + processPath);
                        //MpConsole.WriteLine("Parsed Handle Ptr: " + winHandle);
                        if (runningApps.TryGetValue(processPath.ToLower(), out var handles)) {
                            handles.Add(winHandle);
                            runningApps[processPath.ToLower()] = handles;
                        } else {
                            runningApps.TryAdd(processPath.ToLower(), new() { winHandle });
                        }
                    }
                }
            }
            return runningApps;
        }

        protected override Tuple<string, string, IntPtr> RefreshRunningProcessLookup() {
            lock (_lockObj) {
                string activePidStr = "xdotool getactivewindow getwindowpid".Bash();
                IntPtr activeIntPtr = IntPtr.Zero;
                string activeWindowTitle = null;

                if(!activePidStr.IsStringNullOrWhiteSpace()) {
                    try {
                        activeIntPtr = new IntPtr(int.Parse(activePidStr));
                        if(activeIntPtr != IntPtr.Zero) {
                            activeWindowTitle = "xdotool getactivewindow getwindowname".Bash();
                        }
                    }catch(Exception ex) {
                        MpConsole.WriteTraceLine(ex);
                        activeIntPtr = IntPtr.Zero;
                    }
                } 
                Tuple<string, string, IntPtr>? activeAppTuple = _lastProcessTuple;


                var filteredApps = GetRunningAppsWrapper();
                var refreshedPaths = new List<string>();

                foreach (var runningApp in filteredApps) {
                    foreach(var handle in runningApp.Value) {
                        IntPtr handlePtr = new IntPtr(int.Parse(handle));

                        ObservableCollection<IntPtr> handles = null;
                        if (RunningProcessLookup.TryGetValue(runningApp.Key.ToLower(), out handles)) {
                            // application is already known
                            if (!handles.Contains(handlePtr)) {
                                //handle is new
                                handles.Add(handlePtr);
                            }

                        } else {
                            // new process found
                            handles = new() { handlePtr };
                            RunningProcessLookup.TryAdd(runningApp.Key.ToLower(), handles);
                        }
                        if (handles == null) {
                            Debugger.Break();
                        }
                        if (handlePtr == activeIntPtr) {
                            // handle is active
                            // if (activeAppTuple != null) {
                            //     Debugger.Break();
                            // } else {
                                int runningAppIdx = handles.IndexOf(handlePtr);
                                handles.Move(runningAppIdx, 0);

                                activeAppTuple = new Tuple<string, string, IntPtr>(
                                    runningApp.Key.ToLower(),
                                    activeWindowTitle,
                                    handlePtr);
                            //}
                        }

                        RunningProcessLookup[runningApp.Key.ToLower()] = handles;

                        if (!refreshedPaths.Contains(runningApp.Key.ToLower())) {
                            refreshedPaths.Add(runningApp.Key.ToLower());
                        }
                    }
                    
                }

                //remove apps that were terminated
                var appsToRemove = RunningProcessLookup.Where(x => !refreshedPaths.Contains(x.Key.ToLower()));
                for (int i = 0; i < appsToRemove.Count(); i++) {
                    RunningProcessLookup.TryRemove(appsToRemove.ElementAt(i));
                }

                return activeAppTuple;
            }
        }
        
    }
}
