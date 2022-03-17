using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static MpProcessHelper.MpProcessManager;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;
using MonkeyPaste.Plugin;
using System.Threading.Tasks;

namespace MpProcessHelper {
    public static class MpProcessAutomation {
        #region Private Variables

        private static string _OriginalPath;

        private const int _MAX_ARG_LENGTH = 32698;

        #endregion

        #region Properties

        public static MpIExternalPasteHandler PasteService { get; set; }

        #endregion

        #region Public Methods

        public static void Init() {
            _OriginalPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
        }

        public static MpProcessInfo StartProcess(
            string processPath,
            string args,
            bool asAdministrator,
            bool isSilent,
            bool useShellExecute,
            string workingDirectory,
            bool showError,
            WinApi.ShowWindowCommands windowState,
            IntPtr mainWindowHandle,
            bool waitForInputIdle,
            bool createNoWindow,
            string userName,
            string password,
            int waitForInputIdleTimeout = 30000) {

            //IntPtr outHandle = IntPtr.Zero;

            string stdOut = string.Empty;
            string stdErr = string.Empty;

            if (args.Length > _MAX_ARG_LENGTH) {
                var result = MessageBox.Show(
                    $"Cannot start '{processPath}' args must be less than or equal to {_MAX_ARG_LENGTH} characters and args is {args.Length}",
                    "Start Process Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return null;
            }
            try {
                string processName = Path.GetFileName(processPath);
                string processDir = Path.GetDirectoryName(processPath);
                string pathEnvVarStr = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
                
                if(!pathEnvVarStr.ToLower().Contains(processDir)) {
                    AddDirectoryToPath(processDir);
                }

                Process p = new Process();
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


                if (!useShellExecute) {
                    // To avoid deadlocks, use an asynchronous read operation on at least one of the streams.  
                    p.BeginErrorReadLine();
                    stdOut = p.StandardOutput.ReadToEnd();
                }

                RestoreOriginalPath();
                WinApi.ShowWindowAsync(p.Handle, GetShowWindowValue(windowState));

                if(waitForInputIdle) {
                    p.WaitForInputIdle(waitForInputIdleTimeout);
                }


                WinApi.SetForegroundWindow(p.Handle);
                WinApi.SetActiveWindow(p.Handle);
                p.Dispose();

                return new MpProcessInfo() {
                    Handle = p.Handle,
                    StandardOutput = stdOut,
                    StandardError = stdErr,
                    ProcessPath = processPath
                };
            }
            catch (Exception ex) {
                MpConsole.WriteLine("Start Process error (Admin to Normal mode): " + ex);
                RestoreOriginalPath();

                return new MpProcessInfo() {
                    Handle = IntPtr.Zero,
                    StandardOutput = stdOut,
                    StandardError = stdErr,
                    ProcessPath = processPath
                };
            }
            // TODO pass args to clipboard (w/ ignore in the manager) then activate window and paste
        }

        public static IntPtr SetActiveProcess(
            MpProcessInfo pi,
            int waitForInputIdleTimeout = 30000) {
            try {
                if (string.IsNullOrEmpty(pi.ProcessPath)) {
                    if(pi.Handle == null  || ((IntPtr)pi.Handle) == IntPtr.Zero) {
                        return IntPtr.Zero;
                    }
                    pi.ProcessPath = GetProcessPath((IntPtr)pi.Handle);
                }

                if (pi.ProcessPath.StartsWith("%")) {
                    //only occurs for hardcoded %windir%\cmd.exe
                    pi.ProcessPath = string.Format(
                        @"{0}\System32\cmd.exe",
                        Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName);
                }
                pi.ProcessPath = pi.ProcessPath.Replace(@"\\", @"\").ToLower();
                //MpConsole.WriteLine(pi.ProcessPath);

                //pi.Handle is only passed when its a running application
                IntPtr handle = pi.Handle == null ? IntPtr.Zero : (IntPtr)pi.Handle;
                if (!IsHandleRunningProcess(handle)) {
                    //if process is not running anymore or needs to be started (custom pastetoapppath)
                    var npi = StartProcess(
                        pi.ProcessPath, 
                        pi.Arguments, pi.IsAdmin, pi.IsSilent,pi.UseShellExecute,pi.WorkingDirectory, true,
                        pi.WindowState, 
                        GetThisApplicationMainWindowHandle(), true,
                        pi.CreateNoWindow, pi.UserName,pi.Password);

                    handle = npi.Handle;
                } else { 
                    //ensure the process has a handle matching isAdmin, if not it needs to be created
                    var handleList = CurrentProcessWindowHandleStackDictionary[pi.ProcessPath];
                    foreach (var h in handleList) {
                        if (pi.IsAdmin == IsProcessAdmin(h)) {
                            handle = h;
                            if (CurrentWindowStateHandleDictionary.ContainsKey(handle)) {
                                pi.WindowState = CurrentWindowStateHandleDictionary[handle];
                            }
                            break;
                        }
                    }
                    if (handle == IntPtr.Zero) {
                        //no handle found matching admin rights
                        var npi = StartProcess(
                        pi.ProcessPath,
                        pi.Arguments, pi.IsAdmin, pi.IsSilent, pi.UseShellExecute, pi.WorkingDirectory, true,
                        pi.WindowState,
                        GetThisApplicationMainWindowHandle(), true,
                        pi.CreateNoWindow, pi.UserName, pi.Password);
                        handle = npi.Handle;
                    } else {
                        //show running window with last known window state
                        WinApi.ShowWindowAsync(handle, GetShowWindowValue(pi.WindowState));
                        WinApi.SetForegroundWindow(handle);
                        WinApi.SetActiveWindow(handle);

                        //var p = GetProcessByHandle(handle);
                        //if(p != null) {
                        //    //p.WaitForInputIdle(waitForInputIdleTimeout);
                        //}
                        //uint result = WinApi.WaitForInputIdle(handle, waitForInputIdleTimeout);
                        //if(result != 0) {
                        //    throw new Exception("Process " + pi.ProcessPath + " never became available :(");
                        //}
                    }
                }

                return handle;
            }
            catch (Exception) {
                //MpConsole.WriteLine("MpRunningApplicationManager.SetActiveApplication error: " + ex.ToString());
                return IntPtr.Zero;
            }
        }

        public static bool SetActiveProcess(IntPtr handle) {
            if (CurrentProcessWindowHandleStackDictionary.All(x => !x.Value.Contains(handle))) {
                return false;
            }
            try {
                WinApi.ShowWindowCommands windowState = WinApi.ShowWindowCommands.Normal;
                if(CurrentWindowStateHandleDictionary.ContainsKey(handle)) {
                    var currentState = CurrentWindowStateHandleDictionary[handle];
                    if(currentState == WinApi.ShowWindowCommands.Maximized) {
                        // NOTE (I'm assuming) the window must be visible to paste into so cannot be hidden/minimized
                        windowState = currentState;
                    }
                }
                WinApi.ShowWindowAsync(handle, GetShowWindowValue(windowState));

                return true;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("MpRunningApplicationManager.SetActiveApplication error: " + ex);
                return false;
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

        #endregion
    }
}
