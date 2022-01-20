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
using MonkeyPaste;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace MpWpfApp {

    public static class MpLanguageTranslator  {
        #region private static Variables

        private static bool _isLoaded = false;
        private static string COGNITIVE_SERVICES_KEY = MpPreferences.Instance.AzureCognitiveServicesKey;
        private static string TEXT_TRANSLATION_API_ENDPOINT = "https://api.cognitive.microsofttranslator.com/{0}?api-version=3.0";

        #endregion

        #region Properties
        public static SortedDictionary<string, MpAzureTranslatorLanguageFormat> LanguageCodesAndTitles { get; private set; } =
            new SortedDictionary<string, MpAzureTranslatorLanguageFormat>(Comparer<string>.Create((a, b) => string.Compare(a, b, true)));

        public static bool IsLoaded => _isLoaded;

        public static List<string> LanguageList => LanguageCodesAndTitles.Select(x => x.Value.LanguageName).ToList();


        #endregion


        #region Constructors

        public static async Task Init() {
            if(IsLoaded) {
                return;
            }
            await GetLanguagesForTranslate();
            // Populate drop-downs with values from GetLanguagesForTranslate
            foreach (string menuItem in LanguageCodesAndTitles.Keys) {
                LanguageList.Add(menuItem);
            }
            _isLoaded = true;
        }

        #endregion

        // ***** GET TRANSLATABLE LANGUAGE CODES
        private static async Task GetLanguagesForTranslate(int retryCount = 5) {
            if(IsLoaded || !MpHelpers.IsConnectedToInternet()) {
                return;
            }
            if(retryCount <= 0) {
                LanguageList.Clear();
                LanguageList.Add("No network connection");
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
                    //var result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(reader.ReadToEnd());
                    //var languages = result["translation"];
                    var result = JsonConvert.DeserializeObject<MpAzureTranslatableLanguagesRequestFormat>(reader.ReadToEnd());
                    foreach (var kv in result.Translation) {
                        if(!LanguageCodesAndTitles.ContainsKey(kv.Key)) {
                            LanguageCodesAndTitles.Add(kv.Key, kv.Value);
                        } 
                    }
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("Problem connecting to language server (" + ex.ToString() + ")");
                MpConsole.WriteTraceLine($"Problem connecting to language server, re-attempt #{6 - retryCount} to connect..");
                await GetLanguagesForTranslate(retryCount--);
                return;
            }
        }

        public static async Task<string> TranslateAsync(string textToTranslate, string toLanguageCode, string fromLanguageCode = "") {
            if(!IsLoaded) {
                MpConsole.WriteTraceLine("Is not connected, ignoring translation");
                return string.Empty;
            }
            try {
                // Handle null operations: no text or same source/target languages
                if (string.IsNullOrEmpty(textToTranslate) || fromLanguageCode == toLanguageCode) {
                    return textToTranslate;
                }

                // Send translation request
                string endpoint = string.Format(TEXT_TRANSLATION_API_ENDPOINT, "translate");
                string fromStr = string.Format(@"&from={0}", fromLanguageCode);
                string toStr = string.Format(@"&to={0}", toLanguageCode);
                string fromToStr = fromStr + toStr;
                string uri = endpoint + fromToStr;

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
                    MpConsole.WriteLine(responseBody);
                    //dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    //var result = JsonConvert.DeserializeObject<List<Dictionary<string, List<Dictionary<string, string>>>>>(responseBody);
                    var result = JsonConvert.DeserializeObject<List<MpAzureTranslateResultFormat>>(responseBody);
                    //string translatedText = (string)jsonResponse[0]["translations"][0]["text"];
                    string translatedText = result[0].Translations[0].Text;
                    if (!string.IsNullOrEmpty(translatedText)) {
                        return translatedText;
                    }
                    return string.Empty;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteLine("LanguageTranslation exception: " + ex.ToString());
                return string.Empty;
            }
        }

        public static string GetCodeByLanguageName(string langName) {
            var result = LanguageCodesAndTitles.Where(x => x.Value.LanguageName.ToLower() == langName.ToLower()).FirstOrDefault();
            return null ?? result.Key;
        }

        public static string GetLanguageNameByCode(string code) {
            if(LanguageCodesAndTitles.ContainsKey(code)) {
                return LanguageCodesAndTitles[code].LanguageName;
            }
            return null;
        }

    }
}
