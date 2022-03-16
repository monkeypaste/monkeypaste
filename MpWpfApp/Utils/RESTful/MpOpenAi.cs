using MonkeyPaste;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.Web.UI.WebControls;
using System.Windows.Threading;
using Microsoft.ML.OnnxRuntime;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpOpenAi : MpRestfulAction {
        private static readonly Lazy<MpOpenAi> _Lazy = new Lazy<MpOpenAi>(() => new MpOpenAi());
        public static MpOpenAi Instance { get { return _Lazy.Value; } }

        private MpOpenAi() { }

        public async Task<string> Summarize(string text) {
            text = text.Replace(Environment.NewLine, " ");
            string openAiToken = MpPreferences.RestfulOpenAiApiKey;
            /*
             curl https://api.openai.com/v1/engines/davinci/completions \
                  -H "Content-Type: application/json" \
                  -H "Authorization: Bearer $OPENAI_API_KEY" \
                  -d '{
                  "prompt": "Jupiter is the fifth planet from the Sun and the largest in the Solar System. It is a gas giant with a mass one-thousandth that of the Sun, but two-and-a-half times that of all the other planets in the Solar System combined. Jupiter is one of the brightest objects visible to the naked eye in the night sky, and has been known to ancient civilizations since before recorded history. It is named after the Roman god Jupiter.[19] When viewed from Earth, Jupiter can be bright enough for its reflected light to cast visible shadows,[20] and is on average the third-brightest natural object in the night sky after the Moon and Venus.\n\nJupiter is primarily composed of hydrogen with a quarter of its mass being helium, though helium comprises only about a tenth of the number of molecules. It may also have a rocky core of heavier elements,[21] but like the other giant planets, Jupiter lacks a well-defined solid surface. Because of its rapid rotation, the planet's shape is that of an oblate spheroid (it has a slight but noticeable bulge around the equator).\n\ntl;dr:",
                  "temperature": 0.3,
                  "max_tokens": 64,
                  "top_p": 1,
                  "frequency_penalty": 0,
                  "presence_penalty": 0,
                  "stop": ["\n"]
                }'
             */
            using (var httpClient = new HttpClient()) {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"),"https://api.openai.com/v1/engines/davinci/completions")) {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {MpPreferences.RestfulOpenAiApiKey}");
                    string reqStr = "{\n  \"prompt\": \"" + text + "\",\n  \"temperature\": 0.3,\n  \"max_tokens\": 64,\n  \"top_p\": 1,\n  \"frequency_penalty\": 0,\n  \"presence_penalty\": 0 \n}";//,\n  \"stop\": [\"\\n\"]\n}";
                    request.Content = new StringContent(reqStr);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var response = await httpClient.SendAsync(request);
                    var responsestr = await response.Content.ReadAsStringAsync();
                    MpConsole.WriteLine("OpenAi Response: ");
                    MpConsole.WriteLine(responsestr);

                    dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(responsestr);

                    Debugger.Break();

                    return responsestr;
                }
            }
        }

        public async Task<string> Request(string endpoint,string reqStr) {
            /*
             curl https://api.openai.com/v1/engines/davinci/completions \
                  -H "Content-Type: application/json" \
                  -H "Authorization: Bearer $OPENAI_API_KEY" \
                  -d '{
                  "prompt": "Jupiter is the fifth planet from the Sun and the largest in the Solar System. It is a gas giant with a mass one-thousandth that of the Sun, but two-and-a-half times that of all the other planets in the Solar System combined. Jupiter is one of the brightest objects visible to the naked eye in the night sky, and has been known to ancient civilizations since before recorded history. It is named after the Roman god Jupiter.[19] When viewed from Earth, Jupiter can be bright enough for its reflected light to cast visible shadows,[20] and is on average the third-brightest natural object in the night sky after the Moon and Venus.\n\nJupiter is primarily composed of hydrogen with a quarter of its mass being helium, though helium comprises only about a tenth of the number of molecules. It may also have a rocky core of heavier elements,[21] but like the other giant planets, Jupiter lacks a well-defined solid surface. Because of its rapid rotation, the planet's shape is that of an oblate spheroid (it has a slight but noticeable bulge around the equator).\n\ntl;dr:",
                  "temperature": 0.3,
                  "max_tokens": 64,
                  "top_p": 1,
                  "frequency_penalty": 0,
                  "presence_penalty": 0,
                  "stop": ["\n"]
                }'
             */
            using (var httpClient = new HttpClient()) {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), endpoint)) {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {MpPreferences.RestfulOpenAiApiKey}");
                    request.Content = new StringContent(reqStr);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var response = await httpClient.SendAsync(request);
                    var responseStr = await response.Content.ReadAsStringAsync();

                    //dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(responsestr);

                    return responseStr;
                }
            }
        }

        protected override int GetCurCallCount() {
            return MpPreferences.RestfulOpenAiCount;
        }

        protected override int GetMaxCallCount() {
            return MpPreferences.RestfulOpenAiMaxCount;
        }

        protected override void IncrementCallCount() {
            MpPreferences.RestfulOpenAiCount++;
        }

        protected override void ClearCount() {
            MpPreferences.RestfulOpenAiCount = 0;
        }
    }
}
