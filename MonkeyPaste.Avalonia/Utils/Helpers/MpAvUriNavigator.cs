using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvUriNavigator : MpViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        private static MpAvUriNavigator _instance;
        public static MpAvUriNavigator Instance => _instance ?? (_instance = new MpAvUriNavigator());
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public MpAvUriNavigator() : base(null) { }
        #endregion

        #region Public Methods


        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void NavigateToUri(Uri uri) {
            if (uri == null) {
                return;
            }
            if (uri.Scheme == Uri.UriSchemeFile) {
                NavigateToPath(uri.LocalPath);
                return;
            }


            if (OperatingSystem.IsWindows()) {
                Process.Start("explorer.exe", uri.AbsoluteUri);
            } else {
                using (var myProcess = new Process()) {
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = uri.AbsoluteUri;
                    myProcess.Start();
                }
            }
        }

        private void NavigateToPath(string path, bool useFileBrowser = true) {
            if (path.IsFile() && useFileBrowser) {
                path = path.FindParentDirectory();
            }
            path = path.Contains(" ") ? $"\"{path}\"" : path;

            if (OperatingSystem.IsWindows()) {
                //using (var myProcess = new Process()) {
                //    myProcess.StartInfo.UseShellExecute = false;
                //    myProcess.StartInfo.FileName = "explorer.exe";
                //    if (path.IsFile()) {
                //        myProcess.StartInfo.ArgumentList.Add(@"/select");
                //    } else if (path.IsDirectory()) {
                //        myProcess.StartInfo.ArgumentList.Add(@"/open");
                //    }
                //    myProcess.StartInfo.ArgumentList.Add(path);
                //    myProcess.Start();
                //}
                Process.Start("explorer.exe", path);
            } else {
                using (var myProcess = new Process()) {
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = path;
                    myProcess.Start();
                }
            }
        }
        #endregion

        #region Commands

        public ICommand NavigateToUriCommand => new MpCommand<object>(
            (args) => {
                Uri uri = null;
                if (args is Uri) {
                    uri = args as Uri;
                } else if (args is string argStr) {
                    if (!Uri.IsWellFormedUriString(argStr, UriKind.Absolute) &&
                        argStr.IsFileOrDirectory()) {
                        argStr = argStr.ToFileSystemUriFromPath();
                    }
                    if (Uri.IsWellFormedUriString(argStr, UriKind.Absolute)) {
                        uri = new Uri(argStr, UriKind.Absolute);
                    }
                }
                NavigateToUri(uri);
            });
        #endregion

    }
}
