using Microsoft.Win32;
using MonkeyPaste;
using System;
using System.IO;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using Avalonia.Threading;
using PropertyChanged;
using Avalonia.Controls;
using System.Windows.Input;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvFolderWatcherTriggerViewModel : MpAvTriggerActionViewModelBase, MpIFileSystemEventHandler {
        #region Properties

        #region State

        #endregion

        #region Model

        public string FolderPath {
            get {
                if (Arg4 == null) {
                    return null;
                }
                return Arg4;
            }
            set {
                if (FolderPath != value) {
                    Arg4 = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(FolderPath));
                }
            }
        }

        public bool IncludeSubdirectories {
            get {
                if (Arg5 == null) {
                    return false;
                }
                return Arg5 == "1";
            }
            set {
                if (IncludeSubdirectories != value) {
                    Arg5 = value ? "1":"0";
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IncludeSubdirectories));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvFolderWatcherTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpFileSystemTriggerViewModel_PropertyChanged;
        }

        #endregion

        #region Public Overrides
        #endregion

        #region Protected Methods

        protected override void EnableTrigger() {            
            MpAvFileSystemWatcher.Instance.RegisterActionComponent(this);
        }

        protected override void DisableTrigger() {            
            MpAvFileSystemWatcher.Instance.UnregisterActionComponent(this);
        }

        protected override async Task ValidateActionAsync() {
            await Task.Delay(1);
            if (string.IsNullOrEmpty(FolderPath)) {
                ValidationText = $"No folder specified for trigger action '{FullName}'";
            } else if(!FolderPath.IsDirectory()) {
                ValidationText = $"Folder'{FolderPath}' not found for trigger action '{FullName}'";
            } else {
                ValidationText = string.Empty;
            }

            if (!IsValid) {
                ShowValidationNotification();
            }
        }
        #endregion

        #region MpIFileSystemWatcher Implementation

        [SuppressPropertyChangedWarnings]
        public void OnFileSystemItemChanged(object sender, FileSystemEventArgs e) {
            if(!MpBootstrapperViewModelBase.IsCoreLoaded) {
                // NOTE this check maybe unnecessary. Rtf test was being generated onto desktop during startup and interfering w/ this trigger's lifecycle
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                MpCopyItem ci = null;
                switch (e.ChangeType) {
                    case WatcherChangeTypes.Changed:
                    case WatcherChangeTypes.Created:
                        ci = await MpCopyItem.CreateAsync(
                            //sourceId: MpDefaultDataModelTools.ThisOsFileManagerAppId,
                            itemType: MpCopyItemType.FileList,
                            data: e.FullPath,
                            suppressWrite: true);
                        if (!ci.WasDupOnCreate) {
                            // new item, create source ref
                            //await MpTransactionSource.CreateAsync(
                            //    copyItemId: ci.Id,
                            //    sourceObjId: MpDefaultDataModelTools.ThisOsFileManagerAppId,
                            //    sourceType: MpCopyItemSourceType.App);
                        }
                        break;
                    case WatcherChangeTypes.Renamed:
                        RenamedEventArgs re = e as RenamedEventArgs;
                        ci = await MpDataModelProvider.GetCopyItemByDataAsync(re.OldFullPath);
                        if(ci == null) {
                            ci = await MpCopyItem.CreateAsync(
                                //sourceId: MpDefaultDataModelTools.ThisOsFileManagerAppId,
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
                        bool isVisible = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.CopyItemId == ci.Id) != null;
                        await ci.DeleteFromDatabaseAsync();
                        if(isVisible) {
                            MpPlatform.Services.QueryInfo.NotifyQueryChanged();
                            
                        }
                        break;
                }

                if (ci != null) {
                    var ao = new MpAvFileSystemTriggerOutput() {
                        CopyItem = ci,
                        FileSystemChangeType = e.ChangeType
                    };
                    await base.PerformActionAsync(ao);
                }
            });
        }

        #endregion

        #region Private Methods

        private void MpFileSystemTriggerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {
            //    case nameof(IncludeSubdirectories):
            //    case nameof(FileSystemPath):
            //        if(IsBusy) {
            //            return;
            //        }
            //        if(IsEnabled.IsTrue()) {
            //            ReEnableTrigger();
            //        }
            //        break;
            //}
        }

        #endregion

        #region Commands

        public ICommand ToggleIncludeSubDirectoriesCommand => new MpAsyncCommand(
            async () => {
                bool wasEnabled = IsEnabled.IsTrue();
                DisableTriggerCommand.Execute(null);
                IncludeSubdirectories = !IncludeSubdirectories;

                if (wasEnabled) {
                    while (IsBusy) { await Task.Delay(100); }
                    await Task.Delay(300);
                    EnableTriggerCommand.Execute(null);
                }
            },()=>IsValid);

        public ICommand SelectFileSystemPathCommand => new MpAsyncCommand(
            async () => {
                string initDir = FolderPath;
                if(string.IsNullOrEmpty(initDir)) {
                    initDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                } else if(File.Exists(initDir)) {
                    initDir = Path.GetDirectoryName(initDir);
                }
                MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;
                var selectedDir = await new OpenFolderDialog() {
                    Title = "Select folder",
                    Directory = initDir
                }.ShowAsync(MpAvMainWindow.Instance);
                
                MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;

                bool wasEnabled = IsEnabled.IsTrue();
                // remove old watcher
                DisableTriggerCommand.Execute(null);
                FolderPath = selectedDir;
                if(string.IsNullOrEmpty(FolderPath)) {
                    IncludeSubdirectories = false;
                }


                if(wasEnabled) {
                    while (IsBusy) { await Task.Delay(100); }
                    await Task.Delay(300);
                    EnableTriggerCommand.Execute(null);
                }
            });

        #endregion
    }
}
