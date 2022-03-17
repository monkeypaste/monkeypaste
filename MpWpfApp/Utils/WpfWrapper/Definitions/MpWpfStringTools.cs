using MonkeyPaste;

namespace MpWpfApp {
    public class MpWpfStringTools : MpIStringTools {
        public string ToPlainText(string text) {
            return text.ToPlainText();
        }

        public string ToRichText(string text) {
            return text.ToRichText();
        }

        public string ToCsv(string text) {
            return text.ToCsv();
        }
    }
}
