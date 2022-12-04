
using MonkeyPaste.Common;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Input;
using Avalonia.Controls;

namespace MonkeyPaste.Avalonia {

    public class MpAvFileWriterActionViewModel : MpAvActionViewModelBase, MpITooltipInfoViewModel {
        #region Properties

        #region View Models


        #endregion

        #region Appearance

        public override object Tooltip => "Prefix is used for non-file clipboard items. If unset file will use the content's title.";
        #endregion
        #region State

        public bool IsValidFileSystemPath {
            get {
                if (string.IsNullOrWhiteSpace(FileSystemPath)) {
                    return false;
                }
                return FileSystemPath.IsFileOrDirectory();
            }
        }

        #endregion

        #region Model

        public string NamePrefix {
            get {
                if(Arg2 == null) {
                    return null;
                }
                return Arg2;
            }
            set {
                if(NamePrefix != value) {
                    Arg2 = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(NamePrefix));
                }
            }
        }

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

        #endregion

        #endregion

        #region Constructors

        public MpAvFileWriterActionViewModel(MpAvActionCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Protected Overrides

        public override async Task PerformActionAsync(object arg) {
            if (!CanPerformAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            string fn = string.IsNullOrEmpty(NamePrefix) ? actionInput.CopyItem.Title : NamePrefix;
            var fl = new List<string>();//MpCopyItemMerger.GetFileList(actionInput.CopyItem);
            string outputData = string.Empty;
            foreach(var fp in fl) {
                fn = actionInput.CopyItem.ItemType == MpCopyItemType.FileList ? Path.GetFileNameWithoutExtension(fp) : fn;
                string fe = Path.GetExtension(fp);
                string tfp = Path.Combine(FileSystemPath, fn + fp);

                File.Copy(fp, tfp);

                if(!string.IsNullOrEmpty(outputData)) {
                    outputData += Environment.NewLine;
                }
                outputData += tfp;
            }

            await base.PerformActionAsync(
                    new MpAvFileWriterOutput() {
                        Previous = arg as MpAvActionOutput,
                        CopyItem = actionInput.CopyItem,
                        OutputFilePath = outputData
                    });
        }

        #endregion

        #region Protected Methods
        protected override async Task<bool> Validate() {
            await base.Validate();
            if (!IsValid) {
                return IsValid;
            }

            if (string.IsNullOrWhiteSpace(FileSystemPath)) {
                ValidationText = $"File Writer Path for Action '{FullName}' not set";
                await ShowValidationNotification();
            } else if (!IsValidFileSystemPath) {
                ValidationText = $"File Writer Path '{FileSystemPath}' for Action '{FullName}' not found.";
                await ShowValidationNotification();
            } else {
                ValidationText = string.Empty;
            }
            return IsValid;
        }

        #endregion

        #region Private Methods

        private void MpFileSystemTriggerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(FileSystemPath):
                    if (IsBusy) {
                        return;
                    }
                    if (IsEnabled.HasValue && IsEnabled.Value) {
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
                if (string.IsNullOrEmpty(initDir)) {
                    initDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                } else if (File.Exists(initDir)) {
                    initDir = Path.GetDirectoryName(initDir);
                }

                MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;
                var selectedDir = await new OpenFolderDialog() {
                    Title = "Select folder",
                    Directory = initDir
                }.ShowAsync(MpAvMainWindow.Instance);
                

                MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
                if (string.IsNullOrEmpty(selectedDir)) {
                    FileSystemPath = selectedDir;
                    await ReEnable();
                }
            });

        #endregion
    }
}
