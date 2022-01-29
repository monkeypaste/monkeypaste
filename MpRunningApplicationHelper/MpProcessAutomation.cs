using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static MpProcessHelper.MpProcessManager;
using MonkeyPaste;

namespace MpProcessHelper {
    public static class MpProcessAutomation {
        public static void PasteData(string targetProcessPath) {

        }
        public static void PasteData(IntPtr targetWindowHandle) {

        }

        public static IntPtr StartProcess(
            string args,
            string processPath,
            bool asAdministrator,
            bool isSilent,
            WinApi.ShowWindowCommands windowState = WinApi.ShowWindowCommands.Normal) {
            try {
                IntPtr outHandle = IntPtr.Zero;
                if (isSilent) {
                    windowState = WinApi.ShowWindowCommands.Hide;
                }
                ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo();
                processInfo.FileName = processPath;//Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe"; //Sets the FileName property of myProcessInfo to %SystemRoot%\System32\cmd.exe where %SystemRoot% is a system variable which is expanded using Environment.ExpandEnvironmentVariables
                if (!string.IsNullOrEmpty(args)) {
                    processInfo.Arguments = args;
                }
                processInfo.WindowStyle = isSilent ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal; //Sets the WindowStyle of myProcessInfo which indicates the window state to use when the process is started to Hidden
                processInfo.Verb = asAdministrator ? "runas" : string.Empty; //The process should start with elevated permissions

                if (asAdministrator) {
                    using (var process = Process.Start(processInfo)) {
                        while (!process.WaitForInputIdle(100)) {
                            Thread.Sleep(100);
                            process.Refresh();
                        }
                        outHandle = process.Handle;
                    }
                } else {
                    using (var process = UACHelper.UACHelper.StartLimited(processInfo)) {
                        //using (var process = Process.Start(processInfo)) {
                        while (!process.WaitForInputIdle(100)) {
                            Thread.Sleep(100);
                            process.Refresh();
                        }
                        outHandle = process.Handle;
                    }
                }
                if (outHandle == IntPtr.Zero) {
                    MonkeyPaste.MpConsole.WriteLine("Error starting process: " + processPath);
                    return outHandle;
                }

                WinApi.ShowWindowAsync(outHandle, GetShowWindowValue(windowState));
                return outHandle;
            }
            catch (Exception ex) {
                MonkeyPaste.MpConsole.WriteLine("Start Process error (Admin to Normal mode): " + ex);
                return IntPtr.Zero;
            }
            // TODO pass args to clipboard (w/ ignore in the manager) then activate window and paste
        }

        public static IntPtr SetActiveProcess(
            string processPath,
            bool isAdmin,
            bool isSilent = false,
            string args = "",
            object forceHandle = null,
            WinApi.ShowWindowCommands forceWindowState = WinApi.ShowWindowCommands.Maximized) {
            try {
                if (string.IsNullOrEmpty(processPath)) {
                    return IntPtr.Zero;
                }
                if (processPath[0] == '%') {
                    //only occurs for hardcoded %windir%\cmd.exe
                    processPath = string.Format(
                        @"{0}\System32\cmd.exe",
                        Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName);
                }
                processPath = processPath.Replace(@"\\", @"\").ToLower();
                //MonkeyPaste.MpConsole.WriteLine(processPath);

                //forceHandle is only passed when its a running application
                IntPtr handle = forceHandle == null ? IntPtr.Zero : (IntPtr)forceHandle;
                if (handle != IntPtr.Zero || !CurrentProcessWindowHandleStackDictionary.ContainsKey(processPath)) {
                    //if process is not running anymore or needs to be started (custom pastetoapppath)
                    handle = StartProcess(args, processPath, isAdmin, isSilent, forceWindowState);
                } else {
                    //ensure the process has a handle matching isAdmin, if not it needs to be created
                    var handleList = CurrentProcessWindowHandleStackDictionary[processPath];
                    foreach (var h in handleList) {
                        if (isAdmin == IsProcessAdmin(h)) {
                            handle = h;
                            if (LastWindowStateHandleDictionary.ContainsKey(handle)) {
                                forceWindowState = LastWindowStateHandleDictionary[handle];
                            }
                            break;
                        }
                    }
                    if (handle == IntPtr.Zero) {
                        //no handle found matching admin rights
                        handle = StartProcess(args, processPath, isAdmin, isSilent, forceWindowState);
                    } else {
                        //show running window with last known window state
                        WinApi.ShowWindowAsync(handle, GetShowWindowValue(forceWindowState));
                    }
                }

                return handle;
            }
            catch (Exception) {
                //MonkeyPaste.MpConsole.WriteLine("MpRunningApplicationManager.SetActiveApplication error: " + ex.ToString());
                return IntPtr.Zero;
            }
        }
    }
}
