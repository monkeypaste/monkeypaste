using DynamicData;
using MonkeyPaste.Common;

using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvHandledClipboardFormatViewModel :
        //MpAvTreeSelectorViewModelBase<MpAvClipboardHandlerItemViewModel, MpAvClipboardFormatPresetViewModel>,
        MpAvPresetParamHostViewModelBase<MpAvClipboardHandlerItemViewModel, MpAvClipboardFormatPresetViewModel>,
        MpISelectableViewModel,
        //MpIParameterHostViewModel,
        MpIHoverableViewModel {
        #region Private Variables
        #endregion


        #region Interfaces
        #region MpISelectableViewModel Implementation

        public bool IsSelected {
            get {
                if (Parent == null) {
                    return false;
                }
                return Parent.SelectedItem == this;
            }
            set {
                if (IsSelected != value) {
                    if (Parent != null && value) {
                        Parent.SelectedItem = this;
                        OnPropertyChanged(nameof(IsSelected));
                    }
                }
            }
        }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #region MpIParameterHost Implementation

        public override MpPluginWrapper PluginFormat {
            get {
                if (Parent == null) {
                    return null;
                }
                return Parent.PluginFormat;
            }
        }
        public override int IconId => HandledFormatIconId;
        public override string PluginGuid => FormatGuid;

        public override MpParameterHostBaseFormat ComponentFormat => ClipboardPluginFormat;

        public override MpParameterHostBaseFormat BackupComponentFormat =>
            PluginFormat == null ||
            PluginFormat.backupCheckPluginFormat == null ||
            PluginFormat.backupCheckPluginFormat.oleHandler == null ||
            (IsReader && PluginFormat.backupCheckPluginFormat.oleHandler.readers == null) ||
            (IsWriter && PluginFormat.backupCheckPluginFormat.oleHandler.writers == null) ?
                null :
                IsReader ?
                    PluginFormat.backupCheckPluginFormat.oleHandler.readers.FirstOrDefault(x => x.formatGuid == FormatGuid) :
                    PluginFormat.backupCheckPluginFormat.oleHandler.writers.FirstOrDefault(x => x.formatGuid == FormatGuid);
        public override MpIPluginComponentBase PluginComponent => ClipboardPluginComponent;

        public string FormatGuid { get; private set; }

        public MpClipboardHandlerFormat ClipboardPluginFormat {
            get {
                if (PluginFormat == null) {
                    return null;
                }
                if (IsReader) {
                    return Parent.ClipboardPluginFormat.readers.FirstOrDefault(x => x.formatGuid == FormatGuid);
                }
                if (IsWriter) {
                    return Parent.ClipboardPluginFormat.writers.FirstOrDefault(x => x.formatGuid == FormatGuid);
                }
                MpConsole.WriteTraceLine($"Error finding ClipboardHandler format for formatGuid: '{FormatGuid}'");
                return null;
            }
        }

        public MpIOlePluginComponent ClipboardPluginComponent =>
            PluginFormat == null || PluginFormat.Components == null ?
                null :
                IsReader ?
                    PluginFormat.Components.OfType<MpIOleReaderComponent>().FirstOrDefault() :
                    PluginFormat.Components.OfType<MpIOleWriterComponent>().FirstOrDefault();

        public bool IsReader =>
            PluginFormat == null ?
                false :
                PluginFormat
                .oleHandler
                .readers.Any(x => x.formatGuid == FormatGuid);

        public bool IsWriter => PluginFormat == null ?
                                    false :
                                    PluginFormat.oleHandler.writers.Any(x => x.formatGuid == FormatGuid); //ClipboardPluginComponent is MpIOleReaderComponent;


        #endregion

        #endregion

        #region Properties

        #region MpAvTreeSelectorViewModelBase Overrides

        public override MpITreeItemViewModel ParentTreeItem => Parent;


        #endregion

        #region View Models

        //public MpITreeItemViewModel ParentTreeItem => Parent;

        //public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        #endregion


        #region Appearance

        public string ManageLabel => $"{Title} Preset Manager";

        public string ItemBackgroundHexColor {
            get {
                if (IsSelected) {
                    return MpSystemColors.navyblue;
                }
                if (IsHovering) {
                    return MpSystemColors.lightgray;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string ItemTitleForegroundHexColor {
            get {
                if (IsSelected) {
                    return MpSystemColors.white;
                }
                if (IsHovering) {
                    return MpSystemColors.black;
                }
                return MpSystemColors.white;
            }
        }


        public string HandledFormatIconResourceKey {
            get {
                return Mp.Services.PlatformResource.GetResource("AppImage") as string;

                //switch (HandledFormat) {
                //    case MpClipboardFormatType.Bitmap:
                //        return MpPlatformWrapper.Services.PlatformResource.GetResource("ImageIcon") as string;
                //    case MpClipboardFormatType.Html:
                //        return MpPlatformWrapper.Services.PlatformResource.GetResource("HtmlIcon") as string;
                //    case MpClipboardFormatType.AvCsv:
                //        return MpPlatformWrapper.Services.PlatformResource.GetResource("CsvIcon") as string;
                //    case MpClipboardFormatType.FileDrop:
                //        return MpPlatformWrapper.Services.PlatformResource.GetResource("CsvIcon") as string;
                //}
            }
        }
        #endregion

        #region State

        public bool IsPrimaryFormat =>
            HandledFormat == MpPortableDataFormats.Text ||
            HandledFormat == MpPortableDataFormats.Image ||
            HandledFormat == MpPortableDataFormats.Files;

        public override bool IsLoaded => Items.Count > 0 && Items[0].Items.Count > 0;

        public bool IsValid { get; private set; }
        //public bool IsAnyEditingParameters => Items.Any(x => x.IsEditingParameters);

        public bool IsHovering { get; set; } = false;

        public string HandledFormat {
            get {
                if (ClipboardPluginFormat == null) {
                    return null;
                }
                return ClipboardPluginFormat.formatName;
            }
        }

        #endregion

        #region Model

        #region Db

        public int HandledFormatIconId { get; private set; }

        #endregion

        #region ClipboardHandler (Reader or Writer) Plugin

        public bool IsCoreHandler {
            get {
                if (PluginFormat == null) {
                    return false;
                }
                return PluginFormat.guid == MpPluginLoader.CoreClipboardHandlerGuid;
            }
        }
        public string Title {
            get {
                if (ClipboardPluginFormat == null) {
                    return string.Empty;
                }
                return ClipboardPluginFormat.displayName;
            }
        }
        public string SelectorLabel =>
            $"{Title} ({HandledFormat})";

        public string Description {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.description;
            }
        }



        #endregion

        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors

        public MpAvHandledClipboardFormatViewModel() : base(null) { }

        public MpAvHandledClipboardFormatViewModel(MpAvClipboardHandlerItemViewModel parent) : base(parent) {
            PropertyChanged += MpHandledClipboardFormatViewModel_PropertyChanged;
            Items.CollectionChanged += PresetViewModels_CollectionChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpClipboardHandlerFormat handlerFormat) {
            IsValid = await ValidateClipboardHandlerFormatAsync(handlerFormat);
            if (!IsValid) {
                return;
            }
            if (IsLoaded) {
                return;
            }
            IsBusy = true;

            FormatGuid = handlerFormat.formatGuid;

            if (IsReader && IsWriter) {
                MpDebug.Break();
            }

            if (ClipboardPluginComponent == null) {
                throw new Exception("Cannot find component");
            }

            HandledFormatIconId = await MpAvPluginIconLocator.LocatePluginIconIdAsync(PluginFormat, ClipboardPluginFormat.iconUri);
            var presets = await MpAvPluginPresetLocator.LocatePresetsAsync(
                this,
                enableOnReset: IsCoreHandler,
                showMessages: false);


            Items.Clear();

            foreach (var preset in presets) {
                var naipvm = await CreatePresetViewModelAsync(preset);
                Items.Add(naipvm);
            }
            await MpAvPluginParameterBuilder.CleanupMissingParamsAsync(Items);
            OnPropertyChanged(nameof(Items));

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            IsBusy = false;
        }

        public async Task<MpAvClipboardFormatPresetViewModel> CreatePresetViewModelAsync(MpPluginPreset aip) {
            MpAvClipboardFormatPresetViewModel naipvm = new MpAvClipboardFormatPresetViewModel(this);
            await naipvm.InitializeAsync(aip);
            return naipvm;
        }

        public string GetUniquePresetName() {
            int uniqueIdx = 1;
            string uniqueName = UiStrings.CommonPresetLabel;
            string testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);

            while (Items.Any(x => x.Label.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);
            }
            return uniqueName + uniqueIdx;
        }

        public virtual async Task<MpParameterFormat> DeferredCreateParameterModel(MpParameterFormat aip) {
            //used to load remote content and called from CreateParameterViewModel in preset
            await Task.Delay(1);
            return aip;
        }

        public virtual bool ValidateParameters() {
            if (SelectedItem == null) {
                return true;
            }
            return SelectedItem.IsAllValid;
        }

        public bool IsDataObjectValid(MpPortableDataObject pdo) {
            return pdo.ContainsData(HandledFormat);
        }

        public override string ToString() {
            return $"Format: {Title} Preset: {(SelectedItem == null ? "None" : SelectedItem.Label)} Enabled: {(SelectedItem == null ? "Null" : SelectedItem.IsEnabled)}";
        }
        #endregion

        #region Protected Methods

        #region Db Event Handlers

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpPluginPreset aip) {

            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpPluginPreset aip) {
                if (aip.PluginGuid == FormatGuid) {
                    var presetVm = Items.FirstOrDefault(x => x.Preset.Id == aip.Id);
                    if (presetVm != null) {
                        int presetIdx = Items.IndexOf(presetVm);
                        if (presetIdx >= 0) {
                            Items.RemoveAt(presetIdx);
                            OnPropertyChanged(nameof(Items));
                            OnPropertyChanged(nameof(SelectedItem));
                        }
                    }
                }
            }
        }

        #endregion

        protected virtual async Task TransformContent() {
            await Task.Delay(1);
        }

        protected virtual async Task AppendContent() {
            await Task.Delay(1);
        }

        #endregion

        #region Private Methods


        private void PresetViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdatePresetSortOrderAsync().FireAndForgetSafeAsync(this);
        }

        private void MpHandledClipboardFormatViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;

                        if (SelectedItem == null) {
                            SelectedItem = Items.AggregateOrDefault((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
                        }
                        OnPropertyChanged(nameof(Items));
                        SelectedItem.OnPropertyChanged(nameof(SelectedItem.Items));
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
                    Parent.OnPropertyChanged(nameof(Parent.SelectedItem));

                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.IsAnySelected));
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.SelectedItem));
                    OnPropertyChanged(nameof(ItemBackgroundHexColor));
                    OnPropertyChanged(nameof(ItemTitleForegroundHexColor));

                    break;
                case nameof(IsHovering):
                    OnPropertyChanged(nameof(ItemBackgroundHexColor));
                    OnPropertyChanged(nameof(ItemTitleForegroundHexColor));
                    break;
                case nameof(SelectedItem):
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.SelectedPresetViewModel));
                    break;
                case nameof(Title):
                    OnPropertyChanged(nameof(SelectorLabel));
                    break;
            }
        }

        private async Task UpdatePresetSortOrderAsync(bool fromModel = false) {
            if (fromModel) {
                Items.Sort(x => x.SortOrderIdx);
            } else {
                foreach (var aipvm in Items) {
                    aipvm.SortOrderIdx = Items.IndexOf(aipvm);
                }
                if (!MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    foreach (var pvm in Items) {
                        await pvm.Preset.WriteToDatabaseAsync();
                    }
                }
            }
        }
        private async Task<bool> ValidateClipboardHandlerFormatAsync(MpClipboardHandlerFormat chf) {
            await Task.Delay(1);
            bool isValid = true;

            var error_notifications = new List<MpNotificationFormat>();
            var sb = new StringBuilder();

            if (string.IsNullOrEmpty(chf.iconUri)
                //|| !Uri.IsWellFormedUriString(chf.iconUri, UriKind.RelativeOrAbsolute)
                ) {
                //sb.AppendLine($"Plugin {PluginFormat.title} has malformed icon uri '{chf.iconUri}', plugin must have valid icon");
                //error_notifications.Add(MpPluginLoader.CreateInvalidPluginNotification(sb.ToString(), PluginFormat));
                chf.iconUri = null;// Mp.Services.PlatformResource.GetResource<string>("QuestionMarkImage");
            }
            bool needs_fixing = error_notifications.Count > 0;
            MpDebug.Assert(!needs_fixing, sb.ToString());
            return isValid;
        }

        private async Task CleanupMissingParamsAsync() {
            var presets_w_missing_params =
                Items
                .Where(x => x.Items.Any(x => x is MpAvMissingParameterViewModel));
            foreach (var pvm in presets_w_missing_params) {
                var missing_params = pvm.Items.OfType<MpAvMissingParameterViewModel>().ToList();
                for (int i = 0; i < missing_params.Count; i++) {
                    await missing_params[i].PresetValueModel.DeleteFromDatabaseAsync();
                    pvm.Items.Remove(missing_params[i]);
                }
            }

        }
        #endregion

        #region Commands


        public ICommand SelectPresetCommand => new MpCommand<MpAvClipboardFormatPresetViewModel>(
             (selectedPresetVm) => {
                 //if(!IsLoaded) {
                 //    await LoadChildren();
                 //}
                 if (!IsSelected) {
                     Parent.SelectedItem = this;
                 }
                 SelectedItem = selectedPresetVm;
             });

        public ICommand ManageClipboardHandlerCommand => new MpCommand(
             () => {
                 if (!IsSelected) {
                     Parent.SelectedItem = this;
                 }
                 if (!Parent.IsSelected) {
                     Parent.IsSelected = true;
                 }

                 if (SelectedItem == null && Items.Count > 0) {
                     SelectedItem = Items.AggregateOrDefault((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
                 }
                 MpAvSidebarItemCollectionViewModel.Instance.SelectSidebarItemCommand.Execute(Parent.Parent);
                 OnPropertyChanged(nameof(SelectedItem));

             });


        public ICommand DeletePresetCommand => new MpAsyncCommand<object>(
            async (presetVmArg) => {
                // NOTE delete will never get trnasaction type of parameter so it can be ignored
                IsBusy = true;

                var presetVm = presetVmArg as MpAvClipboardFormatPresetViewModel;
                foreach (var presetVal in presetVm.Items) {
                    await presetVal.PresetValueModel.DeleteFromDatabaseAsync();
                }
                await presetVm.Preset.DeleteFromDatabaseAsync();

                IsBusy = false;
            },
            (presetVmArg) => {
                if (presetVmArg is MpAvClipboardFormatPresetViewModel aipvm &&
                     aipvm.CanDelete(null)) {
                    return true;
                } else if (presetVmArg is object[] argParts &&
                        argParts[0] is MpAvClipboardFormatPresetViewModel trans_aipvm) {
                    return trans_aipvm.CanDelete(argParts[1]);
                }
                return false;
            });
        public ICommand ResetPresetCommand => new MpAsyncCommand<object>(
            async (presetVmArg) => {
                MpConsole.WriteLine("Resetting...");
                IsBusy = true;

                MpAvClipboardFormatPresetViewModel aipvm = null;
                if (presetVmArg is MpAvClipboardFormatPresetViewModel arg_vm) {
                    aipvm = arg_vm;
                } else if (presetVmArg is object[] argParts &&
                        argParts[0] is MpAvClipboardFormatPresetViewModel trans_aipvm) {
                    aipvm = trans_aipvm;
                    if (argParts[1] is MpOlePluginRequest aprf) {
                        // loop through req settings and set preset to those values
                        foreach (var req_kvp in aprf.items) {
                            if (aipvm.Items.FirstOrDefault(x => x.ParamId == req_kvp.paramId) is MpAvParameterViewModelBase pvm) {
                                pvm.CurrentValue = req_kvp.value;
                            } else {
                                MpConsole.WriteLine($"Param req item id '{req_kvp.paramId}' w/ value '{req_kvp.value}' not found on preset '{aipvm}'");
                            }
                        }
                        IsBusy = false;
                        return;
                    }
                }
                if (aipvm == null) {
                    IsBusy = false;
                    return;
                }

                Dictionary<string, string> shared_params_to_clear_or_restore = new Dictionary<string, string>();
                foreach (var sp in aipvm.Items.Where(x => x.IsSharedValue)) {
                    shared_params_to_clear_or_restore.Add(sp.ParamId.ToString(), sp.CurrentValue);
                }

                if (shared_params_to_clear_or_restore.Any()) {
                    // ntf w/ yes/no/cancel to reset shared values
                    var result = await Mp.Services.PlatformMessageBox.ShowYesNoCancelMessageBoxAsync(
                        title: UiStrings.CommonConfirmLabel,
                        message: $"'{aipvm.Label}' contains shared values. Would you like to reset those as well?",
                        iconResourceObj: "QuestionMarkImage",
                        owner: MpAvWindowManager.MainWindow);
                    if (result.IsNull()) {
                        // cancel
                        IsBusy = false;
                        return;
                    }
                    if (result.IsTrue()) {
                        // clear shared values
                        foreach (var kvp in shared_params_to_clear_or_restore) {
                            shared_params_to_clear_or_restore[kvp.Key] = string.Empty;
                        }
                    }
                }
                // recreate default preset record (name, icon, etc.)
                var defaultPresetModel = await MpAvPluginPresetLocator.CreateOrResetManifestPresetModelAsync(
                    this, aipvm.PresetGuid, Items.IndexOf(aipvm));

                // before initializing preset remove current values from db or it won't reset values
                await Task.WhenAll(aipvm.Items.Select(x => x.PresetValueModel.DeleteFromDatabaseAsync()));

                await aipvm.InitializeAsync(defaultPresetModel);

                if (shared_params_to_clear_or_restore.Any()) {
                    // update shared values and trigger db write for other instance updates
                    foreach (var shared_param_to_restore in shared_params_to_clear_or_restore) {
                        var cur_param = aipvm.Items.FirstOrDefault(x => x.ParamId.ToString() == shared_param_to_restore.Key);
                        MpDebug.Assert(cur_param != null, $"Can't find shared param '{shared_param_to_restore.Key}'");
                        cur_param.CurrentValue = shared_param_to_restore.Value;
                        cur_param.SaveCurrentValueCommand.Execute(null);
                    }
                }
                Items.ForEach(x => x.IsSelected = x.PresetId == aipvm.PresetId);
                OnPropertyChanged(nameof(SelectedItem));

                IsBusy = false;
            },
            (presetVmArg) => {
                if (presetVmArg is MpAvClipboardFormatPresetViewModel aipvm &&
                     !aipvm.CanDelete(null)) {
                    return true;
                } else if (presetVmArg is object[] argParts &&
                        argParts[0] is MpAvClipboardFormatPresetViewModel trans_aipvm) {
                    return !trans_aipvm.CanDelete(argParts[1]);
                }
                return false;
            });

        public ICommand ResetOrDeletePresetCommand => new MpCommand<object>(
            (presetVmArg) => {
                if (ResetPresetCommand.CanExecute(presetVmArg)) {
                    ResetPresetCommand.Execute(presetVmArg);
                } else {
                    DeletePresetCommand.Execute(presetVmArg);
                }
            }, (presetVmArg) => {
                return presetVmArg is MpAvClipboardFormatPresetViewModel;
            });

        public ICommand ShiftPresetCommand => new MpCommand<object>(
            // [0] = shift dir [1] = presetvm
            (args) => {
                if (args is object[] argParts &&
                    argParts.Length == 2 &&
                    argParts[0] is int new_idx &&
                    argParts[1] is MpAvClipboardFormatPresetViewModel pvm) {

                    new_idx = Math.Max(0, Math.Min(Items.Count - 1, new_idx));

                    int curSortIdx = Items.IndexOf(pvm);
                    Items.Move(curSortIdx, new_idx);
                    for (int i = 0; i < Items.Count; i++) {
                        Items[i].SortOrderIdx = i;
                    }
                }
            });


        public ICommand CreateNewPresetCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;
                var def_icon = await MpDataModelProvider.GetItemAsync<MpIcon>(HandledFormatIconId);

                var np_icon = await def_icon.CloneDbModelAsync();

                MpPluginPreset newPreset = await MpPluginPreset.CreateOrUpdateAsync(
                        pluginGuid: FormatGuid,
                        sortOrderIdx: Items.Count,
                        iconId: np_icon.Id,
                        label: GetUniquePresetName());

                var npvm = await CreatePresetViewModelAsync(newPreset);
                Items.Add(npvm);
                Items.ForEach(x => x.IsSelected = x == npvm);

                OnPropertyChanged(nameof(Items));

                IsBusy = false;
            });
        public ICommand DuplicatePresetCommand => new MpAsyncCommand<object>(
            async (args) => {
                IsBusy = true;

                var aipvm = args as MpAvClipboardFormatPresetViewModel;
                if (aipvm == null) {
                    throw new Exception("DuplicatedPresetCommand must have preset as argument");
                }
                var p_to_clone = aipvm.Preset;
                p_to_clone.Label += " - Clone";
                p_to_clone.SortOrderIdx = Items.Count;
                p_to_clone.IsModelReadOnly = false;
                var dp = await aipvm.Preset.CloneDbModelAsync(
                    deepClone: true,
                    suppressWrite: false);

                var dpvm = await CreatePresetViewModelAsync(dp);
                Items.Add(dpvm);
                ShiftPresetCommand.Execute(new object[] { aipvm.SortOrderIdx + 1, dpvm });
                Items.ForEach(x => x.IsSelected = x == dpvm);
                OnPropertyChanged(nameof(Items));

                IsBusy = false;
            });
        #endregion
    }
}
