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
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

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

            if(mpdo.DataFormatLookup.ContainsKey(MpClipboardFormatType.Rtf)) {
                string rtf = mpdo.DataFormatLookup[MpClipboardFormatType.Rtf];
                var fd = rtf.ToFlowDocument();

                if(useRegEx) {
                    string pt = new TextRange(fd.ContentStart, fd.ContentEnd).Text;

                    Regex regex = new Regex(searchTerm, RegexOptions.Compiled | RegexOptions.Multiline);
                    MatchCollection mc = regex.Matches(pt);
                    CoreTextExtensions.FindFlags flags = isCaseSensitive ? CoreTextExtensions.FindFlags.MatchCase : CoreTextExtensions.FindFlags.None;

                    foreach (Match m in mc) {
                        foreach (Group mg in m.Groups) {
                            foreach (Capture c in mg.Captures) {
                                var trl = fd.FindText(c.Value, flags, null);

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

                mpdo.SetCustomData("RtfTextRangeCollection", MpJsonObject.SerializeObject(ranges));

                return mpdo;
            }

            return mpdo;
        }
    }

    public class CoreTextRange : MpITextRange {
        public int Offset { get; set; }
        public int Length { get; set; }

    }

    internal static class CoreTextExtensions {
        // TODO Migrate helper/extension methods to Plugin.Wpf
        public static FlowDocument ToFlowDocument(this string str) {
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(str))) {
                try {
                    var fd = new FlowDocument();
                    var range = new TextRange(fd.ContentStart, fd.ContentEnd);
                    range.Load(stream, System.Windows.DataFormats.Rtf);
                    return fd;
                }
                catch (Exception ex) {
                    MpConsole.WriteLine("Exception converting richtext to flowdocument, attempting to fall back to plaintext...");
                    MpConsole.WriteLine("Exception Details: " + ex);
                    return null;
                }
            }
        }
        private static MethodInfo findMethod = null;
        [Flags]
        public enum FindFlags {
            FindInReverse = 2,
            FindWholeWordsOnly = 4,
            MatchAlefHamza = 0x20,
            MatchCase = 1,
            MatchDiacritics = 8,
            MatchKashida = 0x10,
            None = 0
        }

        public static IEnumerable<TextRange> FindAllText(
            this TextPointer start,
            TextPointer end,
            string input,
            bool isCaseSensitive = true) {
            if (start == null) {
                yield return null;
            }

            //var matchRangeList = new List<TextRange>();
            while (start != null && start != end) {
                var matchRange = start.FindText(end, input, isCaseSensitive ? FindFlags.MatchCase : FindFlags.None);
                if (matchRange == null) {
                    break;
                }
                //matchRangeList.Add(matchRange);
                start = matchRange.End.GetNextInsertionPosition(LogicalDirection.Forward);
                yield return matchRange;
            }

            //return matchRangeList;
        }
        public static List<TextRange> FindText(
            this FlowDocument fd,
            string input,
            bool isCaseSensitive = false,
            bool matchWholeWord = false,
            bool useRegEx = false) {

            input = input.Replace(Environment.NewLine, string.Empty);


            if (matchWholeWord || useRegEx) {
                string pattern;
                if (useRegEx) {
                    pattern = input;
                } else {
                    pattern = $"\b{input}\b";
                }
                string pt = new TextRange(fd.ContentStart, fd.ContentEnd).Text;
                var mc = Regex.Matches(pt, pattern, isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);

                var trl = new List<TextRange>();
                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            var c_trl = fd.ContentStart.FindAllText(fd.ContentEnd, c.Value);
                            trl.AddRange(c_trl);
                        }
                    }
                }
                trl = trl.Distinct().ToList();
                if (useRegEx && matchWholeWord) {
                    trl = trl.Where(x => Regex.IsMatch(x.Text, $"\b{x.Text}\b")).ToList();
                }
                return trl;
            }

            return fd.ContentStart.FindAllText(fd.ContentEnd, input, isCaseSensitive).ToList();
        }

        public static List<TextRange> FindText(
            this FlowDocument fd,
            string input,
            FindFlags flags = FindFlags.MatchCase,
            CultureInfo cultureInfo = null) {
            input = input.Replace(Environment.NewLine, string.Empty);
            return fd.ContentStart.FindAllText(fd.ContentEnd, input, flags.HasFlag(FindFlags.MatchCase)).ToList();
            var trl = new List<TextRange>();
            //var tp = fd.ContentStart;

            //var inputParts = input.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            //while (tp != null && tp != fd.ContentEnd) {
            //    var ctp = tp;
            //    tp = null;
            //    int i;
            //    for(i = 0;i < inputParts.Length && ctp != null && ctp != fd.ContentEnd;i++) {
            //        string inputPart = inputParts[i];
            //        var tr = ctp.FindText(fd.ContentEnd, inputPart, flags, cultureInfo);
            //        if (tr == null) {
            //            break;
            //        }
            //        if(tp == null) {
            //            tp = tr.Start;
            //        }
            //        ctp = tr.End.GetNextInsertionPosition(LogicalDirection.Forward);
            //    }
            //    if(i != inputParts.Length) {
            //        break;
            //    }
            //    trl.Add(new TextRange(tp, ctp));

            //    tp = ctp.GetNextInsertionPosition(LogicalDirection.Forward);
            //}
            return trl;
        }

        public static TextRange FindText(
            this TextPointer start,
            TextPointer end,
            string input,
            FindFlags flags = FindFlags.MatchCase,
            CultureInfo cultureInfo = null) {
            if (string.IsNullOrEmpty(input) || start == null || end == null) {
                return null;
            }
            cultureInfo = cultureInfo == null ? CultureInfo.CurrentCulture : cultureInfo;

            TextRange textRange = null;
            if (start.CompareTo(end) < 0) {
                try {
                    if (findMethod == null) {
                        findMethod = typeof(FrameworkElement).Assembly
                                        .GetType("System.Windows.Documents.TextFindEngine")
                                        .GetMethod("Find", BindingFlags.Static | BindingFlags.Public);
                    }
                    object result = findMethod.Invoke(null, new object[] {
                        start,
                        end,
                        input, flags, cultureInfo });
                    textRange = result as TextRange;
                }
                catch (ApplicationException) {
                    textRange = null;
                }
            }

            return textRange;
        }

    }
}
