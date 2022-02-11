using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Plugin {
    public interface MpICommandLine {

    }
    public class MpCommandLinePlugin : MpIAnalyzerPluginComponent {
        private string _outputData = null;
        private int _timeout = 10000;

        public string Endpoint { get; set; }
        public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();

        public async Task<object> AnalyzeAsync(object args) {
            await Task.Delay(1);
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c DIR";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += new DataReceivedEventHandler(ErrorOutputHandler);
            //* Start process
            process.Start();
            //* Read one element asynchronously
            process.BeginErrorReadLine();
            //* Read the other one synchronously
            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
            process.WaitForExit();

            return output;
        }

        static void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine) {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            Console.WriteLine(outLine.Data);
        }
    }
}
