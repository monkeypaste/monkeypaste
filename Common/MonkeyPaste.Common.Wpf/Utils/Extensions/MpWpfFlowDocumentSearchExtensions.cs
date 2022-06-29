using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xml;
using MonkeyPaste.Common.Wpf;

namespace MonkeyPaste.Common.Wpf {
    public static class MpWpfFlowDocumentSearchExtensions {

        public static string ToRichText(this TextRange tr) {
            //if(tr == null) {
            //    return string.Empty;
            //}
            //using (var rangeStream = new MemoryStream()) {
            //    using(var writerStream = new StreamWriter(rangeStream)) {
            //        try {
            //            if (tr.CanLoad(DataFormats.Rtf)) {
            //                tr.Load(rangeStream, DataFormats.Rtf);

            //                rangeStream.Seek(0, SeekOrigin.Begin);
            //                using (var rtfStreamReader = new StreamReader(rangeStream)) {
            //                    return rtfStreamReader.ReadToEnd();
            //                }
            //            }
            //        }
            //        catch (Exception ex) {
            //            MpConsole.WriteTraceLine(ex);
            //            return tr.Text;
            //        }
            //    }
            //}
            //return tr.Text;
            using (MemoryStream ms = new MemoryStream()) {
                tr.Save(ms, DataFormats.Rtf);
                return Encoding.Default.GetString(ms.ToArray());
            }

        }

        public static List<TextRange> FindStringRangesFromPosition(TextPointer position, string matchStr, bool isCaseSensitive = false) {
            if (string.IsNullOrEmpty(matchStr)) {
                return null;
            }
            var orgPosition = position;
            TextPointer nextDocPosition = null;
            var matchRangeList = new List<TextRange>();
            TextSelection rtbSelection = null;
            var rtb = position.Parent.FindParentOfType<FlowDocument>().GetVisualAncestor<RichTextBox>(); //(RichTextBox)FindParentOfType(position.Parent, typeof(RichTextBox));
            if (rtb != null) {
                rtbSelection = rtb.Selection;
            }
            while (position != null) {
                var hlr = FindStringRangeFromPosition(position, matchStr, isCaseSensitive);
                if (hlr == null) {
                    if (nextDocPosition != null) {
                        position = nextDocPosition;
                        nextDocPosition = null;
                        continue;
                    }
                    break;
                } else {
                    matchRangeList.Add(hlr);
                    if (!hlr.End.IsInSameDocument(orgPosition)) {
                        var phl = hlr.End.Parent.FindParentOfType<InlineUIContainer>();
                        nextDocPosition = phl.ElementEnd.GetNextContextPosition(LogicalDirection.Forward);
                    }
                    position = hlr.End;
                }
            }
            if (rtbSelection != null) {
                rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
            return matchRangeList;
        }

        public static TextRange FindStringRangeFromPosition(TextPointer position, string matchStr,bool isCaseSensitive = false) {
            if (string.IsNullOrEmpty(matchStr)) {
                return null;
            }
            int curIdx = 0;
            //TextSelection rtbSelection = null;
            //var rtb = (RichTextBox)FindParentOfType(position.Parent, typeof(RichTextBox));
            //if (rtb != null) {
            //    rtbSelection = rtb.Selection;
            //}
            TextPointer postOfUiElement = null;
            TextPointer startPointer = null;
            TextRange matchRange = null;
            StringComparison stringComparison = isCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            while (matchRange == null && (position != null || postOfUiElement != null)) {
                //if (ct.IsCancellationRequested) {
                //    break;
                //}
                if (position == null) {
                    position = postOfUiElement;
                    postOfUiElement = null;
                }
                if (position.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text) {
                    if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.EmbeddedElement) {
                        var iuc = position.Parent.FindParentOfType<InlineUIContainer>();///(InlineUIContainer)FindParentOfType(position.Parent, typeof(InlineUIContainer));
                        var hl = iuc;// (Hyperlink)iuc.Parent;
                        var tb = (iuc.Child as Border).Child as TextBlock;
                        postOfUiElement = hl.ElementEnd.GetNextContextPosition(LogicalDirection.Forward);
                        position = tb.ContentStart;
                        continue;
                    }
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                    continue;
                }
                var runStr = position.GetTextInRun(LogicalDirection.Forward);
                if (string.IsNullOrEmpty(runStr)) {
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                    continue;
                }
                //only concerned with current character of match string
                int runIdx = runStr.IndexOf(matchStr[curIdx].ToString(), stringComparison);
                if (runIdx == -1) {
                    //if no match found reset search
                    curIdx = 0;
                    if (startPointer == null) {
                        position = position.GetNextContextPosition(LogicalDirection.Forward);
                    } else {
                        //when no match somewhere after first character reset search to the position AFTER beginning of last partial match
                        position = startPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                        startPointer = null;
                    }
                    continue;
                }
                if (curIdx == 0) {
                    //beginning of range found at runIdx
                    startPointer = position.GetPositionAtOffset(runIdx, LogicalDirection.Forward);
                }
                if (curIdx == matchStr.Length - 1) {
                    //each character has been matched
                    var endPointer = position.GetPositionAtOffset(runIdx, LogicalDirection.Forward);
                    if (!startPointer.IsInSameDocument(endPointer)) {
                        endPointer = endPointer.Parent.FindParentOfType<InlineUIContainer>().ElementEnd; //((Hyperlink)FindParentOfType(endPointer.Parent, typeof(Hyperlink))).ElementEnd;
                    }
                    //for edge cases of repeating characters these loops ensure start is not early and last character isn't lost 
                    if (isCaseSensitive) {
                        while (endPointer != null && !new TextRange(startPointer, endPointer).Text.Contains(matchStr)) {
                            //if (ct.IsCancellationRequested) {
                            //    //trigger break out of parent loop
                            //    endPointer = null;
                            //    break;
                            //}
                            endPointer = endPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                        }
                    } else {
                        while (endPointer != null && !new TextRange(startPointer, endPointer).Text.ToLower().Contains(matchStr.ToLower())) {
                            //if (ct.IsCancellationRequested) {
                            //    //trigger break out of parent loop
                            //    endPointer = null;
                            //    break;
                            //}
                            endPointer = endPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                        }
                    }
                    if (endPointer == null) {
                        break;
                        //return null;
                    }
                    while (startPointer != null && new TextRange(startPointer, endPointer).Text.Length > matchStr.Length) {
                        startPointer = startPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                    }
                    if (startPointer == null) {
                        break;
                        //return null;
                    }
                    matchRange = new TextRange(startPointer, endPointer);
                } else {
                    //prepare loop for next match character
                    curIdx++;
                    //iterate position one offset AFTER match offset
                    position = position.GetPositionAtOffset(runIdx + 1, LogicalDirection.Forward);
                }
            }
            return matchRange;
        }
    }
}
