using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using MonkeyPaste.Plugin;
using MpProcessHelper;
using Newtonsoft.Json;

namespace ProcessAutomator
{
    public class ProcessAutomator : MpIAnalyzerPluginComponent {
        public async Task<object> AnalyzeAsync(object args) {
            var reqParts = JsonConvert.DeserializeObject<MpAnalyzerPluginRequestFormat>(args.ToString());

            string processPath = reqParts.items.FirstOrDefault(x => x.paramId == 1).value;
            string processArgs = reqParts.items.FirstOrDefault(x => x.paramId == 2).value;
            bool asAdmin = reqParts.items.FirstOrDefault(x => x.paramId == 3).value.ToLower() == "true";
            bool isSilent = reqParts.items.FirstOrDefault(x => x.paramId == 4).value.ToLower() == "true";
            bool useShellExecute = reqParts.items.FirstOrDefault(x => x.paramId == 5).value.ToLower() == "true";
            string workingDir = reqParts.items.FirstOrDefault(x => x.paramId == 6).value;
            WinApi.ShowWindowCommands windowState = (WinApi.ShowWindowCommands)Enum.Parse(typeof(WinApi.ShowWindowCommands), reqParts.items.FirstOrDefault(x => x.paramId == 7).value);
            bool suppressErrors = reqParts.items.FirstOrDefault(x => x.paramId == 8).value.ToLower() == "true";
            var mw = Application.Current.MainWindow;
            var mwh = new WindowInteropHelper(mw).Handle;

            MpProcessHelper.MpProcessAutomation.StartProcess(
                processPath: processPath,
                args: processArgs,
                asAdministrator: asAdmin,
                isSilent: isSilent,
                useShellExecute: useShellExecute,
                workingDirectory: workingDir,
                showError: !suppressErrors,
                windowState: windowState,
                mainWindowHandle: mwh,
                out string stdOut,
                out string stdErr);
            await Task.Delay(1);

            var response = new MpPluginResponseFormat() {
                
            };
            return null;
        }
    }
}
