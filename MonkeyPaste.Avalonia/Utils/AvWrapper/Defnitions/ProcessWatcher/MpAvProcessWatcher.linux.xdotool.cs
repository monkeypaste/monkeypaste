﻿using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

using System;
//using Avalonia.Gtk3;
using System.Collections.Generic;
//using Gio;
//
//using GLib;
//using Gdk;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {

    public partial class MpAvProcessWatcher {
        #region Private Variables

        private string[] _requiredTools = new string[] {
            "xdotool"
        };

        private object _lockObj = new object();

        private MpPortableProcessInfo _lastActiveInfo;

        private List<string> _errorWindows = new List<string>();

        #endregion

        #region Properties

        protected override bool CanWatchProcesses() {
            string xdotoolPath = "command -v xdotool".ShellExec();
            MpConsole.WriteLine("CanWatchProcessOutput: " + xdotoolPath);
            return !string.IsNullOrEmpty(xdotoolPath);
        }

        #endregion

        #region Constructors

        #endregion

        #region MpIProcessWatcher Overrides
        protected override bool IsAdmin(object handleIdOrTitle) {
            throw new NotImplementedException();
        }
        protected override ProcessWindowStyle GetWindowStyle(object handleIdOrTitle) {
            throw new NotImplementedException();
        }
        protected override IntPtr SetActiveProcess(IntPtr handle, ProcessWindowStyle windowStyle) {
            throw new NotImplementedException();
        }
        protected override IntPtr GetParentHandleAtPoint(MpPoint poIntPtr) {
            return IntPtr.Zero;
        }

        protected override IntPtr SetActiveProcess(IntPtr handle) {
            IntPtr lastHandle = GetActiveProcessInfo().Handle;
            int handle_val = handle.ToInt32();
            $"xdotool windowactivate {handle_val}".ShellExec();
            return lastHandle;
        }
        protected override MpPortableProcessInfo GetActiveProcessInfo() {
            var active_info = new MpPortableProcessInfo();
            string activeWindow = "xdotool getfocuswindow".ShellExec().Trim();
            active_info.MainWindowTitle = "xdotool getfocuswindow getwindowname".ShellExec().Trim();
            try {
                active_info.Handle = new IntPtr(int.Parse(activeWindow));

                if (!_errorWindows.Contains(activeWindow)) {
                    string activePidStr = "xdotool getfocuswindow getwindowpid".ShellExec().Trim();

                    if (!activePidStr.IsStringNullOrWhiteSpace()) {
                        string xdotool_error = "has no pid associated with it.";
                        if (activePidStr.Contains(xdotool_error)) {
                            // window handle was found but no pid, note this to avoid rechecking since it errors
                            _errorWindows.Add(activeWindow);
                        } else {
                            try {
                                IntPtr active_pid = new IntPtr(int.Parse(activePidStr));
                                if (active_pid != IntPtr.Zero) {
                                    string active_path_with_args = $"ps -q {activePidStr} -o cmd=".ShellExec().Trim();
                                    if (!string.IsNullOrWhiteSpace(active_path_with_args)) {
                                        string clean_path = MpX11ShellHelpers.GetCleanShellStr(active_path_with_args);
                                        if (!string.IsNullOrWhiteSpace(clean_path)) {
                                            // check parse here
                                            string argParts = active_path_with_args.Substring(clean_path.Length, active_path_with_args.Length - clean_path.Length);
                                            Debugger.Break();
                                            if (!string.IsNullOrWhiteSpace(argParts)) {
                                                active_info.ArgumentList = new List<string>() { argParts };
                                            }
                                            active_info.ProcessPath = clean_path;
                                        }
                                    }
                                }
                            }
                            catch (Exception) {
                                //MpConsole.WriteTraceLine(ex);
                            }
                        }
                    }

                }
            }
            catch (Exception) {
            }
            return active_info;
        }

        protected void CreateRunningProcessLookup() {
            var runningApps = GetRunningAppsWrapper();
            if (runningApps == null) {
                return;
            }
            //foreach (var kvp in runningApps) {
            //    foreach (var handle in kvp.Value) {
            //        IntPtr handlePtr = new IntPtr(int.Parse(handle));
            //        if (!RunningProcessLookup.ContainsKey(kvp.Key)) {
            //            RunningProcessLookup.TryAdd(kvp.Key, new() { handlePtr });
            //        } else if (RunningProcessLookup.TryGetValue(kvp.Key, out var handles)) {
            //            handles.Add(handlePtr);
            //            RunningProcessLookup[kvp.Key] = handles;
            //        }
            //    }
            //}
        }

        protected MpPortableProcessInfo RefreshRunningProcessLookup() {
            lock (_lockObj) {
                string activeWindow = "xdotool getactivewindow".ShellExec().Trim();
                if (_errorWindows.Contains(activeWindow)) {
                    // window reports error when accessing pid, i think this slows system down
                    return null;
                }

                string activePidStr = "xdotool getactivewindow getwindowpid".ShellExec().Trim();
                IntPtr activeIntPtr = IntPtr.Zero;
                string activeWindowTitle = null;
                string xdotool_error = "has no pid associated with it.";

                if (!activePidStr.IsStringNullOrWhiteSpace()) {
                    if (activePidStr.Contains(xdotool_error)) {
                        // window handle was found but no pid, note this to avoid rechecking since it errors
                        _errorWindows.Add(activeWindow);
                        return null;
                    }
                    try {
                        activeIntPtr = new IntPtr(int.Parse(activePidStr));
                        if (activeIntPtr != IntPtr.Zero) {
                            if (_lastActiveInfo != null && _lastActiveInfo.Handle == activeIntPtr) {
                                return _lastActiveInfo;
                            }

                            activeWindowTitle = "xdotool getactivewindow getwindowname".ShellExec().Trim();
                            string activeWindowPath = $"ps -q {activePidStr} -o cmd=".ShellExec().Trim();
                            if (!string.IsNullOrEmpty(activeWindowPath)) {
                                _lastActiveInfo = new MpPortableProcessInfo() {
                                    Handle = activeIntPtr,
                                    ProcessPath = activeWindowPath,
                                    MainWindowTitle = activeWindowTitle
                                };
                                return _lastActiveInfo;
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

        protected override string GetProcessTitle(nint handle) {
            throw new NotImplementedException();
        }

        protected override string GetProcessPath(nint handle) {
            throw new NotImplementedException();
        }

        protected override MpPortableProcessInfo GetProcessInfoByHandle(nint handle) {
            throw new NotImplementedException();
        }
        #endregion

    }
}
