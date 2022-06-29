using MonkeyPaste;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpWpfStringTools : MpIStringTools {
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
