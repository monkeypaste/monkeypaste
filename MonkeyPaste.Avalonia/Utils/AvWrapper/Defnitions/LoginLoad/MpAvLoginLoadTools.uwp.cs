using MonkeyPaste.Common;
using System;
using System.Threading.Tasks;

#if WAP
using Windows.ApplicationModel;
using Windows.UI.Popups; 
#else
using MonkeyPaste.Common.Plugin;
using Microsoft.Win32.TaskScheduler;
using SchedulerTask = Microsoft.Win32.TaskScheduler.Task;
using Task = System.Threading.Tasks.Task;
#endif

namespace MonkeyPaste.Avalonia {
    public partial class MpAvLoginLoadTools {
#if WAP
        public bool IsLoadOnLoginEnabled { get; private set; }

        string TASK_ID = "5c8dfa29-456c-425a-8fd7-9d5540663691";

        private bool IsStartupEnabled(StartupTaskState sts) {
            return sts == StartupTaskState.EnabledByPolicy || sts == StartupTaskState.Enabled;
        }
        public MpAvLoginLoadTools() {
            InitAsync().FireAndForgetSafeAsync();
        }

        private async Task InitAsync() {
            StartupTask startupTask = await StartupTask.GetAsync(TASK_ID);
            IsLoadOnLoginEnabled = IsStartupEnabled(startupTask.State);
        }
        public async Task SetLoadOnLoginAsync(bool isLoadOnLogin, bool silent = false) {
            // from https://learn.microsoft.com/en-us/uwp/api/windows.applicationmodel.startuptask?view=winrt-22621


            StartupTask startupTask = await StartupTask.GetAsync(TASK_ID);
            bool is_enabled = IsStartupEnabled(startupTask.State);
            if (is_enabled == isLoadOnLogin) {
                return;
            }
            StartupTaskState newState;
            if (isLoadOnLogin) {
                newState = await startupTask.RequestEnableAsync();
            } else {
                startupTask.Disable();
                newState = StartupTaskState.Disabled;
            }

            switch (newState) {
                case StartupTaskState.DisabledByUser:
                    // Task is disabled and user must enable it manually.
                    MessageDialog dialog = new MessageDialog(
                        "You have disabled this app's ability to run " +
                        "as soon as you sign in, but if you change your mind, " +
                        "you can enable this in the Startup tab in Task Manager.",
                        "Startup Blocked".ToWindowTitleText());
                    await dialog.ShowAsync();
                    break;
                case StartupTaskState.DisabledByPolicy:
                    //Debug.WriteLine("Startup disabled by group policy, or not supported on this device");
                    MessageDialog dialog2 = new MessageDialog(
                        "Startup disabled by group policy, or not supported on this device",
                        "Startup Blocked".ToWindowTitleText());
                    await dialog2.ShowAsync();
                    break;
            }
        }
#else
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

        public async Task SetLoadOnLoginAsync(bool isLoadOnLogin, bool silent = false) {
            // NOTE creating tasks requires app to be run as admin or current user has admin rights

            // from https://stackoverflow.com/a/7394955/105028
            if (isLoadOnLogin == IsLoadOnLoginEnabled) {
                // nothing to do (after reset and enabled in welcome, another instance is opened since task still exists)
                return;
            }
            bool success;

            try {
                if (isLoadOnLogin) {
                    // delete any existing task (mainly for debugging)
                    await SetLoadOnLoginAsync(false, true);
                    // Create a new task definition and assign properties
                    TaskDefinition td = TaskService.Instance.NewTask();
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.RegistrationInfo.Description = $"Loads {Mp.Services.ThisAppInfo.ThisAppProductName} on login";

                    // Create a trigger that will fire the task at this time every other day
                    string userId = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    td.Triggers.Add(new LogonTrigger { UserId = userId, Delay = TimeSpan.FromSeconds(3) });

                    //td.Actions.Add(new ExecAction(ExecProcessPath, ExecProcessArgs));
                    td.Actions.Add(new ExecAction(Mp.Services.PlatformInfo.ExecutingPath, App.LOGIN_LOAD_ARG));

                    // Register the task in the root folder
                    var task = TaskService.Instance.RootFolder.RegisterTaskDefinition(LoginLoadTaskName, td);
                    //var task = TaskService.Instance.RootFolder.RegisterTaskDefinition(LoginLoadTaskName, td, TaskCreation.CreateOrUpdate, "SYSTEM", null, TaskLogonType.ServiceAccount);
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
            string msg;
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
    
#endif
    }
}
