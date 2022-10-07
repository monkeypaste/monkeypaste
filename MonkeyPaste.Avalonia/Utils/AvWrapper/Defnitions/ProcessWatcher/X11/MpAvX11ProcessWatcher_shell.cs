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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Avalonia {
    
    public static class MpAvX11ProcessWatcher_shell {
        #region Private Variables

        private static string[] _requiredTools = new string[] {
            "xdotool"
        };
        

        #endregion


        public static Dictionary<string, List<string>> GetRunningApps() {
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
        
    }

    public static class X11Extensions {

        //await $"scripts/00magic.sh --param {arg}".Bash(this.logger);
        public static string Bash(this string cmd, int timeout_ms = 3000) {
            var start_dt = DateTime.Now;

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
                //MpConsole.WriteLine($"Output for cmd '{cmd}'");
                //MpConsole.WriteLine(outputStr);

                process.Dispose();
                output = outputStr;
            };

            try {
                process.Start();

                while(output == null) {
                    if(DateTime.Now - start_dt >= TimeSpan.FromMilliseconds(timeout_ms)) {
                        // timeout reached
                        output = string.Empty;                        
                    }
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
