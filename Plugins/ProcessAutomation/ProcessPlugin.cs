﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using MpProcessHelper;
using MpClipboardHelper;
using System.Diagnostics;

namespace ProcessAutomation {
    public class ProcessPlugin : MpIAnalyzerPluginComponent {
        private const int _WAIT_FOR_INPUT_IDLE_MS = 30000;

        private static Dictionary<string, IntPtr> _lastRanNonExeProcessLookup = new Dictionary<string, IntPtr>();

        public async Task<object> AnalyzeAsync(object args) {
            await Task.Delay(1);

            var reqParts = JsonConvert.DeserializeObject<MpAnalyzerPluginRequestFormat>(args.ToString());

            string processPath = reqParts.items.FirstOrDefault(x => x.paramId == 1).value.ToLower();
            var processArgs = reqParts.items.FirstOrDefault(x => x.paramId == 2).value.ToListFromCsv();
            bool asAdmin = reqParts.items.FirstOrDefault(x => x.paramId == 3).value.ToLower() == "true";
            bool isSilent = reqParts.items.FirstOrDefault(x => x.paramId == 4).value.ToLower() == "true";
            bool useShellExecute = reqParts.items.FirstOrDefault(x => x.paramId == 5).value.ToLower() == "true";
            string workingDir = reqParts.items.FirstOrDefault(x => x.paramId == 6).value.ToLower();
            MpProcessHelper.WinApi.ShowWindowCommands windowState = (MpProcessHelper.WinApi.ShowWindowCommands)Enum.Parse(typeof(MpProcessHelper.WinApi.ShowWindowCommands), reqParts.items.FirstOrDefault(x => x.paramId == 7).value);
            bool suppressErrors = reqParts.items.FirstOrDefault(x => x.paramId == 8).value.ToLower() == "true";
            bool preferRunningApp = reqParts.items.FirstOrDefault(x => x.paramId == 9).value.ToLower() == "true";
            bool closeOnComplete = reqParts.items.FirstOrDefault(x => x.paramId == 10).value.ToLower() == "true";
            string username = reqParts.items.FirstOrDefault(x => x.paramId == 11).value;
            string password = reqParts.items.FirstOrDefault(x => x.paramId == 12).value;
            bool createNoWindow = reqParts.items.FirstOrDefault(x => x.paramId == 13).value.ToLower() == "true";
            string domain = reqParts.items.FirstOrDefault(x => x.paramId == 14).value;

            IntPtr lastActiveInstanceHandle = IntPtr.Zero;
            if (preferRunningApp) {
                // only set lastActiveHandle if prefer running process is checked
                if(processPath.ToLower().EndsWith("exe")) {
                    //when process is executable the handle can be located by the process path
                    lastActiveInstanceHandle = MpProcessManager.GetLastActiveInstance(processPath);
                } else if(_lastRanNonExeProcessLookup.ContainsKey(processPath)) {
                    //if the process is batch file this is tracked here internally to reference its handle
                    lastActiveInstanceHandle = _lastRanNonExeProcessLookup[processPath];
                    if(!MpProcessManager.IsHandleRunningProcess(lastActiveInstanceHandle)) {
                        // since only the started non-exe process is tracked internally,
                        // make sure process is still active, when not remove it so its known then start a new one
                        _lastRanNonExeProcessLookup.Remove(processPath);
                        lastActiveInstanceHandle = IntPtr.Zero;
                    }
                }
            }

            var pi = new MpProcessInfo() {
                ProcessPath =  processPath,
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

                pi = MpProcessAutomation.StartProcess(pi);

                if(!processPath.ToLower().EndsWith("exe") && 
                    pi.Handle != IntPtr.Zero) {
                    _lastRanNonExeProcessLookup.Add(processPath, pi.Handle);
                }

                if(!pi.UseShellExecute) {
                    string pasteStr = string.Join(" ", processArgs);
                    var mpdo = MpDataObject.Create(
                        data: pasteStr,
                        textFormat: ".txt",
                        formats: new List<MpClipboardFormatType>() { MpClipboardFormatType.Text });

                    await MpClipboardManager.PasteService.PasteDataObject(mpdo, pi.Handle, true);

                }
            } else {
                string pasteStr = string.Join(Environment.NewLine, processArgs);

                pi.Handle = lastActiveInstanceHandle;
                pi = MpProcessHelper.MpProcessAutomation.SetActiveProcess(pi);

                var p = MpProcessManager.GetProcessByHandle(pi.Handle);

                DataReceivedEventHandler receivedOutput = null;
                DataReceivedEventHandler receivedError = null;
                if(p != null) {
                    receivedOutput = (s, e) => {
                        if(e.Data == pasteStr) {
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
                await Task.Delay(100);

                var mpdo = MpDataObject.Create(
                    data: pasteStr,
                    textFormat: ".txt",
                    formats: new List<MpClipboardFormatType>() { MpClipboardFormatType.Text });

                await MpClipboardManager.PasteService.PasteDataObject(mpdo, pi.Handle, true);

                if(p != null) {
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
            MpPluginResponseFormat response = new MpPluginResponseFormat() {
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