using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpAnnotationNodeFormat : 
        MpJsonObject, 
        MpILabelText,
        MpIIconResource, MpIClampedValue,
        MpIAnnotationNode {
        public string type { get; set; }

        #region MpIIconResource Implementation
        public object IconResourceObj { get; set; }
        #endregion

        #region MpILabelText Implementation
        public string label { get; set; }
        string MpILabelText.LabelText => label;
        #endregion

        #region MpIClampedValue Implementation
        double MpIClampedValue.min => minScore;
        double MpIClampedValue.max => maxScore;
        double MpIClampedValue.value => score;

        public double minScore { get; set; } = 0;
        public double maxScore { get; set; } = 1;
        public double score { get; set; }
        #endregion

        public string body { get; set; }
        public string footer { get; set; }

        public IEnumerable<MpITreeNode> Children { get; set; }
        public bool IsExpanded { get; set; }
    }
       
    public class MpContentElementFormat : MpIContentElement {
        public string type { get; set; }
        public string content { get; set; }
        public string bgColor { get; }
        public string fgColor { get; }
        public string fontSize { get; }
        public string fontFamily { get; }
        public string fontWeight { get; }
        
        
    }

}
