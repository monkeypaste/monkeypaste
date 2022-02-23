using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {    
    public class MpPluginResponseFormat {
        public string message { get; set; }

        public MpPluginResponseNewContentFormat newContentItem { get; set; }
        public List<MpPluginResponseAnnotationFormat> annotations { get; set; }
    }

    public class MpPluginResponseItemBaseFormat {
        public string name { get; set; }

        public MpJsonPathProperty label { get; set; }
        public MpJsonPathProperty description { get; set; }

        public MpPluginResponseAppearanceFormat appearance { get; set; } = new MpPluginResponseAppearanceFormat();

        public MpJsonPathProperty<double> score { get; set; }
        public double minScore { get; set; } = 0;
        public double maxScore { get; set; } = 1;

        public List<MpPluginResponseAnnotationFormat> children { get; set; }
        public List<MpPluginResponseAnnotationFormat> dynamicChildren { get; set; }

        public MpPluginResponseItemBaseFormat() { }

        public MpPluginResponseItemBaseFormat(string label) : this(label, 1) { }
        public MpPluginResponseItemBaseFormat(double score) : this(string.Empty, score) { }
        public MpPluginResponseItemBaseFormat(string label, double score) {
            this.label = new MpJsonPathProperty(label);
            this.score = new MpJsonPathProperty<double>(score);
        }
    }

    public class MpPluginResponseNewContentFormat : MpPluginResponseItemBaseFormat {
        public MpJsonPathProperty content { get; set; }

        public bool isRequestChild { get; set; } = true;

        public List<MpPluginResponseAnnotationFormat> annotations { get; set; }
    }

    public class MpPluginResponseAnnotationFormat : MpPluginResponseItemBaseFormat {
        public MpAnalyzerPluginImageTokenResponseValueFormat box { get; set; }

        public MpAnalyzerPluginTextTokenResponseValueFormat range { get; set; }
    }

    public class MpAnalyzerPluginImageTokenResponseValueFormat {
        public MpJsonPathProperty<double> x { get; set; } 
        public MpJsonPathProperty<double> y { get; set; } 
        public MpJsonPathProperty<double> width { get; set; } 
        public MpJsonPathProperty<double> height { get; set; }


        public MpAnalyzerPluginImageTokenResponseValueFormat() { }
        public MpAnalyzerPluginImageTokenResponseValueFormat(double x, double y, double w, double h) {
            this.x = new MpJsonPathProperty<double>(x);
            this.y = new MpJsonPathProperty<double>(y);
            this.width = new MpJsonPathProperty<double>(w);
            this.height = new MpJsonPathProperty<double>(h);
        }

        public override string ToString() {
            if(x == null || y == null || width == null || height == null) {
                return base.ToString();
            }
            return string.Format(@"x:{0} y:{1} w:{2} h:{4}", x, y, width, height);
        }
    }

    public class MpAnalyzerPluginTextTokenResponseValueFormat {
        public MpJsonPathProperty<int> rangeStart { get; set; }
        public MpJsonPathProperty<int> rangeEnd { get; set; }

        public MpAnalyzerPluginTextTokenResponseValueFormat() { }
        public MpAnalyzerPluginTextTokenResponseValueFormat(int start,int end) {
            rangeStart = new MpJsonPathProperty<int>(start);
            rangeEnd = new MpJsonPathProperty<int>(end);
        }
    }

    

    public class MpPluginResponseAppearanceFormat {
        public MpJsonPathProperty color { get; set; } = new MpJsonPathProperty("#FFD3D3D3");

        public MpPluginResponseFontAppearanceFormat font { get; set; }
    }

    public class MpPluginResponseFontAppearanceFormat {
        public bool isBold { get; set; }
        public bool isItalic { get; set; }

        public string size { get; set; } = "medium"; //xx-small,x-small,small,medium,large,x-large,xx-large,xxx-large
    }
}
