using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public abstract class MpAnnotationBaseFormat : 
        MpJsonObject, 
        MpIAnnotationNode {
        public string annotationType { get; set; }
        public double score { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public string body { get; set; }
        public IEnumerable<MpITreeNode> Children { get; }
        public bool IsExpanded { get; set; }
    }

}
