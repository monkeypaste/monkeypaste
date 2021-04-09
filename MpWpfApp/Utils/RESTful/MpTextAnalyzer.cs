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

namespace MpWpfApp {
    public class MpTextAnalyzer {
        private static readonly Lazy<MpTextAnalyzer> _Lazy = new Lazy<MpTextAnalyzer>(() => new MpTextAnalyzer());
        public static MpTextAnalyzer Instance { get { return _Lazy.Value; } }

        // Add your Computer Vision subscription key and endpoint to your environment variables.
        private string _subscriptionKey = Properties.Settings.Default.AzureTextAnalyticsKey;

        private static string _endpoint = Properties.Settings.Default.AzureTextAnalyticsEndpoint;



        /// <summary>
        /// Gets the text visible in the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file with printed text.</param>
        public async Task<string> AnalyzeTextAsync(string text) {
            try {
                var client = new TextAnalyticsClient(new Uri(_endpoint), new AzureKeyCredential(_subscriptionKey));
               
                try {
                    Response<DocumentSentiment> response = await client.AnalyzeSentimentAsync(text);
                    DocumentSentiment docSentiment = response.Value;

                    Console.WriteLine($"Sentiment was {docSentiment.Sentiment}, with confidence scores: ");
                    Console.WriteLine($"  Positive confidence score: {docSentiment.ConfidenceScores.Positive}.");
                    Console.WriteLine($"  Neutral confidence score: {docSentiment.ConfidenceScores.Neutral}.");
                    Console.WriteLine($"  Negative confidence score: {docSentiment.ConfidenceScores.Negative}.");

                    foreach(var sentence in docSentiment.Sentences) {
                        Console.WriteLine($"Sentence: {sentence.Text}");
                        Console.WriteLine($"Sentiment: {sentence.Sentiment}");
                    }

                    return docSentiment.ToString();
                }
                catch (RequestFailedException exception) {
                    Console.WriteLine($"Error Code: {exception.ErrorCode}");
                    Console.WriteLine($"Message: {exception.Message}");
                }
            }
            catch (Exception e) {
                Console.WriteLine("\n" + e.Message);
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