using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public interface MpIDescriptor {
        string label { get; set; }
        string description { get; set; }

        double score { get; set; }
    }

    public interface MpIImageDescriptorBox : MpIDescriptor {
        double x { get; set; }
        double y { get; set; }
        double width { get; set; }
        double height { get; set; }
    }

    

    public interface MpITextDescriptor : MpIDescriptor {
        string content { get; set; }
    }


    

    public class MpAnalyzerPluginTextResponseFormat :
        MpAnalyzerResponseValueFormatBase, MpITextDescriptor {

        public string content { get; set; }
    }

    public class MpAnalyzerPluginTextTokenResponseValueFormat :
        MpAnalyzerResponseValueFormatBase,
        MpITextTokenDescriptorRange {
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

        public MpPluginResponseAppearanceFormat appearance { get; set; }

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

    public class MpAnalyzerPluginImageTokenResponseValueFormat :
        MpAnalyzerResponseValueFormatBase {
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

    public interface MpITextTokenDescriptorRange : MpIDescriptor {
        int rangeStart { get; set; }
        int rangeEnd { get; set; }
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
        public double borderThickness { get; set; } = 2;
        public string borderColor { get; set; } = "#FFD3D3D3";

        public string foregroundColor { get; set; } = "#FF000000";
        public string backgroundColor { get; set; } = "#FFFFFFFF";
    }
}
