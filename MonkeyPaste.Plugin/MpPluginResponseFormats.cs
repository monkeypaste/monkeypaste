using System;
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

    public interface MpITextTokenDescriptorRange : MpIDescriptor {
        int rangeStart { get; set; }
        int rangeEnd { get; set; }
    }

    public interface MpITextDescriptor : MpIDescriptor {
        string content { get; set; }
    }


    public class MpAnalyzerPluginImageTokenResponseValueFormat :
        MpAnalyzerResponseValueFormatBase,
        MpIImageDescriptorBox {
        public double x { get; set; } = 0;
        public double y { get; set; } = 0;
        public double width { get; set; } = 0;
        public double height { get; set; } = 0;
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


    public class MpPluginResponseContentMap {
        public List<string> contentPath { get; set; } = new List<string>();
        public List<string> titlePath { get; set; } = new List<string>();
        public List<string> descriptionPath { get; set; } = new List<string>();

        public bool omitContentIfPathNotFound { get; set; }
        public bool omitTitleIfPathNotFound { get; set; }
        public bool omitDescriptionIfPathNotFound { get; set; }
    }
}
