using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using System.Linq;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;

namespace ProcessAutomation {
    public static class ProcessFactory {
        #region Private Variables

        private const char Quote = '\"';
        private const char Backslash = '\\';
        private const int _MAX_ARG_LENGTH = 32698;

        #endregion
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);


        #region Properties


        #endregion

        #region Public Methods

        public static MpPortableStartProcessInfo StartProcess(MpPortableStartProcessInfo pi, int waitForInputIdleTimeout = 30000) {
            var result = StartProcess(
                                processPath: pi.ProcessPath,
                                argList: pi.ArgumentList,
                                asAdministrator: pi.IsAdmin,
                                isSilent: pi.IsSilent,
                                useShellExecute: pi.UseShellExecute,
                                workingDirectory: pi.WorkingDirectory,
                                showError: pi.ShowError,
                                windowState: pi.WindowState,
                                closeOnComplete: pi.CloseOnComplete,
                                createNoWindow: pi.CreateNoWindow,
                                domain: pi.Domain,
                                userName: pi.UserName,
                                password: pi.Password);
            return result;
        }

        public static MpPortableStartProcessInfo StartProcess(
            string processPath = null,
            List<string> argList = null,
            bool asAdministrator = false,
            bool isSilent = false,
            bool useShellExecute = true,
            string workingDirectory = null,
            bool showError = true,
            ProcessWindowStyle windowState = ProcessWindowStyle.Normal,
            bool closeOnComplete = false,
            bool createNoWindow = false,
            string domain = null,
            string userName = null,
            string password = null,
            int waitForInputIdleTimeout = 30000) {
            string originalPath = GetMachinePathEnvironmentVariable();
            IntPtr outHandle = IntPtr.Zero;

            string stdOut = string.Empty;
            string stdErr = string.Empty;

            MpPortableStartProcessInfo out_psi = null;

            argList = argList == null ? new List<string>() : argList;
            int argsLength = string.Join(string.Empty, argList).Length;
            if (argsLength > _MAX_ARG_LENGTH) {
                //var result = MessageBox.Show(
                //    $"Cannot start '{processPath}' args must be less than or equal to {_MAX_ARG_LENGTH} characters and args is {argsLength}",
                //    "Start Process Error",
                //    MessageBoxButtons.OK,
                //    MessageBoxIcon.Error);

                //return null;
                throw new Exception($"Cannot start '{processPath}' args must be less than or equal to {_MAX_ARG_LENGTH} characters and args is {argsLength}");
            }
            bool loadUserProfile = false;
            if (!string.IsNullOrEmpty(userName)) {
                loadUserProfile = true;
                if (useShellExecute) {
                    MpConsole.WriteTraceLine("Warning! Malformed StartProcessInfo parameter detected, must leave useShellExecute unchecked if providing username. Pretending its not checked...");
                    useShellExecute = false;
                }
                if (string.IsNullOrEmpty(workingDirectory)) {
                    MpConsole.WriteTraceLine(@"Warning! When providing user credentials a working directory should be provided..defaulting to %SYSTEMROOT%\system32");
                }
                if (userName.Contains("@") && !string.IsNullOrEmpty(domain)) {
                    MpConsole.WriteTraceLine(@"Warning! Username appears to follow UPN format which negates usage of domain, pretending domain was not provided");
                    domain = null;
                }
            }
            try {
                string processName = Path.GetFileName(processPath);
                string processDir = Path.GetDirectoryName(processPath);

                if (!originalPath.ToLower().Contains(processDir.ToLower())) {
                    AddDirectoryToPath(processDir);
                }

                Process p = new Process();
                p.StartInfo.UseShellExecute = useShellExecute;
                p.StartInfo.Verb = asAdministrator ? "runas" : string.Empty;
                p.StartInfo.ErrorDialog = showError;

                p.StartInfo.UserName = userName;
                if(OperatingSystem.IsWindows()) {
                    p.StartInfo.LoadUserProfile = loadUserProfile;
                    p.StartInfo.Domain = domain;
                    p.StartInfo.PasswordInClearText = password;
                }
                

                if (showError) {
                    p.StartInfo.ErrorDialogParentHandle = ProcessPlugin.ThisAppHandle;
                }
                if (!useShellExecute) {
                    p.StartInfo.WorkingDirectory = string.IsNullOrEmpty(workingDirectory) ? processDir : workingDirectory;

                    if (!Path.GetExtension(processPath).ToLower().Contains("exe")) {
                        if (p.StartInfo.WorkingDirectory != processDir) {
                            argList.Insert(0, processPath);
                        } else {
                            argList.Insert(0, processName);
                        }
                        processName = "cmd.exe";
                    }

                    p.StartInfo.CreateNoWindow = createNoWindow;
                    if (!isSilent) {
                        ProcessWindowStyle windowStyle = windowState.ToEnum<ProcessWindowStyle>();
                        p.StartInfo.WindowStyle = windowStyle;
                    }
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;

                    p.OutputDataReceived += (s, e) => {
                        stdOut += e.Data;
                    };
                    p.ErrorDataReceived += (s, e) => {
                        stdErr += e.Data;
                    };
                } else {
                    p.StartInfo.WorkingDirectory = processDir;
                }

                p.StartInfo.FileName = processName;
                if (closeOnComplete) {
                    argList.Insert(0, "/c");
                }
                argList.ForEach(x => p.StartInfo.ArgumentList.Add(x));

                p.Start();

                SetPath(originalPath);
                //ShowWindowAsync(p.Handle, (int)windowState);

                //SetForegroundWindow(p.Handle);
                //SetActiveWindow(p.Handle);
                out_psi = new MpPortableStartProcessInfo() {
                    Handle = outHandle,
                    ProcessPath = processPath,
                    ArgumentList = argList,
                    IsAdmin = asAdministrator,
                    IsSilent = isSilent,
                    UseShellExecute = useShellExecute,
                    WorkingDirectory = workingDirectory,
                    ShowError = showError,
                    WindowState = windowState,
                    CloseOnComplete = closeOnComplete,
                    CreateNoWindow = createNoWindow,
                    Domain = domain,
                    UserName = userName,
                    Password = password,
                    StandardOutput = stdOut,
                    StandardError = stdErr
                };
                out_psi = SetActiveProcess(out_psi,waitForInputIdleTimeout);

                if (!useShellExecute) {
                    // To avoid deadlocks, use an asynchronous read operation on at least one of the streams.  
                    p.BeginErrorReadLine();
                    p.BeginOutputReadLine();
                }

                if (!createNoWindow && !p.HasExited) {
                    p.WaitForInputIdle(waitForInputIdleTimeout);
                }

                outHandle = p.Handle;
                p.Dispose();
            }
            catch (Exception ex) {
                MpConsole.WriteLine("Start Process error (Admin to Normal mode): " + ex);
                SetPath(originalPath);
            }
            if(out_psi == null) {
                out_psi = new MpPortableStartProcessInfo() {
                    Handle = outHandle,
                    ProcessPath = processPath,
                    ArgumentList = argList,
                    IsAdmin = asAdministrator,
                    IsSilent = isSilent,
                    UseShellExecute = useShellExecute,
                    WorkingDirectory = workingDirectory,
                    ShowError = showError,
                    WindowState = windowState,
                    CloseOnComplete = closeOnComplete,
                    CreateNoWindow = createNoWindow,
                    Domain = domain,
                    UserName = userName,
                    Password = password,
                    StandardOutput = stdOut,
                    StandardError = stdErr
                };
            }
            return out_psi;
        }


