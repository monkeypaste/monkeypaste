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

        public static MpAvITextRange ContentRange(this MpAvHtmlDocument doc) {
            return new MpAvTextRange(doc.ContentStart, doc.ContentEnd);
        }

        public static MpAvITextRange ToTextRange(this MpAvITextPointer tp) {
            return new MpAvTextRange(tp,tp);
        }

        public static string ToPlainText(this MpAvHtmlDocument doc) {
            return doc.Html.ToPlainText();
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
