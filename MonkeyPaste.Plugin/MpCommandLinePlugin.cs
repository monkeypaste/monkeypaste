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
            Process process = new Process();
            process.StartInfo.FileName = Endpoint;
            process.StartInfo.Arguments = args.ToString();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            //process.OutputDataReceived -= Process_OutputDataReceived;
            //process.OutputDataReceived += Process_OutputDataReceived;

            try {
                process.Start();

                while (!process.StandardOutput.EndOfStream) {
                    string result = process.StandardOutput.ReadLine();
                    return result;
                    // do something with line
                }
            }
            catch(Exception ex) {
                Console.WriteLine("Analyzer plugin error: ", ex);
                Console.WriteLine($"Warning timeout reached at endpoint: '{Endpoint}'");
                Console.WriteLine($"With args:");
                Console.WriteLine(args.ToString());
                return null;
            }

            int elapsed = 0;
            while(true) {
                if(_outputData != null) {
                    break;
                }
                if(elapsed >= _timeout) {
                    Console.WriteLine($"Warning timeout reached at endpoint: '{Endpoint}'");
                    Console.WriteLine($"With args:");
                    Console.WriteLine(args.ToString());
                    return null;
                }
                await Task.Delay(100);
                elapsed += 100;
            }

            return _outputData;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            _outputData = e.Data;
        }

        public object Analyze(object args) {
            throw new NotImplementedException();
        }
    }
}
