using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace TextToSpeech {
    public class TextToSpeechPlugin :
        MpIAnalyzeComponent,
        MpISupportHeadlessAnalyzerComponentFormat,
        MpISupportDeferredValue {
        const int TEXT_PARAM_ID = 1;
        const int VOICE_PARAM_ID = 2;
        const int VOLUME_PARAM_ID = 3;

        SpeechSynthesizer speechSynthesizer;

        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            _ = Task.Run(() => {
                if (!OperatingSystem.IsWindows()) {
                    return;
                }
                string text = req.GetRequestParamStringValue(TEXT_PARAM_ID) ?? string.Empty;
                string voice_name = req.GetRequestParamStringValue(VOICE_PARAM_ID);
                int volume = req.GetRequestParamIntValue(VOLUME_PARAM_ID);

                if (speechSynthesizer == null) {
                    speechSynthesizer = new SpeechSynthesizer();
                    speechSynthesizer.SpeakCompleted += SpeechSynthesizer_SpeakCompleted;
                }
                if (!string.IsNullOrEmpty(voice_name)) {
                    // when no voice provided, use default
                    speechSynthesizer.SelectVoice(voice_name);
                }
                speechSynthesizer.SetOutputToDefaultAudioDevice();
                speechSynthesizer.Volume = volume;
                //speechSynthesizer.Rate = 0;
                //speechSynthesizer.Speak(text);

                PromptBuilder promptBuilder = new PromptBuilder();
                promptBuilder.AppendText(text);
                speechSynthesizer.SpeakAsync(promptBuilder);
            });

            return null;
        }

        private void SpeechSynthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e) {
            if (!OperatingSystem.IsWindows()) {
                return;
            }
            speechSynthesizer.Dispose();
            speechSynthesizer = null;
        }

        public MpPluginDeferredParameterValueResponseFormat RequestParameterValue(MpPluginDeferredParameterValueRequestFormat req) {

            if (!OperatingSystem.IsWindows()) {
                return null;
            }
            if (req.paramId != VOICE_PARAM_ID.ToString()) {
                return new MpPluginDeferredParameterValueResponseFormat() {
                    errorMessage = $"Unknown deferred parameter id '{req.paramId}'"
                };
            }

            var speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.SetOutputToDefaultAudioDevice();
            var voices = speechSynthesizer.GetInstalledVoices();

            var resp = new MpPluginDeferredParameterValueResponseFormat() {
                Values = voices.Select((x, idx) => new MpPluginParameterValueFormat() {
                    isDefault = idx == 0,
                    value = OperatingSystem.IsWindows() ? x.VoiceInfo.Name : null,
                    label = OperatingSystem.IsWindows() ? x.VoiceInfo.Name.Replace(@"Microsoft ", string.Empty).Replace(@" Desktop", string.Empty) : null
                }).ToList()
            };

            return resp;
        }

        public MpAnalyzerPluginFormat GetFormat(MpHeadlessAnalyzerComponentFormatRequest request) {
            return new MpAnalyzerPluginFormat() {
                inputType = new MpPluginInputFormat() {
                    text = true
                },
                parameters = new List<MpParameterFormat>() {
                    new MpParameterFormat() {
                        label = "Text to say",
                        description = "Will speak text with selected voice",
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainTextContentQuery,
                        paramId = "1",
                        values = new List<MpPluginParameterValueFormat>() {
                            new MpPluginParameterValueFormat() {
                                value = "{ItemData}"
                            }
                        }
                    },
                    new MpParameterFormat() {
                        label = "Voice",
                        description = "One of the available voices in windows",
                        controlType = MpParameterControlType.ComboBox,
                        unitType = MpParameterValueUnitType.PlainText,
                        isVisible = true,
                        paramId = "2",
                        isValueDeferred = true
                    },
                    new MpParameterFormat() {
                        label = "Volume",
                        controlType = MpParameterControlType.Slider,
                        unitType = MpParameterValueUnitType.Integer,
                        isVisible = true,
                        paramId = "3",
                        minimum = 0,
                        maximum = 100,
                        value = new MpPluginParameterValueFormat("50",true)
                    }
                }
            };
        }
    }
}
