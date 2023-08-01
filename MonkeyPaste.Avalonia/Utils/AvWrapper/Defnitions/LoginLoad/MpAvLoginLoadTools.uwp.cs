using Microsoft.Win32.TaskScheduler;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvLoginLoadTools {
        string LoginLoadTaskPath =>
            $"{Mp.Services.ThisAppInfo.ThisAppProductName}LoginLoad".Replace(" ", string.Empty);

        public bool IsLoadOnLoginEnabled {
            get {
                if (TaskService.Instance.GetTask(LoginLoadTaskPath) is Task t) {
                    return t.Enabled;
                }
                return false;
            }
        }
        public void SetLoadOnLogin(bool isLoadOnLogin) {
            // from https://stackoverflow.com/a/7394955/105028
            bool success;

            try {
                if (isLoadOnLogin) {
                    // Create a new task definition and assign properties
                    TaskDefinition td = TaskService.Instance.NewTask();
                    td.RegistrationInfo.Description = $"Loads {Mp.Services.ThisAppInfo.ThisAppProductName} on login";

                    // Create a trigger that will fire the task at this time every other day
                    string userId = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    td.Triggers.Add(new LogonTrigger { UserId = userId });

                    // Create an action that will launch Notepad whenever the trigger fires
                    td.Actions.Add(new ExecAction(Mp.Services.PlatformInfo.ExecutingPath, App.LOGIN_LOAD_ARG));

                    // Register the task in the root folder
                    var task = TaskService.Instance.RootFolder.RegisterTaskDefinition(LoginLoadTaskPath, td);
                    success = td.Validate();
                } else {
                    TaskService.Instance.RootFolder.DeleteTask(LoginLoadTaskPath);
                    success = true;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error creating login load task '{LoginLoadTaskPath}'.", ex);
                success = false;
            }

            Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                title: $"Result",
                message: $"Login load {(isLoadOnLogin ? "enable" : "disable")} {(success ? "success" : "failed")}",
                iconResourceObj: success ? "BananaImage" : "WarningImage").FireAndForgetSafeAsync();
        }
    }
}
