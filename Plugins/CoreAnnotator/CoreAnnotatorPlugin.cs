using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreAnnotator {

    public class CoreAnnotatorPlugin : MpIAnalyzeComponent {
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

            if (delta.ops.Count > 0) {
                // content was annotated, return it as dataobjectitem
                resp.dataObjectLookup = new Dictionary<string, object>() {
                    { MpPortableDataFormats.INTERNAL_CONTENT_DELTA_FORMAT, delta.SerializeJsonObject() }
                };
                return resp;
            }

            return null;
        }
    }
}