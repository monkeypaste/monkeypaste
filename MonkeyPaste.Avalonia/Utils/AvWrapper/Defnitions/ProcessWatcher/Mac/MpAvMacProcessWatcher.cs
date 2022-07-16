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
        public override void InitPlatform() {
            MpAvMacHelpers.EnsureInitialized();
        }
        public override IntPtr GetLastActiveInstance(string path) {
            if(RunningProcessLookup.TryGetValue(path.ToLower(), out var handles) && handles.Count > 0) {
                return handles[0];
            }
            return IntPtr.Zero;
        }

        public override IntPtr GetParentHandleAtPoint(MpPoint poIntPtr) {
            throw new NotImplementedException();
        }

        public override string GetProcessApplicationName(IntPtr handle) {
            var runningApps = NSWorkspace.SharedWorkspace.RunningApplications;
            var matchApp = runningApps.FirstOrDefault(x => x.Handle == handle);
            string name = matchApp == default ? String.Empty : matchApp.LocalizedName;
            
            runningApps.ForEach(x => x.Dispose());
            return name;
        }

        public override string GetProcessMainWindowTitle(IntPtr handle) {
            return GetProcessApplicationName(handle);
        }

        public override string GetProcessPath(IntPtr handle) {
            var runningApps = NSWorkspace.SharedWorkspace.RunningApplications;
            var matchApp = runningApps.FirstOrDefault(x => x.Handle == handle);
            string path = matchApp == default ? String.Empty : matchApp.ExecutableUrl.AbsoluteString.ToLower();

            runningApps.ForEach(x => x.Dispose());
            return path;
        }

        public override bool IsHandleRunningProcess(IntPtr handle) {
            var runningApps = NSWorkspace.SharedWorkspace.RunningApplications;
            var matchApp = runningApps.FirstOrDefault(x => x.Handle == handle);
            bool isRunning = matchApp == default ? false : true;

            runningApps.ForEach(x => x.Dispose());
            return isRunning;
        }
        #endregion

        #region Protected Methods

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

        protected override Tuple<string, string, IntPtr> RefreshRunningProcessLookup() {
            lock(_lockObj) {
                Tuple<string, string, IntPtr>? activeAppTuple = null;

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
                        if (activeAppTuple != null) {
                            Debugger.Break();
                        } else {
                            int runningAppIdx = handles.IndexOf(runningApp.Handle);
                            handles.Move(runningAppIdx, 0);

                            activeAppTuple = new Tuple<string, string, IntPtr>(
                                runningApp.ExecutableUrl.AbsoluteString.ToLower(),
                                runningApp.LocalizedName,
                                runningApp.Handle);
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

                return activeAppTuple;
            }
        }

        private IEnumerable<NSRunningApplication> FilterRunningApplications(IEnumerable<NSRunningApplication> apps) {
            return apps.Where(x => !x.Terminated && x.ActivationPolicy != NSApplicationActivationPolicy.Prohibited && !string.IsNullOrEmpty(x.LocalizedName));
        }
        #endregion
    }
}

