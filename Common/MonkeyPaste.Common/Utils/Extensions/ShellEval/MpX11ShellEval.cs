using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpX11ShellEval {
        #region Private Variables

        #endregion
        public static async Task<string> ShellExecAsync(this string cmd) {
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

            var sb = new StringBuilder();
            bool has_exited = false;

            process.Exited += (sender, args) => {
                try {
                    has_exited = true;
                    string errorStr = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(errorStr)) {
                        MpConsole.WriteLine($"Error for cmd '{cmd}':");
                        MpConsole.WriteLine(errorStr);
                    }
                } catch(Exception ex) {
                    MpConsole.WriteTraceLine($"Shell exit error. cmd '{cmd}'.", ex);
                }
                if (process == null) {
                    return;
                }
                process.Dispose();
                process = null;
            };

            try {
                process.Start();
                while(true) {
                    if(has_exited) {
                        break;
                    }
                    string output_line = await process.StandardOutput.ReadToEndAsync();
                    if(output_line == null) {
                        break;
                    }
                    sb.Append(output_line);
                } 
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error executing bash cmd '{cmd}'.", ex);
            }

            return sb.ToString();
        }

        public static string ShellExec(this string cmd, int timeout_ms = 3000) {
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

            process.Exited += (sender, args) => {
                string errorStr = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(errorStr)) {
                    MpConsole.WriteLine($"Error for cmd '{cmd}':");
                    MpConsole.WriteLine(errorStr);
                    output = errorStr;
                    return;
                }

                string outputStr = process.StandardOutput.ReadToEnd();
                // MpConsole.WriteLine($"Output for cmd '{cmd}'");
                // MpConsole.WriteLine(outputStr);

                process.Dispose();
                output = outputStr;
            };

            try {
                process.Start();

                while (output == null) {
                    if (DateTime.Now - start_dt >= TimeSpan.FromMilliseconds(timeout_ms)) {
                        // timeout reached
                        output = string.Empty;
                        //MpConsole.WriteLine("waiting for shell output timeout reached, aborting");                    
                    }
                    //MpConsole.WriteLine("waiting for shell output...");
                    //await System.Threading.Tasks.Task.Delay(100);
                    System.Threading.Thread.Sleep(100);
                }
                //MpConsole.WriteLine($"Shell output received (for cmd '{cmd}'):"+(string.IsNullOrEmpty(output) ? "NOPE":"YES")); 
            }
            catch (Exception) {
                //MpConsole.WriteLine(e, "Command {} failed", cmd);                
            }

            return output.ToStringOrEmpty();
        }
    }
}