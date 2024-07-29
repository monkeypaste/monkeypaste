
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace iosKeyboardTest.iOS {
    public static class TextCorrector {
        public static bool IsLoaded { get; private set; }
        public static void Init(IAssetLoader loader) {
            if(loader.LoadStream("words_5000.txt") is not { } stream) {
                return;
            }
            IsLoaded = false;

            string text = null;
            using (StreamReader f = new StreamReader(stream)) {
                text = f.ReadToEnd();
                f.Close();
            }

            var raw_rows = text.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            int count = raw_rows.Length;
            var rows = raw_rows.Skip(1);
            var nodes = rows.Select(x => new Node(x));

            int max_len = nodes.Max(x => x.Word.Length);
            BKTree.Init(count, max_len);
            int i = 0;
            foreach (var node in nodes) {
                BKTree.Add(BKTree.RootNode, node);
            }
            IsLoaded = true;
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
                string result = null;
                if(enumerator.MoveNext()) {
                    result = enumerator.Current;
                } else if(def_enumerator.MoveNext()) {
                    result = def_enumerator.Current.Word;
                }
                if(string.IsNullOrEmpty(result)) {
                    continue;
                }
                yield return result;
            }
        }
    }


}




