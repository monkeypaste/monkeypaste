using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
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
            (int code, string output) = await MpCli.RunAsync(
                args: $"/c start /min \"\" powershell -windowstyle Hidden -executionpolicy bypass -Command \"Add-Type –AssemblyName System.Speech; (New-Object System.Speech.Synthesis.SpeechSynthesizer).Speak('{text}');\"");
            if (code != 0) {
                return new MpAnalyzerPluginResponseFormat() {
                    errorMessage = $"Error code: {code}{Environment.NewLine}{output}"
                };
            }
            return null;
        }
        public MpAnalyzerComponent GetFormat(MpHeadlessComponentFormatRequest request) {
            return new MpAnalyzerComponent() {
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
