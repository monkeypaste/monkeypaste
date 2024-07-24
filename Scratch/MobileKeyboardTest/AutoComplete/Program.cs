
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;

namespace AutoComplete {
    internal class Program {
        static void Main(string[] args) {
            // words_4000 from: https://raw.githubusercontent.com/pkLazer/password_rank/master/4000-most-common-english-words-csv.csv
            // 
            var text = MpFileIo.ReadTextFromFile(@"C:\Users\tkefauver\Source\Repos\MonkeyPaste\Scratch\MobileKeyboardTest\AutoComplete\words_4000.txt");
            string[] dictionary = text.SplitNoEmpty(Environment.NewLine);
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
                Console.WriteLine($"Enter text to complete (or Empty to quit): ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) {
                    return;
                }
                Console.WriteLine("-----------------------------------------------------------------------");
                var match = BKTree.GetSimilarWords(BKTree.RootNode, input).OrderBy(x=>x.Item2).Select(x=>x.Item1);
                foreach (var x in match) {
                    Console.WriteLine(x);
                }
                Console.WriteLine("");
                Console.WriteLine("Press any key to continue");
                Console.ReadLine();
            }

            //string w1 = "ops";
            //string w2 = "helt";
            //List<string> match = BKTree.GetSimilarWords(BKTree.RootNode, w1);
            //Console.WriteLine("Similar words in dictionary for : " + w1 + ":");
            //foreach (var x in match)
            //    Console.WriteLine(x);

            //match = BKTree.GetSimilarWords(BKTree.RootNode, w2);
            //Console.WriteLine("Correct words in dictionary for " + w2 + ":");
            //foreach (var x in match)
            //    Console.WriteLine(x);
        }
    }
}




