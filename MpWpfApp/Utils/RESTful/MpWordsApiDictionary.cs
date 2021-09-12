using System;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace MpWpfApp {
    public class MpWordsApiDictionary {
        private static readonly Lazy<MpWordsApiDictionary> _Lazy = new Lazy<MpWordsApiDictionary>(() => new MpWordsApiDictionary());
        public static MpWordsApiDictionary Instance { get { return _Lazy.Value; } }

        #region Properties

        #endregion

        #region Public Methods

        public async void TestWordsGet() {
            var client = new HttpClient();
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://wordsapiv1.p.rapidapi.com/words/null"),
                Headers = {
                        { "x-rapidapi-key", "cd3e83d880msh7673f41c6e5f234p16317fjsn051964761239" },
                        { "x-rapidapi-host", "wordsapiv1.p.rapidapi.com" },
                    },
                };
            using (var response = await client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                //var result = JsonConvert.DeserializeObject<List<Dictionary<string, List<Dictionary<string, string>>>>>(body);
                //return result[0]["translations"][0]["text"];
                MonkeyPaste.MpConsole.WriteLine(body);
            }
        }
        #endregion
    }
}
