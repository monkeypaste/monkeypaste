using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace TextToSpeech {
    public class TextToSpeechPlugin_codebased :
        MpIAnalyzeComponent,
        MpISupportHeadlessAnalyzerFormat
        , MpISupportDeferredValue {
        const string TEXT_PARAM_ID = "1";
        const string VOICE_PARAM_ID = "2";
        const string VOLUME_PARAM_ID = "3";
        SpeechSynthesizer speechSynthesizer;


        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            _ = Task.Run(() => {
                if (!OperatingSystem.IsWindows()) {
                    return;
                }
                string text = req.GetParamValue<string>(TEXT_PARAM_ID.ToString()) ?? string.Empty;
                string voice_name = req.GetParamValue<string>(VOICE_PARAM_ID.ToString());
                int volume = req.GetParamValue<int>(VOLUME_PARAM_ID.ToString());

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

        public MpAnalyzerPluginFormat GetFormat(MpHeadlessComponentFormatRequest request) {
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
                        paramId = TEXT_PARAM_ID,
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
                        paramId = VOICE_PARAM_ID,
                        isValueDeferred = true
                    },
                    new MpParameterFormat() {
                        label = "Volume",
                        controlType = MpParameterControlType.Slider,
                        unitType = MpParameterValueUnitType.Integer,
                        isVisible = true,
                        paramId = VOLUME_PARAM_ID,
                        minimum = 0,
                        maximum = 100,
                        value = new MpPluginParameterValueFormat("50",true)
                    }
                }
            };
        }
    }
}
