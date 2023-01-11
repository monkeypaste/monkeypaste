using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpAnnotationNodeFormat : 
        MpJsonObject, 
        MpIAnnotationNode {
        public double score { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public string body { get; set; }
        public IEnumerable<MpIAnnotation> children { get; set; }
    }
}
