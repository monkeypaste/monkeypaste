using MonkeyPaste.Common.Plugin;
using System;
using System.Text;

namespace MonkeyPaste.Common.Plugin {
    public class MpImageAnnotationNodeFormat : 
        MpAnnotationNodeFormat, 
        MpIRectangle{

        #region MpIRectangle Implementation

        public double x { get; set; }
        public double y { get; set; }
        public double width { get; set; }
        public double height { get; set; }

        public MpRect box => new MpRect(x, y, width, height);
        #endregion


    }

    public class MpTextAnnotationNodeFormat : 
        MpAnnotationNodeFormat, MpITextRange {

        #region MpITextRange Implementation
        public int Offset { get; set; }
        public int Length { get; set; }
        #endregion
    }
}
