using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

namespace CoreTextAnnotater {
    public class CoreTextAnnotater : MpIAnnotationComponent {
        public object Annotate(object args) {
            var request = MpJsonObject.DeserializeObject<MpAnalyzerPluginRequestFormat>(args);
            if (request == null) {
                return null;
            }
            MpPortableDataObject mpdo = request.data;

            bool isCaseSensitive = false;
            bool useRegEx = false;
            string searchTerm = string.Empty;

            if(request.items.Any(x=>x.paramId == 1)) {
                isCaseSensitive = request.items.FirstOrDefault(x => x.paramId == 1).value.ToLower() == "true";
            }
            if (request.items.Any(x => x.paramId == 2)) {
                useRegEx = request.items.FirstOrDefault(x => x.paramId == 2).value.ToLower() == "true";
            }
            if (request.items.Any(x => x.paramId == 3)) {
                searchTerm = request.items.FirstOrDefault(x => x.paramId == 3).value;
            }

            List<CoreTextRange> ranges = new List<CoreTextRange>();

            if(mpdo.ContainsData(MpPortableDataFormats.Rtf)) {
                string rtf = mpdo.GetData(MpPortableDataFormats.Rtf).ToString();
                var fd = rtf.ToFlowDocument();

                if(useRegEx) {
                    string pt = new TextRange(fd.ContentStart, fd.ContentEnd).Text;

                    Regex regex = new Regex(searchTerm, RegexOptions.Compiled | RegexOptions.Multiline);
                    MatchCollection mc = regex.Matches(pt);
                    foreach (Match m in mc) {
                        foreach (Group mg in m.Groups) {
                            foreach (Capture c in mg.Captures) {
                                //var trl = fd.ContentStart.FindAllText(fd.ContentEnd,c.Value, isCaseSensitive);
                                var trl = fd.FindText(c.Value, isCaseSensitive);

                                foreach(var tr in trl) {
                                    int offset = fd.ContentStart.GetOffsetToPosition(tr.Start);
                                    int length = fd.ContentStart.GetOffsetToPosition(tr.End) - offset;
                                    var ctr = new CoreTextRange() {
                                        Offset = offset,
                                        Length = length
                                    };
                                    ranges.Add(ctr);
                                }
                            }
                        }
                    }
                }

                mpdo.SetData("RtfTextRangeCollection", MpJsonObject.SerializeObject(ranges));

                return mpdo;
            }

            return mpdo;
        }
    }

    public class CoreTextRange : MpITextRange {
        public int Offset { get; set; }
        public int Length { get; set; }

    }
}
