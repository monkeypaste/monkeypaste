using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using System.Linq;
using Jint;

namespace CodeClassifier {
    public class CodeClassifier : MpIAnalyzerPluginComponent {
        Engine _engine;
        public CodeClassifier() {
            _engine = new Engine();
            string highlightjs = "CodeClassifier.Highlightjs.highlightjs_packed.js";
            string detectorjs = "CodeClassifier.Highlightjs.classifier.html";
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(highlightjs)) {
                if (stream == null) {
                    throw new FileNotFoundException("Cannot find file.", highlightjs);
                }
                using (StreamReader reader = new StreamReader(stream)) {
                    string result = reader.ReadToEnd();

                    //_engine.Execute(result);
                    using (var stream2 = assembly.GetManifestResourceStream(detectorjs)) {
                        if (stream2 == null) {
                            throw new FileNotFoundException("Cannot find file.", detectorjs);
                        }
                        using (StreamReader reader2 = new StreamReader(stream2)) {
                            string result2 = reader2.ReadToEnd();

                            _engine.Execute(result2);

                        }
                    }
                }
            }

        }
        public async Task<object> AnalyzeAsync(object args) {
            await Task.Delay(1);
            
            var reqParts = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestItemFormat>>(args.ToString());

            var languages = reqParts.FirstOrDefault(x => x.enumId == 1).value.Split(new string[]{","},StringSplitOptions.RemoveEmptyEntries);
            string code = reqParts.FirstOrDefault(x => x.enumId == 2).value;

            _engine.SetValue ("languages", languages);

            _engine.SetValue("code", code);
            var outResult = _engine.Invoke("detect");
            
            Console.WriteLine("Detection: " + outResult);
            return outResult.AsString();
        }
    }
}
