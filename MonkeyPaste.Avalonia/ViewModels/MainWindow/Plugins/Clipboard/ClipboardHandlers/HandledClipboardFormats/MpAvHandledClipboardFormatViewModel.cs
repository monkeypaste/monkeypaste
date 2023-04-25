using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public class MpAvHandledClipboardFormatViewModel :
        MpAvTreeSelectorViewModelBase<MpAvClipboardHandlerItemViewModel, MpAvClipboardFormatPresetViewModel>,
        MpISelectableViewModel,
        MpIParameterHostViewModel,
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

        int MpIParameterHostViewModel.IconId => HandledFormatIconId;
        public string PluginGuid => ClipboardHandlerGuid;

        MpParameterHostBaseFormat MpIParameterHostViewModel.ComponentFormat => ClipboardPluginFormat;

        MpParameterHostBaseFormat MpIParameterHostViewModel.BackupComponentFormat =>
            PluginFormat == null ||
            PluginFormat.backupCheckPluginFormat == null ||
            PluginFormat.backupCheckPluginFormat.clipboardHandler == null ||
            (IsReader && PluginFormat.backupCheckPluginFormat.clipboardHandler.readers == null) ||
            (IsWriter && PluginFormat.backupCheckPluginFormat.clipboardHandler.writers == null) ?
                null :
                IsReader ?
                    PluginFormat.backupCheckPluginFormat.clipboardHandler.readers.FirstOrDefault(x => x.handlerGuid == ClipboardHandlerGuid) :
                    PluginFormat.backupCheckPluginFormat.clipboardHandler.writers.FirstOrDefault(x => x.handlerGuid == ClipboardHandlerGuid);
        public MpIPluginComponentBase PluginComponent => ClipboardPluginComponent;

        public string ClipboardHandlerGuid { get; private set; }

        public MpClipboardHandlerFormat ClipboardPluginFormat {
            get {
                if (PluginFormat == null) {
                    return null;
                }
                if (IsReader) {
                    return Parent.ClipboardPluginFormat.readers.FirstOrDefault(x => x.handlerGuid == ClipboardHandlerGuid);
                }
                if (IsWriter) {
                    return Parent.ClipboardPluginFormat.writers.FirstOrDefault(x => x.handlerGuid == ClipboardHandlerGuid);
                }
                MpConsole.WriteTraceLine($"Error finding ClipboardHandler format for handlerGuid: '{ClipboardHandlerGuid}'");
                return null;
            }
        }

        public MpIClipboardPluginComponent ClipboardPluginComponent =>
            PluginFormat == null ? null : PluginFormat.Component as MpIClipboardPluginComponent;

        public bool IsReader => PluginFormat == null ?
                                    false :
                                    PluginFormat.clipboardHandler.readers.Any(x => x.handlerGuid == ClipboardHandlerGuid); //ClipboardPluginComponent is MpIClipboardReaderComponent;

        public bool IsWriter => PluginFormat == null ?
                                    false :
                                    PluginFormat.clipboardHandler.writers.Any(x => x.handlerGuid == ClipboardHandlerGuid); //ClipboardPluginComponent is MpIClipboardReaderComponent;


        public MpPluginFormat PluginFormat {
            get {
                if (Parent == null) {
                    return null;
                }
                return Parent.PluginFormat;
            }
        }
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

        public virtual bool IsLoaded => Items.Count > 0 && Items[0].Items.Count > 0;

        public bool IsValid { get; private set; }
        //public bool IsAnyEditingParameters => Items.Any(x => x.IsEditingParameters);

        public bool IsHovering { get; set; } = false;

        public string HandledFormat {
            get {
                if (ClipboardPluginFormat == null) {
                    return null;
                }
                return ClipboardPluginFormat.clipboardName;
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
                return PluginFormat.guid == MpPrefViewModel.Instance.CoreClipboardHandlerGuid;
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
            $"{Title} ({(IsReader ? "Reader" : "Writer")})";

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
            IsValid = ValidateClipboardHandlerFormat(handlerFormat);
            if (!IsValid) {
                return;
            }
            if (IsLoaded) {
                return;
            }
            IsBusy = true;

            ClipboardHandlerGuid = handlerFormat.handlerGuid;

            if (IsReader && IsWriter) {
                Debugger.Break();
            }

            if (ClipboardPluginComponent == null) {
                throw new Exception("Cannot find component");
            }

            HandledFormatIconId = await MpAvPluginIconLocator.LocatePluginIconIdAsync(this, ClipboardPluginFormat.iconUri);
            var presets = await MpAvPluginPresetLocator.LocatePresetsAsync(this, IsCoreHandler);


            Items.Clear();

            foreach (var preset in presets) {
                var naipvm = await CreatePresetViewModelAsync(preset);
                Items.Add(naipvm);
            }

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
            string uniqueName = $"Preset";
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
                if (aip.PluginGuid == ClipboardHandlerGuid) {
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
                            SelectedItem = Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
                        }
                        //CollectionViewSource.GetDefaultView(Items).Refresh();
                        //CollectionViewSource.GetDefaultView(SelectedItem.Items).Refresh();
                        OnPropertyChanged(nameof(Items));
                        SelectedItem.OnPropertyChanged(nameof(SelectedItem.Items));

                        //Items.ForEach(x => x.IsEditingParameters = false);
                        //SelectedIt em.IsEditingParameters = true;

                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
                    //Parent.OnPropertyChanged(nameof(Parent.SelectedItem));

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
        private bool ValidateClipboardHandlerFormat(MpClipboardHandlerFormat chf) {
            bool isValid = true;
            var sb = new StringBuilder();

            if (string.IsNullOrEmpty(chf.iconUri) || !Uri.IsWellFormedUriString(chf.iconUri, UriKind.RelativeOrAbsolute)) {
                sb.AppendLine($"Plugin {PluginFormat.title} has malformed icon uri '{chf.iconUri}', plugin must have valid icon");
                isValid = false;
            }

            return isValid;
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
                 if (SelectedItem == null && Items.Count > 0) {
                     SelectedItem = Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
                 }
                 //if (!Parent.Parent.IsSidebarVisible) {
                 //    Parent.Parent.IsSidebarVisible = true;
                 //}
                 MpAvSidebarItemCollectionViewModel.Instance.SelectSidebarItemCommand.Execute(Parent.Parent);
                 OnPropertyChanged(nameof(SelectedItem));

             });

        public ICommand DeletePresetCommand => new MpAsyncCommand<MpAvClipboardFormatPresetViewModel>(
            async (presetVm) => {
                if (presetVm.IsDefault) {
                    return;
                }
                IsBusy = true;

                foreach (var presetVal in presetVm.Items) {
                    await presetVal.PresetValueModel.DeleteFromDatabaseAsync();
                }
                await presetVm.Preset.DeleteFromDatabaseAsync();

                IsBusy = false;
            });

        public ICommand ResetDefaultPresetCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;

                var defvm = Items.FirstOrDefault(x => x.IsDefault);
                if (defvm == null) {
                    throw new Exception("Analyzer is supposed to have a default preset");
                }

                // recreate default preset record (name, icon, etc.)
                var defaultPresetModel = await MpAvPluginPresetLocator.CreateOrResetManifestPresetModelAsync(
                    this, defvm.PresetGuid, Items.IndexOf(defvm));

                // store IsEnabled to current state
                bool wasEnabled = defvm.IsEnabled;

                // before initializing preset remove current values from db or it won't reset values
                await Task.WhenAll(defvm.Items.Select(x => x.PresetValueModel.DeleteFromDatabaseAsync()));

                await defvm.InitializeAsync(defaultPresetModel);

                Items.ForEach(x => x.IsSelected = x.PresetId == defvm.PresetId);
                OnPropertyChanged(nameof(SelectedItem));

                if (wasEnabled) {
                    MpAvClipboardHandlerCollectionViewModel.Instance.ToggleFormatPresetIsEnabled.Execute(defvm);
                }

                IsBusy = false;
            });


        public ICommand ResetOrDeletePresetCommand => new MpCommand<object>(
            (presetVmArg) => {
                //if (ResetPresetCommand.CanExecute(presetVmArg)) {
                //    ResetPresetCommand.Execute(presetVmArg);
                //} else {
                //    DeletePresetCommand.Execute(presetVmArg);
                //}
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

                MpPluginPreset newPreset = await MpPluginPreset.CreateOrUpdateAsync(
                        pluginGuid: ClipboardHandlerGuid,
                        //format: ClipboardPluginFormat,
                        iconId: HandledFormatIconId,
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
