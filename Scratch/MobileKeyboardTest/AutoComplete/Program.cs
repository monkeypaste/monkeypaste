using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;

namespace AutoComplete {

    internal class Program {

        static void Main(string[] args) {
            TextCorrector.Init();
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
            Console.WriteLine($"Enter text to complete (or q! to quit): ");
            string input = Console.ReadLine();
            if (input == "q!") {
                return;
            }
            Console.WriteLine("-----------------------------------------------------------------------");

            var merged_matches = TextCorrector.GetResults(input, true, 8, out string autoCorrectResult);
           if(!string.IsNullOrEmpty(autoCorrectResult)) {
                Console.WriteLine($"Auto-corrected to: {autoCorrectResult}");
            }
            foreach (var x in merged_matches) {
                Console.WriteLine(x);
            }
            Console.WriteLine("-----------------------------------------------------------------------");
        }

    }


}




