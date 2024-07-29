
using MonkeyPaste.Common;
using System.Text;

namespace AutoComplete {
    public static class TextCorrector {
        public static void Init() {

            // words_4000 from: https://raw.githubusercontent.com/pkLazer/password_rank/master/4000-most-common-english-words-csv.csv
            // 
            //var text = MpFileIo.ReadTextFromFile(@"C:\Users\tkefauver\Source\Repos\MonkeyPaste\Scratch\MobileKeyboardTest\AutoComplete\words_4000.txt");
            //var text = MpFileIo.ReadTextFromFile(@"C:\Users\tkefauver\Source\Repos\MonkeyPaste\Scratch\MobileKeyboardTest\AutoComplete\words_alpha.txt");
            //dictionary = text.SplitNoEmpty(Environment.NewLine);

            var text = MpFileIo.ReadTextFromFile(@"C:\Users\tkefauver\Source\Repos\MonkeyPaste\Scratch\MobileKeyboardTest\AutoComplete\words_5000.txt");
            var raw_rows = text.SplitNoEmpty(Environment.NewLine);
            int count = raw_rows.Length;
            var rows = raw_rows.Skip(1);
            var nodes = rows.Select(x => new Node(x));

            int max_len = nodes.Max(x => x.Word.Length);
            BKTree.Init(count, max_len);
            int i = 0;
            foreach (var node in nodes) {
                BKTree.Add(BKTree.RootNode, node);

                int percent = (int)(((double)i++ / (double)count) * 100);
                Console.Clear();
                Console.WriteLine($"Loading...{percent}%");
            }

            int max_freq = 100_000_000;
            var sb = new StringBuilder();
            i = 1;
            foreach(var node in nodes) {
                double new_freq = (double)node.Entry.Frequency / max_freq;
                sb.AppendLine($"{node.Entry.Rank},{node.Entry.Word},{string.Format("{0:N8}", new_freq.ToString())}");
            }
            MpFileIo.WriteTextToFile(@"C:\Users\tkefauver\Source\Repos\MonkeyPaste\Scratch\MobileKeyboardTest\AutoComplete\words_5000_normalized.txt",sb.ToString());
        }

        public static IEnumerable<string> GetResults(string input, bool autoCorrect, int maxResults, out string autoCorrectResult) {
            autoCorrectResult = null;

            // get completion results ordered by frequency
            var starts_with_strs = 
                BKTree.Entries
                .Where(x => x.Word.StartsWith(input) && x.Word != input)
                .OrderByDescending(x => x.Frequency);
            if(starts_with_strs.Any()) {
                // word can be completed
                return FillWithDefaults(starts_with_strs.Select(x => x.Word).Take(maxResults),maxResults);
            }
            // might be misspelled
            var similar_matches = BKTree.GetSimilarWords(BKTree.RootNode, input).OrderBy(x => x.Item2);
            if(autoCorrect && similar_matches.FirstOrDefault() is { } best_match) {
                // use best match as auto-correct
                autoCorrectResult = best_match.Item1;
            }
            // return closest auto-corrected words
            return FillWithDefaults(similar_matches.Select(x=>x.Item1), maxResults);
        }
        public static void UpdateMatcher(string confirmedText) {
            // TODO called when keyboard is done. regex for words, scan data file and add/update new use count field then re-initialize (in background) 
        }
        static IEnumerable<string> FillWithDefaults(IEnumerable<string> results, int desiredCount) {
            var enumerator = results.GetEnumerator();
            var def_enumerator = BKTree.Entries.GetEnumerator();
            for (int i = 0; i < desiredCount; i++) {
                if(enumerator.MoveNext()) {
                    yield return enumerator.Current;
                } else if(def_enumerator.MoveNext()) {
                    yield return def_enumerator.Current.Word;
                }
            }
        }
    }


}




