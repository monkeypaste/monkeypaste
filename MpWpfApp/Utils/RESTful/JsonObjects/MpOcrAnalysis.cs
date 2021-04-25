using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpOcrAnalysis {
        public string language { get; set; }
        public double textAngle { get; set; }
        public string orientation { get; set; }
        public List<MpOcrRegion> regions { get; set; }

    }
    public class MpOcrRegion {
        public string boundingBox { get; set; }
        public List<MpOcrRegionLine> lines { get; set; }
    }
    public class MpOcrRegionLine {
        public string boundingBox { get; set; }
        public List<MpOcrRegionWord> words { get; set; }
    }
    public class MpOcrRegionWord {
        public string boundingBox { get; set; }
        public string text { get; set; }
    }
}
