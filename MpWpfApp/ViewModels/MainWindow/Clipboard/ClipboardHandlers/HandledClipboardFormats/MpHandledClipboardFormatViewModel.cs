using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpHandledClipboardFormatViewModel :
        MpSelectorViewModelBase<MpClipboardHandlerItemViewModel,MpClipboardFormatPresetViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpITreeItemViewModel {
        #region Private Variables


        #endregion

        #region Properties

        #region View Models

        public MpClipboardFormatPresetViewModel DefaultPresetViewModel => Items.FirstOrDefault(x => x.IsDefault);


        public MpITreeItemViewModel ParentTreeItem => Parent;

        public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #region Appearance

        public string ManageLabel => $"{Title} Preset Manager";

        public Brush ItemBackgroundBrush {
            get {
                if (IsSelected) {
                    return Brushes.DimGray;
                }
                if (IsHovering) {
                    return Brushes.LightGray;
                }
                return Brushes.Transparent;
            }
        }

        public Brush ItemTitleForegroundBrush {
            get {
                if (IsSelected) {
                    return Brushes.White;
                }
                if (IsHovering) {
                    return Brushes.Black;
                }
                return Brushes.White;
            }
        }


        #endregion

        #region State

        public virtual bool IsLoaded => Items.Count > 0 && Items[0].Items.Count > 0;

        //public bool IsAnyEditingParameters => Items.Any(x => x.IsEditingParameters);

        public bool IsHovering { get; set; } = false;


        public bool IsExpanded { get; set; } = false;

        public MpClipboardFormatType HandledFormat {
            get {
                if(ClipboardPluginFormat == null) {
                    return MpClipboardFormatType.None;
                }
                MpClipboardFormatType cft = ClipboardPluginFormat.clipboardName.ToEnum<MpClipboardFormatType>();
                if(cft == default) {
                    return MpClipboardFormatType.Custom;
                }
                return cft;
            }
        }
        #endregion

        #region Models

        

        #region MpAnalyticItem

        public int IconId { get; private set; }

        public string Title {
            get {
                if (ClipboardPluginFormat == null) {
                    return string.Empty;
                }
                return ClipboardPluginFormat.displayName;
            }
        }

        public string Description {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.description;
            }
        }

        public string ClipboardHandlerGuid => ClipboardPluginFormat == null ? string.Empty : ClipboardPluginFormat.handlerGuid;


        #region  Plugin

        public MpPluginFormat PluginFormat { get; set; }

        public MpClipboardHandlerFormat ClipboardPluginFormat { get; private set; }

        public MpIClipboardPluginComponent ClipboardPluginComponent => PluginFormat == null ? null : PluginFormat.Component as MpIClipboardPluginComponent;

        #endregion

        #endregion

        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors

        public MpHandledClipboardFormatViewModel() : base(null) { }

        public MpHandledClipboardFormatViewModel(MpClipboardHandlerItemViewModel parent) : base(parent) {
            PropertyChanged += MpHandledClipboardFormatViewModel_PropertyChanged;
            Items.CollectionChanged += PresetViewModels_CollectionChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpPluginFormat analyzerPlugin, int handlerIdx) {
            if (IsLoaded) {
                return;
            }
            IsBusy = true;

            PluginFormat = analyzerPlugin;

            ClipboardPluginFormat = PluginFormat.clipboardHandler.handledFormats[handlerIdx];
            if (ClipboardPluginComponent == null) {
                throw new Exception("Cannot find component");
            }


            var presets = await MpDataModelProvider.GetAnalyticItemPresetsByAnalyzerGuid(ClipboardHandlerGuid);

            if (string.IsNullOrEmpty(PluginFormat.iconUrl)) {
                IconId = MpPreferences.ThisAppIcon.Id;
            } else if (presets.Count > 0 &&
                presets.FirstOrDefault(x => x.IsDefault) != default &&
                      presets.FirstOrDefault(x => x.IsDefault).IconId > 0) {
                IconId = presets.FirstOrDefault(x => x.IsDefault).IconId;
            } else {
                var bytes = await MpFileIo.ReadBytesFromUriAsync(PluginFormat.iconUrl);
                var icon = await MpIcon.Create(
                    iconImgBase64: bytes.ToBase64String(),
                    createBorder: false);
                IconId = icon.Id;
            }

            bool isNew = presets.Count == 0;
            bool isManifestModified = presets.Any(x => x.ManifestLastModifiedDateTime < PluginFormat.manifestLastModifiedDateTime);
            bool needsReset = isNew || isManifestModified;
            if (needsReset) {
                MpNotificationCollectionViewModel.Instance.ShowUserAction(
                    MpNotificationDialogType.AnalyzerUpdated,
                    MpNotificationExceptionSeverityType.Warning,
                    $"Analyzer '{Title}' Updated",
                    "Reseting presets to default...",
                    3000).FireAndForgetSafeAsync(this);

                presets = await ResetPresets(presets);
                isNew = true;
            }

            presets.ForEach(x => x.ClipboardFormat = ClipboardPluginFormat);

            Items.Clear();

            foreach (var preset in presets.OrderBy(x => x.SortOrderIdx)) {
                var naipvm = await CreatePresetViewModel(preset);
                Items.Add(naipvm);
            }

            OnPropertyChanged(nameof(IconId));
            OnPropertyChanged(nameof(Items));

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            IsBusy = false;
        }

        public async Task<MpClipboardFormatPresetViewModel> CreatePresetViewModel(MpAnalyticItemPreset aip) {
            MpClipboardFormatPresetViewModel naipvm = new MpClipboardFormatPresetViewModel(this);
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

        public virtual async Task<MpPluginParameterFormat> DeferredCreateParameterModel(MpPluginParameterFormat aip) {
            //used to load remote content and called from CreateParameterViewModel in preset
            await Task.Delay(1);
            return aip;
        }

        public virtual bool Validate() {
            if (SelectedItem == null) {
                return true;
            }
            return SelectedItem.IsAllValid;
        }

        public bool IsDataObjectValid(MpPortableDataObject pdo) {
            if(pdo.DataFormatLookup.ContainsKey(HandledFormat)) {
                if(HandledFormat == MpClipboardFormatType.Custom) {
                    return pdo.GetCustomData(ClipboardPluginFormat.clipboardName) != null;
                }
                return true;
            }
            return false;
        }

        #endregion

        #region Protected Methods

        #region Db Event Handlers

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpAnalyticItemPreset aip) {

            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpAnalyticItemPreset aip) {
                if (aip.AnalyzerPluginGuid == ClipboardHandlerGuid) {
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

        private async Task<MpAnalyticItemPreset> CreateDefaultPresetModel(int existingDefaultPresetId = 0) {
            if (ClipboardPluginFormat.parameters == null) {
                throw new Exception($"Parameters for '{Title}' not found");
            }

            var aip = await MpAnalyticItemPreset.Create(
                                analyzerPluginGuid: ClipboardHandlerGuid,
                                isDefault: true,
                                label: $"{Title} - Default",
                                iconId: IconId,
                                sortOrderIdx: existingDefaultPresetId == 0 ? 0 : Items.FirstOrDefault(x => x.IsDefault).SortOrderIdx,
                                description: $"Auto-generated default preset for '{Title}'",
                                format: ClipboardPluginFormat,
                                manifestLastModifiedDateTime: PluginFormat.manifestLastModifiedDateTime,
                                existingDefaultPresetId: existingDefaultPresetId);

            //var paramPresetValues = new List<MpAnalyzerPresetValueFormat>();
            ////derive default preset & preset values from parameter formats
            //if (AnalyzerPluginFormat.parameters != null) {
            //    foreach (var param in AnalyzerPluginFormat.parameters) {
            //        string defVal = string.Join(",", param.values.Where(x=>x.isDefault).Select(x=>x.value));
            //        var presetVal = await MpAnalyticItemPresetParameterValue.Create(presetId: aip.Id, paramEnumId: param.paramId, value: defVal);

            //        paramPresetValues.Add(presetVal);
            //    }
            //}

            return aip;
        }


        private async void PresetViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            await UpdatePresetSortOrder();
        }

        private void MpHandledClipboardFormatViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;

                        if (SelectedItem == null) {
                            SelectedItem = Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
                        }
                        CollectionViewSource.GetDefaultView(Items).Refresh();
                        CollectionViewSource.GetDefaultView(SelectedItem.Items).Refresh();
                        //Items.ForEach(x => x.IsEditingParameters = false);
                        //SelectedIt em.IsEditingParameters = true;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
                    Parent.OnPropertyChanged(nameof(Parent.SelectedItem));

                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.IsAnySelected));
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.SelectedItem));
                    OnPropertyChanged(nameof(ItemBackgroundBrush));
                    OnPropertyChanged(nameof(ItemTitleForegroundBrush));

                    break;
                case nameof(IsHovering):
                    OnPropertyChanged(nameof(ItemBackgroundBrush));`
                    OnPropertyChanged(nameof(ItemTitleForegroundBrush));
                    break;
                case nameof(SelectedItem):
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.SelectedPresetViewModel));
                    break;
            }
        }

        private async Task UpdatePresetSortOrder(bool fromModel = false) {
            if (fromModel) {
                Items.Sort(x => x.SortOrderIdx);
            } else {
                foreach (var aipvm in Items) {
                    aipvm.SortOrderIdx = Items.IndexOf(aipvm);
                }
                if (!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    foreach (var pvm in Items) {
                        await pvm.Preset.WriteToDatabaseAsync();
                    }
                }
            }
        }

        private async Task<List<MpAnalyticItemPreset>> ResetPresets(List<MpAnalyticItemPreset> presets = null) {
            //if manifest has been modified
            //(for now clear all presets and either load predefined presets or create from parameter default values)

            // TODO maybe less forceably handle add/remove/update of presets when manifest changes
            presets = presets == null ? await MpDataModelProvider.GetAnalyticItemPresetsByAnalyzerGuid(ClipboardHandlerGuid) : presets;
            foreach (var preset in presets) {
                var vals = await MpDataModelProvider.GetAnalyticItemPresetValuesByPresetId(preset.Id);
                await Task.WhenAll(vals.Select(x => x.DeleteFromDatabaseAsync()));
            }
            await Task.WhenAll(presets.Select(x => x.DeleteFromDatabaseAsync()));

            presets.Clear();
            if (ClipboardPluginFormat.presets.IsNullOrEmpty()) {
                //only generate default preset if no presets defined in manifest
                var defualtPreset = await CreateDefaultPresetModel();
                presets.Add(defualtPreset);
            } else {
                foreach (var preset in ClipboardPluginFormat.presets) {
                    var aip = await MpAnalyticItemPreset.Create(
                        analyzerPluginGuid: ClipboardHandlerGuid,
                        isDefault: preset.isDefault,
                        label: preset.label,
                        iconId: IconId,
                        sortOrderIdx: ClipboardPluginFormat.presets.IndexOf(preset),
                        description: preset.description,
                        format: ClipboardPluginFormat,
                        manifestLastModifiedDateTime: PluginFormat.manifestLastModifiedDateTime);
                    presets.Add(aip);
                }
                if (presets.All(x => x.IsDefault == false) && presets.Count > 0) {
                    presets[0].IsDefault = true;
                }
            }
            return presets;
        }

        #endregion

        #region Commands

        public ICommand CreateNewPresetCommand => new RelayCommand(
            async () => {
                IsBusy = true;

                MpAnalyticItemPreset newPreset = await MpAnalyticItemPreset.Create(
                        analyzerPluginGuid: ClipboardHandlerGuid,
                        format: ClipboardPluginFormat,
                        iconId: IconId,
                        label: GetUniquePresetName());

                var npvm = await CreatePresetViewModel(newPreset);
                Items.Add(npvm);
                Items.ForEach(x => x.IsSelected = x == npvm);

                OnPropertyChanged(nameof(Items));

                IsBusy = false;
            });

        public ICommand SelectPresetCommand => new RelayCommand<MpClipboardFormatPresetViewModel>(
             (selectedPresetVm) => {
                 //if(!IsLoaded) {
                 //    await LoadChildren();
                 //}
                 if (!IsSelected) {
                     Parent.SelectedItem = this;
                 }
                 SelectedItem = selectedPresetVm;
             });

        public ICommand ManageAnalyticItemCommand => new RelayCommand(
             () => {
                 if (!IsSelected) {
                     Parent.SelectedItem = this;
                 }
                 if (SelectedItem == null && Items.Count > 0) {
                     SelectedItem = Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
                 }
                 if (!Parent.Parent.IsSidebarVisible) {
                     Parent.Parent.IsSidebarVisible = true;
                 }
                 OnPropertyChanged(nameof(SelectedItem));

             });

        public ICommand DeletePresetCommand => new RelayCommand<MpClipboardFormatPresetViewModel>(
            async (presetVm) => {
                if (presetVm.IsDefault) {
                    return;
                }
                IsBusy = true;

                foreach (var presetVal in presetVm.Items) {
                    await presetVal.PresetValue.DeleteFromDatabaseAsync();
                }
                await presetVm.Preset.DeleteFromDatabaseAsync();

                IsBusy = false;
            });

        public MpIAsyncCommand ResetDefaultPresetCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;

                var defvm = Items.FirstOrDefault(x => x.IsDefault);
                if (defvm == null) {
                    throw new Exception("Analyzer is supposed to have a default preset");
                }

                var defaultPresetModel = await CreateDefaultPresetModel(defvm.AnalyticItemPresetId);

                await defvm.InitializeAsync(defaultPresetModel);

                Items.ForEach(x => x.IsSelected = x.AnalyticItemPresetId == defvm.AnalyticItemPresetId);
                OnPropertyChanged(nameof(SelectedItem));

                IsBusy = false;
            });

        public ICommand ShiftPresetCommand => new RelayCommand<object>(
            // [0] = shift dir [1] = presetvm
            async (args) => {
                var argParts = args as object[];
                int dir = (int)Convert.ToInt32(argParts[0].ToString());
                MpClipboardFormatPresetViewModel pvm = argParts[1] as MpClipboardFormatPresetViewModel;
                int curSortIdx = Items.IndexOf(pvm);
                int newSortIdx = curSortIdx + dir;

                Items.Move(curSortIdx, newSortIdx);
                for (int i = 0; i < Items.Count; i++) {
                    Items[i].SortOrderIdx = i;
                    await Items[i].Preset.WriteToDatabaseAsync();
                }
            },
            (args) => {
                if (args == null) {
                    return false;
                }
                if (args is object[] argParts) {
                    int dir = (int)Convert.ToInt32(argParts[0].ToString());
                    MpClipboardFormatPresetViewModel pvm = argParts[1] as MpClipboardFormatPresetViewModel;
                    int curSortIdx = Items.IndexOf(pvm);
                    int newSortIdx = curSortIdx + dir;
                    if (newSortIdx < 0 || newSortIdx >= Items.Count || newSortIdx == curSortIdx) {
                        return false;
                    }
                    return true;
                }
                return false;
            });


        public MpIAsyncCommand<object> DuplicatePresetCommand => new MpAsyncCommand<object>(
            async (args) => {
                IsBusy = true;

                var aipvm = args as MpClipboardFormatPresetViewModel;
                if (aipvm == null) {
                    throw new Exception("DuplicatedPresetCommand must have preset as argument");
                }
                var dp = await aipvm.Preset.CloneDbModel();
                var dpvm = await CreatePresetViewModel(dp);
                Items.Add(dpvm);
                Items.ForEach(x => x.IsSelected = x == dpvm);
                OnPropertyChanged(nameof(Items));

                IsBusy = false;
            });
        #endregion
    }
}
