using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public class MpCommandLinePlugin : MpIAnalyzeAsyncComponent, MpIAnalyzeComponent {
        public static string SCRIPTS_ROOT_DIR =>
            Path.Combine(MpCommonHelpers.GetSolutionDir(), "MonkeyPaste.Avalonia", "scripts");
        public string Endpoint { get; set; }
        public string TargetPath { get; set; }

        public MpCommandLinePlugin() { }
        public MpCommandLinePlugin(string targetPath) {
            TargetPath = targetPath;
        }

        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            await Task.Delay(1);
            return PerformAnalyzeRequest(req);
        }
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            return PerformAnalyzeRequest(req);
        }

        protected virtual MpAnalyzerPluginResponseFormat PerformAnalyzeRequest(MpAnalyzerPluginRequestFormat req) {
            Process process = new Process();
            process.StartInfo.FileName = @"C:\Users\tkefauver\AppData\Local\Programs\Python\Python310\python.exe";
            //process.StartInfo.WorkingDirectory = Path.GetDirectoryName(Endpoint);
            process.StartInfo.Arguments = TargetPath + " " + req.SerializeJsonObjectToBase64();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            MpAnalyzerPluginResponseFormat resp = null;
            string ex_str = null;
            var error_and_ex_sb = new StringBuilder();
            DataReceivedEventHandler error_handler = (s, e) => {
                error_and_ex_sb.Append(e.Data);
            };
            process.ErrorDataReceived += error_handler;

            try {
                process.Start();
                process.BeginErrorReadLine();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                if (output.ToStringFromBase64() is string jsonStr) {
                    object result = JsonConvert.DeserializeObject(jsonStr);
                    if (result is JObject jobj) {
                        resp = new MpAnalyzerPluginResponseFormat() {
                            errorMessage = jobj.SelectToken(nameof(MpAnalyzerPluginResponseFormat.errorMessage)).ToString(),
                            retryMessage = jobj.SelectToken(nameof(MpAnalyzerPluginResponseFormat.retryMessage)).ToString(),
                            dataObject = MpPortableDataObject.Parse(jobj.SelectToken(nameof(MpAnalyzerPluginResponseFormat.dataObject)).ToString()),
                            annotations = MpJsonConverter.DeserializeObject<List<MpPluginResponseAnnotationFormat>>(
                                jobj.SelectToken(nameof(MpAnalyzerPluginResponseFormat.annotations)).ToString()),
                        };
                    }
                }
                //resp = MpJsonConverter.DeserializeBase64Object<MpAnalyzerPluginResponseFormat>(output, settings: new Newtonsoft.Json.JsonSerializerSettings() { Converters = new[] { new MpPortableDataFormatConverter() } });
            }
            catch (Exception ex) {
                if (!string.IsNullOrEmpty(error_and_ex_sb.ToString())) {
                    error_and_ex_sb.AppendLine();
                }
                error_and_ex_sb.AppendLine(ex_str.ToString());
            }
            process.ErrorDataReceived -= error_handler;
            process.Dispose();

            if (resp == null) {
                resp = new MpAnalyzerPluginResponseFormat();
                if (error_and_ex_sb.ToString() is string errorMsg &&
                    !string.IsNullOrEmpty(errorMsg)) {
                    resp.errorMessage = errorMsg;
                } else {
                    resp.otherMessage = "NO RESPONSE";
                }
            }

            return resp;
        }

        private void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine) {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            Console.WriteLine(outLine.Data);
        }
    }
}
