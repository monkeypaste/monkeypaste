
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;

namespace AutoComplete {
    internal class Program {
        static string[] dictionary { get; set; }
        static void Main(string[] args) {
            // words_4000 from: https://raw.githubusercontent.com/pkLazer/password_rank/master/4000-most-common-english-words-csv.csv
            // 
            var text = MpFileIo.ReadTextFromFile(@"C:\Users\tkefauver\Source\Repos\MonkeyPaste\Scratch\MobileKeyboardTest\AutoComplete\words_4000.txt");
            //var text = MpFileIo.ReadTextFromFile(@"C:\Users\tkefauver\Source\Repos\MonkeyPaste\Scratch\MobileKeyboardTest\AutoComplete\words_alpha.txt");
            dictionary = text.SplitNoEmpty(Environment.NewLine);
            int max_len = dictionary.Max(x => x.Length);
            BKTree.SetLimits(dictionary.Length, max_len);

            BKTree.ptr = 0;

            BKTree.RootNode = new Node(); // Initialize RT before using it


            // adding dict[] words onto the tree
            for (int i = 0; i < dictionary.Length; i++) {
                Node tmp = new Node(dictionary[i]);
                BKTree.Add(BKTree.RootNode, tmp);

                int percent = (int)(((double)i / (double)dictionary.Length)*100);
                Console.Clear();
                Console.WriteLine($"Loading...{percent}%");
            }

            while(true) {
                Console.Clear();
                //DoDistance();
                DoSimilarWords();
                Console.WriteLine("Press any key to continue");
                Console.ReadLine();
            }
        }
        static void DoDistance() {
            Console.WriteLine($"Enter first word: ");
            string word1 = Console.ReadLine();
            Console.WriteLine($"Enter second word: ");
            string word2 = Console.ReadLine();
            Console.WriteLine($"Distance: {BKTree.EditDistance(word1, word2)}");
        }
        static void DoSimilarWords() {
            Console.WriteLine($"Enter text to complete (or Empty to quit): ");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) {
                return;
            }
            Console.WriteLine("-----------------------------------------------------------------------");

            var similar_matches = BKTree.GetSimilarWords(BKTree.RootNode, input);
            var starts_with_strs = dictionary.Where(x => x.StartsWith(input) && x != input).OrderByDescending(x => x.Length);
            int max_starts_with_len = 0;
            if(starts_with_strs.FirstOrDefault() is { } max) {
                max_starts_with_len = max.Length + 1;
            }
            var starts_with_matches = starts_with_strs.Select(x => (x, -max_starts_with_len + x.Length)).OrderBy(x=>x.Item2);
            var merged_matches =
                starts_with_matches.Union(similar_matches.Where(x => !starts_with_matches.Contains(x)).OrderBy(x => x.Item2));
            foreach (var x in merged_matches) {
                Console.WriteLine(x);
            }
            Console.WriteLine("-----------------------------------------------------------------------");
        }

    }


}




