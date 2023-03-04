using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace WpfTextToSpeech {
    public class WpfTextToSpeechPlugin :
        MpIAnalyzeComponent,
        MpISupportDeferredValue {
        private const int TEXT_PARAM_ID = 1;
        private const int VOICE_PARAM_ID = 2;

        private SpeechSynthesizer speechSynthesizer;
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            string text = req.GetRequestParamStringValue(TEXT_PARAM_ID) ?? string.Empty;
            string voice_name = req.GetRequestParamStringValue(VOICE_PARAM_ID);

            speechSynthesizer = new SpeechSynthesizer();
            if (!string.IsNullOrEmpty(voice_name)) {
                // when no voice provided, use default
                speechSynthesizer.SelectVoice(voice_name);
            }
            speechSynthesizer.SetOutputToDefaultAudioDevice();
            speechSynthesizer.Rate = 0;
            speechSynthesizer.SpeakCompleted += SpeechSynthesizer_SpeakCompleted;
            // Create a PromptBuilder object and append a text string.
            PromptBuilder promptBuilder = new PromptBuilder();

            promptBuilder.AppendText(Environment.NewLine + text);

            // Speak the contents of the prompt asynchronously.
            speechSynthesizer.SpeakAsync(promptBuilder);

            return new MpAnalyzerPluginResponseFormat();
        }

        private void SpeechSynthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e) {
            speechSynthesizer.Dispose();
            speechSynthesizer = null;
        }

        public MpPluginDeferredParameterValueResponseFormat RequestParameterValue(MpPluginDeferredParameterValueRequestFormat req) {
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
                    value = x.VoiceInfo.Name,
                    label = x.VoiceInfo.Name.Replace(@"Microsoft ", string.Empty).Replace(@" Desktop", string.Empty)
                }).ToList()
            };

            return resp;
        }
    }
}
