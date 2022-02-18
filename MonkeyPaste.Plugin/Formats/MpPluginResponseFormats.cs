using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {

    public class MpAnalyzerPluginTextResponseFormat {

        public string content { get; set; }
    }

    public class MpAnalyzerPluginTextTokenResponseValueFormat {
        public int rangeStart { get; set; }
        public int rangeEnd { get; set; }
    }



    public class MpPluginResponseFormat {
        public List<MpPluginResponseNewContentFormat> newContentItems { get; set; }
        public List<MpPluginResponseAnnotationFormat> annotations { get; set; }
    }

    public class MpPluginResponseAnnotationFormat {
        public string name { get; set; }

        public MpJsonPathProperty label { get; set; }

        public MpPluginResponseAppearanceFormat appearance { get; set; } = new MpPluginResponseAppearanceFormat();

        public MpAnalyzerPluginImageTokenResponseValueFormat box { get; set; }

        public MpAnalyzerPluginTextTokenResponseValueFormat range { get; set; }

        public MpJsonPathProperty<double> score { get; set; }
        public double minScore { get; set; } = 0;
        public double maxScore { get; set; } = 1;

        public List<MpPluginResponseAnnotationFormat> children { get; set; }
        public List<MpPluginResponseAnnotationFormat> dynamicChildren { get; set; }

        public MpPluginResponseAnnotationFormat() { }

        public MpPluginResponseAnnotationFormat(string label) : this(label,1) { }
        public MpPluginResponseAnnotationFormat(double score) :this(string.Empty,score) { }
        public MpPluginResponseAnnotationFormat(string label, double score) {
            this.label = new MpJsonPathProperty(label);
            this.score = new MpJsonPathProperty<double>(score);
        }
    }

    public class MpAnalyzerPluginImageTokenResponseValueFormat {
        public MpAnalyzerPluginImageTokenResponseValueFormat() { }
        public MpAnalyzerPluginImageTokenResponseValueFormat(double x,double y,double w,double h) {
            this.x = new MpJsonPathProperty<double>(x);
            this.y = new MpJsonPathProperty<double>(y);
            this.width = new MpJsonPathProperty<double>(w);
            this.height = new MpJsonPathProperty<double>(h);
        }

        public MpJsonPathProperty<double> x { get; set; } 
        public MpJsonPathProperty<double> y { get; set; } 
        public MpJsonPathProperty<double> width { get; set; } 
        public MpJsonPathProperty<double> height { get; set; } 

    }

    public class MpPluginResponseNewContentFormat {
        public List<string> contentPath { get; set; } = new List<string>();
        public List<string> titlePath { get; set; } = new List<string>();
        public List<string> descriptionPath { get; set; } = new List<string>();

        public bool omitContentIfPathNotFound { get; set; }
        public bool omitTitleIfPathNotFound { get; set; }
        public bool omitDescriptionIfPathNotFound { get; set; }

        public bool isRequestChild { get; set; } = true;
    }

    public class MpPluginResponseAppearanceFormat {
        public string color { get; set; } = "#FFD3D3D3";
    }
}
