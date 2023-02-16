using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
#if WINDOWS

using MonkeyPaste.Common.Wpf;

#endif

namespace MonkeyPaste.Avalonia {
    public class MpAvStringTools : MpIStringTools {
        public string ToPlainText(string text) {
            if (text.IsStringRichText()) {
#if WINDOWS
                return text.RtfToPlainText();
#endif
            }
            if (text.IsStringRichHtml()) {
                return MpRichHtmlToPlainTextConverter.Convert(text);
            }
            return text;
        }

        public string ToRichText(string text) {
            return text.ToContentRichText();
        }

        public string ToCsv(string text) {
            if (text.IsStringRichHtml()) {
                return text.RichHtmlToCsv();
            }
#if WINDOWS
            if (text.IsStringRichText()) {
                return MpWpfStringExtensions.RtfTableToCsv(text);
            }
#endif
            // return unaltered text
            return text;
        }
    }
}
