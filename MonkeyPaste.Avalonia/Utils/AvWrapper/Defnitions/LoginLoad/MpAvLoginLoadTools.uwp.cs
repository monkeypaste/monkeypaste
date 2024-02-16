using Microsoft.Win32.TaskScheduler;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.IO;
using Windows.ApplicationModel;
using SchedulerTask = Microsoft.Win32.TaskScheduler.Task;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvLoginLoadTools {
        string LoginLoadTaskName =>
            $"{Mp.Services.ThisAppInfo.ThisAppProductName}LoginLoad".Replace(" ", string.Empty);

        public bool IsLoadOnLoginEnabled {
            get {
                if (TaskService.Instance.FindTask(LoginLoadTaskName) is not SchedulerTask t) {
                    return false;
                }
                return t.Enabled;
            }
        }
        static string ExecProcessPath =>
            !MpCommonHelpers.IsRunningAsStoreApp() ?
                Mp.Services.PlatformInfo.ExecutingPath :
                Environment.Is64BitOperatingSystem ?
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "cmd.exe") :
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");

        static string ExecProcessArgs {
            get {
                string args = App.LOGIN_LOAD_ARG;

                if (MpCommonHelpers.IsRunningAsStoreApp()) {
                    string package_family_name = Package.Current.Id.FamilyName;
                    return $"/C \"start shell:AppsFolder\\{package_family_name}!App\" {args}";
                }
                return args;
            }
        }
        public void SetLoadOnLogin(bool isLoadOnLogin, bool silent = false) {
            // from https://stackoverflow.com/a/7394955/105028
            if (isLoadOnLogin == IsLoadOnLoginEnabled) {
                // nothing to do (after reset and enabled in welcome, another instance is opened since task still exists)
                return;
            }
            bool success;

            try {
                if (isLoadOnLogin) {
                    // delete any existing task (mainly for debugging)
                    SetLoadOnLogin(false, true);
                    // Create a new task definition and assign properties
                    TaskDefinition td = TaskService.Instance.NewTask();
                    td.RegistrationInfo.Description = $"Loads {Mp.Services.ThisAppInfo.ThisAppProductName} on login";

                    // Create a trigger that will fire the task at this time every other day
                    string userId = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    td.Triggers.Add(new LogonTrigger { UserId = userId, Delay = TimeSpan.FromSeconds(3) });

                    td.Actions.Add(new ExecAction(ExecProcessPath, ExecProcessArgs));

                    // Register the task in the root folder
                    var task = TaskService.Instance.RootFolder.RegisterTaskDefinition(LoginLoadTaskName, td);
                    success = true;
                } else {
                    TaskService.Instance.RootFolder.DeleteTask(LoginLoadTaskName);
                    success = true;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error creating login load task '{LoginLoadTaskName}'.", ex);
                success = false;
            }

            if (success || silent) {
                // don't ntf if no problems
                return;
            }
            string msg = string.Empty;
            if (isLoadOnLogin) {
                msg = success ? UiStrings.LoginLoadEnableSuccess : UiStrings.LoginLoadEnableFailed;
            } else {
                msg = success ? UiStrings.LoginLoadDisableSuccess : UiStrings.LoginLoadDisableFailed;
            }
            Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                title: UiStrings.CommonResultLabel,
                message: msg,
                iconResourceObj: success ? "BananaImage" : "WarningImage").FireAndForgetSafeAsync();
        }


    }
}
