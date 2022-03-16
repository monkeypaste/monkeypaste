using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static MpProcessHelper.MpProcessManager;
using System.Windows.Forms;
using System.Xml.Linq;

namespace MpProcessHelper {
    public static class MpProcessAutomation {
        private static string _OriginalPath;

        private const int _MAX_ARG_LENGTH = 32698;

        public static void Init() {
            _OriginalPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
        }

        public static IntPtr StartProcess(
            string processPath,
            string args,
            bool asAdministrator,
            bool isSilent,
            bool useShellExecute,
            string workingDirectory,
            bool showError,
            WinApi.ShowWindowCommands windowState,
            IntPtr mainWindowHandle,
            out string standardOutput,
            out string standardError) {
            IntPtr outHandle = IntPtr.Zero;
            string stdOut = string.Empty;
            string stdErr = string.Empty;

            if (args.Length > _MAX_ARG_LENGTH) {
                var result = MessageBox.Show(
                    $"Cannot start '{processPath}' args must be less than or equal to {_MAX_ARG_LENGTH} characters and args is {args.Length}",
                    "Start Process Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                standardOutput = standardError = string.Empty;
                return outHandle;
            }
            try {
                string processName = Path.GetFileName(processPath);
                string processDir = Path.GetDirectoryName(processPath);
                string pathEnvVarStr = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
                
                if(!pathEnvVarStr.ToLower().Contains(processDir)) {
                    AddDirectoryToPath(processDir);
                }

                using (Process p = new Process()) {
                    p.StartInfo.UseShellExecute = useShellExecute;
                    p.StartInfo.Verb = asAdministrator ? "runas" : string.Empty;
                    p.StartInfo.ErrorDialog = showError;
                    if(showError) {
                        p.StartInfo.ErrorDialogParentHandle = mainWindowHandle;
                    }
                    if(!useShellExecute) {
                        p.StartInfo.WorkingDirectory = string.IsNullOrEmpty(workingDirectory) ? processDir : workingDirectory;

                        if (!Path.GetExtension(processPath).ToLower().Contains("exe")) {
                            if(p.StartInfo.WorkingDirectory != processDir) {
                                args = "/c" + processPath + " " + args;
                            } else {
                                args = "/c " + processName + " " + args;
                            }
                            processName = "cmd.exe";                            
                        }

                        p.StartInfo.CreateNoWindow = isSilent;
                        if(!isSilent) {
                            ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal;
                            if(windowState == WinApi.ShowWindowCommands.Hide) {
                                windowStyle = ProcessWindowStyle.Hidden;
                            } else if (windowState == WinApi.ShowWindowCommands.Minimized) {
                                windowStyle = ProcessWindowStyle.Minimized;
                            } else if (windowState == WinApi.ShowWindowCommands.Maximized) {
                                windowStyle = ProcessWindowStyle.Maximized;
                            }
                            p.StartInfo.WindowStyle = windowStyle;
                        }
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.RedirectStandardError = true;
                        p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => { stdErr += e.Data; });
                    } else {
                        p.StartInfo.WorkingDirectory = processDir;
                    }
                    p.StartInfo.FileName = processName;
                    p.StartInfo.Arguments = args;
                    p.Start();

                    if(!useShellExecute) {
                        // To avoid deadlocks, use an asynchronous read operation on at least one of the streams.  
                        p.BeginErrorReadLine();
                        stdOut = p.StandardOutput.ReadToEnd();
                    }

                    outHandle = p.Handle;
                }

                standardOutput = stdOut;
                standardError = stdErr;
                RestoreOriginalPath();
                WinApi.ShowWindowAsync(outHandle, GetShowWindowValue(windowState));
                return outHandle;
            }
            catch (Exception ex) {
                Console.WriteLine("Start Process error (Admin to Normal mode): " + ex);
                RestoreOriginalPath();
                standardOutput = stdOut;
                standardError = stdErr;
                return outHandle;
            }
            // TODO pass args to clipboard (w/ ignore in the manager) then activate window and paste
        }

