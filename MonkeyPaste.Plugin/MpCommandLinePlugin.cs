using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyPaste.Plugin {
    public interface MpICommandLine {

    }
    public class MpCommandLinePlugin : MpIAnalyzerPluginComponent {
        private string _outputData = null;
        private int _timeout = 10000;

        public string Endpoint { get; set; }

        public async Task<object> AnalyzeAsync(object args) {
            await Task.Delay(1);
            Process process = new Process();
            process.StartInfo.FileName = Endpoint;
            process.StartInfo.Arguments = Base64Encode(args.ToString());
            process.StartInfo.CreateNoWindow = true;
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

        public static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        /// <summary>
        /// Encodes an argument for passing into a program
        /// </summary>
        /// <param name="original">The value that should be received by the program</param>
        /// <returns>The value which needs to be passed to the program for the original value 
        /// to come through</returns>
        static string EncodeParameterArgument(string original) {
            if (string.IsNullOrEmpty(original))
                return original;
            string value = original.Replace("\"", "\\\"");
            return value;
        }

        // This is an EDIT
        // Note that this version does the same but handles new lines in the arugments
        static string EncodeParameterArgumentMultiLine(string original) {
            if (string.IsNullOrEmpty(original))
                return original;
            string value = Regex.Replace(original, @"(\\*)" + "\"", @"$1\$0");
            value = Regex.Replace(value, @"^(.*\s.*?)(\\*)$", "\"$1$2$2\"", RegexOptions.Singleline);

            return value;
        }
    }
}
