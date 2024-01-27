using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public static class MpCli {
        public static string CmdExe => Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "System32",
                    "cmd.exe");
        static string DefFileName =>
            CmdExe;
        static string DefWorkingDir =>
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static (int, string) Run(
            string file = default,
            string dir = default,
            string args = default) {
            var proc = CreateProcess(file, dir, args);
            proc.Start();
            string proc_output = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();
            int exit_code = proc.ExitCode;
            proc.Close();
            proc.Dispose();
            return (exit_code, proc_output);
        }

        public static async Task<(int, string)> RunAsync(
            string file = default,
            string dir = default,
            string args = default) {
            var proc = CreateProcess(file, dir, args);
            proc.Start();
            string proc_output = await proc.StandardOutput.ReadToEndAsync();
            proc.WaitForExit();
            int exit_code = proc.ExitCode;
            proc.Close();
            proc.Dispose();
            return (exit_code, proc_output);
        }

        private static Process CreateProcess(
            string file = default,
            string dir = default,
            string args = default) {
            var proc = new Process();
            proc.StartInfo.FileName = file ?? DefFileName;
            proc.StartInfo.WorkingDirectory = dir ?? DefWorkingDir;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            return proc;
        }

    }
}