        public static MpPortableStartProcessInfo SetActiveProcess(MpPortableStartProcessInfo pi, int waitForInputIdleTimeout = 30000) {
            if (pi == null) {
                return null;
            }
            try {
                //enforce that process won't be closed
                pi.CloseOnComplete = false;

                if (string.IsNullOrEmpty(pi.ProcessPath)) {
                    if (pi.Handle == null || pi.Handle == IntPtr.Zero) {
                        return pi;
                    }
                    pi.ProcessPath = MpCommonTools.Services.ProcessWatcher.GetProcessPath((IntPtr)pi.Handle);
                }

                //pi.Handle is only passed when its a running application

                if (!MpCommonTools.Services.ProcessWatcher.IsHandleRunningProcess(pi.Handle) && 
                    !MpCommonTools.Services.ProcessWatcher.RunningProcessLookup.ContainsKey(pi.ProcessPath)) {
                    //if process is not running anymore or needs to be started (custom pastetoapppath)
                    pi = StartProcess(pi);
                } else {
                    //ensure the process has a handle matching isAdmin, if not it needs to be created
                    bool wasMatched = false;
                    if (MpCommonTools.Services.ProcessWatcher.RunningProcessLookup.ContainsKey(pi.ProcessPath)) {
                        var handleList = MpCommonTools.Services.ProcessWatcher.RunningProcessLookup[pi.ProcessPath];
                        foreach (var h in handleList) {
                            if (pi.IsAdmin == MpCommonTools.Services.ProcessWatcher.IsAdmin(h)) {
                                pi.Handle = h;
                                if (MpCommonTools.Services.ProcessWatcher.RunningProcessLookup.Any(x=>x.Value.Contains(h))) {
                                    pi.WindowState = MpCommonTools.Services.ProcessWatcher.GetWindowStyle(h);
                                }
                                wasMatched = true;
                                break;
                                //if (CurrentWindowStateHandleDictionary.ContainsKey(handle)) {
                                //    pi.WindowState = CurrentWindowStateHandleDictionary[handle];
                                //}
                                //break;
                            }
                        }
                    }
                    if (!wasMatched) {
                        //
                        pi = StartProcess(pi);
                    } else {
                        //show running window with last known window state
                        MpCommonTools.Services.ProcessWatcher.SetActiveProcess(pi.Handle, pi.WindowState);
                    }
                }

                return pi;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("SetActiveApplication error: " + ex);
                return null;
            }
        }


