using System;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;
using System.Linq;
using System.Diagnostics;

namespace HttpAutomation {
    public class HttpAutomation : MpIAnalyzerComponent{
        public async Task<object> AnalyzeAsync(object args) {
            await Task.Delay(1);

            var response = new MpPluginResponseFormat();
            
            var request = MpJsonObject.DeserializeObject<MpAnalyzerPluginRequestFormat>(args.ToString());
            if(request == null) {
                response.message = "Cannot read request: " + args.ToString();
                return response;
            }

            var urlParam = request.items.FirstOrDefault(x => x.paramId == 1);
            if(urlParam == null) {
                response.message = "No url provided";
                return response;
            }
            Process process = null;
            try {
                process = Process.Start(urlParam.value);
                response.message = "Success";
            } catch(Exception ex) {
                if(process != null) {
                    process.Dispose();
                }
                MpConsole.WriteTraceLine($"Error opening url '{urlParam.value}' ", ex);
                response.message = MpPluginResponseFormat.ERROR_MESSAGE;
            }

            
            return response;
        }
    }
}
