using Microsoft.Win32.TaskScheduler;
using MonkeyPaste.Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvAppRestarter {
        static string RestartTaskPath =>
            "MonkeyPasteRestart";


        public static void ShutdownWithRestartTask() {
            RemoveRestartTask();

            int start_delay_ms = 1_000;
            int dur_ms = 30_000;
            var st = DateTime.Now + TimeSpan.FromMilliseconds(start_delay_ms);
            var et = st + TimeSpan.FromMilliseconds(dur_ms);
            try {
                var tt = new TimeTrigger();
                tt.StartBoundary = st;
                tt.EndBoundary = et;

                var ea = new ExecAction(
                    path: Mp.Services.PlatformInfo.ExecutingPath,
                    arguments: App.RESTART_ARG);

                TaskDefinition td = TaskService.Instance.NewTask();
                td.Triggers.Add(tt);
                td.Actions.Add(ea);

                TaskService.Instance.RootFolder.RegisterTaskDefinition(RestartTaskPath, td);
                MpConsole.WriteLine("Restart scheduled");
                MpConsole.WriteLine("Between:");
                MpConsole.WriteLine(st.ToString());
                MpConsole.WriteLine("And:");
                MpConsole.WriteLine(et.ToString());
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error creating restart task from task path: '{RestartTaskPath}'", ex);
                Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                    title: UiStrings.CommonErrorLabel,
                    message: UiStrings.RestarterErrorNtfText,
                    iconResourceObj: "ErrorImage").FireAndForgetSafeAsync();
                return;
            }

            Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.Restart, $"Broken if not restarted by: '{et}'");
        }

        public static void RemoveRestartTask() {
            try {
                TaskService.Instance.RootFolder.DeleteTask(RestartTaskPath);
                MpConsole.WriteLine("Restart task successfully removed");
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error removing restart task from task path: '{RestartTaskPath}'", ex);
            }
        }
    }
}
