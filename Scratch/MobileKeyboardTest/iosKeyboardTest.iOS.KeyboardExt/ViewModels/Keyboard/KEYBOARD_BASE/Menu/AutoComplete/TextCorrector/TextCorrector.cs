
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace iosKeyboardTest.iOS.KeyboardExt {
    public static class TextCorrector {
        const int MAX_COMPLETION_RESULTS = 20;
        const int INITIAL_DICT_LEN = 5_000;

        public static bool IsLoaded { get; private set; }
        static IEnumerable<string> Defaults { get; set; }
        public static void Init(IAssetLoader loader) {
            // TODO should add another interface to pre-check local storage here and load from there if available
            // when NOT available load from bundle then write to local storage and recall self
            if(IsLoaded) {
                return;
            }
            if (loader.LoadStream("words_5000_bare.txt") is not { } stream) {
                return;
            }
            IsLoaded = false;

            string text = null;
            using (StreamReader f = new StreamReader(stream)) {
                text = f.ReadToEnd();
                f.Close();
            }

            var rows = text.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            int count = rows.Length;
            var nodes = rows.Select((x,idx) => new Node(idx + 1, x));
            int max_len = nodes.Max(x => x.Word.Length);
            BKTree.Init(count+1, max_len);
            foreach (var node in nodes) {
                BKTree.Add(BKTree.RootNode, node);
            }
            Defaults = BKTree.Entries.Skip(1).Take(MAX_COMPLETION_RESULTS).Select(x => x.Word).ToArray();

            IsLoaded = true;
        }

        public static IEnumerable<string> GetResults(string input, bool autoCorrect, int maxResults, out string autoCorrectResult) {
            autoCorrectResult = null;

            if(string.IsNullOrWhiteSpace(input)) {
                Defaults = Defaults.Randomize();
                return Defaults.Take(maxResults);
            }
            // get completion results ordered by frequency
            var starts_with_strs = 
                BKTree.Entries
                .Where(x => x.Word.StartsWith(input) && x.Word != input)
                .OrderBy(x=>x.Rank)
                //.OrderBy(x=>x.Word.Length)
                //.ThenBy(x => x.Rank)
                ;
            if(starts_with_strs.Any()) {
                // word can be completed
                return starts_with_strs.Select(x => x.Word).Take(maxResults);
            }
            // might be misspelled
            var similar_matches = BKTree.GetSimilarWords(BKTree.RootNode, input).Where(x=>!string.IsNullOrEmpty(x.Item1)).OrderBy(x => x.Item2).ToList();
            if(autoCorrect && similar_matches.FirstOrDefault() is { } best_match) {
                // use best match as auto-correct
                autoCorrectResult = best_match.Item1;
            }
            // return closest auto-corrected words
            return similar_matches.Select(x=>x.Item1).Take(maxResults);
        }

        public static void UpdateMatcher(string confirmedText) {
            // TODO called when keyboard is done. regex for words, scan data file and add/update new use count field then re-initialize (in background) 
        }
    }


}




