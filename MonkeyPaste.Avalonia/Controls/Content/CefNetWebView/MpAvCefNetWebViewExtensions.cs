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

        


        public static void LoadImage(this MpAvITextRange tr,string base64Str, out MpSize size) {
            size = new MpSize();
        }

        public static void LoadItemData(this MpAvITextRange tr, string data, MpCopyItemType itemType, out MpSize size) {
            size = new MpSize();
            tr.SetTextAsync(data).FireAndForgetSafeAsync((tr.Start.Document.Owner as Control).DataContext as MpViewModelBase);
        }
        public static void LoadTable(this MpAvITextRange tr, string csvStr) {

            tr.SetTextAsync(csvStr).FireAndForgetSafeAsync((tr.Start.Document.Owner as Control).DataContext as MpViewModelBase);
        }

        public static async Task<string> ToEncodedPlainTextAsync(this MpAvITextRange tr) {
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
            string text = await tr.GetTextAsync();
            return text;
        }

        public static async Task<string> ToEncodedRichTextAsync(this MpAvITextRange tr) {
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
            string text = await tr.GetTextAsync();
            return text;
        }
    }
}
