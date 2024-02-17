using Microsoft.Win32.TaskScheduler;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.IO;
using Windows.ApplicationModel;
using SchedulerTask = Microsoft.Win32.TaskScheduler.Task;

namespace MonkeyPaste.Avalonia {
    public static class MpAvAppRestarter {
        static string RestartTaskPath =>
            "MonkeyPasteRestart";

        static string ExecProcessPath =>
            !MpPlatformHelpers.IsRunningAsStoreApp() ?
                Mp.Services.PlatformInfo.ExecutingPath :
                Environment.Is64BitOperatingSystem ?
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "cmd.exe") :
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");

        static string ExecProcessArgs {
            get {
                string args = App.RESTART_ARG;

                if (MpPlatformHelpers.IsRunningAsStoreApp()) {
                    string package_family_name = Package.Current.Id.FamilyName;
                    return $"/C \"start shell:AppsFolder\\{package_family_name}!App\" {args}";
                }
                return args;
            }
        }

        public static void ShutdownWithRestartTask(string detail) {
            RemoveRestartTask();

            int start_delay_ms = 1_000;
            int dur_ms = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            var st = DateTime.Now + TimeSpan.FromMilliseconds(start_delay_ms);
            var et = st + TimeSpan.FromMilliseconds(dur_ms);
            try {
                var tt = new TimeTrigger();
                tt.StartBoundary = st;
                tt.EndBoundary = et;

                var ea = new ExecAction(
                    path: ExecProcessPath,
                    arguments: ExecProcessArgs);

                TaskDefinition td = TaskService.Instance.NewTask();
                td.Settings.DisallowStartIfOnBatteries = false;
                td.Settings.StopIfGoingOnBatteries = false;
                td.Triggers.Add(tt);
                td.Actions.Add(ea);


                TaskService.Instance.RootFolder.RegisterTaskDefinition(RestartTaskPath, td);
                MpConsole.WriteLine($"Restart scheduled. Between {st} and {et}");
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error creating restart task from task path: '{RestartTaskPath}'", ex);
                Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                    title: UiStrings.CommonErrorLabel,
                    message: UiStrings.RestarterErrorNtfText,
                    iconResourceObj: "ErrorImage").FireAndForgetSafeAsync();
                return;
            }

            Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.Restart, detail);
        }

        public static void RemoveRestartTask() {
            if (TaskService.Instance.FindTask(RestartTaskPath) is not SchedulerTask rt) {
                return;
            }
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
