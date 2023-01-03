using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;
namespace MonkeyPaste {

    public class MpCommandLinePlugin : MpIAnalyzeComponent {
        public string Endpoint { get; set; }

        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat request) {
            Process process = new Process();
            process.StartInfo.FileName = Endpoint;
            process.StartInfo.Arguments = request.SerializeJsonObjectToBase64();//Base64EncodeArgs(args.ToString());
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

            return new MpAnalyzerPluginResponseFormat() {
                dataObject = new MpPortableDataObject(MpPortableDataFormats.Text,output)
            };
        }

        static void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine) {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            Console.WriteLine(outLine.Data);
        }

        public static string Base64EncodeArgs(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
