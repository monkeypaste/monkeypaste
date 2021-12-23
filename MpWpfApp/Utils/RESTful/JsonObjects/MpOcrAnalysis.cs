using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Ocr;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpOcrAnalysis : MpJsonMessage {
        public string language { get; set; }
        public double textAngle { get; set; }
        public string orientation { get; set; }
        public List<MpOcrRegion> regions { get; set; }

        public MpOcrAnalysis() { }

        public MpOcrAnalysis(OcrResult ocrr) {
            if(ocrr == null) {
                return;
            }

            var ocrRegion = new MpOcrRegion();
            foreach(var line in ocrr.Lines) {
                var ocrLine = new MpOcrRegionLine();
                foreach(var word in line.Words) {
                    if(ocrLine.words == null) {
                        ocrLine.words = new List<MpOcrRegionWord>();
                    }
                    ocrLine.words.Add(new MpOcrRegionWord() { boundingBox = word.BoundingRect.ToString(), text = word.Text });
                }
                if(ocrRegion.lines == null) {
                    ocrRegion.lines = new List<MpOcrRegionLine>();
                }
                ocrRegion.lines.Add(ocrLine);
            }
            regions = new List<MpOcrRegion>();
            regions.Add(ocrRegion);
        }

        public override string ToString() {
            if(regions == null) {
                return string.Empty;
            }
            foreach(var region in regions) {
                MonkeyPaste.MpConsole.WriteLine(@"Region#: " + regions.IndexOf(region) + " bb: "+region.boundingBox);
                MonkeyPaste.MpConsole.WriteLine(@"{");
                if(region.lines == null) {
                    continue;
                }
                foreach (var line in region.lines) {
                    if(line.words == null) {
                        continue;
                    }
                    MonkeyPaste.MpConsole.WriteLine("\t Line#: " + region.lines.IndexOf(line) + " bb: " + line.boundingBox);
                    MonkeyPaste.MpConsole.WriteLine("\t{");
                    foreach (var word in line.words) {
                        MonkeyPaste.MpConsole.WriteLine("\t\t Word#: " + line.words.IndexOf(word) + " bb: " + word.boundingBox);
                        MonkeyPaste.MpConsole.WriteLine("\t\t'" + word.text + "'");
                    }
                    MonkeyPaste.MpConsole.WriteLine("\t}");                   
                }
                MonkeyPaste.MpConsole.WriteLine(@"}");
            }
            return base.ToString();
        }
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
