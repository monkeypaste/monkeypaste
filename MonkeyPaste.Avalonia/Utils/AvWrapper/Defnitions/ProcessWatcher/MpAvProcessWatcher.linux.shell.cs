using MonkeyPaste.Common;
using System;
//using Gio;
//
//using GLib;
//using Gdk;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {

    public static class MpAvX11ProcessWatcher_shell {
        #region Private Variables

        private static string[] _requiredTools = new string[] {
            "xdotool"
        };


        #endregion


        public static Dictionary<string, List<string>> GetRunningApps() {
            var runningApps = new Dictionary<string, List<string>>();

            string winHandleStr = @"ps -o pid=".ShellExec();
            var winHandles = winHandleStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string winHandle in winHandles) {
                if (winHandle.IsStringNullOrWhiteSpace()) {
                    continue;
                }
                int handleInt = 0;

                try {
                    handleInt = int.Parse(winHandle);
                }
                catch (FormatException) {
                    //MpConsole.WriteTraceLine($"Error parsing x11 handle: '{winHandle}'", ex);

                }
                if (handleInt == 0) {
                    continue;
                }
                //MpConsole.WriteLine("WindowHandleStr: " + winHandle + " Int: "+handleInt);
                string processPathsStr = $"ps -q {winHandle} -o cmd=".ShellExec();
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

    }


}
