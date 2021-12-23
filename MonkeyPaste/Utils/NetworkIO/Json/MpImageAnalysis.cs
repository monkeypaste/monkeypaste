using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpImageAnalysis {
        public List<MpImageCategory> categories { get; set; }
        public MpImageColor color { get; set; }
        public MpImageDescription description { get; set; }
        public string requestId { get; set; }
        public MpImageMetaData metadata { get; set; }
    }

    public class MpImageCategory {
        public string name { get; set; }
        public double score { get; set; }
    }
    public class MpImageColor {
        public string dominantColorForeground { get; set; }
        public string dominantColorBackground { get; set; }
        public List<string> dominantColors { get; set; }
        public string accentColor { get; set; }
        public bool isBwImg { get; set; }
    }
    public class MpImageDescription {
        public List<string> tags { get; set; }
        public List<MpImageCaptions> captions { get; set; }
    }
    public class MpImageCaptions {
        public string text { get; set; }
        public double confidence { get; set; }
    }
    public class MpImageMetaData {
        public int height { get; set; }
        public int width { get; set; }
        public string format { get; set; }
    }
}
