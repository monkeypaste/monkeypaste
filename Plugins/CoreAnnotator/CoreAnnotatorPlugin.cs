using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreAnnotator {
    enum TextAnnotationTypes {
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
            var resp = new MpAnalyzerPluginResponseFormat();

            string content_pt = req.GetParamValue(CONTENT_PARAM_ID);
            if (string.IsNullOrWhiteSpace(content_pt)) {
                return null;
            }
            content_pt = content_pt.Replace(Environment.NewLine, "\n");
            var formats = new List<MpRegExType>();
            foreach (var (param, idx) in req.items.WithIndex()) {
                if (idx < FIRST_FORMAT_IDX) {
                    continue;
                }
                MpRegExType rt = param.paramId.ToStringOrEmpty().ToEnum<MpRegExType>();
                if (rt == MpRegExType.None || !req.GetParamValue<bool>(param.paramId)) {
                    continue;
                }
                formats.Add(rt);
            }
            MpQuillDelta delta = DeltaAnnotator.Annotate(content_pt, formats);
            MpConsole.WriteLine($"annotation Count: {delta.ops.Count} types: {string.Join(",", delta.ops.Select(x => x.attributes.linkType))}");

            resp.dataObjectLookup = new Dictionary<string, object>();
            if (delta.ops.Count > 0) {
                // content was annotated, return it as dataobjectitem
                resp.dataObjectLookup.Add(
                    MpPortableDataFormats.INTERNAL_CONTENT_DELTA_FORMAT,
                    JsonConvert.SerializeObject(
                        delta,
                        new JsonSerializerSettings() {
                            NullValueHandling = NullValueHandling.Ignore
                        }));

            }
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
                        values = new[] {
                            new MpPresetValueFormat(MpRegExType.Url.ToFlagNamesCsvString(), true.ToString()),
                            new MpPresetValueFormat(MpRegExType.Email.ToFlagNamesCsvString(), true.ToString()),
                            new MpPresetValueFormat(MpRegExType.PhoneNumber.ToFlagNamesCsvString(), true.ToString()),
                            new MpPresetValueFormat(MpRegExType.Currency.ToFlagNamesCsvString(), true.ToString()),
                            new MpPresetValueFormat(MpRegExType.HexColor.ToFlagNamesCsvString(), true.ToString())
                        }.ToList()
                    }
                },
                parameters = new List<MpParameterFormat>() {
                    new MpParameterFormat() {
                        label = "PlainTexr to annotate",
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainTextContentQuery,
                        paramId = CONTENT_PARAM_ID,
                        isVisible = false,
                        value = new MpPluginParameterValueFormat("{ItemData}",true)
                    },
                    new MpParameterFormat() {
                        label = Resources.UrlLabel,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        paramId = MpRegExType.Url.ToString(),
                        value = new MpPluginParameterValueFormat(true.ToString(),true)
                    },
                    new MpParameterFormat() {
                        label = Resources.EmailLabel,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        paramId = MpRegExType.Email.ToString(),
                        value = new MpPluginParameterValueFormat(true.ToString(),true)
                    },
                    new MpParameterFormat() {
                        label = Resources.PhoneLabel,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        paramId = MpRegExType.PhoneNumber.ToString(),
                        value = new MpPluginParameterValueFormat(true.ToString(),true)
                    },
                    new MpParameterFormat() {
                        label = Resources.CurrencyLabel,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        paramId = MpRegExType.Currency.ToString(),
                        value = new MpPluginParameterValueFormat(true.ToString(),true)
                    },
                    new MpParameterFormat() {
                        label = Resources.HexLabel,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        paramId = MpRegExType.HexColor.ToString(),
                        value = new MpPluginParameterValueFormat(true.ToString(),true)
                    },
                }
            };
        }
    }
}