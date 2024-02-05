using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TextToSpeech {
    public class TextToSpeechPlugin :
        MpIAnalyzeComponentAsync,
        MpISupportHeadlessAnalyzerFormat {
        const string TEXT_PARAM_ID = "1";

        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            if (req == null ||
                req.GetParamValue<string>(TEXT_PARAM_ID) is not string text) {
                return null;
            }
            (int code, string output) = await RunAsync(
                args: $"/c start /min \"\" powershell -windowstyle Hidden -executionpolicy bypass -Command \"Add-Type –AssemblyName System.Speech; (New-Object System.Speech.Synthesis.SpeechSynthesizer).Speak('{text}');\"");
            if (code != 0) {
                return new MpAnalyzerPluginResponseFormat() {
                    errorMessage = $"Error code: {code}{Environment.NewLine}{output}"
                };
            }
            return null;
        }

        public MpAnalyzerComponent GetFormat(MpHeadlessComponentFormatRequest request) {
            Resources.Culture = new System.Globalization.CultureInfo(request.culture);

            return new MpAnalyzerComponent() {
                inputType = new MpPluginInputFormat() {
                    text = true
                },
                parameters = new List<MpParameterFormat>() {
                    new MpParameterFormat() {
                        isVisible = false,
                        label = Resources.TextLabel,
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainTextContentQuery,
                        paramId = TEXT_PARAM_ID,
                        values = new List<MpParameterValueFormat>() {
                            new MpParameterValueFormat() {
                                value = "{ClipText}"
                            }
                        }
                    },
                }
            };
        }

        #region Cli Helpers
        string CmdExe => Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "System32",
                    "cmd.exe");
        string DefFileName =>
            CmdExe;
        string DefWorkingDir =>
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        async Task<(int, string)> RunAsync(
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

        Process CreateProcess(
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
        #endregion
    }
}
