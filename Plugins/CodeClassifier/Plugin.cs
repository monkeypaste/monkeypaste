using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;

namespace CodeClassifier {
    public class Plugin : MpIAnalyzerPluginComponent {
        public async Task<object> AnalyzeAsync(object args) {
            await Task.Delay(1);

            var reqParts = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestItemFormat>>(args.ToString());

            var languages = reqParts.FirstOrDefault(x => x.enumId == 1).value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            double minScore = Convert.ToDouble(reqParts.FirstOrDefault(x => x.enumId == 2).value);
            string code = reqParts.FirstOrDefault(x => x.enumId == 3).value;

            CodeClassifier.INCLUDED_LANGUAGES = languages.ToList();

            double certainty;
            Dictionary<string, double> scores;
            string bestLanguage = CodeClassifier.Classify(code, out certainty, out scores);
            string languagesAndScores = "";

            KeyValuePair<string, double> maxLanguage = scores.Aggregate((l, r) => l.Value > r.Value ? l : r);
            KeyValuePair<string, double> minLanguage = scores.Aggregate((l, r) => l.Value < r.Value ? l : r);
            scores.Remove(maxLanguage.Key);
            KeyValuePair<string, double> secondLanguage = scores.Aggregate((l, r) => l.Value > r.Value ? l : r);
            scores.Add(maxLanguage.Key, maxLanguage.Value);

            double scorePercentageDiff = Math.Round((maxLanguage.Value - secondLanguage.Value) / (maxLanguage.Value - minLanguage.Value) * 100, 2);

            foreach (KeyValuePair<string, double> keyValuePair in scores) {
                languagesAndScores += keyValuePair.Key + "\t" + keyValuePair.Value + (keyValuePair.Key == bestLanguage ? " certainty: " + Math.Round(certainty * 100, 0) : "") + "\n";
            }
            string OutputString = languagesAndScores + "\nDifference between first and runner-up: " + scorePercentageDiff + "%.";
            Console.WriteLine(OutputString);


            if(certainty >= minScore) {
                return bestLanguage;
            }
            return null;
        }
    }
}
