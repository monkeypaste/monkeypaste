
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace iosKeyboardTest.Android {
    public static class TextCorrector {
        const int MAX_COMPLETION_RESULTS = 20;
        const int INITIAL_DICT_LEN = 5_000;
        public static bool IS_CASE_SENSITIVE = true;

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
            BKTree.Init(count+1, max_len, IS_CASE_SENSITIVE);
            foreach (var node in nodes) {
                BKTree.Add(BKTree.RootNode, node);
            }

            Defaults = BKTree.Entries
                .Skip(1)
                .Take(MAX_COMPLETION_RESULTS)
                .Select(x => x.Word)
                .ToArray();

            IsLoaded = true;
        }

        public static IEnumerable<string> GetResults(string input, bool autoCorrect, int maxResults, out string autoCorrectResult) {
            autoCorrectResult = null;
            input = input.ToLower();

            if(string.IsNullOrWhiteSpace(input)) {
                Defaults = Defaults.Randomize();
                return Defaults.Take(maxResults);
            }
            // get completion results ordered by frequency
            IEnumerable<string> matches = BKTree.Entries
                    .Select((x, idx) => (x.Word.ToLower(), idx))
                    .Where(x => x.Item1.StartsWith(input))
                    .OrderBy(x => x.Item1)
                    .Select(x => x.Item1);

            if(!matches.Where(x=>x.ToLower() != input).Any()) {
                matches = BKTree.GetSimilarWords(BKTree.RootNode, input)
                    .Where(x => !string.IsNullOrEmpty(x.Item1))
                    .OrderBy(x => x.Item2)
                    .Select(x => x.Item1);
                if (autoCorrect && matches.Where(x => x.ToLower() != input).FirstOrDefault() is { } best_match) {
                    // use best match as auto-correct
                    autoCorrectResult = best_match;
                }
            }
            
            // return closest auto-corrected words
            return matches.Where(x => x.ToLower() != input).Distinct().Take(maxResults);
        }

        public static void UpdateMatcher(string confirmedText) {
            // TODO called when keyboard is done. regex for words, scan data file and add/update new use count field then re-initialize (in background) 
        }
    }


}




