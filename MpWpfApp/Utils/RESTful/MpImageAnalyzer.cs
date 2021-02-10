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
    public class MpImageAnalyzer {
        private static readonly Lazy<MpImageAnalyzer> _Lazy = new Lazy<MpImageAnalyzer>(() => new MpImageAnalyzer());
        public static MpImageAnalyzer Instance { get { return _Lazy.Value; } }

        // Add your Computer Vision subscription key and endpoint to your environment variables.
        private string _subscriptionKey = Properties.Settings.Default.AzureCognitiveServicesKey;

        private static string _endpoint = Properties.Settings.Default.AzureCognitiveServicesEndpoint;

        // the OCR method endpoint
        private string _uriBase = _endpoint + "vision/v3.1/analyze";


        /// <summary>
        /// Gets the text visible in the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file with printed text.</param>
        public async Task<MpImageAnalysis> AnalyzeImage(byte[] byteData) {
            try {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                // The Analyze Image method returns information about the following
                // visual features:
                // Categories:  categorizes image content according to a
                //              taxonomy defined in documentation.
                // Description: describes the image content with a complete
                //              sentence in supported languages.
                // Color:       determines the accent color, dominant color, 
                //              and whether an image is black & white.
                string requestParameters = "visualFeatures=Categories,Description,Color";

                // Assemble the URI for the REST API method.
                string uri = _uriBase + "?" + requestParameters;

                HttpResponseMessage response;

                // Read the contents of the specified local image
                // into a byte array.
                //byte[] byteData = GetImageAsByteArray(imageFilePath);

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData)) {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Asynchronously call the REST API method.
                    response = await client.PostAsync(uri, content);
                }

                // Asynchronously get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                // Display the JSON response.
                Console.WriteLine("\nResponse:\n\n{0}\n",
                    JToken.Parse(contentString).ToString());
                                
                return JsonConvert.DeserializeObject<MpImageAnalysis>(contentString);
                //var sb = new StringBuilder();

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