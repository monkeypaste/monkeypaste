﻿using Microsoft.Win32;
using MonkeyPaste;
using System;
using System.IO;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {
    public class MpFileSystemTriggerViewModel : MpAvTriggerActionViewModelBase, MpIFileSystemEventHandler {
        #region Properties

        #region State

        public bool IsValidFileSystemPath {
            get {
                if(string.IsNullOrWhiteSpace(FileSystemPath)) {
                    return false;
                }
                return FileSystemPath.IsFileOrDirectory();
            }
        }

        #endregion

        #region Model

        public string FileSystemPath {
            get {
                if (Arg1 == null) {
                    return null;
                }
                return Arg1;
            }
            set {
                if (FileSystemPath != value) {
                    Arg1 = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(FileSystemPath));
                }
            }
        }

        public bool IncludeSubdirectories {
            get {
                if (Arg2 == null) {
                    return false;
                }
                return Arg2 == "1";
            }
            set {
                if (IncludeSubdirectories != value) {
                    Arg2 = value ? "1":"0";
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IncludeSubdirectories));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpFileSystemTriggerViewModel(MpActionCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpFileSystemTriggerViewModel_PropertyChanged;
        }

        #endregion

        #region Public Overrides
        #endregion

        #region Protected Methods

        protected override async Task Enable() {
            await base.Enable();
            MpAvFileSystemWatcher.Instance.RegisterActionComponent(this);
        }

        protected override async Task Disable() {
            await base.Disable();
            MpAvFileSystemWatcher.Instance.UnregisterActionComponent(this);
        }

        protected override async Task<bool> Validate() {
            await base.Validate();
            if (!IsValid) {
                return IsValid;
            }

            if (string.IsNullOrEmpty(FileSystemPath)) {
                ValidationText = $"No file system path specified for trigger action '{FullName}'";
                await ShowValidationNotification();
            } else if(!IsValidFileSystemPath) {
                ValidationText = $"File system path '{FileSystemPath}' not found for trigger action '{FullName}'";
                await ShowValidationNotification();
            } else {
                ValidationText = string.Empty;
            }
            return IsValid;
        }
        #endregion

        #region MpIFileSystemWatcher Implementation

        public void OnFileSystemItemChanged(object sender, FileSystemEventArgs e) {
            if(!MpBootstrapperViewModelBase.IsLoaded) {
                // NOTE this check maybe unnecessary. Rtf test was being generated onto desktop during startup and interfering w/ this trigger's lifecycle
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                MpCopyItem ci = null;
                switch (e.ChangeType) {
                    case WatcherChangeTypes.Changed:
                    case WatcherChangeTypes.Created:
                        ci = await MpCopyItem.Create(
                            sourceId: MpPrefViewModel.Instance.ThisOsFileManagerSource.Id,
                            itemType: MpCopyItemType.FileList,
                            data: e.FullPath,
                            suppressWrite: true);
                        break;
                    case WatcherChangeTypes.Renamed:
                        RenamedEventArgs re = e as RenamedEventArgs;
                        ci = await MpDataModelProvider.GetCopyItemByDataAsync(re.OldFullPath);
                        if(ci == null) {
                            ci = await MpCopyItem.Create(
                                sourceId: MpPrefViewModel.Instance.ThisOsFileManagerSource.Id,
                                itemType: MpCopyItemType.FileList,
                                data: e.FullPath,
                                suppressWrite: true);
                        }
                        ci.ItemData = re.FullPath;
                        await ci.WriteToDatabaseAsync();
                        break;
                    case WatcherChangeTypes.Deleted:
                        ci = await MpDataModelProvider.GetCopyItemByDataAsync(e.FullPath);
                        if(ci == null) {
                            return;
                        }
                        bool isVisible = MpAvClipTrayViewModel.Instance.GetClipTileViewModelById((int)ci.Id) != null;
                        await ci.DeleteFromDatabaseAsync();
                        if(isVisible) {
                            MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                        }
                        break;
                }

                if (ci != null) {
                    var ao = new MpFileSystemTriggerOutput() {
                        CopyItem = ci,
                        FileSystemChangeType = e.ChangeType
                    };
                    await base.PerformAction(ao);
                }
            });
        }

        #endregion

        #region Private Methods

        private void MpFileSystemTriggerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IncludeSubdirectories):
                case nameof(FileSystemPath):
                    if(IsBusy) {
                        return;
                    }
                    if(IsEnabled.HasValue && IsEnabled.Value) {
                        ReEnable().FireAndForgetSafeAsync(this);
                    }
                    break;
            }
        }

        #endregion

        #region Commands

        public MpIAsyncCommand SelectFileSystemPathCommand => new MpAsyncCommand(
            async () => {
                string initDir = FileSystemPath;
                if(string.IsNullOrEmpty(initDir)) {
                    initDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                } else if(File.Exists(initDir)) {
                    initDir = Path.GetDirectoryName(initDir);
                }
                //MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
                //var dlg = new MpFolderPicker() {
                //    InputPath = initDir,
                //    Title = "Select folder"
                //};
                //bool result = dlg.ShowDialog() == true;

                //MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
                //if (result) {
                //    FileSystemPath = dlg.ResultPath;
                //    await ReEnable();
                //}
            });

        #endregion
    }
}