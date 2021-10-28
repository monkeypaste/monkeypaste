using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpMinifyUrl : MpRestfulApi {
        private static readonly Lazy<MpMinifyUrl> _Lazy = new Lazy<MpMinifyUrl>(() => new MpMinifyUrl());
        public static MpMinifyUrl Instance { get { return _Lazy.Value; } }

        private MpMinifyUrl() : base("Bit.ly Minifier") { }

        public async Task<string> ShortenUrl(string url) {
            var result = CheckRestfulApiStatus();
            if(result == null || result.Value == false) {
                return string.Empty;
            }
            string bitlyToken = MpPreferences.Instance.BitlyApiToken;
            using (HttpClient client = new HttpClient()) {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api-ssl.bitly.com/v4/shorten")) {
                    request.Content = new StringContent($"{{\"long_url\":\"{url}\"}}", Encoding.UTF8, "application/json");
                    try {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bitlyToken);
                        using (var response = await client.SendAsync(request).ConfigureAwait(false)) {
                            if (!response.IsSuccessStatusCode) {
                                MonkeyPaste.MpConsole.WriteLine("Minify error: " + response.Content.ToString());
                                return string.Empty;
                            }

                            var responsestr = await response.Content.ReadAsStringAsync();

                            dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(responsestr);
                            var linkText = (string)jsonResponse["link"];
                            if(!string.IsNullOrEmpty(linkText)) {
                                IncrementCallCount();
                                return linkText;
                            }
                            ShowError();
                            return String.Empty;
                        }
                    }
                    catch (Exception ex) {
                        MonkeyPaste.MpConsole.WriteLine("Minify exception: " + ex.ToString());
                        ShowError();
                        return string.Empty;
                    }
                }
            }
        }

        protected override int GetCurCallCount() {
            return MpPreferences.Instance.RestfulLinkMinificationCount;
        }

        protected override int GetMaxCallCount() {
            return MpPreferences.Instance.RestfulLinkMinificationMaxCount;
        }

        protected override void IncrementCallCount() {
            MpPreferences.Instance.RestfulLinkMinificationCount++;
        }

        protected override void ClearCount() {
            MpPreferences.Instance.RestfulLinkMinificationCount = 0;
        }
    }
}
