using MonkeyPaste.Common.Plugin;

namespace MinimalExample {
    public class MinimalExample : MpIAnalyzeComponent {
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            return new MpAnalyzerPluginResponseFormat() {
                dataObjectLookup = new Dictionary<string, object>() {
                    {"Text", "Hello World" }
                }
            };
        }
    }
}