        public static string GetArgumentStr(List<string> argList) {
            if (argList == null || argList.Count == 0) {
                return string.Empty;
            }
            var sb = new StringBuilder();
            foreach (var arg in argList) {
                AppendArgument(sb, arg);
            }
            return sb.ToString();
        }

        private static void AppendArgument(StringBuilder stringBuilder, string argument) {
            // from https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/PasteArguments.cs
            if (stringBuilder.Length != 0) {
                stringBuilder.Append(' ');
            }

            // Parsing rules for non-argv[0] arguments:
            //   - Backslash is a normal character except followed by a quote.
            //   - 2N backslashes followed by a quote ==> N literal backslashes followed by unescaped quote
            //   - 2N+1 backslashes followed by a quote ==> N literal backslashes followed by a literal quote
            //   - Parsing stops at first whitespace outside of quoted region.
            //   - (post 2008 rule): A closing quote followed by another quote ==> literal quote, and parsing remains in quoting mode.
            if (argument.Length != 0 && ContainsNoWhitespaceOrQuotes(argument)) {
                // Simple case - no quoting or changes needed.
                stringBuilder.Append(argument);
            } else {
                stringBuilder.Append(Quote);
                int idx = 0;
                while (idx < argument.Length) {
                    char c = argument[idx++];
                    if (c == Backslash) {
                        int numBackSlash = 1;
                        while (idx < argument.Length && argument[idx] == Backslash) {
                            idx++;
                            numBackSlash++;
                        }

                        if (idx == argument.Length) {
                            // We'll emit an end quote after this so must double the number of backslashes.
                            stringBuilder.Append(Backslash, numBackSlash * 2);
                        } else if (argument[idx] == Quote) {
                            // Backslashes will be followed by a quote. Must double the number of backslashes.
                            stringBuilder.Append(Backslash, numBackSlash * 2 + 1);
                            stringBuilder.Append(Quote);
                            idx++;
                        } else {
                            // Backslash will not be followed by a quote, so emit as normal characters.
                            stringBuilder.Append(Backslash, numBackSlash);
                        }

                        continue;
                    }

                    if (c == Quote) {
                        // Escape the quote so it appears as a literal. This also guarantees that we won't end up generating a closing quote followed
                        // by another quote (which parses differently pre-2008 vs. post-2008.)
                        stringBuilder.Append(Backslash);
                        stringBuilder.Append(Quote);
                        continue;
                    }

                    stringBuilder.Append(c);
                }

                stringBuilder.Append(Quote);
            }
        }

        private static bool ContainsNoWhitespaceOrQuotes(string s) {
            for (int i = 0; i < s.Length; i++) {
                char c = s[i];
                if (char.IsWhiteSpace(c) || c == Quote) {
                    return false;
                }
            }

            return true;
        }

        private static string GetMachinePathEnvironmentVariable() {
            return Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
        }

        private static void AddDirectoryToPath(string dir) {
            var name = "PATH";
            var scope = EnvironmentVariableTarget.Machine; // or User
            string oldValue = Environment.GetEnvironmentVariable(name, scope);
            var newValue = oldValue + string.Format(@";{0}", dir);
            try {
                Environment.SetEnvironmentVariable(name, newValue, scope);
            } catch(SecurityException sex) {
                MpConsole.WriteTraceLine("MpProcessAutomation.SetPath exception (likely this app is not running as admin and therefore cannot set env var): ", sex);
            }
            
        }

        private static void SetPath(string newPath) {
            var name = "PATH";
            var scope = EnvironmentVariableTarget.Machine;
            try {
                Environment.SetEnvironmentVariable(name, newPath, scope);
            } catch(SecurityException sex) {
                MpConsole.WriteTraceLine("MpProcessAutomation.SetPath exception (likely this app is not running as admin and therefore cannot set env var): ", sex);

            }
            
        }

        #endregion
    }
}
