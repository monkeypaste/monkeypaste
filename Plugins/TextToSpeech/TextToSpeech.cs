using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TextToSpeech {
    public class TextToSpeechPlugin :
        MpIAnalyzeComponent,
        MpISupportHeadlessAnalyzerFormat {
        const string TEXT_PARAM_ID = "1";

        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            if (req == null ||
                req.GetParamValue<string>(TEXT_PARAM_ID) is not string text) {
                return null;
            }
            try {
                // from https://stackoverflow.com/a/39647762/105028

                string ps_path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "System32", "WindowsPowerShell", "v1.0", "powershell.exe");
                if (!ps_path.IsFile()) {
                    throw new Exception("cannot speak");
                }
                var proc = new Process();
                proc.StartInfo.FileName = ps_path;
                proc.StartInfo.Arguments = $"-Command \"Add-Type –AssemblyName System.Speech; (New-Object System.Speech.Synthesis.SpeechSynthesizer).Speak('{text}');";
                proc.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.Start();
                string proc_output = proc.StandardOutput.ReadToEnd();

                proc.WaitForExit();
                int exit_code = proc.ExitCode;
                proc.Close();

                if (exit_code != 0) {
                    throw new Exception($"Cannot speak.{Environment.NewLine}Error #{exit_code}{Environment.NewLine}{proc_output}");
                }

            }
            catch (Exception ex) {
                return new MpAnalyzerPluginResponseFormat() {
                    userNotifications = new[] {
                            new MpPluginUserNotificationFormat() {
                                NotificationType = MpPluginNotificationType.PluginResponseError,
                                Title = "Web Search Error",
                                Body = ex.Message,
                                IconSourceObj = MpBase64Images.Error
                            }
                        }.ToList()
                };
            }
            return null;
        }
        public MpAnalyzerPluginFormat GetFormat(MpHeadlessComponentFormatRequest request) {
            return new MpAnalyzerPluginFormat() {
                inputType = new MpPluginInputFormat() {
                    text = true
                },
                parameters = new List<MpParameterFormat>() {
                    new MpParameterFormat() {
                        label = "Text to say",
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainTextContentQuery,
                        paramId = TEXT_PARAM_ID,
                        values = new List<MpPluginParameterValueFormat>() {
                            new MpPluginParameterValueFormat() {
                                value = "{ItemData}"
                            }
                        }
                    },
                }
            };
        }
    }
}
