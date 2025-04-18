﻿using Avalonia.Controls;
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

        #region State
        #endregion

        #endregion

        #region Constructors
        public MpAvUriNavigator() : base(null) { }
        #endregion

        #region Public Methods


        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void NavigateToUri(Uri uri, bool ignoreScheme = false) {
            if (uri == null) {
                return;
            }
            if (MpAvWindowManager.ActiveWindow != null) {
                // disable topmost so opened window comes to top
                MpAvWindowManager.ActiveWindow.Topmost = false;
            }
            if (!ignoreScheme && uri.Scheme == Uri.UriSchemeFile) {
                NavigateToPath(uri.LocalPath);
                return;
            }

            using (var myProcess = new Process()) {
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = uri.AbsoluteUri;
#pragma warning disable CA1416 // Validate platform compatibility
                myProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
            }
        }

        private void NavigateToPath(string path, bool useFileBrowser = true) {

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
                if (useFileBrowser) {
                    string args = string.Empty;
                    if (path.IsFile()) {
                        args = "/select, \"" + path + "\"";
                    } else {
                        // if its not a dir it'll just open default...
                        args = "/open, \"" + path + "\"";
                    }
                    Process.Start("explorer.exe", args);
                    return;
                }
                Process.Start(path);
            } else {
                if (path.IsFile() && useFileBrowser) {
                    path = path.FindParentDirectory();
                }
                using (var myProcess = new Process()) {
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = path;
#pragma warning disable CA1416 // Validate platform compatibility
                    myProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
                }
            }
        }

        private Uri ParseUriFromArgStr(string argStr) {
            if (!Uri.IsWellFormedUriString(argStr, UriKind.Absolute) &&
                        argStr.IsFileOrDirectory()) {
                argStr = argStr.ToFileSystemUriFromPath();
            }
            if (Uri.IsWellFormedUriString(argStr, UriKind.Absolute)) {
                return new Uri(argStr, UriKind.Absolute);
            }
            return null;
        }
        #endregion

        #region Commands

        public ICommand OpenFileUriCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is string argStr) {
                    args = new object[] { argStr, false, true };
                }
                await NavigateToUriCommand.ExecuteAsync(args);
            });
        public MpIAsyncCommand<object> NavigateToUriCtrlEnforcedCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (!NavigateToUriCommand.CanExecute(args) ||
                    !MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown) {
                    return;
                }
                await NavigateToUriCommand.ExecuteAsync(args);
            });

        public MpIAsyncCommand<object> NavigateToUriAltEnforcedCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (!NavigateToUriCommand.CanExecute(args) ||
                    !MpAvShortcutCollectionViewModel.Instance.GlobalIsAltDown) {
                    return;
                }
                await NavigateToUriCommand.ExecuteAsync(args);
            });
        public MpIAsyncCommand<object> NavigateToUriCommand => new MpAsyncCommand<object>(
            async (args) => {
                bool needsConfirm = false;
                bool openFile = false;
                Uri uri = null;
                if (args is Uri) {
                    uri = args as Uri;
                } else if (args is string argStr) {
                    uri = ParseUriFromArgStr(argStr);
                } else if (args is object[] argParts &&
                            argParts.Length >= 2 &&
                            argParts[0] is string uriStr &&
                            argParts[1] is bool confirmArg) {
                    uri = ParseUriFromArgStr(uriStr);
                    needsConfirm = confirmArg;
                    if (argParts.Length == 3 &&
                        argParts[2] is bool open_it && open_it) {
                        openFile = true;
                    }
                }
                if (needsConfirm) {
                    var result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                        title: UiStrings.CommonConfirmLabel,
                        message: string.Format(UiStrings.UrlNavConfirmText, uri),
                        iconResourceObj: "QuestionMarkImage");
                    if (!result) {
                        // cancel
                        return;
                    }
                }
                if (MpAvWindowManager.MainWindow != null &&
                    MpAvWindowManager.MainWindow.IsActive) {
                    MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
                }
                NavigateToUri(uri, openFile);
                await Task.Delay(2_000);
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
                if (sr is MpPreset pp) {
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
