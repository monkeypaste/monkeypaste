using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginResponseAnnotationFormat : MpPluginResponseItemBaseFormat {
        public static List<MpPluginResponseAnnotationFormat> ParseAnnotations(string json) {
            List<MpPluginResponseAnnotationFormat> anns = new List<MpPluginResponseAnnotationFormat>();
            if (JsonConvert.DeserializeObject(json) is JToken jtoken) {
                JArray jarr = jtoken as JArray;
                if (jarr == null && jtoken is JObject jobj) {
                    jarr = new JArray(jobj);
                }
                if (jarr == null) {
                    return anns;
                }
                foreach (var ann_frag in jarr) {
                    if (ann_frag.SelectToken(MpPluginR))
                        var anf = MpAnnotationNodeFormat.Parse(ann_frag.ToString());
                    if (anf is MpImageAnnotationNodeFormat imnf)
                        anns.Add(new MpPluginResponseAnnotationFormat())
                }
            }
            return anns;
        }


        public MpAnalyzerPluginImageTokenResponseValueFormat box { get; set; }

        public MpTextAnnotationFormat range { get; set; }
    }

}
