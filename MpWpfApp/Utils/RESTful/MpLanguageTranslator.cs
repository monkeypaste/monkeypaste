using System;
using System.Windows;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpLanguageTranslator : MpRestfulApi {
        private static readonly Lazy<MpLanguageTranslator> _Lazy = new Lazy<MpLanguageTranslator>(() => new MpLanguageTranslator());
        public static MpLanguageTranslator Instance { get { return _Lazy.Value; } }

        // This sample uses the Cognitive Services subscription key for all services. To learn more about
        // authentication options, see: https://docs.microsoft.com/azure/cognitive-services/authentication.
        private string COGNITIVE_SERVICES_KEY = Properties.Settings.Default.AzureCognitiveServicesKey;
        // Endpoints for Translator Text and Bing Spell Check
        public static readonly string TEXT_TRANSLATION_API_ENDPOINT = "https://api.cognitive.microsofttranslator.com/{0}?api-version=3.0";
        const string BING_SPELL_CHECK_API_ENDPOINT = "https://westus.api.cognitive.microsoft.com/bing/v7.0/spellcheck/";
        // An array of language codes
        private string[] languageCodes;

        // Dictionary to map language codes from friendly name (sorted case-insensitively on language name)
        private SortedDictionary<string, string> languageCodesAndTitles =
            new SortedDictionary<string, string>(Comparer<string>.Create((a, b) => string.Compare(a, b, true)));

        public List<string> LanguageList { get; private set; } = new List<string>();

        public MpLanguageTranslator() : base("Language Translation") {
            try {
                if (!MpHelpers.Instance.IsConnectedToInternet()) {
                    Console.WriteLine("Client offline. Language Translation is inactive");
                    return;
                }
                // at least show an error dialog if there's an unexpected error
                //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(HandleExceptions);

                if (COGNITIVE_SERVICES_KEY.Length != 32) {
                    MessageBox.Show("One or more invalid API subscription keys.\n\n" +
                        "Put your keys in the *_API_SUBSCRIPTION_KEY variables in MainWindow.xaml.cs.",
                        "Invalid Subscription Key(s)", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Windows.Application.Current.Shutdown();
                } else {
                    // Get languages for drop-downs
                    GetLanguagesForTranslate();
                    // Populate drop-downs with values from GetLanguagesForTranslate
                    foreach (string menuItem in languageCodesAndTitles.Keys) {
                        LanguageList.Add(menuItem);
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Error trying to connect to internet: " + ex.ToString());
            }
            
        }

        // Global exception handler to display error message and exit
        private static void HandleExceptions(object sender, UnhandledExceptionEventArgs args) {
            Exception e = (Exception)args.ExceptionObject;
            MessageBox.Show("Caught " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            System.Windows.Application.Current.Shutdown();
        }

        // ***** DETECT LANGUAGE OF TEXT TO BE TRANSLATED
        public string DetectLanguage(string text) {
            string detectUri = string.Format(TEXT_TRANSLATION_API_ENDPOINT, "detect");

            // Create request to Detect languages with Translator Text
            HttpWebRequest detectLanguageWebRequest = (HttpWebRequest)WebRequest.Create(detectUri);
            detectLanguageWebRequest.Headers.Add("Ocp-Apim-Subscription-Key", COGNITIVE_SERVICES_KEY);
            detectLanguageWebRequest.Headers.Add("Ocp-Apim-Subscription-Region", "westus");
            detectLanguageWebRequest.ContentType = "application/json; charset=utf-8";
            detectLanguageWebRequest.Method = "POST";

            // Send request
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string jsonText = serializer.Serialize(text);

            string body = "[{ \"Text\": " + jsonText + " }]";
            byte[] data = Encoding.UTF8.GetBytes(body);

            detectLanguageWebRequest.ContentLength = data.Length;

            using (var requestStream = detectLanguageWebRequest.GetRequestStream())
                requestStream.Write(data, 0, data.Length);

            HttpWebResponse response = (HttpWebResponse)detectLanguageWebRequest.GetResponse();

            // Read and parse JSON response
            var responseStream = response.GetResponseStream();
            var jsonString = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
            dynamic jsonResponse = serializer.DeserializeObject(jsonString);

            // Fish out the detected language code
            var languageInfo = jsonResponse[0];
            if (languageInfo["score"] > (decimal)0.5) {
                //DetectedLanguageLabel.Content = languageInfo["language"];
                return languageInfo["language"];
            } else
                return "Unable to confidently detect input language.";
        }

        // ***** CORRECT SPELLING OF TEXT TO BE TRANSLATED
        private string CorrectSpelling(string text) {
            string uri = BING_SPELL_CHECK_API_ENDPOINT + "?mode=spell&mkt=en-US";

            // Create a request to Bing Spell Check API
            HttpWebRequest spellCheckWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            spellCheckWebRequest.Headers.Add("Ocp-Apim-Subscription-Key", COGNITIVE_SERVICES_KEY);
            spellCheckWebRequest.Method = "POST";
            spellCheckWebRequest.ContentType = "application/x-www-form-urlencoded"; // doesn't work without this

            // Create and send the request
            string body = "text=" + System.Web.HttpUtility.UrlEncode(text);
            byte[] data = Encoding.UTF8.GetBytes(body);
            spellCheckWebRequest.ContentLength = data.Length;
            using (var requestStream = spellCheckWebRequest.GetRequestStream()) {
                requestStream.Write(data, 0, data.Length);
            }
            HttpWebResponse response = (HttpWebResponse)spellCheckWebRequest.GetResponse();

            // Read and parse the JSON response; get spelling corrections
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var responseStream = response.GetResponseStream();
            var jsonString = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
            dynamic jsonResponse = serializer.DeserializeObject(jsonString);
            var flaggedTokens = jsonResponse["flaggedTokens"];

            // Construct sorted dictionary of corrections in reverse order (right to left)
            // This ensures that changes don't impact later indexes
            var corrections = new SortedDictionary<int, string[]>(Comparer<int>.Create((a, b) => b.CompareTo(a)));
            for (int i = 0; i < flaggedTokens.Length; i++) {
                var correction = flaggedTokens[i];
                var suggestion = correction["suggestions"][0];  // consider only first suggestion
                if (suggestion["score"] > (decimal)0.7) {       // take it only if highly confident
                    corrections[(int)correction["offset"]] = new string[]   // dict key   = offset
                        { correction["token"], suggestion["suggestion"] };  // dict value = {error, correction}
                }
            }

            // Apply spelling corrections, in order, from right to left
            foreach (int i in corrections.Keys) {
                var oldtext = corrections[i][0];
                var newtext = corrections[i][1];

                // Apply capitalization from original text to correction - all caps or initial caps
                if (text.Substring(i, oldtext.Length).All(char.IsUpper)) {
                    newtext = newtext.ToUpper();
                } else if (char.IsUpper(text[i])) {
                    newtext = newtext[0].ToString().ToUpper() + newtext.Substring(1);
                }

                text = text.Substring(0, i) + newtext + text.Substring(i + oldtext.Length);
            }

            return text;
        }

        // ***** GET TRANSLATABLE LANGUAGE CODES
        private void GetLanguagesForTranslate() {
            if(!MpHelpers.Instance.IsConnectedToInternet()) {
                Console.WriteLine("Client offline. Language Translation is inactive");
                return;
            }
            // Send a request to get supported language codes
            string uri = String.Format(TEXT_TRANSLATION_API_ENDPOINT, "languages") + "&scope=translation";
            WebRequest WebRequest = WebRequest.Create(uri);
            WebRequest.Headers.Add("Ocp-Apim-Subscription-Key", COGNITIVE_SERVICES_KEY);
            WebRequest.Headers.Add("Accept-Language", "en");
            WebResponse response = null;
            // Read and parse the JSON response
            try {
                response = WebRequest.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream(), UnicodeEncoding.UTF8)) {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(reader.ReadToEnd());
                    var languages = result["translation"];

                    languageCodes = languages.Keys.ToArray();
                    foreach (var kv in languages) {
                        languageCodesAndTitles.Add(kv.Value["name"], kv.Key);
                    }
                }
            } catch(Exception ex) {
                if(MpHelpers.Instance.IsConnectedToInternet()) {
                    Console.WriteLine("Problem connecting to language server (" + ex.ToString() + "), re-attempting to connect..");
                    GetLanguagesForTranslate();
                } else {
                    Console.WriteLine("Problem connecting to language server (" + ex.ToString() + ")");
                }
            }
        }

        public async Task<string> Translate(string textToTranslate, string toLanguage, bool doSpellCheck) {
            var apiStatus = CheckRestfulApiStatus();
            if (apiStatus == null || apiStatus.Value == false) {
                return string.Empty;
            }

            try {
                var fromLanguageCode = DetectLanguage(textToTranslate);
                var toLanguageCode = languageCodesAndTitles[toLanguage];

                // Spell-check the source text if the source language is English
                if (doSpellCheck) {
                    textToTranslate = CorrectSpelling(textToTranslate);
                }

                // Handle null operations: no text or same source/target languages
                if (string.IsNullOrEmpty(textToTranslate) || fromLanguageCode == toLanguageCode) {
                    return textToTranslate;
                }

                // Send translation request
                string endpoint = string.Format(TEXT_TRANSLATION_API_ENDPOINT, "translate");
                string uri = string.Format(endpoint + "&from={0}&to={1}", fromLanguageCode, toLanguageCode);

                System.Object[] body = new System.Object[] { new { Text = textToTranslate } };
                var requestBody = JsonConvert.SerializeObject(body);

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage()) {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(uri);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", COGNITIVE_SERVICES_KEY);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", "westus");
                    request.Headers.Add("X-ClientTraceId", Guid.NewGuid().ToString());

                    var response = await client.SendAsync(request);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<List<Dictionary<string, List<Dictionary<string, string>>>>>(responseBody);
                    var translatedText = result[0]["translations"][0]["text"];
                    if(!string.IsNullOrEmpty(translatedText)) {
                        IncrementCallCount();
                        return translatedText;
                    }
                    ShowError();
                    return string.Empty;
                }
            }
            catch(Exception ex) {
                Console.WriteLine("LanguageTranslation exception: " + ex.ToString());
                ShowError();
                return string.Empty;
            }
        }

        protected override int GetMaxCallCount() {
            return Properties.Settings.Default.RestfulTranslationMaxCount;
        }

        protected override int GetCurCallCount() {
            return Properties.Settings.Default.RestfulTranslationCount;
        }

        protected override void IncrementCallCount() {
            Properties.Settings.Default.RestfulTranslationCount++;
            Properties.Settings.Default.Save();
        }

        protected override void ClearCount() {
            Properties.Settings.Default.RestfulTranslationCount = 0;
            Properties.Settings.Default.Save();
        }

        // ***** PERFORM TRANSLATION ON BUTTON CLICK
        //private async void TranslateButton_Click(object sender, EventArgs e) {
        //    string textToTranslate = TextToTranslate.Text.Trim();

        //    string fromLanguage = FromLanguageComboBox.SelectedValue.ToString();
        //    string fromLanguageCode;

        //    // Auto-detect source language if requested
        //    if (fromLanguage == "Detect") {
        //        fromLanguageCode = DetectLanguage(textToTranslate);
        //        if (!languageCodes.Contains(fromLanguageCode)) {
        //            MessageBox.Show("The source language could not be detected automatically " +
        //                "or is not supported for translation.", "Language detection failed",
        //                MessageBoxButton.OK, MessageBoxImage.Error);
        //            return;
        //        }
        //    } else
        //        fromLanguageCode = languageCodesAndTitles[fromLanguage];

        //    string toLanguageCode = languageCodesAndTitles[ToLanguageComboBox.SelectedValue.ToString()];

        //    // Spell-check the source text if the source language is English
        //    if (fromLanguageCode == "en") {
        //        if (textToTranslate.StartsWith("-"))    // don't spell check in this case
        //            textToTranslate = textToTranslate.Substring(1);
        //        else {
        //            textToTranslate = CorrectSpelling(textToTranslate);
        //            TextToTranslate.Text = textToTranslate;     // put corrected text into input field
        //        }
        //    }

        //    // Handle null operations: no text or same source/target languages
        //    if (textToTranslate == "" || fromLanguageCode == toLanguageCode) {
        //        TranslatedTextLabel.Content = textToTranslate;
        //        return;
        //    }

        //    // Send translation request
        //    string endpoint = string.Format(TEXT_TRANSLATION_API_ENDPOINT, "translate");
        //    string uri = string.Format(endpoint + "&from={0}&to={1}", fromLanguageCode, toLanguageCode);

        //    System.Object[] body = new System.Object[] { new { Text = textToTranslate } };
        //    var requestBody = JsonConvert.SerializeObject(body);

        //    using (var client = new HttpClient())
        //    using (var request = new HttpRequestMessage()) {
        //        request.Method = HttpMethod.Post;
        //        request.RequestUri = new Uri(uri);
        //        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        //        request.Headers.Add("Ocp-Apim-Subscription-Key", COGNITIVE_SERVICES_KEY);
        //        request.Headers.Add("Ocp-Apim-Subscription-Region", "westus");
        //        request.Headers.Add("X-ClientTraceId", Guid.NewGuid().ToString());

        //        var response = await client.SendAsync(request);
        //        var responseBody = await response.Content.ReadAsStringAsync();

        //        var result = JsonConvert.DeserializeObject<List<Dictionary<string, List<Dictionary<string, string>>>>>(responseBody);
        //        var translation = result[0]["translations"][0]["text"];

        //        // Update the translation field
        //        TranslatedTextLabel.Content = translation;
        //    }
        //}
    }
}
