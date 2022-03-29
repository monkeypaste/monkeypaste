using MonkeyPaste.Plugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public enum MpTextFlowDirectionType {
        LeftToRight = 0,
        RightToLeft
    }

    public class MpTextFormatInfoFormat : MpJsonObject {
        public string cultureInfo { get; set; } = "en-US";

        public MpTextFlowDirectionType flowDirection { get; set; } = MpTextFlowDirectionType.LeftToRight;

        public string fgHexColor { get; set; } = "#FF000000";

        public string bgHexColor { get; set; } = "#FFFFFFFF";

        public double fontSize { get; set; } = 12.0d;

        public double PixelsPerDip { get; set; } = 1.0d;

    }

    public class MpTypeFaceInfoFormat : MpJsonObject {
        public string fontFamily { get; set; } = "Arial";

        public string fontStyle { get; set; } = "Normal";

        public string fontWeight { get; set; } = "Normal";

        public string fontStretch { get; set; } = "Normal";

        public string fallbackFontFamily { get; set; } = "Arial";
    }
}
