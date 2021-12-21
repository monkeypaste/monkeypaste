﻿using System;
using System.Windows;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using MonkeyPaste;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace MpWpfApp {
    public class MpTranslatorLanguage {        
        public string LanguageName { get; set; }
        public string NativeName { get; set; }

        public override string ToString() {
            return string.Format(@"{0} {1}", LanguageName, NativeName);
        }
    }

    public class MpLanguageTranslator  {
        private static readonly Lazy<MpLanguageTranslator> _Lazy = new Lazy<MpLanguageTranslator>(() => new MpLanguageTranslator());
        public static MpLanguageTranslator Instance { get { return _Lazy.Value; } }

        // This sample uses the Cognitive Services subscription key for all services. To learn more about
        // authentication options, see: https://docs.microsoft.com/azure/cognitive-services/authentication.
        private string COGNITIVE_SERVICES_KEY = MpPreferences.Instance.AzureCognitiveServicesKey;

        // Endpoints for Translator Text and Bing Spell Check
        public static readonly string TEXT_TRANSLATION_API_ENDPOINT = "https://api.cognitive.microsofttranslator.com/{0}?api-version=3.0";
        const string BING_SPELL_CHECK_API_ENDPOINT = "https://westus.api.cognitive.microsoft.com/bing/v7.0/spellcheck/";


        public SortedDictionary<string, MpTranslatorLanguage> LanguageCodesAndTitles { get; private set; } =
            new SortedDictionary<string, MpTranslatorLanguage>(Comparer<string>.Create((a, b) => string.Compare(a, b, true)));

        public bool IsLoaded => LanguageList.Count > 0;

        private string NoConnectionItemHeader = "Cannot connect to lanugage server";

        public List<string> LanguageList => LanguageCodesAndTitles.Select(x => x.Value.LanguageName).ToList();
        
        public async Task Init() {
            try {
                // at least show an error dialog if there's an unexpected error
                // Get languages for drop-downs
                await GetLanguagesForTranslate();
                // Populate drop-downs with values from GetLanguagesForTranslate
                foreach (string menuItem in LanguageCodesAndTitles.Keys) {
                    LanguageList.Add(menuItem);
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Error trying to connect to internet: " + ex.ToString());
            }
        }

        public MpLanguageTranslator() { }

        // ***** DETECT LANGUAGE OF TEXT TO BE TRANSLATED
        public async Task<string> DetectLanguage(string text) {
            if (!IsLoaded) {
                MpConsole.WriteTraceLine("Is not connected, ignoring translation");
                return string.Empty;
            }
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

            using (var requestStream = detectLanguageWebRequest.GetRequestStream()) {
                requestStream.Write(data, 0, data.Length);
            }

            HttpWebResponse response = (HttpWebResponse)await detectLanguageWebRequest.GetResponseAsync();

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
        private async Task<IEnumerable<Spellc>> spellcheck() {
            List<Spellc> spelc = new List<Spellc>();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Properties.Settings.Default.AzureBingSearchApiKey);
            string text = "The Spell Check lets patners chck a text sting for speling and gramar erors. ";
            string mode = "proof";
            string mkt = "en-us";
            var SpellEndPoint = "https://api.cognitive.microsoft.com/bing/v7.0/spellcheck";
            var result = await client.GetAsync(string.Format("{0}text={1}&mode={2}&mkt={3}", SpellEndPoint, text, mode, mkt));
            result.EnsureSuccessStatusCode();
            var json = await result.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(json);
            for (int i = 0; i < 6; i++) {
                spelc.Add(new Spellc {
                    Offset = "Offset : " + data.flaggedTokens[i].offset,
                    token = "Wrong Word : " + data.flaggedTokens[i].token,
                    Suggestion = "Spelling Suggestion : " + data.flaggedTokens[i].suggestions[0].suggestion
                });
            }
            return spelc;
        }
        private async Task<string> CorrectSpelling(string text) {
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
            HttpWebResponse response = (HttpWebResponse)await spellCheckWebRequest.GetResponseAsync();

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
        private async Task GetLanguagesForTranslate(int retryCount = 5) {
            if(IsLoaded) {
                return;
            }
            if(retryCount <= 0) {
                LanguageList.Clear();
                LanguageList.Add(NoConnectionItemHeader);
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
                response = await WebRequest.GetResponseAsync();
                using (var reader = new StreamReader(response.GetResponseStream(), UnicodeEncoding.UTF8)) {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(reader.ReadToEnd());
                    var languages = result["translation"];

                    foreach (var kv in languages) {
                        if(!LanguageCodesAndTitles.ContainsKey(kv.Key)) {
                            LanguageCodesAndTitles.Add(kv.Key, new MpTranslatorLanguage() { LanguageName = kv.Value["name"], NativeName = kv.Value["nativeName"]});
                        } 
                    }
                }
            }
            catch (Exception ex) {
                if (MpHelpers.Instance.IsConnectedToNetwork()) {
                    Console.WriteLine($"Problem connecting to language server, re-attempt #{6-retryCount} to connect..");
                    await GetLanguagesForTranslate(retryCount--);
                    return;
                } else {
                    Console.WriteLine("Problem connecting to language server (" + ex.ToString() + ")");
                }
            }
        }

        public async Task<string> TranslateAsync(string textToTranslate, string toLanguageCode, string fromLanguageCode = "", bool doSpellCheck = false) {
            if(!IsLoaded) {
                MpConsole.WriteTraceLine("Is not connected, ignoring translation");
                return string.Empty;
            }
            try {
                if(string.IsNullOrEmpty(fromLanguageCode)) {
                    fromLanguageCode = await DetectLanguage(textToTranslate);
                }
                //var toLanguageCode = languageCodesAndTitles[toLanguage];

                // Spell-check the source text if the source language is English
                if (doSpellCheck) {
                    textToTranslate = await CorrectSpelling(textToTranslate);
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
                    if (!string.IsNullOrEmpty(translatedText)) {
                        return translatedText;
                    }
                    return string.Empty;
                }
            }
            catch (Exception ex) {
                Console.WriteLine("LanguageTranslation exception: " + ex.ToString());
                return string.Empty;
            }
        }

        public string GetCodeByLanguageName(string langName) {
            var result = LanguageCodesAndTitles.Where(x => x.Value.LanguageName.ToLower() == langName.ToLower()).FirstOrDefault();
            return null ?? result.Key;
        }

        public string GetLanguageNameByCode(string code) {
            if(LanguageCodesAndTitles.ContainsKey(code)) {
                return LanguageCodesAndTitles[code].LanguageName;
            }
            return null;
        }

        internal class Spellc {
            public string Offset {
                get;
                set;
            }
            public string token {
                get;
                set;
            }
            public string Suggestion {
                get;
                set;
            }
        }

        
        public void Save() {
            throw new NotImplementedException();
        }

        public void Reset() {
            throw new NotImplementedException();
        }

        public bool? GetServiceStatus() {
            throw new NotImplementedException();
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
