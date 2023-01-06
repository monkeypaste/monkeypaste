using System;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using MonoMac.AppKit;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvMacProcessWatcher : MpAvProcessWatcherBase {
        #region Private Variables

        private object _lockObj = new object();

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public override ProcessWindowStyle GetWindowStyle(object handleIdOrTitle) {
            throw new NotImplementedException();
        }
        public override bool IsAdmin(object handleIdOrTitle) {
            throw new NotImplementedException();
        }
        public override IntPtr SetActiveProcess(IntPtr handle) {
            throw new NotImplementedException();
        }
        public override IntPtr GetParentHandleAtPoint(MpPoint poIntPtr) {
            throw new NotImplementedException();
        }
        public override IntPtr SetActiveProcess(IntPtr handle, ProcessWindowStyle windowStyle) {
            throw new NotImplementedException();
        }
        //public override string getprocessapplicationname(intptr handle) {
        //    var runningapps = nsworkspace.sharedworkspace.runningapplications;
        //    var matchapp = runningapps.firstordefault(x => x.handle == handle);
        //    string name = matchapp == default ? string.empty : matchapp.localizedname;

        //    runningapps.foreach(x => x.dispose());
        //    return name;
        //}

        //public override string GetProcessMainWindowTitle(IntPtr handle) {
        //    return GetProcessApplicationName(handle);
        //}

        //public override string GetProcessPath(IntPtr handle) {
        //    var runningApps = NSWorkspace.SharedWorkspace.RunningApplications;
        //    var matchApp = runningApps.FirstOrDefault(x => x.Handle == handle);
        //    string path = matchApp == default ? String.Empty : matchApp.ExecutableUrl.AbsoluteString.ToLower();

        //    runningApps.ForEach(x => x.Dispose());
        //    return path;
        //}

        //public override bool IsHandleRunningProcess(IntPtr handle) {
        //    var runningApps = NSWorkspace.SharedWorkspace.RunningApplications;
        //    var matchApp = runningApps.FirstOrDefault(x => x.Handle == handle);
        //    bool isRunning = matchApp == default ? false : true;

        //    runningApps.ForEach(x => x.Dispose());
        //    return isRunning;
        //}
        #endregion

        #region Protected Methods
        public override MpPortableProcessInfo GetActiveProcessInfo() {
            var active_app = NSWorkspace.SharedWorkspace.FrontmostApplication;
            var active_info = new MpPortableProcessInfo() {
                Handle = active_app.Handle,
                ProcessPath = active_app.ExecutableUrl.AbsoluteString.ToLower(),
                MainWindowTitle = active_app.LocalizedName
            };
            return active_info;
        }

        protected override void CreateRunningProcessLookup() {
            if (RunningProcessLookup == null) {
                RunningProcessLookup = new ConcurrentDictionary<string, ObservableCollection<IntPtr>>();
            }

            var runningApps = NSWorkspace.SharedWorkspace.RunningApplications;
            var filteredApps = FilterRunningApplications(runningApps);

            foreach (var runningApp in filteredApps) {
                if (RunningProcessLookup.TryGetValue(runningApp.ExecutableUrl.AbsoluteString.ToLower(), out var handles)) {
                    handles.Add(runningApp.Handle);
                    RunningProcessLookup[runningApp.ExecutableUrl.AbsoluteString.ToLower()] = handles;
                } else {
                    RunningProcessLookup.TryAdd(runningApp.ExecutableUrl.AbsoluteString.ToLower(), new() { runningApp.Handle });
                }
            }
            runningApps.ForEach(x => x.Dispose());
        }

        protected override MpPortableProcessInfo RefreshRunningProcessLookup() {
            lock(_lockObj) {
                MpPortableProcessInfo activeAppInfo = null;

                var runningApps = NSWorkspace.SharedWorkspace.RunningApplications;
                var filteredApps = FilterRunningApplications(runningApps);
                var refreshedPaths = new List<string>();

                foreach (var runningApp in filteredApps) {
                    ObservableCollection<IntPtr> handles = null;
                    if (RunningProcessLookup.TryGetValue(runningApp.ExecutableUrl.AbsoluteString.ToLower(), out handles)) {
                        // application is already known
                        if (!handles.Contains(runningApp.Handle)) {
                            //handle is new
                            handles.Add(runningApp.Handle);
                        }

                    } else {
                        // new process found
                        handles = new() { runningApp.Handle };
                        RunningProcessLookup.TryAdd(runningApp.ExecutableUrl.AbsoluteString.ToLower(), handles);
                    }
                    if (handles == null) {
                        Debugger.Break();
                    }
                    if (runningApp.Active) {                        
                        // handle is active
                        if (activeAppInfo != null) {
                            Debugger.Break();
                        } else {
                            int runningAppIdx = handles.IndexOf(runningApp.Handle);
                            handles.Move(runningAppIdx, 0);

                            activeAppInfo = new MpPortableProcessInfo() {
                                Handle = runningApp.Handle,
                                ProcessPath = runningApp.ExecutableUrl.AbsoluteString.ToLower(),
                                MainWindowTitle = runningApp.LocalizedName
                            };
                        }
                    }

                    RunningProcessLookup[runningApp.ExecutableUrl.AbsoluteString.ToLower()] = handles;

                    if(!refreshedPaths.Contains(runningApp.ExecutableUrl.AbsoluteString.ToLower())) {
                        refreshedPaths.Add(runningApp.ExecutableUrl.AbsoluteString.ToLower());
                    }
                }
                runningApps.ForEach(x => x.Dispose());

                //remove apps that were terminated
                var appsToRemove = RunningProcessLookup.Where(x => !refreshedPaths.Contains(x.Key.ToLower()));
                for (int i = 0; i < appsToRemove.Count(); i++) {
                    RunningProcessLookup.TryRemove(appsToRemove.ElementAt(i));
                }

                return activeAppInfo;
            }
        }

        private IEnumerable<NSRunningApplication> FilterRunningApplications(IEnumerable<NSRunningApplication> apps) {
            return apps.Where(x => !x.Terminated && x.ActivationPolicy != NSApplicationActivationPolicy.Prohibited && !string.IsNullOrEmpty(x.LocalizedName));
        }
        #endregion
    }
}

