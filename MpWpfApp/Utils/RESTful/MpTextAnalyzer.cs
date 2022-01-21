using Azure;
using Azure.AI.TextAnalytics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpTextAnalyzer {
        private static readonly Lazy<MpTextAnalyzer> _Lazy = new Lazy<MpTextAnalyzer>(() => new MpTextAnalyzer());
        public static MpTextAnalyzer Instance { get { return _Lazy.Value; } }

        // Add your Computer Vision subscription key and endpoint to your environment variables.
        private string _subscriptionKey = MpPreferences.AzureTextAnalyticsKey;

        private static string _endpoint = MpPreferences.AzureTextAnalyticsEndpoint;

        public async Task<string> AnalyzeTextAsync(string text) {
            try {
                var client = new TextAnalyticsClient(new Uri(_endpoint), new AzureKeyCredential(_subscriptionKey));
               
                try {
                    Response<DocumentSentiment> response = await client.AnalyzeSentimentAsync(text);
                    DocumentSentiment docSentiment = response.Value;

                    MpConsole.WriteLine($"Sentiment was {docSentiment.Sentiment}, with confidence scores: ");
                    MpConsole.WriteLine($"  Positive confidence score: {docSentiment.ConfidenceScores.Positive}.");
                    MpConsole.WriteLine($"  Neutral confidence score: {docSentiment.ConfidenceScores.Neutral}.");
                    MpConsole.WriteLine($"  Negative confidence score: {docSentiment.ConfidenceScores.Negative}.");

                    foreach(var sentence in docSentiment.Sentences) {
                        MpConsole.WriteLine($"Sentence: {sentence.Text}");
                        MpConsole.WriteLine($"Sentiment: {sentence.Sentiment}");
                    }

                    return docSentiment.ToString();
                }
                catch (RequestFailedException exception) {
                    MpConsole.WriteLine($"Error Code: {exception.ErrorCode}");
                    MpConsole.WriteLine($"Message: {exception.Message}");
                }
            }
            catch (Exception e) {
                MpConsole.WriteLine("\n" + e.Message);
            }

            return null;
        }

        public class MpImageAnalysis {
            public List<MpImageCategory> categories { get; set; }
            public MpImageColor color { get; set; }
            public MpImageDescription description { get; set; }
            public string requestId { get; set; }
            public MpImageMetaData metadata { get; set; }
        }

        public class MpImageCategory {
            public string name { get; set; }
            public double score { get; set; }
        }
        public class MpImageColor {
            public string dominantColorForeground { get; set; }
            public string dominantColorBackground { get; set; }
            public List<string> dominantColors { get; set; }
            public string accentColor { get; set; }
            public bool isBwImg { get; set; }
        }
        public class MpImageDescription {
            public List<string> tags { get; set; }
            public List<MpImageCaptions> captions { get; set; }
        }
        public class MpImageCaptions {
            public string text { get; set; }
            public double confidence { get; set; }
        }
        public class MpImageMetaData {
            public int height { get; set; }
            public int width { get; set; }
            public string format { get; set; }
        }
    }
}