using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpCommandLinePlugin : MpDeclarativePluginBase {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        public static string SCRIPTS_ROOT_DIR =>
            Path.Combine(MpPlatformHelpers.GetSolutionDir(), "MonkeyPaste.Avalonia", "scripts");
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        public string CommandPath { get; set; }
        public string BundlePath { get; set; }
        #endregion

        #region Constructors
        public MpCommandLinePlugin() { }
        public MpCommandLinePlugin(string targetPath) {
            BundlePath = targetPath;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        protected override async Task<string> RunDeclarativeAnalyzerAsync(MpAnalyzerPluginRequestFormat req) {
            await Task.Delay(1);
            string resp = RunDeclarativeAnalyzer(req);
            return resp;
        }

        protected override string RunDeclarativeAnalyzer(MpAnalyzerPluginRequestFormat req) {
            Process process = new Process();
            process.StartInfo.FileName = CommandPath;
            process.StartInfo.Arguments = BundlePath + " " + req.SerializeObjectToBase64();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

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
                return output;
            }
            catch (Exception ex) {
                if (!string.IsNullOrEmpty(error_and_ex_sb.ToString())) {
                    error_and_ex_sb.AppendLine();
                }
                error_and_ex_sb.AppendLine(ex.ToString());
            }
            process.ErrorDataReceived -= error_handler;
            process.Dispose();

            return error_and_ex_sb.ToString();
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion



    }
}
