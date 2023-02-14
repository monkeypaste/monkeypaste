namespace MonkeyPaste.Common.Plugin {

    public class MpPluginResponseFontAppearanceFormat : MpJsonObject {
        public string fontFamily { get; set; } = "Consolas";
        public string fontSize { get; set; } = "medium"; //xx-small,x-small,small,medium,large,x-large,xx-large,xxx-large


        public bool isBold { get; set; }
        public bool isItalic { get; set; }
        public bool isUnderlined { get; set; }
        public bool isStrikethough { get; set; }
    }

}
