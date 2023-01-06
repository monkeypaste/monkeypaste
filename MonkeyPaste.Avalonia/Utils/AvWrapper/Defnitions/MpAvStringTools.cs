using MonkeyPaste;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using HtmlAgilityPack;
using MonkeyPaste.Common.Wpf;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringTools : MpIStringTools {
        public string ToPlainText(string text) {
            return text.RtfToPlainText();
        }

        public string ToRichText(string text) {
            return text.ToContentRichText();
        }

        public string ToCsv(string text) {
            if(text.IsStringRichHtml()) {
                return text.RichHtmlToCsv();
            }
#if WINDOWS
            if(text.IsStringRichText()) {
                return MpWpfStringExtensions.RtfTableToCsv(text);
            }
#endif
            // return unaltered text
            return text;
        }
    }
}
