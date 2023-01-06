using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace ProcessAutomation {
    public class ProcessPlugin : MpIAnalyzeAsyncComponent {
        private const int _WAIT_FOR_INPUT_IDLE_MS = 30000;

        private static Dictionary<string, IntPtr> _lastRanNonExeProcessLookup = new Dictionary<string, IntPtr>();

        public static IntPtr ThisAppHandle;
        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            

            string processPath = req.GetRequestParamStringValue(1);
            var processArgs = req.GetRequestParamStringValue(2).ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value);
            bool asAdmin = req.GetRequestParamBoolValue(3);
            bool isSilent = req.GetRequestParamBoolValue(4);
            bool useShellExecute = req.GetRequestParamBoolValue(5);
            string workingDir = req.GetRequestParamStringValue(6);
            //WinApi.ShowWindowCommands windowState = (WinApi.ShowWindowCommands)Enum.Parse(typeof(WinApi.ShowWindowCommands), req.items.FirstOrDefault(x => x.paramId == 7).value);
            ProcessWindowStyle windowState = req.GetRequestParamStringValue(7).ToEnum<ProcessWindowStyle>();
            bool suppressErrors = req.GetRequestParamBoolValue(8);
            bool preferRunningApp = req.GetRequestParamBoolValue(9);
            bool closeOnComplete = req.GetRequestParamBoolValue(10);
            string username = req.GetRequestParamStringValue(11);
            string password = req.GetRequestParamStringValue(12);
            bool createNoWindow = req.GetRequestParamBoolValue(13);
            string domain = req.GetRequestParamStringValue(14);

            IntPtr lastActiveInstanceHandle = IntPtr.Zero;
            if (preferRunningApp) {
                // only set lastActiveHandle if prefer running process is checked
                if (processPath.ToLower().EndsWith("exe")) {
                    //when process is executable the handle can be located by the process path
                    lastActiveInstanceHandle = MpCommonTools.Services.ProcessWatcher.GetLastActiveInstance(processPath);
                } else if (_lastRanNonExeProcessLookup.ContainsKey(processPath)) {
                    //if the process is batch file this is tracked here internally to reference its handle
                    lastActiveInstanceHandle = _lastRanNonExeProcessLookup[processPath];
                    if (!MpCommonTools.Services.ProcessWatcher.IsHandleRunningProcess(lastActiveInstanceHandle)) {
                        // since only the started non-exe process is tracked internally,
                        // make sure process is still active, when not remove it so its known then start a new one
                        _lastRanNonExeProcessLookup.Remove(processPath);
                        lastActiveInstanceHandle = IntPtr.Zero;
                    }
                }
            }

            var pi = new MpPortableStartProcessInfo() {
                ProcessPath = processPath,
                ArgumentList = processArgs,
                IsAdmin = asAdmin,
                IsSilent = isSilent,
                UseShellExecute = useShellExecute,
                WorkingDirectory = workingDir,
                ShowError = !suppressErrors,
                WindowState = windowState,
                CloseOnComplete = closeOnComplete,
                CreateNoWindow = createNoWindow,
                Domain = domain,
                UserName = username,
                Password = password
            };

            string stdOut = string.Empty;
            string stdErr = string.Empty;
            if (lastActiveInstanceHandle == IntPtr.Zero) {

                pi = ProcessFactory.StartProcess(pi);

                if (!processPath.ToLower().EndsWith("exe") &&
                    pi.Handle != IntPtr.Zero) {
                    _lastRanNonExeProcessLookup.Add(processPath, pi.Handle);
                }

                if (!pi.UseShellExecute) {
                    string pasteStr = string.Join(" ", processArgs);
                    var mpdo = new MpPortableDataObject(MpPortableDataFormats.Text, pasteStr);
                    //var mpdo = MpPortableDataObject.Create(
                    //    data: pasteStr,
                    //    textFormat: ".txt",
                    //    formats: new List<MpClipboardFormatType>() { MpClipboardFormatType.Text });

                    await MpCommonTools.Services.ExternalPasteHandler.PasteDataObject(mpdo, pi);

                }
            } else {
                string pasteStr = string.Join(Environment.NewLine, processArgs);

                pi.Handle = lastActiveInstanceHandle;
                pi.Handle = MpCommonTools.Services.ProcessWatcher.SetActiveProcess(pi.Handle,pi.WindowState);

                var p = MpCommonTools.Services.ProcessWatcher.GetProcess(pi.Handle);


                DataReceivedEventHandler? receivedOutput = null;
                DataReceivedEventHandler? receivedError = null;
                if (p != null) {
                    receivedOutput = (s, e) => {
                        if (e.Data == pasteStr) {
                            //ignore input
                            return;
                        }
                        stdOut += e.Data;
                    };
                    receivedError = (s, e) => {
                        stdErr += e.Data;
                    };

                    p.OutputDataReceived += receivedOutput;
                    p.ErrorDataReceived += receivedError;
                }

                // lil' wait for window switch...
                //await Task.Delay(100);

                //var mpdo = MpPortableDataObject.Create(
                //    data: pasteStr,
                //    textFormat: ".txt",
                //    formats: new List<MpClipboardFormatType>() { MpClipboardFormatType.Text });

                //await MpClipboardManager.PasteService.PasteDataObject(mpdo, pi.Handle, true);

                if (p != null) {
                    p.WaitForInputIdle(_WAIT_FOR_INPUT_IDLE_MS);
                    p.OutputDataReceived -= receivedOutput;
                    p.ErrorDataReceived -= receivedError;
                    p.Dispose();
                } else {
                    Debugger.Break();
                }
                //await Task.Run(async () => {
                //    //when pasting to running process accumulate all output/error 
                //    DateTime startTime = DateTime.Now;
                //    DateTime? receiveDateTime = null;
                //    string curStdOut = stdOut;
                //    string curStdErr = stdErr;

                //    while(true) {
                //        if(curStdOut != stdOut || curStdErr != stdErr) {
                //            curStdOut = stdOut;
                //            curStdErr = stdErr;
                //            receiveDateTime = DateTime.Now;
                //        }
                //    }
                //});
            }
            MpAnalyzerPluginResponseFormat response = new MpAnalyzerPluginResponseFormat() {
                annotations = new List<MpPluginResponseAnnotationFormat>() {
                                 new MpPluginResponseAnnotationFormat() {
                                     name = "Output",
                                     label = new MpJsonPathProperty(stdOut)
                                 },
                                 new MpPluginResponseAnnotationFormat() {
                                     name = "Error",
                                     label = new MpJsonPathProperty(stdErr)
                                 }
                             }
            };

            return response;
        }

        private void P_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e) {
            throw new NotImplementedException();
        }

        private void P_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e) {
            throw new NotImplementedException();
        }
    }
}
