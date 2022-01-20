using MonkeyPaste;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Ocr;

namespace MpWpfApp {
    public class MpImageOcr {
        private static readonly Lazy<MpImageOcr> _Lazy = new Lazy<MpImageOcr>(() => new MpImageOcr());
        public static MpImageOcr Instance { get { return _Lazy.Value; } }

        private bool _isOcrLocal = true;

        // Add your Computer Vision subscription key and endpoint to your environment variables.
        private string _subscriptionKey = MpPreferences.Instance.AzureCognitiveServicesKey;
        private static string _endpoint = MpPreferences.Instance.AzureCognitiveServicesEndpoint;

        // the OCR method endpoint
        private string _uriBase = _endpoint + "vision/v2.1/ocr";


        /// <summary>
        /// Gets the text visible in the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file with printed text.</param>
        public async Task<MpOcrAnalysis> OcrImage(byte[] byteData) {
            if(_isOcrLocal) {
                var ocrResult = await OcrEngineFromByteArrayAsync(byteData);
                MonkeyPaste.MpConsole.WriteLine(ocrResult.ToString());
                return ocrResult;
            }
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
                MonkeyPaste.MpConsole.WriteLine("\nResponse:\n\n{0}\n",
                    JToken.Parse(contentString).ToString());

                return JsonConvert.DeserializeObject<MpOcrAnalysis>(contentString);
            }
            catch (Exception e) {
                MonkeyPaste.MpConsole.WriteLine("\n" + e.Message);
            }
            return null;
        }

        public async Task<string> OcrImageForText(byte[] byteData) {
            try {
                var j = await OcrImage(byteData);
                var sb = new StringBuilder();
                if(j.regions != null) {
                    foreach (var region in j.regions) {
                        if(region.lines == null) {
                            continue;
                        }
                        foreach (var line in region.lines) {
                            if (line.words == null) {
                                continue;
                            }
                            foreach (var word in line.words) {
                                sb.Append(word.text + " ");
                            }
                            sb.Append(Environment.NewLine);
                        }
                    }
                }

                return sb.ToString();
            } catch (Exception e) {
                MonkeyPaste.MpConsole.WriteLine("\n" + e.Message);
            }
            return string.Empty;
        }

        private async Task<MpOcrAnalysis> OcrEngineFromByteArrayAsync(byte[] byteData, bool isTemporaryFile = true) {
            string imagePath = MpHelpers.WriteBitmapSourceToFile(MpHelpers.GetTempFileNameWithExtension(@".png"), byteData.ToBitmapSource());

            var ocrResult = await OcrEngineFromFileAsync(imagePath, isTemporaryFile);

            return new MpOcrAnalysis(ocrResult);
        }

        private async Task<OcrResult> OcrEngineFromFileAsync(string imagePath, bool isTemporaryFile = true) {
            // see this about packaging https://docs.microsoft.com/en-us/answers/questions/4354/is-the-ocr-api-supported-from-win32-applications-w.html

            var engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language(Properties.Settings.Default.UserCultureInfoName));
            if(engine == null) {
                engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language(Properties.Settings.Default.DefaultCultureInfoName));
            }
            if(engine == null) {
                MonkeyPaste.MpConsole.WriteLine(@"MpImageOcr.OcrEngineFromFileAsync error unable to create engine for language named: " + Properties.Settings.Default.UserCultureInfoName);
                return null;
            }
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(imagePath);
            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read)) {
                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                OcrResult ocrResult = await engine.RecognizeAsync(softwareBitmap);

                MonkeyPaste.MpConsole.WriteLine(ocrResult.Text);

                if(isTemporaryFile) {
                    File.Delete(imagePath);
                }
                return ocrResult;
            }
        }
    }    
}