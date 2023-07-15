using MonkeyPaste.Common;
using System;
using System.IO;
using System.Reflection;

namespace MonkeyPaste.Avalonia {
    public class MpAvLoginLoadTools : MpILoadOnLoginTools {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpILoadOnLoginTools Implementation

        public bool IsLoadOnLoginEnabled {
            get {
                return StartupLinkPath.IsFile();
            }
        }

        public void SetLoadOnLogin(bool loadOnLogin) {
#if WINDOWS
            // see https://stackoverflow.com/questions/674628/how-do-i-set-a-program-to-launch-at-startup

            //if (!Mp.Services.ProcessWatcher.IsAdmin(Mp.Services.ProcessWatcher.ThisAppHandle)) {
            //    //MonkeyPaste.MpConsole.WriteLine("Process not running as admin, cannot alter load on login");
            //    return;
            //}

            //Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            //string appName = Assembly.GetExecutingAssembly().GetName().Name;
            //string appPath = Mp.Services.PlatformInfo.ExecutingPath;
            //if (loadOnLogin) {
            //    // NOTE adding login loaded flag arg for startup state logic
            //    appPath += " " + App.LOGIN_LOAD_ARG;
            //    rk.SetValue(appName, appPath);
            //} else {
            //    rk.DeleteValue(appName, false);
            //}

            string shortcutAddress = StartupLinkPath;

            if (loadOnLogin) {
                if (shortcutAddress.IsFile()) {
                    // already set
                    return;
                }
                try {
                    IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                    System.Reflection.Assembly curAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                    IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutAddress);
                    shortcut.Description = "My App Name";
                    shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    shortcut.TargetPath = curAssembly.Location;
                    shortcut.IconLocation = AppDomain.CurrentDomain.BaseDirectory + @"MyIconName.ico";
                    shortcut.Save();
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error create startup .lnk to path '{shortcutAddress}'", ex);
                    Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                        title: "Error",
                        message: "Could not enabled load on login. Try running this as administrator to enable",
                        iconResourceObj: "WarningImage",
                        owner: MpAvWindowManager.ActiveWindow);
                }
                return;
            }
            if (!shortcutAddress.IsFile()) {
                // doesn't exist 
                return;
            }
            MpFileIo.DeleteFile(shortcutAddress);
            return;
#else
            // TODO add other os'
            loadOnLogin = false;
            MpPrefViewModel.Instance.LoadOnLogin = loadOnLogin;

            MpConsole.WriteLine($"Load At Login: {(loadOnLogin ? "ON" : "OFF")}");
#endif
        }

        #endregion

        #endregion

        #region Properties
        string StartupLinkPath {
            get {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                    $"{Mp.Services.ThisAppInfo.ThisAppProductName}.lnk");
            }
        }
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        #endregion

        #region Commands
        #endregion


    }
}
