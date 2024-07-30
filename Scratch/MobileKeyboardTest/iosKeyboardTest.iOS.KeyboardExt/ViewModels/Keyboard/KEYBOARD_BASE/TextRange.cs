using System;

namespace iosKeyboardTest.iOS.KeyboardExt {
    public class TextRange {
        public int SelectionStartIdx { get; private set; }
        public int SelectionLength { get; private set; }
        public int SelectionEndIdx =>
            SelectionStartIdx + SelectionLength;
        public string Text { get; private set; } = string.Empty;
        public string LeadingText =>
            Text.Substring(0, SelectionStartIdx);
        public string SelectedText {
            get => SelectionStartIdx < Text.Length ? Text.Substring(SelectionStartIdx, SelectionLength) : string.Empty;
            set {
                string new_sel_text = value ?? string.Empty;
                string new_text = LeadingText + new_sel_text + TrailingText;
                Text = new_text;
                SelectionLength = new_sel_text.Length;
            }
        }
        public string TrailingText => 
            SelectionEndIdx < Text.Length ? 
                Text.Substring(SelectionEndIdx, Math.Max(0,Text.Length - SelectionEndIdx)) : 
                string.Empty;

        public TextRange(string text, int sidx,int len) {
            Text = text;
            SelectionStartIdx = sidx;
            SelectionLength = len;
        }
        public void Select(int sidx,int len) {
            SelectionStartIdx = Math.Clamp(sidx, 0, Math.Max(0,Text.Length));
            SelectionLength = Math.Clamp(len, 0, Math.Max(0,Text.Length - SelectionStartIdx));
        }

        public bool IsEqual(TextRange otherRange) {
            if(otherRange == null) {
                return false;
            }
            return
                SelectionStartIdx == otherRange.SelectionStartIdx &&
                SelectionEndIdx == otherRange.SelectionEndIdx &&
                Text == otherRange.Text;
        }
        public override string ToString() {
            return $"[{SelectionStartIdx},{SelectionEndIdx}]'{Text}'";
        }

        public TextRange Clone() {
            return new TextRange(Text, SelectionStartIdx, SelectionLength);
        }
    }
}
