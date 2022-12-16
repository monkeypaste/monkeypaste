using System;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System.Linq;
using System.Diagnostics;

namespace HttpAutomation {
    public class HttpAutomation : MpIAnalyzerComponent{
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat request) {
            if(request == null) {
                return new MpAnalyzerPluginResponseFormat() {
                    errorMessage = "Cannot read request: " + request.Serialize()
                };
            }

            var urlParam = request.items.FirstOrDefault(x => x.paramId == 1);
            if(urlParam == null) {
                return new MpAnalyzerPluginResponseFormat() {
                    errorMessage = "No Url Provided"
                };
            }
            var response = new MpAnalyzerPluginResponseFormat();
            Process process = null;
            try {
                process = Process.Start(urlParam.value);
                response.otherMessage = "Success";
            } catch(Exception ex) {
                if(process != null) {
                    process.Dispose();
                }
                MpConsole.WriteTraceLine($"Error opening url '{urlParam.value}' ", ex);
                response.errorMessage = ex.ToString();
            }

            
            return response;
        }
    }
}
