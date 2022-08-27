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
using Avalonia.Gtk3;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    
    public class MpAvX11ProcessWatcher : MpAvProcessWatcherBase {
        private object _lockObj = new object();

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
            if (RunningProcessLookup == null) {
                RunningProcessLookup = new ConcurrentDictionary<string, ObservableCollection<IntPtr>>();
            }
            RunningProcessLookup.Clear();

            var runningApps = GetRunningApps();

            foreach(var kvp in runningApps) {
                foreach(var handle in kvp.Value) {
                    IntPtr handlePtr = new IntPtr(int.Parse(handle));
                    if(!RunningProcessLookup.ContainsKey(kvp.Key)) {
                        RunningProcessLookup.TryAdd(kvp.Key, new() { handlePtr });
                    } else if(RunningProcessLookup.TryGetValue(kvp.Key, out var handles)) {
                        handles.Add(handlePtr);
                        RunningProcessLookup[kvp.Key] = handles;
                    }
                }
            }
        }

        private Dictionary<string, List<string>> GetRunningApps() {
            var runningApps = new Dictionary<string, List<string>>();

            string winHandleStr = @"ps -o pid=".Bash();
            var winHandles = winHandleStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string winHandle in winHandles) {
                if(winHandle.IsStringNullOrWhiteSpace()) {
                    continue;
                }
                int handleInt = int.Parse(winHandle);
                if(handleInt == 0) {
                    continue;
                }
                MpConsole.WriteLine("WindowHandleStr: " + winHandle + " Int: "+handleInt);
                string processPathsStr = $"ps -q {winHandle} -o cmd=".Bash();
                MpConsole.WriteLine("Window Paths: " + processPathsStr);
                if (!processPathsStr.IsNullOrEmpty()) {
                    var processPaths = processPathsStr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (processPaths != null && processPaths.Length > 0) {
                        string processPath = processPaths[0];
                        MpConsole.WriteLine("Found path: " + processPath);
                        MpConsole.WriteLine("Parsed Handle Ptr: " + winHandle);
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


                var filteredApps = GetRunningApps();
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
                            if (activeAppTuple != null) {
                                Debugger.Break();
                            } else {
                                int runningAppIdx = handles.IndexOf(handlePtr);
                                handles.Move(runningAppIdx, 0);

                                activeAppTuple = new Tuple<string, string, IntPtr>(
                                    runningApp.Key.ToLower(),
                                    activeWindowTitle,
                                    handlePtr);
                            }
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

    public static class X11Extensions {

        //await $"scripts/00magic.sh --param {arg}".Bash(this.logger);
        public static string Bash(this string cmd) {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            var process = new System.Diagnostics.Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            string output = null;

            process.Exited += (sender, args) =>
            {
                string errorStr = process.StandardError.ReadToEnd();
                if(!errorStr.IsNullOrEmpty()) {
                    MpConsole.WriteLine($"Error for cmd '{cmd}':");
                    MpConsole.WriteLine(errorStr);
                    output = errorStr;
                    return;
                }

                string outputStr = process.StandardOutput.ReadToEnd();
                MpConsole.WriteLine($"Output for cmd '{cmd}'");
                MpConsole.WriteLine(outputStr);

                process.Dispose();
                output = outputStr;
            };

            try {
                process.Start();

                while(output == null) {
                    //await System.Threading.Tasks.Task.Delay(100);
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception e) {
                MpConsole.WriteLine(e, "Command {} failed", cmd);                
            }

            return output;
        }
    }
}
