using Avalonia.Controls;
using AvaloniaEdit.Document;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvCefNetWebViewExtensions { 
        public static IEnumerable<MpAvITextRange> FindAllText(
             this MpAvITextPointer start,
             MpAvITextPointer end,
             string input,
             bool isCaseSensitive = false,
             bool matchWholeWord = false,
             bool useRegEx = false) {
            if (start == null) {
                yield return null;
            }
            while (start != null && start != end) {
                if (!start.IsInSameDocument(end)) {
                    break;
                }

                var matchRange = start.FindText(end, input, isCaseSensitive, matchWholeWord, useRegEx);
                if (matchRange == null) {
                    break;
                }
                start = matchRange.End.GetNextInsertionPosition(LogicalDirection.Forward);
                yield return matchRange;
            }
        }

        public static MpAvITextRange FindText(
            this MpAvITextPointer start,
            MpAvITextPointer end,
            string matchText,
            bool isCaseSensitive = false,
            bool matchWholeWord = false,
            bool useRegEx = false,
            CultureInfo cultureInfo = null) {
            if (string.IsNullOrEmpty(matchText) || start == null || end == null) {
                return null;
            }
            cultureInfo = cultureInfo == null ? CultureInfo.CurrentCulture : cultureInfo;

            //string searchText = new MpAvTextRange(start, end).Text;
            //searchText = isCaseSensitive ? searchText : searchText.ToLower();
            //matchText = isCaseSensitive ? matchText : matchText.ToLower();

            //int matchIdx = searchText.IndexOf(matchText);
            //if(matchIdx < 0) {
            //    return null;
            //}

            if (start.Document is MpAvHtmlDocument doc) {
                HtmlNodeCollection nodes = null;
                if(isCaseSensitive) {
                    nodes = doc.HtmlDocument.DocumentNode.SelectNodes($"//*[contains(., '{matchText}')]");
                } else {
                    nodes = doc.HtmlDocument.DocumentNode.SelectNodes($"//*[contains(lower-case(.), '{matchText.ToLower()}')]");
                }
                if(nodes == null || nodes.Count == 0) {
                    return null;
                }
                var startNode = nodes.Where(x=>x.OuterStartIndex >= start.Offset && x.OuterStartIndex + x.OuterLength <= end.Offset)
                                        .Aggregate((a, b) => a.OuterStartIndex < b.OuterStartIndex ? a : b);
                var endNode = nodes.Where(x => x.OuterStartIndex >= start.Offset && x.OuterStartIndex + x.OuterLength <= end.Offset)
                                        .Aggregate((a, b) => a.OuterStartIndex + a.OuterLength > b.OuterStartIndex + b.OuterLength ? a : b);
                return new MpAvTextRange(
                    new MpAvTextPointer(start.Document, startNode.OuterStartIndex),
                    new MpAvTextPointer(start.Document, endNode.OuterStartIndex + endNode.OuterLength));
            }

            return null;
        }

        public static IEnumerable<MpAvITextRange> FindText(
            this MpAvHtmlDocument doc,
            string input,
            bool isCaseSensitive = false,
            bool matchWholeWord = false,
            bool useRegEx = false) {

            input = input.Replace(Environment.NewLine, string.Empty);


            if (useRegEx) {
                string pattern = input;
                string pt = doc.Html.ToPlainText();
                var mc = Regex.Matches(pt, pattern, isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);

                var trl = new List<MpAvITextRange>();
                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            var c_trl = doc.ContentStart.FindAllText(doc.ContentEnd, c.Value);
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

            return doc.ContentStart.FindAllText(doc.ContentEnd, input, isCaseSensitive, matchWholeWord).ToList();
        }

        public static MpAvITextRange ContentRange(this MpAvHtmlDocument doc) {
            return new MpAvTextRange(doc.ContentStart, doc.ContentEnd);
        }

        public static MpAvITextRange ToTextRange(this MpAvITextPointer tp) {
            return new MpAvTextRange(tp,tp);
        }

        public static MpAvITextPointer GetPosisitionFromPoint(this MpAvHtmlDocument doc, MpPoint point, bool snapToText) {
            return null;
        }
        public static string ToPlainText(this MpAvHtmlDocument doc) {
            return doc.Html.ToPlainText();
        }
        public static MpRect GetCharacterRect(this MpAvITextPointer tp, LogicalDirection dir) {
            return null;
        }

        public static MpAvITextPointer GetLineStartPosition(this MpAvITextPointer tp, int lineOffset) {
            return null;
        }
        public static MpAvITextPointer GetLineEndPosition(this MpAvITextPointer tp, int lineOffset) {
            return null;
        }

        public static void LoadImage(this MpAvITextRange tr,string base64Str, out MpSize size) {
            size = new MpSize();
        }

        public static void LoadItemData(this MpAvITextRange tr, string data, MpCopyItemType itemType, out MpSize size) {
            size = new MpSize();
            tr.Text = data;
        }
        public static void LoadTable(this MpAvITextRange tr, string csvStr) {
            tr.Text = csvStr;
        }

        public static string ToEncodedPlainText(this MpAvITextRange tr) {
            //if (tr.IsEmpty) {
            //    return string.Empty;
            //}
            //var templatesToEncode = tr.GetAllTextElements()
            //                                    .Where(x => x is InlineUIContainer && x.Tag is MpTextTemplate)
            //                                    .OrderBy(x => tr.Start.GetOffsetToPosition(x.ContentStart))
            //                                    .ToList();
            //if (templatesToEncode.Count() == 0) {
            //    return tr.Text;
            //}

            //var sb = new StringBuilder();
            //var ctp = tr.Start;
            //foreach (var te in templatesToEncode) {
            //    var ntp = te.ElementStart;
            //    sb.Append(new TextRange(ctp, ntp).Text);
            //    sb.Append((te.Tag as MpTextTemplate).EncodedTemplate);
            //    ctp = te.ElementEnd;
            //    if (te == templatesToEncode.Last()) {
            //        sb.Append(new TextRange(ctp, tr.End).Text);
            //    }
            //}
            //return sb.ToString();
            return tr.Text;
        }

        public static string ToEncodedRichText(this MpAvITextRange tr) {
            //if (tr.IsEmpty) {
            //    return string.Empty;
            //}
            //var templatesToEncode = tr.GetAllTextElements()
            //                                    .Where(x => x is MpTextTemplateInlineUIContainer && x.Tag is MpTextTemplate)
            //                                    .OrderBy(x => tr.Start.GetOffsetToPosition(x.ContentStart))
            //                                    .Cast<MpTextTemplateInlineUIContainer>()
            //                                    .ToList();
            //var doc = tr.Start.Parent.FindParentOfType<FlowDocument>();
            //var clonedDoc = doc.Clone(tr, out TextRange encodedRange);

            //return encodedRange.ToRichText();
            return tr.Text;
        }
    }
}
