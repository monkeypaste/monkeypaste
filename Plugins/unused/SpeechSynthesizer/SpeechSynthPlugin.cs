using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SpeechSynth {
    public class SpeechSynthPlugin :
        MpIAnalyzeComponentAsync,
        MpISupportHeadlessAnalyzerFormat,
        MpISupportDeferredValue {
        private SpeechSynthesizer synthesizer;
        private ResourceContext speechContext;
        private MediaElement media;

        const string TEXT_PARAM_ID = "text_to_say";
        const string VOICE_ID_PARAM_ID = "voice_id_to_speak";
        const string VOLUME_PARAM_ID = "voice_volume";

        public SpeechSynthPlugin() {
        }

        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            string text = req.GetParamValue<string>(TEXT_PARAM_ID, string.Empty);
            string voice_id = req.GetParamValue<string>(VOICE_ID_PARAM_ID);
            double volume = req.GetParamValue<double>(VOLUME_PARAM_ID, 0.5);

            if (string.IsNullOrEmpty(text)) {
                return null;
            }

            if (media == null) {
                media = new MediaElement() {
                    AutoPlay = false
                };
                synthesizer = new SpeechSynthesizer();

                speechContext = ResourceContext.GetForCurrentView();
                speechContext.Languages = new string[] { SpeechSynthesizer.DefaultVoice.Language };
            }

            // If the media is playing, the user has pressed the button to stop the playback.
            if (media.CurrentState == MediaElementState.Playing) {
                media.Stop();
            }
            try {
                var voice = SpeechSynthesizer.AllVoices.FirstOrDefault(x => x.Id == voice_id);
                synthesizer.Voice = voice;

                // update UI text to be an appropriate default translation.
                speechContext.Languages = new string[] { voice.Language };

                // Create a stream from the text. This will be played using a media element.
                SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(text);

                // Set the source and start playing the synthesized audio stream.
                media.AutoPlay = true;
                media.Volume = volume;
                media.SetSource(synthesisStream, synthesisStream.ContentType);
                media.Play();
            }
            catch (System.IO.FileNotFoundException) {
                // If media player components are unavailable, (eg, using a N SKU of windows), we won't
                // be able to start media playback. Handle this gracefully
                return new MpAnalyzerPluginResponseFormat() {
                    errorMessage = "Media player components unavailable"
                };
            }
            catch (Exception) {
                // If the text is unable to be synthesized, throw an error message to the user.
                media.AutoPlay = false;
                return new MpAnalyzerPluginResponseFormat() {
                    errorMessage = "Unable to synthesize text"
                };
            }

            return null;
        }

        public MpPluginDeferredParameterValueResponseFormat RequestParameterValue(MpPluginDeferredParameterValueRequestFormat req) {
            if (req.paramId != VOICE_ID_PARAM_ID.ToString()) {
                return new MpPluginDeferredParameterValueResponseFormat() {
                    errorMessage = $"Unknown deferred parameter id '{req.paramId}'"
                };
            }

            var voices = SpeechSynthesizer.AllVoices;
            VoiceInformation currentVoice = synthesizer.Voice;

            var resp = new MpPluginDeferredParameterValueResponseFormat() {
                Values = voices.OrderBy(x => x.Language).Select((x, idx) => new MpPluginParameterValueFormat() {
                    isDefault = currentVoice == null ? idx == 0 : x.Id == currentVoice.Id,
                    value = x.Id,
                    label = x.DisplayName
                }).ToList()
            };

            return resp;
        }

        public MpAnalyzerPluginFormat GetFormat(MpHeadlessComponentFormatRequest request) {
            return new MpAnalyzerPluginFormat() {
                inputType = new MpPluginInputFormat() {
                    text = true
                },
                parameters = new[] {
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
                        paramId = VOICE_ID_PARAM_ID,
                        isValueDeferred = true
                    },
                    new MpParameterFormat() {
                        label = "Volume",
                        controlType = MpParameterControlType.Slider,
                        unitType = MpParameterValueUnitType.Decimal,
                        isVisible = true,
                        paramId = VOLUME_PARAM_ID,
                        value = new MpPluginParameterValueFormat("0.5",true)
                    },
                }.ToList()
            };
        }
    }
}
