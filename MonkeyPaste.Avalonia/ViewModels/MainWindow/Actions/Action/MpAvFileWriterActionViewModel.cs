using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvFileWriterActionViewModel :
        MpAvActionViewModelBase, MpITooltipInfoViewModel {
        #region Constants

        public const string SELECTED_DIR_PARAM_ID = "SelectedDirectory";
        public const string CUSTOM_FILE_NAME_PARAM_ID = "CustomName";

        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = "Directory",
                                controlType = MpParameterControlType.DirectoryChooser,
                                unitType = MpParameterValueUnitType.FileSystemPath,
                                isRequired = true,
                                paramId = SELECTED_DIR_PARAM_ID,
                                description = "The directory where input content will be written."
                            },
                            new MpParameterFormat() {
                                label = "Custom Name",
                                controlType = MpParameterControlType.TextBox,
                                unitType = MpParameterValueUnitType.PlainTextContentQuery,
                                isRequired = false,
                                paramId = CUSTOM_FILE_NAME_PARAM_ID,
                                description = "When left blank, the content will use its title as the file name."
                            },
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

        #endregion

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

        public string FileSystemPath {
            get {
                if (ArgLookup.TryGetValue(SELECTED_DIR_PARAM_ID, out var param_vm) &&
                    param_vm.CurrentValue is string curVal) {
                    return curVal;
                }
                return string.Empty;
            }
            set {
                if (FileSystemPath != value) {
                    ArgLookup[SELECTED_DIR_PARAM_ID].CurrentValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(FileSystemPath));
                }
            }
        }

        // Arg2

        public string NamePrefix {
            get {
                if (ArgLookup.TryGetValue(CUSTOM_FILE_NAME_PARAM_ID, out var param_vm) &&
                    param_vm.CurrentValue is string curVal) {
                    return curVal;
                }
                return string.Empty;
            }
            set {
                if (NamePrefix != value) {
                    ArgLookup[CUSTOM_FILE_NAME_PARAM_ID].CurrentValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(NamePrefix));
                }
            }
        }


        #endregion

        #endregion

        #region Constructors

        public MpAvFileWriterActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Protected Overrides

        public override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            List<string> fl = null;
            if (actionInput.CopyItem.ItemType == MpCopyItemType.FileList) {
                fl = actionInput.CopyItem.ItemData.SplitNoEmpty(MpCopyItem.FileItemSplitter).ToList();
            } else {
                string ci_fp = await actionInput.CopyItem.ItemData.ToFileAsync(
                                forceNamePrefix: actionInput.CopyItem.Title,
                                forceExt: actionInput.CopyItem.ItemType == MpCopyItemType.Image ? "png" : "txt",
                                isTemporary: true);

                fl = new List<string>() { ci_fp };
            }
            string outputData = string.Empty;
            foreach (var fp in fl) {
                string output = await WriteToFileSystemPath(fp);
                if (output == null) {
                    // error occured break and finish
                    break;
                }

                if (!string.IsNullOrEmpty(outputData)) {
                    outputData += Environment.NewLine;
                }
                outputData += output;
            }

            await base.PerformActionAsync(
                    new MpAvFileWriterOutput() {
                        Previous = arg as MpAvActionOutput,
                        CopyItem = actionInput.CopyItem,
                        OutputFilesStr = outputData
                    });
        }

        #endregion

        #region Protected Methods
        protected override async Task ValidateActionAsync() {
            await Task.Delay(1);
            if (string.IsNullOrWhiteSpace(FileSystemPath)) {
                ValidationText = $"File Writer Path for Action '{FullName}' not set";
                ShowValidationNotification(1);
            } else if (!IsValidFileSystemPath) {
                ValidationText = $"File Writer Path '{FileSystemPath}' for Action '{FullName}' not found.";
                ShowValidationNotification(1);
            } else {
                ValidationText = string.Empty;
            }
        }

        #endregion

        #region Private Methods

        private void MpFileSystemTriggerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {
            //case nameof(FileSystemPath):
            //    if (IsBusy) {
            //        return;
            //    }
            //    if (IsEnabled.IsTrue()) {
            //        ReEnable().FireAndForgetSafeAsync(this);
            //    }
            //    break;
            //}
        }

        private async Task<string> WriteToFileSystemPath(string unformatted_path) {
            // prefix source file name and attempt to copy to target dir
            string fn = Path.GetFileNameWithoutExtension(unformatted_path);
            fn = NamePrefix + fn;
            string fe = Path.GetExtension(unformatted_path);
            string formatted_target_path = Path.Combine(FileSystemPath, fn + fe);

            try {
                File.Copy(unformatted_path, formatted_target_path);
            }
            catch (Exception ex) {
                // on io error, show ntf and allow for fix
                formatted_target_path = null;

                Func<object, object> retryFunc = (args) => {
                    Dispatcher.UIThread.Post(async () => {
                        formatted_target_path = await WriteToFileSystemPath(unformatted_path);
                    });

                    return null;
                };

                string error_msg = ex.Message;

                var result = await MpNotificationBuilder.ShowNotificationAsync(
                    notificationType: MpNotificationType.FileIoError,
                    title: $"{Label} Error",
                    body: error_msg,
                    retryAction: retryFunc,
                    fixCommand: Parent.SelectActionCommand,
                    fixCommandArgs: new object[] { ActionId, 1, error_msg });

                if (result == MpNotificationDialogResultType.Cancel ||
                    result == MpNotificationDialogResultType.Ignore) {
                    formatted_target_path = null;
                } else {
                    // NOTE not sure right strategy so this is off top of head
                    while (formatted_target_path == null) {
                        await Task.Delay(100);
                    }
                }
            }

            return formatted_target_path;
        }

        #endregion

        #region Commands

        public ICommand SelectFileSystemPathCommand => new MpAsyncCommand(
            async () => {
                //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;

                var selectedDir = await Mp.Services.NativePathDialog
                        .ShowFolderDialogAsync($"Select Folder", FileSystemPath);

                //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;

                FileSystemPath = selectedDir;
            });

        #endregion
    }
}