        public static IntPtr StartProcessForPaste(
            string args,
            string processPath,
            bool asAdministrator,
            bool isSilent,
            WinApi.ShowWindowCommands windowState = WinApi.ShowWindowCommands.Normal) {
            try {
                IntPtr outHandle = IntPtr.Zero;
                if (isSilent) {
                    windowState = WinApi.ShowWindowCommands.Hide;
                }
                string processStartInfoInitStr = processPath;
                string processDir = Path.GetDirectoryName(processPath);
                string pathEnvVarStr = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);

                if (pathEnvVarStr.ToLower().Contains(processDir)) {
                    processStartInfoInitStr = Path.GetFileName(processPath);
                }
                ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo(processStartInfoInitStr);
                //Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe"; 
                //Sets the FileName property of myProcessInfo to %SystemRoot%\System32\cmd.exe
                //where %SystemRoot% is a system variable which is expanded using Environment.ExpandEnvironmentVariables
                processInfo.FileName = processPath;
                if (!string.IsNullOrEmpty(args)) {
                    processInfo.Arguments = args;
                }
                processInfo.WindowStyle = isSilent ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal; //Sets the WindowStyle of myProcessInfo which indicates the window state to use when the process is started to Hidden
                processInfo.Verb = asAdministrator ? "runas" : string.Empty; //The process should start with elevated permissions
                processInfo.UseShellExecute = asAdministrator ? true : false;
                processInfo.WorkingDirectory = Path.GetDirectoryName(processPath);

                if (asAdministrator) {
                    using (var process = Process.Start(processInfo)) {
                        while (!process.WaitForInputIdle(100)) {
                            Thread.Sleep(100);
                            process.Refresh();
                        }
                        outHandle = process.Handle;
                    }
                } else {
                    //using (var process = UACHelper.UACHelper.StartLimited(processInfo)) {
                    using (var process = Process.Start(processPath)) {
                        while (!process.WaitForInputIdle(100)) {
                            Thread.Sleep(100);
                            process.Refresh();
                        }
                        outHandle = process.Handle;
                    }
                }
                if (outHandle == IntPtr.Zero) {
                    Console.WriteLine("Error starting process: " + processPath);
                    return outHandle;
                }

                WinApi.ShowWindowAsync(outHandle, GetShowWindowValue(windowState));
                return outHandle;
            }
            catch (Exception ex) {
                Console.WriteLine("Start Process error (Admin to Normal mode): " + ex);
                return IntPtr.Zero;
            }
            // TODO pass args to clipboard (w/ ignore in the manager) then activate window and paste
        }

        public static IntPtr SetActiveProcess(
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
                if (processPath[0] == '%') {
                    //only occurs for hardcoded %windir%\cmd.exe
                    processPath = string.Format(
                        @"{0}\System32\cmd.exe",
                        Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName);
                }
                processPath = processPath.Replace(@"\\", @"\").ToLower();
                //MpConsole.WriteLine(processPath);

                //forceHandle is only passed when its a running application
                IntPtr handle = forceHandle == null ? IntPtr.Zero : (IntPtr)forceHandle;
                if (handle != IntPtr.Zero || !CurrentProcessWindowHandleStackDictionary.ContainsKey(processPath)) {
                    //if process is not running anymore or needs to be started (custom pastetoapppath)
                    handle = StartProcessForPaste(args, processPath, isAdmin, isSilent, forceWindowState);
                } else {
                    //ensure the process has a handle matching isAdmin, if not it needs to be created
                    var handleList = CurrentProcessWindowHandleStackDictionary[processPath];
                    foreach (var h in handleList) {
                        if (isAdmin == IsProcessAdmin(h)) {
                            handle = h;
                            if (LastWindowStateHandleDictionary.ContainsKey(handle)) {
                                forceWindowState = LastWindowStateHandleDictionary[handle];
                            }
                            break;
                        }
                    }
                    if (handle == IntPtr.Zero) {
                        //no handle found matching admin rights
                        handle = StartProcessForPaste(args, processPath, isAdmin, isSilent, forceWindowState);
                    } else {
                        //show running window with last known window state
                        WinApi.ShowWindowAsync(handle, GetShowWindowValue(forceWindowState));
                    }
                }

                return handle;
            }
            catch (Exception) {
                //MpConsole.WriteLine("MpRunningApplicationManager.SetActiveApplication error: " + ex.ToString());
                return IntPtr.Zero;
            }
        }

        public static void AddDirectoryToPath(string dir) {
            var name = "PATH";
            var scope = EnvironmentVariableTarget.Machine; // or User
            var oldValue = Environment.GetEnvironmentVariable(name, scope);
            var newValue = oldValue + string.Format(@";{0}",dir);
            Environment.SetEnvironmentVariable(name, newValue, scope);
        }

        public static void RestoreOriginalPath() {
            if(_OriginalPath == null) {
                return;
            }
            var name = "PATH";
            var scope = EnvironmentVariableTarget.Machine;
            Environment.SetEnvironmentVariable(name, _OriginalPath, scope);
        }
    }
}
