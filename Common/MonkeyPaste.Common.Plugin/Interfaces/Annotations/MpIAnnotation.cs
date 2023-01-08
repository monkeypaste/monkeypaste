using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIAnnotation {
        double score { get; set; }
        string label { get; set; }
        string type { get; set; }
        string body { get; set; }
    }
    public interface MpIAnnotationNode : MpIAnnotation {
        IEnumerable<MpIAnnotation> children { get; }
    }
}
