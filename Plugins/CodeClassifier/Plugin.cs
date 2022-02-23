using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeClassifier.StringTokenizer;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;

namespace CodeClassifier {
    public class Plugin : MpIAnalyzerPluginComponent {
        public async Task<object> AnalyzeAsync(object args) {
            await Task.Delay(1);
            
            MpPluginResponseFormat response = null;

            var reqParts = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestItemFormat>>(args.ToString());
            //languages (SHOULD) always part of request
            if (reqParts.Any(x => x.enumId == 1) &&
               reqParts.Any(x => x.enumId == 3) &&
               reqParts.Any(x => x.enumId == 4)) {
                var languages = reqParts.FirstOrDefault(x => x.enumId == 1).value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                string code = reqParts.FirstOrDefault(x => x.enumId == 3).value;
                bool isTraining = reqParts.FirstOrDefault(x => x.enumId == 4).value.ToLower() == "true";


                if (isTraining) {
                    bool isSuccess = Train(code, languages.ToList());
                    response = new MpPluginResponseFormat() {
                        message = isSuccess ? "Success" : "Training Failed"
                    };

                } else if (reqParts.Any(x => x.enumId == 2)) {
                    //request is to classify snippet
                    double minScore = Convert.ToDouble(reqParts.FirstOrDefault(x => x.enumId == 2).value);

                    var result = Classify(code, languages.ToList(),minScore);

                    if (result.Value >= minScore) {
                        response = new MpPluginResponseFormat() {
                            annotations = new List<MpPluginResponseAnnotationFormat>() {
                                new MpPluginResponseAnnotationFormat() {
                                    label = new MpJsonPathProperty(result.Key),
                                    score = new MpJsonPathProperty<double>(result.Value),
                                    range = new MpAnalyzerPluginTextTokenResponseValueFormat() {
                                        rangeStart = new MpJsonPathProperty<int>(0),
                                        rangeEnd = new MpJsonPathProperty<int>(code.Length)
                                    }
                                }
                            }};
                    }
                } else {
                    Console.WriteLine("Unsupported request: " + args.ToString());
                }
            }


            return response;
        }

        private bool Train(string code, List<string> languages) {            
            if (languages == null || string.IsNullOrWhiteSpace(code)) {
                return false;
            }
            bool isSuccess = true;

            string trainingSetPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (trainingSetPath == null) {
                throw new DirectoryNotFoundException("Could not find the training-set folder.");
            }
            string path = Path.Combine(trainingSetPath, "training-set");

            string[] folders = Directory.GetDirectories(path);
            foreach (string languageFolder in folders) {
                if (!languages.Contains(Path.GetFileNameWithoutExtension(languageFolder))) {
                    continue;
                }

                string newTrainerFilePath = GetUniqueFileName("txt", "trainer", languageFolder);

                newTrainerFilePath = WriteTextToFile(newTrainerFilePath, code);
                if(newTrainerFilePath == null) {
                    isSuccess = false;
                }
            }
            return isSuccess;
        }
        private KeyValuePair<string, double> Classify(string code, List<string> languages, double minScore) {
            CodeClassifier.Init(languages.ToList());

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

            return new KeyValuePair<string, double>(bestLanguage, certainty);
        }

        private string GetUniqueFileName(string ext, string baseName = "", string baseDir = "") {
            //only support Image and RichText fileTypes
            string fp = string.IsNullOrEmpty(baseDir) ? Path.GetTempPath() : baseDir;
            string fn = string.IsNullOrEmpty(baseName) ? Path.GetRandomFileName() : baseName.Trim();
            if (string.IsNullOrEmpty(fn)) {
                fn = Path.GetRandomFileName();
            }
            string fe = "." + ext;

            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fp + fn + fe);
            string extension = Path.GetExtension(fp + fn + fe);
            string path = Path.GetDirectoryName(fp + fn + fe);
            string newFullPath = fp + fn + fe;

            while (File.Exists(newFullPath)) {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            return newFullPath;
        }

        private string WriteTextToFile(string filePath, string text) {
            try {
                using (var of = new StreamWriter(filePath)) {
                    of.Write(text);
                    of.Close();
                    return filePath;
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error writing to path '{filePath}' with text '{text}'", ex);
                return null;
            }
        }
    }
}
