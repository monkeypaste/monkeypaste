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
            var reqParts = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestItemFormat>>(args.ToString());

            string processPath = reqParts.FirstOrDefault(x => x.enumId == 1).value;
            string processArgs = reqParts.FirstOrDefault(x => x.enumId == 2).value;
            bool asAdmin = reqParts.FirstOrDefault(x => x.enumId == 3).value.ToLower() == "true";
            bool isSilent = reqParts.FirstOrDefault(x => x.enumId == 4).value.ToLower() == "true";
            bool useShellExecute = reqParts.FirstOrDefault(x => x.enumId == 5).value.ToLower() == "true";
            string workingDir = reqParts.FirstOrDefault(x => x.enumId == 6).value;
            WinApi.ShowWindowCommands windowState = (WinApi.ShowWindowCommands)Enum.Parse(typeof(WinApi.ShowWindowCommands), reqParts.FirstOrDefault(x => x.enumId == 7).value);
            bool suppressErrors = reqParts.FirstOrDefault(x => x.enumId == 8).value.ToLower() == "true";
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
            return null;
        }
    }
}
