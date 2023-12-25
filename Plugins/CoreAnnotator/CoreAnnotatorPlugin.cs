using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
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
    public class CoreAnnotatorPlugin : MpIAnalyzeComponent, MpISupportHeadlessAnalyzerComponentFormat {
        const int FIRST_FORMAT_IDX = 2;
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            var resp = new MpAnalyzerPluginResponseFormat();

            string content_pt = req.GetRequestParamStringValue("plaintext");
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
                if (rt == MpRegExType.None || !req.GetRequestParamBoolValue(param.paramId)) {
                    continue;
                }
                formats.Add(rt);
            }
            MpQuillDelta delta = DeltaAnnotator.Annotate(content_pt, formats);
            MpConsole.WriteLine($"annotation Count: {delta.ops.Count} types: {string.Join(",", delta.ops.Select(x => x.attributes.linkType))}");

            resp.dataObjectLookup = new Dictionary<string, object>();
            if (delta.ops.Count > 0) {
                // content was annotated, return it as dataobjectitem
                resp.dataObjectLookup.Add(MpPortableDataFormats.INTERNAL_CONTENT_DELTA_FORMAT, delta.SerializeJsonObject());

            }
            return resp;
        }

        public MpAnalyzerPluginFormat GetFormat(MpHeadlessAnalyzerComponentFormatRequest request) {
            Resources.Culture = new System.Globalization.CultureInfo(request.cultureCode);

            return new MpAnalyzerPluginFormat() {
                inputType = new MpPluginInputFormat() {
                    text = true
                },
                outputType = new MpPluginOutputFormat() {
                    textAnnotation = true
                },
                presets = new List<MpPluginPresetFormat>() {
                    new MpPluginPresetFormat() {
                        guid = "a9fa2fbf-025d-4ced-a23b-234085b5ac5f",
                        label = Resources.DefAnnLabel,
                        description = Resources.DefAnnHint,
                        values = new[] {
                            new MpPluginPresetValueFormat(true.ToString(), MpRegExType.Url.ToFlagNamesCsvString()),
                            new MpPluginPresetValueFormat(true.ToString(), MpRegExType.Email.ToFlagNamesCsvString()),
                            new MpPluginPresetValueFormat(true.ToString(), MpRegExType.PhoneNumber.ToFlagNamesCsvString()),
                            new MpPluginPresetValueFormat(true.ToString(), MpRegExType.Currency.ToFlagNamesCsvString()),
                            new MpPluginPresetValueFormat(true.ToString(), MpRegExType.HexColor.ToFlagNamesCsvString())
                        }.ToList()
                    }
                },
                parameters = new List<MpParameterFormat>() {
                    new MpParameterFormat() {
                        label = "Html to annotate",
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.RawDataContentQuery,
                        paramId = "content",
                        isVisible = false,
                        value = new MpPluginParameterValueFormat("{ItemData}",true)
                    },
                    new MpParameterFormat() {
                        label = "PlainTexr to annotate",
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainTextContentQuery,
                        paramId = "plaintext",
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