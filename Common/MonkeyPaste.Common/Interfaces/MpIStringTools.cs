namespace MonkeyPaste.Common {
    public interface MpIStringTools {
        string ToPlainText(string text, string source_format = "");
        string ToRichText(string text);
        string ToCsv(string text);
    }
}
