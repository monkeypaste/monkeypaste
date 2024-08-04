
using Avalonia.Controls.Shapes;

namespace iosKeyboardTest.iOS.KeyboardExt {
    public class WordEntry {
        public int Rank { get; set; }
        public string Word { get; set; } = string.Empty;
        public WordEntry(int rank, string word) : this(rank) {
            Word = word ?? string.Empty;
        }
        public WordEntry(int rank) {
            Rank = rank;
        }
        public  WordEntry() { }
    }
}




