namespace MonkeyPaste.Common.Plugin {
    public class MpPluginResponseAppearanceFormat : MpJsonObject {
        public MpJsonPathProperty foregroundColor { get; set; } = new MpJsonPathProperty("#FF000000");
        public MpJsonPathProperty backgroundColor { get; set; } = new MpJsonPathProperty("#FFFFFFFF");

        public MpPluginResponseFontAppearanceFormat font { get; set; } = new MpPluginResponseFontAppearanceFormat();

        public bool isList { get; set; }
        public bool isNumberedList { get; set; }

        public bool isBarChart { get; set; }
        public bool isBarChartItem { get; set; }

        public bool isPieChart { get; set; }
        public bool isPieChartItem { get; set; }

        public bool isScatterChart { get; set; }
        public bool isScatterChartItem { get; set; }

        public string columnGroup { get; set; }
    }

}
