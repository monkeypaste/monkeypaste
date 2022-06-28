using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System.Diagnostics;
using FFImageLoading.Forms.Handlers;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.IO;

namespace MpWpfApp {
    public class MpHandledClipboardFormatViewModel :
        MpSelectorViewModelBase<MpClipboardHandlerItemViewModel,MpClipboardFormatPresetViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpITreeItemViewModel {
        #region Private Variables

        private static string _defaultHandlerPluginGuid = "a7df5078-8c85-4819-9518-dbf389612298";
        #endregion

        #region Properties

        #region View Models

        public MpITreeItemViewModel ParentTreeItem => Parent;

        public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


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
                return Application.Current.Resources["AppIcon"] as string;

                //switch (HandledFormat) {
                //    case MpClipboardFormatType.Bitmap:
                //        return Application.Current.Resources["ImageIcon"] as string;
                //    case MpClipboardFormatType.Html:
                //        return Application.Current.Resources["HtmlIcon"] as string;
                //    case MpClipboardFormatType.Csv:
                //        return Application.Current.Resources["CsvIcon"] as string;
                //    case MpClipboardFormatType.FileDrop:
                //        return Application.Current.Resources["CsvIcon"] as string;
                //}
            }
        }
        #endregion

        #region State

        public virtual bool IsLoaded => Items.Count > 0 && Items[0].Items.Count > 0;

        //public bool IsAnyEditingParameters => Items.Any(x => x.IsEditingParameters);

        public bool IsHovering { get; set; } = false;


        public bool IsExpanded { get; set; } = false;

        public string HandledFormat {
            get {
                if(ClipboardPluginFormat == null) {
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


        public string ClipboardHandlerGuid { get; private set; }

        //public MpClipboardHandlerFormat ClipboardPluginFormat { get; private set; }
        public MpClipboardHandlerFormat ClipboardPluginFormat {
            get {
                if(PluginFormat == null) {
                    return null;
                }
                if(IsReader) {
                    return Parent.ClipboardPluginFormat.readers.FirstOrDefault(x => x.handlerGuid == ClipboardHandlerGuid);
                }
                if (IsWriter) {
                    return Parent.ClipboardPluginFormat.writers.FirstOrDefault(x => x.handlerGuid == ClipboardHandlerGuid);
                }
                MpConsole.WriteTraceLine($"Error finding ClipboardHandler format for handlerGuid: '{ClipboardHandlerGuid}'");
                return null;
            }
        }

        public MpIClipboardPluginComponent ClipboardPluginComponent => PluginFormat == null ? null : PluginFormat.Component as MpIClipboardPluginComponent;

        public bool IsReader => PluginFormat == null ? 
                                    false :
                                    PluginFormat.clipboardHandler.readers.Any(x=>x.handlerGuid == ClipboardHandlerGuid); //ClipboardPluginComponent is MpIClipboardReaderComponent;

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

        //public async Task InitializeAsync(MpPluginFormat plugin, int handlerIdx, bool isReader) {
        public async Task InitializeAsync(MpPluginFormat plugin, MpClipboardHandlerFormat handlerFormat) {
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

            HandledFormatIconId = await GetOrCreateIconIdAsync();


            var presets = await PreparePresetModelsAsync();

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

        public async Task<MpClipboardFormatPresetViewModel> CreatePresetViewModelAsync(MpPluginPreset aip) {
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
            //if(pdo.DataFormatLookup.ContainsKey(HandledFormat)) {
            //    if(HandledFormat == MpClipboardFormatType.Custom) {
            //        return pdo.GetCustomData(ClipboardPluginFormat.clipboardName) != null;
            //    }
            //    return true;
            //}
            //return false;
            return pdo.ContainsData(HandledFormat);
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

        private async Task<MpPluginPreset> CreateDefaultPresetModelAsync(int existingDefaultPresetId = 0) {
            if (ClipboardPluginFormat.parameters == null) {
                throw new Exception($"Parameters for '{Title}' not found");
            }

            var aip = await MpPluginPreset.Create(
                                pluginGuid: ClipboardHandlerGuid,
                                isDefault: true,
                                label: $"{Title} - Default [{(IsReader ? "Reader":"Writer")}]",
                                iconId: HandledFormatIconId,
                                sortOrderIdx: existingDefaultPresetId == 0 ? 0 : Items.FirstOrDefault(x => x.IsDefault).SortOrderIdx,
                                description: $"Auto-generated default preset for '{Title}'",
                                format: ClipboardPluginFormat,
                                manifestLastModifiedDateTime: PluginFormat.manifestLastModifiedDateTime,
                                existingDefaultPresetId: existingDefaultPresetId);
            return aip;
        }


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
                        CollectionViewSource.GetDefaultView(Items).Refresh();
                        CollectionViewSource.GetDefaultView(SelectedItem.Items).Refresh();
                        //Items.ForEach(x => x.IsEditingParameters = false);
                        //SelectedIt em.IsEditingParameters = true;
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
            }
        }

        private async Task UpdatePresetSortOrderAsync(bool fromModel = false) {
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

        private async Task<int> GetOrCreateIconIdAsync() {
            var bytes = await MpFileIo.ReadBytesFromUriAsync(ClipboardPluginFormat.iconUri, PluginFormat.RootDirectory);
            var icon = await MpIcon.Create(
                iconImgBase64: bytes.ToBase64String(),
                createBorder: false);
            return icon.Id;
        }

        private async Task<IEnumerable<MpPluginPreset>> PreparePresetModelsAsync() {
            var presets = await MpDataModelProvider.GetPluginPresetsByPluginGuidAsync(ClipboardHandlerGuid);                        

            bool isNew = presets.Count == 0;
            bool isManifestModified = presets.Any(x => x.ManifestLastModifiedDateTime < PluginFormat.manifestLastModifiedDateTime);
            bool needsReset = isNew || isManifestModified;
            if (needsReset) {
                while (MpIconCollectionViewModel.Instance.IsAnyBusy) {
                    // if this is first load of the plugin the icon may not be added to icon collection yet so wait for it
                    await Task.Delay(100);
                }
                var ivm = MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == HandledFormatIconId);
                MpNotificationCollectionViewModel.Instance.ShowMessage(
                    msgType: MpNotificationDialogType.PluginUpdated,
                    title: $"Clipboard Handler '{Title}' Updated",
                    iconBase64Str: ivm == null ? null : ivm.IconBase64,                    
                    msg: "Reseting presets to default...")
                    .FireAndForgetSafeAsync(this);

                presets = await ResetPresetsAsync(presets);
            }

            presets.ForEach(x => x.ComponentFormat = ClipboardPluginFormat);

            if(isNew && PluginFormat.guid == _defaultHandlerPluginGuid) {
                // this is supposed to handle initial startup for CoreClipboard handler when no formats are enabled 
                // but there's many cases that this may not be initial startup so:
                // TODO instead of doing should notify clipboard collection that default was reset and only enable formats
                // that don't have another handler with that format enabled
                presets.ForEach(x => x.IsEnabled = true);

                await Task.WhenAll(presets.Select(x => x.WriteToDatabaseAsync()));

                MessageBox.Show("All CoreClipboard formats have been set to enabled");
            }

            return presets.OrderBy(x=>x.SortOrderIdx);
        }

        private async Task<List<MpPluginPreset>> ResetPresetsAsync(List<MpPluginPreset> presets = null) {
            //if manifest has been modified
            //(for now clear all presets and either load predefined presets or create from parameter default values)

            // TODO maybe less forceably handle add/remove/update of presets when manifest changes
            presets = presets == null ? await MpDataModelProvider.GetPluginPresetsByPluginGuidAsync(ClipboardHandlerGuid) : presets;
            foreach (var preset in presets) {
                var vals = await MpDataModelProvider.GetPluginPresetValuesByPresetIdAsync(preset.Id);
                await Task.WhenAll(vals.Select(x => x.DeleteFromDatabaseAsync()));
            }
            await Task.WhenAll(presets.Select(x => x.DeleteFromDatabaseAsync()));

            presets.Clear();
            if (ClipboardPluginFormat.presets.IsNullOrEmpty()) {
                //only generate default preset if no presets defined in manifest
                var defualtPreset = await CreateDefaultPresetModelAsync();
                presets.Add(defualtPreset);
            } else {
                foreach (var preset in ClipboardPluginFormat.presets) {
                    var aip = await MpPluginPreset.Create(
                        pluginGuid: ClipboardHandlerGuid,
                        isDefault: preset.isDefault,
                        label: preset.label,
                        iconId: HandledFormatIconId,
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

        public ICommand CreateNewPresetCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;

                MpPluginPreset newPreset = await MpPluginPreset.Create(
                        pluginGuid: ClipboardHandlerGuid,
                        format: ClipboardPluginFormat,
                        iconId: HandledFormatIconId,
                        label: GetUniquePresetName());

                var npvm = await CreatePresetViewModelAsync(newPreset);
                Items.Add(npvm);
                Items.ForEach(x => x.IsSelected = x == npvm);

                OnPropertyChanged(nameof(Items));

                IsBusy = false;
            });

        public ICommand SelectPresetCommand => new MpCommand<MpClipboardFormatPresetViewModel>(
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
                 if (!Parent.Parent.IsSidebarVisible) {
                     Parent.Parent.IsSidebarVisible = true;
                 }
                 OnPropertyChanged(nameof(SelectedItem));

             });

        public ICommand DeletePresetCommand => new MpAsyncCommand<MpClipboardFormatPresetViewModel>(
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

                var defaultPresetModel = await CreateDefaultPresetModelAsync(defvm.PresetId);

                await defvm.InitializeAsync(defaultPresetModel);

                Items.ForEach(x => x.IsSelected = x.PresetId == defvm.PresetId);
                OnPropertyChanged(nameof(SelectedItem));

                IsBusy = false;
            });

        public ICommand ShiftPresetCommand => new MpAsyncCommand<object>(
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
                var dpvm = await CreatePresetViewModelAsync(dp);
                Items.Add(dpvm);
                Items.ForEach(x => x.IsSelected = x == dpvm);
                OnPropertyChanged(nameof(Items));

                IsBusy = false;
            });
        #endregion
    }
}
