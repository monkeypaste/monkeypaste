
namespace iosKeyboardTest.Android {
    public class WordEntry {
        public int Rank { get; private set; }
        public string Word { get; private set; } = string.Empty;
        public string PartOfSpeech { get; private set; } = string.Empty;
        public int Frequency { get; private set; }
        public double Dispersion { get; private set; }
        public WordEntry(string line) {
            if(line.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries) is not { } lineParts) {
                return;
            }
            Rank = int.Parse(lineParts[0]);
            Word = lineParts[1];
            PartOfSpeech = lineParts[2];
            Frequency = int.Parse(lineParts[3]);
            Dispersion = double.Parse(lineParts[4]);
        }
        public WordEntry() {

        }
    }
}




