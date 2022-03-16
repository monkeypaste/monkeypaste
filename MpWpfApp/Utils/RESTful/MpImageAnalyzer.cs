using MonkeyPaste;
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
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpImageAnalyzer {
        private static readonly Lazy<MpImageAnalyzer> _Lazy = new Lazy<MpImageAnalyzer>(() => new MpImageAnalyzer());
        public static MpImageAnalyzer Instance { get { return _Lazy.Value; } }

        // Add your Computer Vision subscription key and endpoint to your environment variables.
        private string _subscriptionKey = MpPreferences.AzureCognitiveServicesKey;

        private static string _endpoint = MpPreferences.AzureCognitiveServicesEndpoint;

        // the OCR method endpoint
        private string _uriBase = _endpoint + "vision/v3.1/analyze";


        /// <summary>
        /// Gets the text visible in the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file with printed text.</param>
        public async Task<MpAzureImageAnalysis> AnalyzeImage(byte[] byteData, MpAzureImageAnalysisRequest req) {
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
                //string requestParameters = "visualFeatures=Categories,Description,Color";
                string requestParameters = string.Empty;
                if(req.VisualFeatures.Count > 0) {
                    requestParameters = "visualFeatures=" + string.Join(",", req.VisualFeatures);
                }
                if (req.Details.Count > 0) {
                    if (req.VisualFeatures.Count > 0) {
                        requestParameters += "&";
                    }
                    requestParameters += "details=" + string.Join(",", req.Details);
                }
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
                //MpConsole.WriteLine("\nResponse:\n\n{0}\n",
                //    JToken.Parse(contentString).ToString());

                //return contentString;                                
                var result = JsonConvert.DeserializeObject<MpAzureImageAnalysis>(contentString);
                return result;
            }
            catch (Exception e) {
                MpConsole.WriteLine("\n" + e.Message);
            }

            return null;
        }
    }
}