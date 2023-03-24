using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvCoreAnnotator {
    public enum AvAnnnotaterParameterType {
        None = 0,
        HtmlContent,
        PlainTextContent,
        FileOrFolder,
        Uri,
        Email,
        PhoneNumber,
        Currency,
        HexColor,
        StreetAddress,
    }

    public class AvCoreAnnotatorPlugin : MpIAnalyzeComponent {
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            var resp = new MpAnalyzerPluginResponseFormat();

            string content_pt = req.GetRequestParamStringValue((int)AvAnnnotaterParameterType.PlainTextContent);
            if (string.IsNullOrWhiteSpace(content_pt)) {
                return null;
            }
            content_pt = content_pt.Replace(Environment.NewLine, "\n");
            var formats = new List<MpRegExType>();
            for (int i = 3; i < Enum.GetValues(typeof(AvAnnnotaterParameterType)).Length; i++) {
                if (req.GetRequestParamBoolValue(i)) {
                    formats.Add((MpRegExType)i - 2);
                }
            }
            MpQuillDelta delta = DeltaAnnotator.Annotate(content_pt, formats);
            MpConsole.WriteLine($"annotation Count: {delta.ops.Count} types: {string.Join(",", delta.ops.Select(x => x.attributes.linkType))}");

            if (delta.ops.Count > 0) {
                // content was annotated, return it as dataobjectitem
                resp.dataObject = new MpPortableDataObject(
                    MpPortableDataFormats.INTERNAL_CONTENT_DELTA_FORMAT, delta.SerializeJsonObject());
                return resp;
            }

            return null;
        }
    }
}