using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginResponseFormatBase : MpJsonObject {
        public string errorMessage { get; set; }
        public string retryMessage { get; set; }

        public string otherMessage { get; set; }

        public List<MpPluginUserNotificationFormat> userNotifications { get; set; } = new List<MpPluginUserNotificationFormat>();

        public MpPluginResponseNewContentFormat newContentItem { get; set; }
        public List<MpPluginResponseAnnotationFormat> annotations { get; set; } = new List<MpPluginResponseAnnotationFormat>();

        public MpPortableDataObject dataObject { get; set; }
    }

    public class MpPluginResponseItemBaseFormat : MpJsonObject {
        public string name { get; set; } = string.Empty;

        public MpJsonPathProperty queryPath { get; set; } = new MpJsonPathProperty("$");

        public MpJsonPathProperty label { get; set; } = new MpJsonPathProperty(string.Empty);

        public MpPluginResponseAppearanceFormat appearance { get; set; } = new MpPluginResponseAppearanceFormat();

        public MpJsonPathProperty<double> score { get; set; } = new MpJsonPathProperty<double>();

        public double minScore { get; set; } = 0;
        public double maxScore { get; set; } = 1;

        public List<MpPluginResponseAnnotationFormat> children { get; set; } = new List<MpPluginResponseAnnotationFormat>();
        public List<MpPluginResponseAnnotationFormat> dynamicChildren { get; set; } = new List<MpPluginResponseAnnotationFormat>();

        public MpPluginResponseItemBaseFormat() { }

        public MpPluginResponseItemBaseFormat(string content) : this(content, 1) { }
        public MpPluginResponseItemBaseFormat(double score) : this(string.Empty, score) { }
        public MpPluginResponseItemBaseFormat(string label, double score) {
            this.label = new MpJsonPathProperty(label);
            this.score = new MpJsonPathProperty<double>(score);
        }
    }

    public class MpPluginResponseNewContentFormat : MpPluginResponseItemBaseFormat {
        public string format { get; set; }
        public MpJsonPathProperty content { get; set; } = new MpJsonPathProperty(string.Empty);

        public bool isRequestChild { get; set; } = true;

        public List<MpPluginResponseAnnotationFormat> annotations { get; set; } = new List<MpPluginResponseAnnotationFormat>();
    }

    public class MpPluginResponseAnnotationFormat : MpPluginResponseItemBaseFormat {
        public MpAnalyzerPluginImageTokenResponseValueFormat box { get; set; }

        public MpAnalyzerPluginTextTokenResponseValueFormat range { get; set; }
    }

    public class MpAnalyzerPluginImageTokenResponseValueFormat : MpJsonObject {
        public MpJsonPathProperty<double> x { get; set; } = new MpJsonPathProperty<double>(0);
        public MpJsonPathProperty<double> y { get; set; } = new MpJsonPathProperty<double>(0);
        public MpJsonPathProperty<double> width { get; set; } = new MpJsonPathProperty<double>(0);
        public MpJsonPathProperty<double> height { get; set; } = new MpJsonPathProperty<double>(0);


        public MpAnalyzerPluginImageTokenResponseValueFormat() { }
        public MpAnalyzerPluginImageTokenResponseValueFormat(double x, double y, double w, double h) {
            this.x = new MpJsonPathProperty<double>(x);
            this.y = new MpJsonPathProperty<double>(y);
            width = new MpJsonPathProperty<double>(w);
            height = new MpJsonPathProperty<double>(h);
        }

        public override string ToString() {
            if (x == null || y == null || width == null || height == null) {
                return base.ToString();
            }
            return string.Format(@"x:{0} y:{1} w:{2} h:{4}", x, y, width, height);
        }
    }

    public class MpAnalyzerPluginTextTokenResponseValueFormat : MpJsonObject {
        public MpJsonPathProperty<int> rangeStart { get; set; }
        public MpJsonPathProperty<int> rangeLength { get; set; }

        public MpAnalyzerPluginTextTokenResponseValueFormat() { }
        public MpAnalyzerPluginTextTokenResponseValueFormat(int start, int end) {
            rangeStart = new MpJsonPathProperty<int>(start);
            rangeLength = new MpJsonPathProperty<int>(end);
        }
    }

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

    public class MpPluginResponseFontAppearanceFormat : MpJsonObject {
        public string fontFamily { get; set; } = "Consolas";
        public string fontSize { get; set; } = "medium"; //xx-small,x-small,small,medium,large,x-large,xx-large,xxx-large


        public bool isBold { get; set; }
        public bool isItalic { get; set; }
        public bool isUnderlined { get; set; }
        public bool isStrikethough { get; set; }
    }

}
