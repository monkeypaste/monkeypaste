using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvUriNavigator : MpAvViewModelBase {
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

            using (var myProcess = new Process()) {
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = uri.AbsoluteUri;
                myProcess.Start();
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

        public ICommand NavigateToUriCommand => new MpAsyncCommand<object>(
            async (args) => {
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
                if (MpAvWindowManager.MainWindow.IsActive) {
                    MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
                }
                NavigateToUri(uri);
                await Task.Delay(500);
                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
            });

        public ICommand NavigateToSourceRefCommand => new MpAsyncCommand<object>(
            async (args) => {
                MpISourceRef sr = args as MpISourceRef;
                if (sr == null) {
                    return;
                }
                if (Mp.Services.SourceRefTools.IsExternalSource(sr)) {
                    NavigateToUriCommand.Execute(Mp.Services.SourceRefTools.ConvertToAbsolutePath(sr));
                    return;
                }
                if (sr is MpCopyItem ci) {
                    MpAvClipTileViewModel nav_to_ctvm =
                        MpAvClipTrayViewModel.Instance
                        .AllActiveItems
                        .FirstOrDefault(x => x.CopyItemId == ci.Id);
                    if (nav_to_ctvm == null) {
                        nav_to_ctvm = await MpAvClipTrayViewModel.Instance.CreateClipTileViewModelAsync(ci);
                    }

                    if (nav_to_ctvm == null) {
                        return;
                    }
                    if (nav_to_ctvm.IsWindowOpen &&
                    MpAvWindowManager.AllWindows.FirstOrDefault(x => x.DataContext == nav_to_ctvm) is MpAvWindow w) {
                        w.Activate();
                    } else {

                        MpAvClipTrayViewModel.Instance.PinTileCommand.Execute(new object[] { nav_to_ctvm, MpPinType.Window });
                    }
                    return;
                }
                if (sr is MpPluginPreset pp) {
                    var nav_to_aipvm =
                        MpAvAnalyticItemCollectionViewModel.Instance
                        .AllPresets
                        .FirstOrDefault(x => x.AnalyticItemPresetId == pp.Id);
                    if (nav_to_aipvm == null) {
                        return;
                    }
                    nav_to_aipvm.Parent.SelectPresetCommand.Execute(nav_to_aipvm);
                    return;
                }
            });
        #endregion

    }
}
