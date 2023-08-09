using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvFolderWatcherTriggerViewModel :
        MpAvTriggerActionViewModelBase,
        MpIFileSystemEventHandler {

        #region Constants

        public const string FOLDER_PATH_PARAM_ID = "WatchFolderPath";
        public const string INCLUDE_SUB_DIRS_PARAM_ID = "IncludeSubDirs";
        public const string EVENT_TYPES_PARAM_ID = "WatchEventTypes";

        #endregion

        #region Statics

        static MpAvFileSystemWatcher _fsWatcher = new MpAvFileSystemWatcher();

        #endregion

        #region Interfaces

        #region MpIFileSystemEventHandler Implementation

        [SuppressPropertyChangedWarnings]
        void MpIFileSystemEventHandler.OnFileSystemItemChanged(object sender, FileSystemEventArgs e) {
            // NOTE 
            MpDebug.Assert(IsEnabled, $"Folder Watcher change shouldn't be received when not enabled");

            bool is_core_loaded = Mp.Services != null &&
                     Mp.Services.StartupState != null &&
                     Mp.Services.StartupState.IsCoreLoaded;
            if (!is_core_loaded) {
                // NOTE this check maybe unnecessary.
                // Rtf test was being generated onto desktop during startup and interfering w/ this trigger's lifecycle
                return;
            }
            var flag_test = e.ChangeType.ToString();
            if (flag_test.Contains(" ") || flag_test.Contains("|") || flag_test.Contains("All")) {
                // this handler is assuming only 1 change comes in at a time and that 'All' isn't one of them...
                // so if thats not the case change handling
                MpDebug.Break("file watch event type not accounted for in parse");
            }
            bool is_handled = ChangeTypeNames.Any(x => x.ToLower() == flag_test.ToLower());
            MpConsole.WriteLine($"Folder watcher event '{flag_test}' occured for path '{FolderPath}' recursive: {IncludeSubdirectories} handled: {is_handled}");
            if (!is_handled) {
                // change ignored
                return;
            }


            Dispatcher.UIThread.Post(async () => {
                var si = await e.FullPath.ToFileOrFolderStorageItemAsync();
                if (si == null) {
                    return;
                }
                MpAvDataObject avdo = new MpAvDataObject(MpPortableDataFormats.AvFileNames, new[] { si });

                // set title to '<FileName> <ChangeType>'
                avdo.SetData(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, Path.GetFileNameWithoutExtension(e.FullPath) + $" {e.ChangeType}");
                MpCopyItem ci = await avdo.ToCopyItemAsync(true);
                if (ci == null) {
                    return;
                }
                var ao = new MpAvFileSystemTriggerOutput() {
                    CopyItem = ci,
                    FileSystemChangeType = e.ChangeType
                };
                await base.PerformActionAsync(ao);
            });
        }

        #endregion

        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = "Folder",
                                controlType = MpParameterControlType.DirectoryChooser,
                                unitType = MpParameterValueUnitType.FileSystemPath,
                                isRequired = true,
                                paramId = FOLDER_PATH_PARAM_ID
                            },
                            new MpParameterFormat() {
                                label = "Ignore Duplicate",
                                controlType = MpParameterControlType.CheckBox,
                                unitType = MpParameterValueUnitType.Bool,
                                isRequired = false,
                                paramId = INCLUDE_SUB_DIRS_PARAM_ID
                            },
                            new MpParameterFormat() {
                                label = "Events",
                                controlType = MpParameterControlType.MultiSelectList,
                                unitType = MpParameterValueUnitType.PlainText,
                                isRequired = false,
                                paramId = EVENT_TYPES_PARAM_ID,
                                values =
                                    typeof(WatcherChangeTypes)
                                    .GetEnumNames()
                                    .Take(typeof(WatcherChangeTypes).Length()-1) // omit 'All'
                                    .Select(x=>
                                        new MpPluginParameterValueFormat() {
                                            isDefault = true,
                                            value = x
                                        })
                                    .ToList()

                            }
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

        #endregion

        #region Properties

        #region Appearance
        public override string ActionHintText =>
            "Folder Changed - Triggered when a folders content changes. The output will be a new content item of the file or folder that has changed along with the type of change.";

        #endregion

        #region State
        protected override MpIActionComponent TriggerComponent =>
            _fsWatcher;

        #endregion

        #region Model

        public string FolderPath {
            get {
                if (ArgLookup.TryGetValue(FOLDER_PATH_PARAM_ID, out var param_vm) &&
                    param_vm.CurrentValue is string curVal) {
                    return curVal;
                }
                return string.Empty;
            }
            set {
                if (FolderPath != value) {
                    ArgLookup[FOLDER_PATH_PARAM_ID].CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(FolderPath));
                }
            }
        }

        public bool IncludeSubdirectories {
            get {
                if (ArgLookup.TryGetValue(INCLUDE_SUB_DIRS_PARAM_ID, out var param_vm) &&
                    param_vm.CurrentValue.ParseOrConvertToBool(false) is bool curVal) {
                    return curVal;
                }
                return false;
            }
            set {
                if (IncludeSubdirectories != value) {
                    ArgLookup[INCLUDE_SUB_DIRS_PARAM_ID].CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IncludeSubdirectories));
                }
            }
        }

        public List<string> ChangeTypeNames {
            get {
                if (ArgLookup.TryGetValue(EVENT_TYPES_PARAM_ID, out var param_vm) &&
                    param_vm.CurrentValue.ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value) is List<string> event_names) {
                    return event_names;
                }
                return null;
            }
            set {
                if (ChangeTypeNames != value &&
                    ChangeTypeNames.Difference(value).Count() > 0) {
                    ArgLookup[INCLUDE_SUB_DIRS_PARAM_ID].CurrentValue = value.ToCsv(MpCsvFormatProperties.DefaultBase64Value);
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ChangeTypeNames));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvFolderWatcherTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvFolderWatcherTriggerViewModel_PropertyChanged;
        }


        #endregion

        #region Public Overrides
        #endregion

        #region Protected Methods

        protected override async Task ValidateActionAsync() {
            await base.ValidateActionAsync();
            if (!IsValid) {
                return;
            }
            if (string.IsNullOrEmpty(FolderPath)) {
                ValidationText = $"No folder specified for trigger action '{FullName}'";
            } else if (!FolderPath.IsDirectory()) {
                ValidationText = $"Folder'{FolderPath}' not found for trigger action '{FullName}'";
            } else {
                ValidationText = string.Empty;
            }

            if (!IsValid) {
                ShowValidationNotification();
            }
        }
        #endregion


        #region Private Methods

        private void MpAvFolderWatcherTriggerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(ActionArgs):
                    //OnPropertyChanged(nameof(SelectedPreset));
                    break;
                case nameof(HasArgsChanged):
                    if (!HasArgsChanged ||
                        !IsEnabled) {
                        break;
                    }
                    DisableTrigger();
                    EnableTrigger();
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand ToggleIncludeSubDirectoriesCommand => new MpAsyncCommand(
            async () => {
                bool wasEnabled = IsEnabled;
                DisableTriggerCommand.Execute(null);
                IncludeSubdirectories = !IncludeSubdirectories;

                if (wasEnabled) {
                    while (IsBusy) { await Task.Delay(100); }
                    await Task.Delay(300);
                    EnableTriggerCommand.Execute(null);
                }
            });

        public ICommand SelectFileSystemPathCommand => new MpAsyncCommand(
            async () => {
                var selectedDir = await Mp.Services.NativePathDialog
                        .ShowFolderDialogAsync($"Select Watch Folder", FolderPath);

                // remove old watcher
                bool wasEnabled = IsEnabled;
                DisableTriggerCommand.Execute(null);
                FolderPath = selectedDir;

                if (wasEnabled) {
                    while (IsBusy) { await Task.Delay(100); }
                    await Task.Delay(300);
                    EnableTriggerCommand.Execute(null);
                }
            });

        #endregion
    }
}
