using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreAnnotator {
    public enum TextAnnotationType {
        Url,
        Email,
        PhoneNumber,
        Currency,
        HexColor
    }
    public class CoreAnnotatorPlugin : MpIAnalyzeComponent, MpISupportHeadlessAnalyzerFormat {
        const int FIRST_FORMAT_IDX = 2;

        const string CONTENT_PARAM_ID = "content";
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            string content_pt = req.GetParamValue(CONTENT_PARAM_ID);
            if (string.IsNullOrWhiteSpace(content_pt)) {
                // nothing to annotate
                return null;
            }
            // ensure consistent line endings
            content_pt = content_pt.Replace(Environment.NewLine, "\n");

            // select all checked ann formats
            var formats =
                req.items.Where(x => x.paramId != CONTENT_PARAM_ID && x.paramValue.ParseOrConvertToBool())
                .Select(x => Enum.Parse(typeof(TextAnnotationType), x.paramId))
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
                    new MpPresetFormat() {
                        guid = "a9fa2fbf-025d-4ced-a23b-234085b5ac5f",
                        label = Resources.DefAnnLabel,
                        description = Resources.DefAnnHint,
                        values =
                            typeof(TextAnnotationType)
                            .GetEnumNames()
                            .Select(x=>new MpPresetValueFormat(x, true.ToString()))
                            .ToList()
                    }
                },
                parameters = new List<MpParameterFormat>() {
                    new MpParameterFormat() {
                        label = "PlainText to annotate",
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainTextContentQuery,
                        paramId = CONTENT_PARAM_ID,
                        isVisible = false,
                        value = new MpParameterValueFormat("{ItemData}",true)
                    },
                    new MpParameterFormat() {
                        label = Resources.UrlLabel,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        paramId = TextAnnotationType.Url.ToString(),
                        value = new MpParameterValueFormat(true.ToString(),true)
                    },
                    new MpParameterFormat() {
                        label = Resources.EmailLabel,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        paramId = TextAnnotationType.Email.ToString(),
                        value = new MpParameterValueFormat(true.ToString(),true)
                    },
                    new MpParameterFormat() {
                        label = Resources.PhoneLabel,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        paramId = TextAnnotationType.PhoneNumber.ToString(),
                        value = new MpParameterValueFormat(true.ToString(),true)
                    },
                    new MpParameterFormat() {
                        label = Resources.CurrencyLabel,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        paramId = TextAnnotationType.Currency.ToString(),
                        value = new MpParameterValueFormat(true.ToString(),true)
                    },
                    new MpParameterFormat() {
                        label = Resources.HexLabel,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        paramId = TextAnnotationType.HexColor.ToString(),
                        value = new MpParameterValueFormat(true.ToString(),true)
                    },
                }
            };
        }
    }
}