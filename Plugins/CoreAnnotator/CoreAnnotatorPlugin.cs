using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreAnnotator {
    public enum TextAnnotationType {
        Currency,
        Email,
        HexColor,
        PhoneNumber,
        Url,
    }
    public class CoreAnnotatorPlugin : MpIAnalyzeComponent, MpISupportHeadlessAnalyzerFormat {

        const string CONTENT_PARAM_ID = "content";
        const string OPTIONS_PARAM_ID = "options";
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            if (req.GetParamValue(CONTENT_PARAM_ID) is not string content_pt ||
                string.IsNullOrWhiteSpace(content_pt)) {
                // nothing to annotate
                return null;
            }
            // ensure consistent line endings
            content_pt = content_pt.Replace(Environment.NewLine, "\n");

            // select all checked ann formats
            var formats =
                req.GetParamValue<List<string>>(OPTIONS_PARAM_ID)
                //req.items.Where(x => x.paramId != CONTENT_PARAM_ID && x.paramValue.ParseOrConvertToBool())
                .Select(x => Enum.Parse(typeof(TextAnnotationType), x))
                .Cast<TextAnnotationType>();

            // create delta of all annotation ranges from plain text
            // more info on delta here: https://github.com/quilljs/delta
            MpQuillDelta delta = DeltaAnnotator.Annotate(content_pt, formats);
            MpConsole.WriteLine($"annotation Count: {delta.ops.Count} types: {string.Join(",", delta.ops.Select(x => x.attributes.linkType))}");

            var resp = new MpAnalyzerPluginResponseFormat();
            resp.dataObjectLookup = new Dictionary<string, object>();
            if (delta.ops.Count > 0) {
                // content was annotated, return it as dataobjectitem
                resp.dataObjectLookup.Add(
                    MpPortableDataFormats.INTERNAL_CONTENT_DELTA_FORMAT,
                    delta.SerializeObject(omitNulls: true));
            }
            // return annotation delta to be applied to request clip
            return resp;
        }
        public MpAnalyzerComponent GetFormat(MpHeadlessComponentFormatRequest request) {
            Resources.Culture = new System.Globalization.CultureInfo(request.culture);

            return new MpAnalyzerComponent() {
                inputType = new MpPluginInputFormat() {
                    text = true
                },
                outputType = new MpPluginOutputFormat() {
                    textAnnotation = true
                },
                presets = new List<MpPresetFormat>() {
                    // NOTE this preset is only needed so its preset.guid is always known
                    new MpPresetFormat() {
                        guid = "a9fa2fbf-025d-4ced-a23b-234085b5ac5f",
                        label = Resources.DefAnnLabel,
                        description = Resources.DefAnnHint,
                        values =
                            new[] {
                                new MpPresetValueFormat(
                                    OPTIONS_PARAM_ID,
                                    "Currency,Email,HexColor,PhoneNumber,Url")
                            }.ToList()
                    }
                },
                parameters = new List<MpParameterFormat>() {
                    new MpParameterFormat() {
                        label = "PlainText to annotate",
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainTextContentQuery,
                        paramId = CONTENT_PARAM_ID,
                        isVisible = false,
                        value = new MpParameterValueFormat("{ClipText}",true)
                    },
                    new MpParameterFormat() {
                        label = Resources.OptionsLabel,
                        description = Resources.OptionsDescription,
                        controlType = MpParameterControlType.MultiSelectList,
                        unitType = MpParameterValueUnitType.PlainText,
                        paramId = OPTIONS_PARAM_ID,
                        values = new (string,string)[] {
                            (TextAnnotationType.Currency.ToString(),Resources.CurrencyLabel),
                            (TextAnnotationType.Email.ToString(),Resources.EmailLabel),
                            (TextAnnotationType.HexColor.ToString(),Resources.HexLabel),
                            (TextAnnotationType.PhoneNumber.ToString(),Resources.PhoneLabel),
                            (TextAnnotationType.Url.ToString(),Resources.UrlLabel),
                        }.Select(x=>new MpParameterValueFormat(x.Item1,x.Item2,true)).ToList()
                    }
                }
            };
        }
    }
}