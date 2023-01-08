using MonkeyPaste.Common.Plugin;
using System;
using System.Text;

namespace MonkeyPaste.Common.Plugin {
    public class ImageAnnotationNodeFormat : MpAnnotationNodeFormat, MpIImageRange {
        public double x { get; set; }
        public double y { get; set; }
        public double width { get; set; }
        public double height { get; set; }
    }
}
