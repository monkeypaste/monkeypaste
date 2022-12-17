using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvPluginPresetCollectionViewModelBase<P,C> :
        MpAvTreeSelectorViewModelBase<P,C>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIAsyncComboBoxItemViewModel,
        MpITreeItemViewModel,
        MpIMenuItemViewModel
        where P : class, MpITreeItemViewModel
        where C : 
            MpViewModelBase, 
            MpITreeItemViewModel, 
            MpISelectableViewModel, 
            MpAvIPluginParameterCollectionViewModel,
            MpISortableItemViewModel {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region MpITreeItemViewModel Implementation

        MpITreeItemViewModel MpITreeItemViewModel.ParentTreeItem => Parent;
        IEnumerable<MpITreeItemViewModel> MpITreeItemViewModel.Children => Items;

        #endregion

        #region MpIMenuItemViewModel Implementation

        public abstract MpMenuItemViewModel ContextMenuItemViewModel { get; }

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; } = false;

        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; } = false;

        #endregion

        #region MpIAsyncComboBoxItemViewModel Implementation
        int MpIComboBoxItemViewModel.IconId => PluginIconId;
        string MpIComboBoxItemViewModel.Label => Title;

        #endregion


        #region Properties

        #region ViewModels 

        #endregion

        #region State

        public virtual bool IsLoaded => Items.Count > 0 && Items[0].Items.Count() > 0;
        #endregion

        #region Model

        #region Db
        public int PluginIconId { get; set; }

        #endregion

        #region PluginFormat
        public string PluginGuid => PluginFormat == null ? string.Empty : PluginFormat.guid;

        public MpPluginFormat PluginFormat { get; private set; }
        public virtual string Title {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.title;
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

        #region PluginComponent

        public abstract MpPluginComponentBaseFormat PluginComponentFormat { get; }

        public MpIPluginComponentBase PluginComponent => PluginFormat == null ? null : PluginFormat.Component as MpIPluginComponentBase;
        #endregion

        #endregion


        #endregion

        #endregion

        #region Constructors
        public MpAvPluginPresetCollectionViewModelBase() : this(null) { }

        public MpAvPluginPresetCollectionViewModelBase(P parent) : base(parent) {
            PropertyChanged += MpAvPluginPresetCollectionViewModelBase_PropertyChanged;
        }
        #endregion

        #region Public Methods
        public virtual async Task InitializeComponentAsync(MpPluginComponentBaseFormat pluginComponentFormat) {
            if (!ValidatePluginComponentFormat(pluginComponentFormat)) {
                return;
            }
            if (IsLoaded) {
                return;
            }
            IsBusy = true;

            if (PluginComponentFormat == null) {
                throw new Exception("Cannot find component");
            }

            PluginIconId = await GetOrCreateIconIdAsync();

            while (MpAvIconCollectionViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            Items.Clear();

            var presets = await PreparePresetModelsAsync();
            foreach (var preset in presets) {
                var naipvm = await CreatePresetViewModelAsync(preset);
                Items.Add(naipvm);
            }

            OnPropertyChanged(nameof(MpAvSelectorViewModelBase<MpAvAnalyticItemCollectionViewModel, MpAvAnalyticItemPresetViewModel>.Items));

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }


            IsBusy = false;
        }
        #endregion

        #region Protected Methods
        protected abstract bool ValidatePluginComponentFormat(MpPluginComponentBaseFormat componentFormat);
        protected abstract Task<C> CreatePresetViewModelAsync(MpPluginPreset aip);
        protected string GetUniquePresetName() {
            int uniqueIdx = 1;
            string uniqueName = $"Preset";
            string testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);

            while (Items.Cast<MpILabelTextViewModel>().Any(x => x.LabelText.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);
            }
            return uniqueName + uniqueIdx;
        }


        #endregion

        #region Private Methods


        private void MpAvPluginPresetCollectionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            throw new NotImplementedException();
        }
        private async Task<MpPluginPreset> CreateDefaultPresetModelAsync(int existingDefaultPresetId = 0) {
            if (PluginComponentFormat.parameters == null) {
                throw new Exception($"Parameters for '{Title}' not found");
            }

            var aip = await MpPluginPreset.CreateAsync(
                                pluginGuid: PluginFormat.guid,
                                isDefault: true,
                                label: $"{Title} - Default",
                                iconId: PluginIconId,
                                sortOrderIdx: existingDefaultPresetId == 0 ? 0 : Items.Cast<MpISortableItemViewModel>().FirstOrDefault(x => x.IsDefault).SortOrderIdx,
                                description: $"Auto-generated default preset for '{Title}'",
                                //format: AnalyzerPluginFormat,
                                manifestLastModifiedDateTime: PluginFormat.manifestLastModifiedDateTime,
                                existingDefaultPresetId: existingDefaultPresetId);

            return aip;
        }

        private async Task<int> GetOrCreateIconIdAsync() {
            var bytes = await MpFileIo.ReadBytesFromUriAsync(PluginFormat.iconUri, PluginFormat.RootDirectory); ;
            var icon = await MpPlatformWrapper.Services.IconBuilder.CreateAsync(
                iconBase64: bytes.ToBase64String(),
                createBorder: false);

            return icon.Id;
        }

        private async Task<IEnumerable<MpPluginPreset>> PreparePresetModelsAsync() {
            var presets = await MpDataModelProvider.GetPluginPresetsByPluginGuidAsync(PluginFormat.guid);

            bool isNew = presets.Count == 0;
            bool isManifestModified = presets.Any(x => x.ManifestLastModifiedDateTime < PluginFormat.manifestLastModifiedDateTime);
            bool needsReset = isNew || isManifestModified;
            if (needsReset) {
                var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == PluginIconId);

                MpNotificationBuilder.ShowMessageAsync(
                    msgType: MpNotificationType.PluginUpdated,
                    title: $"Analyzer '{Title}' Updated",
                    iconSourceStr: ivm.IconBase64,
                    body: "Reseting presets to default...")
                    .FireAndForgetSafeAsync(this);

                presets = await ResetPresetsAsync(presets);
                isNew = true;
            }

            //presets.ForEach(x => x.ComponentFormat = AnalyzerPluginFormat);
            return presets.OrderBy(x => x.SortOrderIdx);
        }

        private async Task<List<MpPluginPreset>> ResetPresetsAsync(List<MpPluginPreset> presets = null) {
            //if manifest has been modified
            //(for now clear all presets and either load predefined presets or create from parameter default values)

            // TODO maybe less forceably handle add/remove/update of presets when manifest changes
            presets = presets == null ? await MpDataModelProvider.GetPluginPresetsByPluginGuidAsync(PluginFormat.guid) : presets;
            foreach (var preset in presets) {
                var vals = await MpDataModelProvider.GetPluginPresetValuesByPresetIdAsync(preset.Id);
                await Task.WhenAll(vals.Select(x => x.DeleteFromDatabaseAsync()));
            }
            await Task.WhenAll(presets.Select(x => x.DeleteFromDatabaseAsync()));

            presets.Clear();
            if (PluginComponentFormat.presets.IsNullOrEmpty()) {
                //only generate default preset if no presets defined in manifest
                var defualtPreset = await CreateDefaultPresetModelAsync();
                presets.Add(defualtPreset);
            } else {
                //when presets are defined in manifest create the preset and its values in the db
                foreach (var presetFormat in PluginComponentFormat.presets) {
                    var presetModel = await MpPluginPreset.CreateAsync(
                        pluginGuid: PluginFormat.guid,
                        isDefault: presetFormat.isDefault,
                        label: presetFormat.label,
                        iconId: PluginIconId,
                        sortOrderIdx: PluginComponentFormat.presets.IndexOf(presetFormat),
                        description: presetFormat.description,
                        //format: AnalyzerPluginFormat,
                        manifestLastModifiedDateTime: PluginFormat.manifestLastModifiedDateTime);

                    foreach (var presetValueModel in presetFormat.values) {
                        // only creat preset values in db, they will then be picked up when the preset vm is initialized
                        var aipv = await MpPluginPresetParameterValue.CreateAsync(
                            presetId: presetModel.Id,
                            paramId: presetValueModel.paramId,
                            value: presetValueModel.value
                            //format: AnalyzerPluginFormat.parameters.FirstOrDefault(x => x.paramName == presetValueModel.paramName)
                            );
                    }

                    presets.Add(presetModel);
                }
                if (presets.All(x => x.IsDefault == false) && presets.Count > 0) {
                    presets[0].IsDefault = true;
                }
            }
            return presets;
        }

        #endregion

        #region Commands
        #endregion
    }

    //public abstract class MpAvPluginPresetItemViewModelBase<P,C> : MpViewModelBase<MpAv {
    //}
}
