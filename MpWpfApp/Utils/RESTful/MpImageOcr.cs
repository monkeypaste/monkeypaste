using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpImageOcr {
        private static readonly Lazy<MpImageOcr> _Lazy = new Lazy<MpImageOcr>(() => new MpImageOcr());
        public static MpImageOcr Instance { get { return _Lazy.Value; } }

        // Add your Computer Vision subscription key and endpoint to your environment variables.
        private string _subscriptionKey = Properties.Settings.Default.AzureCognitiveServicesKey;

        private static string _endpoint = Properties.Settings.Default.AzureCognitiveServicesEndpoint;

        // the OCR method endpoint
        private string _uriBase = _endpoint + "vision/v2.1/ocr";


        /// <summary>
        /// Gets the text visible in the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file with printed text.</param>
        public async Task<MpOcrAnalysis> OcrImage(byte[] byteData) {
            try {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
                // Request parameters. 
                // The language parameter doesn't specify a language, so the 
                // method detects it automatically.
                // The detectOrientation parameter is set to true, so the method detects and
                // and corrects text orientation before detecting text.
                string requestParameters = "language=unk&detectOrientation=true";

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

                return JsonConvert.DeserializeObject<MpOcrAnalysis>(contentString);
            }
            catch (Exception e) {
                Console.WriteLine("\n" + e.Message);
            }
            return null;
        }

        public async Task<string> OcrImageForText(byte[] byteData) {
            try {
                var j = await OcrImage(byteData);
                var sb = new StringBuilder();
                foreach (var region in j.regions) {
                    foreach (var line in region.lines) {
                        foreach (var word in line.words) {
                            sb.Append(word.text + " ");
                        }
                        sb.Append(Environment.NewLine);
                    }
                }

                return sb.ToString();
            } catch (Exception e) {
                Console.WriteLine("\n" + e.Message);
            }
            return string.Empty;
        }
    }    
}