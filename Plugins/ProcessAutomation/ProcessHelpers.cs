using MpProcessHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ProcessAutomation {
    public static class ProcessHelpers {
        private const int _MAX_ARG_LENGTH = 32698;
        private const char Quote = '\"';
        private const char Backslash = '\\';

        public static IntPtr StartProcess(
            string processPath,
            List<string> argList,
            bool asAdministrator,
            bool isSilent,
            bool useShellExecute,
            string workingDirectory,
            bool showError,
            WinApi.ShowWindowCommands windowState,
            IntPtr mainWindowHandle,
            bool closeOnComplete,
            out string standardOutput,
            out string standardError) {
            string originalPath = GetMachinePathEnvironmentVariable();

            IntPtr outHandle = IntPtr.Zero;
            string stdOut = string.Empty;
            string stdErr = string.Empty;

            argList = argList == null ? new List<string>() : argList;
            int argsLength = string.Join(string.Empty, argList).Length;
            if (argsLength > _MAX_ARG_LENGTH) {
                throw new Exception($"Cannot start '{processPath}' args must be less than or equal to {_MAX_ARG_LENGTH} characters and args is {argsLength}");
            }
            try {
                string processName = Path.GetFileName(processPath);
                string processDir = Path.GetDirectoryName(processPath);

                if (!originalPath.ToLower().Contains(processDir.ToLower())) {
                    AddDirectoryToPath(processDir);
                }

                using (Process p = new Process()) {
                    if (!useShellExecute && asAdministrator) {
                        //to run as admin it MUST use shellExecute
                        useShellExecute = true;                        
                    }
                    p.StartInfo.UseShellExecute = useShellExecute;
                    p.StartInfo.Verb = asAdministrator ? "runas" : string.Empty;
                    p.StartInfo.ErrorDialog = showError;
                    if (showError) {
                        p.StartInfo.ErrorDialogParentHandle = mainWindowHandle;
                    }
                    if (!useShellExecute) {
                        p.StartInfo.WorkingDirectory = string.IsNullOrEmpty(workingDirectory) ? processDir : workingDirectory;

                        if (!Path.GetExtension(processPath).ToLower().EndsWith("exe")) {
                            if (p.StartInfo.WorkingDirectory != processDir) {
                                argList.Insert(0,/*"/c" + */processPath);
                            } else {
                                argList.Insert(0,/*"/c " + */processName);
                            }
                            processName = "cmd.exe";
                        }

                        p.StartInfo.CreateNoWindow = isSilent;
                        if (!isSilent) {
                            ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal;
                            if (windowState == WinApi.ShowWindowCommands.Hide) {
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
                    if(closeOnComplete) {
                        argList.Insert(0, "/c");
                    }
                    p.StartInfo.Arguments = GetArgumentStr(argList);
                    p.Start();

                    if (!useShellExecute) {
                        // To avoid deadlocks, use an asynchronous read operation on at least one of the streams.  
                        p.BeginErrorReadLine();
                        stdOut = p.StandardOutput.ReadToEnd();
                    }

                    outHandle = p.Handle;
                }

                standardOutput = stdOut;
                standardError = stdErr;
                SetPath(originalPath);
                WinApi.ShowWindowAsync(outHandle, MpProcessManager.GetShowWindowValue(windowState));
                return outHandle;
            }
            catch (Exception ex) {
                Console.WriteLine("Start Process error (Admin to Normal mode): " + ex);
                SetPath(originalPath);
                standardOutput = stdOut;
                standardError = stdErr;
                return outHandle;
            }
            // TODO pass args to clipboard (w/ ignore in the manager) then activate window and paste
        }

        public static string GetArgumentStr(List<string> argList) {
            if(argList == null || argList.Count == 0) {
                return string.Empty;
            }
            var sb = new StringBuilder();
            foreach(var arg in argList) {
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
            Environment.SetEnvironmentVariable(name, newValue, scope);
        }

        private static void SetPath(string newPath) {
            var name = "PATH";
            var scope = EnvironmentVariableTarget.Machine;
            Environment.SetEnvironmentVariable(name, newPath, scope);
        }
    }
}
