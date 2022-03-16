using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using MpProcessHelper;

namespace ProcessAutomation {
    public class ProcessPlugin : MpIAnalyzerPluginComponent {
        private static Dictionary<string, IntPtr> _lastRanNonExeProcessLookup = new Dictionary<string, IntPtr>();

        public async Task<object> AnalyzeAsync(object args) {
            await Task.Delay(1);

            var reqParts = JsonConvert.DeserializeObject<MpAnalyzerPluginRequestFormat>(args.ToString());

            string processPath = reqParts.items.FirstOrDefault(x => x.paramId == 1).value;
            var processArgs = reqParts.items.FirstOrDefault(x => x.paramId == 2).value.ToListFromCsv();
            bool asAdmin = reqParts.items.FirstOrDefault(x => x.paramId == 3).value.ToLower() == "true";
            bool isSilent = reqParts.items.FirstOrDefault(x => x.paramId == 4).value.ToLower() == "true";
            bool useShellExecute = reqParts.items.FirstOrDefault(x => x.paramId == 5).value.ToLower() == "true";
            string workingDir = reqParts.items.FirstOrDefault(x => x.paramId == 6).value;
            WinApi.ShowWindowCommands windowState = (WinApi.ShowWindowCommands)Enum.Parse(typeof(WinApi.ShowWindowCommands), reqParts.items.FirstOrDefault(x => x.paramId == 7).value);
            bool suppressErrors = reqParts.items.FirstOrDefault(x => x.paramId == 8).value.ToLower() == "true";
            bool preferRunningApp = reqParts.items.FirstOrDefault(x => x.paramId == 9).value.ToLower() == "true";
            bool closeOnComplete = reqParts.items.FirstOrDefault(x => x.paramId == 10).value.ToLower() == "true";

            IntPtr lastActiveInstanceHandle = IntPtr.Zero;
            if (preferRunningApp) {
                if(processPath.ToLower().EndsWith("exe")) {
                    lastActiveInstanceHandle = MpProcessManager.GetLastActiveInstance(processPath);
                } else if(_lastRanNonExeProcessLookup.ContainsKey(processPath)) {
                    lastActiveInstanceHandle = _lastRanNonExeProcessLookup[processPath];
                    if(!MpProcessManager.IsHandleRunningProcess(lastActiveInstanceHandle)) {
                        _lastRanNonExeProcessLookup.Remove(processPath);
                        lastActiveInstanceHandle = IntPtr.Zero;
                    }
                }
            }
            string stdOut = string.Empty;
            string stdErr = string.Empty;

            if(lastActiveInstanceHandle == IntPtr.Zero) {
                IntPtr processHandle = ProcessHelpers.StartProcess(
                                            processPath: processPath,
                                            argList: processArgs,
                                            asAdministrator: asAdmin,
                                            isSilent: isSilent,
                                            useShellExecute: useShellExecute,
                                            workingDirectory: workingDir,
                                            showError: !suppressErrors,
                                            windowState: windowState,
                                            mainWindowHandle: MpProcessManager.GetThisApplicationMainWindowHandle(),
                                            closeOnComplete: closeOnComplete,
                                            out stdOut,
                                            out stdErr);
                if(!processPath.ToLower().EndsWith("exe") && 
                    processHandle != IntPtr.Zero) {
                    _lastRanNonExeProcessLookup.Add(processPath, processHandle);
                }
            } else {
                MpProcessHelper.MpProcessAutomation.SetActiveProcess()
            }
            
            var response = new MpPluginResponseFormat() {
                annotations = new List<MpPluginResponseAnnotationFormat>() {
                                new MpPluginResponseAnnotationFormat() {
                                    label = new MpJsonPathProperty(stdOut)
                                }
                            }
            };
            return response;
        }
    }
}
