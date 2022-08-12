using MonkeyPaste;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using HtmlAgilityPack;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringTools : MpIStringTools {
        public string ToPlainText(string text) {
            return text.ToPlainText();
        }

        public string ToRichText(string text) {
            return text.ToContentRichText();
        }

        public string ToCsv(string text) {
            return text.ToCsv();
        }
    }
}
