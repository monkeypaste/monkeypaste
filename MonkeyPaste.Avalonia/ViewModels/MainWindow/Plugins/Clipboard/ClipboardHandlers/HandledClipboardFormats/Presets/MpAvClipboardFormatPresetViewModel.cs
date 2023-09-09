using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipboardFormatPresetViewModel :
        MpAvSelectorViewModelBase<MpAvHandledClipboardFormatViewModel, MpAvParameterViewModelBase>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIUserIconViewModel,
        MpILabelText,
        MpITreeItemViewModel,
        MpAvIParameterCollectionViewModel {

        #region Interfaces

        #region MpISelectableViewModel Implementation
        public bool IsSelected { get; set; }

        #endregion

        #region MpILabelText Implementation
        string MpILabelText.LabelText => Label;

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpITreeItemViewModel Implementation

        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem => Parent;
        public IEnumerable<MpITreeItemViewModel> Children => Items;

        #endregion

        #region MpIPluginComponentViewModel Implementation
        public MpParameterHostBaseFormat ComponentFormat => ClipboardFormat;

        #endregion

        #region MpAvIParameterCollectionViewModel Implementation

        IEnumerable<MpAvParameterViewModelBase> MpAvIParameterCollectionViewModel.Items =>
            VisibleItems.ToList();

        MpAvParameterViewModelBase
            MpAvIParameterCollectionViewModel.SelectedItem {
            get => SelectedItem;
            set => SelectedItem = value;
        }

        #region MpISaveOrCancelableViewModel Implementation

        public ICommand SaveCommand => new MpCommand(
            () => {
                Items.ForEach(x => x.SaveCurrentValueCommand.Execute(null));
            },
            () => {
                return CanSaveOrCancel;
            }, new[] { this });
        public ICommand CancelCommand => new MpCommand(
            () => {
                Items.ForEach(x => x.RestoreLastValueCommand.Execute(null));
            },
            () => {
                return CanSaveOrCancel;
            }, new[] { this });

        private bool _canSaveOrCancel = false;
        public bool CanSaveOrCancel {
            get {
                bool result = Items.Any(x => x.HasModelChanged);
                if (result != _canSaveOrCancel) {
                    _canSaveOrCancel = result;
                    OnPropertyChanged(nameof(CanSaveOrCancel));
                }
                return _canSaveOrCancel;
            }
        }

        bool MpISaveOrCancelableViewModel.IsSaveCancelEnabled =>
            true;
        #endregion
        #endregion

        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvParameterViewModelBase> VisibleItems => Items.Where(x => x.IsVisible);
        #endregion

        #region Appearance

        public string ResetOrDeleteLabel => $"{(IsManifestPreset ? "Reset" : "Delete")} '{Label}'";

        #endregion

        #region State

        public bool IsManifestPreset =>
            Parent == null ?
                false :
                Parent.ClipboardPluginFormat.presets != null &&
                    Parent.ClipboardPluginFormat.presets.Any(x => x.guid == PresetGuid);
        public bool IsDropItemHovering { get; set; } = false;
        public bool IsLabelTextBoxFocused { get; set; } = false;
        public bool IsLabelReadOnly { get; set; } = true;

        public bool IsAllValid => Items.All(x => x.IsValid);

        public bool IsReader => Parent == null ? false : Parent.IsReader;
        public bool IsWriter => Parent == null ? false : Parent.IsWriter;

        public bool IsFormatOnSourceDragObject {
            get {
                if (MpAvContentWebViewDragHelper.SourceDataObject == null) {
                    return false;
                }
                bool format_exists =
                    MpAvContentWebViewDragHelper.SourceDataObject.ContainsData(ClipboardFormat.formatName);
                return format_exists;
            }
        }

        public bool IsFormatPlaceholderOnTargetDragObject {
            get {
                if (MpAvContentWebViewDragHelper.DragDataObject == null) {
                    return false;
                }
                return MpAvContentWebViewDragHelper.DragDataObject.ContainsPlaceholderFormat(ClipboardFormat.formatName);
            }
        }

        #endregion

        #region Model

        #region Db
        public string FullName {
            get {
                if (Preset == null || Parent == null) {
                    return string.Empty;
                }
                return $"{Parent.Title} - {Label}";
            }
        }

        public bool IsDefault {
            get {
                if (Preset == null) {
                    return false;
                }
                return Preset.IsDefault;
            }
            set {
                if (IsDefault != value) {
                    Preset.IsDefault = value;
                    OnPropertyChanged(nameof(IsDefault));
                }
            }
        }

        public bool IsEnabled {
            get {
                if (Preset == null) {
                    return false;
                }
                return Preset.IsEnabled;
            }
            set {
                if (IsEnabled != value) {
                    Preset.IsEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        public string Label {
            get {
                if (Preset == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(Preset.Label)) {
                    return Preset.Label;
                }
                return Preset.Label;
            }
            set {
                if (Preset.Label != value) {
                    Preset.Label = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public string Description {
            get {
                if (Preset == null) {
                    return null;
                }
                if (string.IsNullOrEmpty(Preset.Description)) {
                    return null;
                }
                return Preset.Description;
            }
            set {
                if (Description != value) {
                    Preset.Description = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public int SortOrderIdx {
            get {
                if (Preset == null) {
                    return 0;
                }
                return Preset.SortOrderIdx;
            }
            set {
                if (Preset != null && SortOrderIdx != value) {
                    Preset.SortOrderIdx = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SortOrderIdx));
                }
            }
        }

        public bool IsQuickAction {
            get {
                if (Preset == null) {
                    return true;
                }
                return Preset.IsQuickAction;
            }
            set {
                if (IsQuickAction != value) {
                    Preset.IsQuickAction = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsQuickAction));
                }
            }
        }

        public int IconId {
            get {
                if (Preset == null) {
                    return 0;
                }
                return Preset.IconId;
            }
            set {
                if (IconId != value) {
                    Preset.IconId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IconId));
                }
            }
        }

        public string PluginGuid {
            get {
                if (Preset == null) {
                    return string.Empty;
                }
                return Preset.PluginGuid;
            }
        }
        public string PresetGuid {
            get {
                if (Preset == null) {
                    return string.Empty;
                }
                return Preset.Guid;
            }
        }
        public int PresetId {
            get {
                if (Preset == null) {
                    return 0;
                }
                return Preset.Id;
            }
        }

        public DateTime LastSelectedDateTime {
            get {
                if (Preset == null) {
                    return DateTime.MinValue;
                }
                return Preset.LastSelectedDateTime;
            }
            set {
                if (LastSelectedDateTime != value) {
                    Preset.LastSelectedDateTime = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(LastSelectedDateTime));
                }
            }
        }

        public MpPluginPreset Preset { get; set; }

        #endregion

        #region Plugin

        public MpClipboardHandlerFormat ClipboardFormat {
            get {
                if (Parent == null) {
                    return null;
                }
                return Parent.ClipboardPluginFormat;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Constructors

        public MpAvClipboardFormatPresetViewModel(MpAvHandledClipboardFormatViewModel parent) : base(parent) {
            PropertyChanged += MpClipboardFormatPresetViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpPluginPreset aip) {
            IsBusy = true;

            Items.Clear();

            Preset = aip;

            //var presetValues = await PrepareParameterValueModelsAsync();

            var presetValues = await MpAvPluginParameterValueLocator.LocateValuesAsync(MpParameterHostType.Preset, PresetId, Parent);

            foreach (var paramVal in presetValues) {
                var naipvm = await CreateParameterViewModelAsync(paramVal);
                Items.Add(naipvm);
            }


            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));
            Items.ForEach(x => x.Validate());
            HasModelChanged = false;

            IsBusy = false;
        }
        public async Task<MpAvParameterViewModelBase> CreateParameterViewModelAsync(MpParameterValue aipv) {
            var naipvm = await MpAvPluginParameterBuilder.CreateParameterViewModelAsync(aipv, Parent);
            naipvm.OnValidate += ParameterViewModel_OnValidate;

            return naipvm;
        }

        public string GetPresetParamJson() {
            return MpJsonConverter.SerializeObject(Items.Select(x => new[] { x.ParamId, x.CurrentValue }).ToList());
        }
        #endregion

        #region Protected Methods

        protected virtual void ParameterViewModel_OnValidate(object sender, EventArgs e) {
            var aipvm = sender as MpAvParameterViewModelBase;
            //var aipvm = sender as MpAvParameterViewModelBase;
            //if (aipvm.IsRequired && string.IsNullOrWhiteSpace(aipvm.CurrentValue)) {
            //    aipvm.ValidationMessage = $"{aipvm.Label} is required";
            //} else {
            //    aipvm.ValidationMessage = string.Empty;
            //}
            aipvm.ValidationMessage = aipvm.GetValidationMessage(false);
            Parent.ValidateParameters();
        }

        #endregion

        #region Private Methods

        private void MpClipboardFormatPresetViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsEnabled): {
                        // this msg is used by dnd helper to update current drag dataobject if dnd in progress
                        MpMessenger.SendGlobal(MpMessageType.ClipboardPresetsChanged);
                    }
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged && IsAllValid) {
                        Task.Run(async () => {
                            await Preset.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
            }
        }
        #endregion

        #region Commands

        public MpIAsyncCommand<object> TogglePresetIsEnabledCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args == null) {
                    IsEnabled = !IsEnabled;
                    return;
                }
                bool cur_def_enabled = IsEnabled;
                MpAvAppViewModel avm = args as MpAvAppViewModel;

                if (avm == null &&
                    args is MpPortableProcessInfo pi) {
                    avm = await MpAvAppCollectionViewModel.Instance.AddOrGetAppByProcessInfoAsync(pi);
                }
                MpDebug.Assert(avm != null, $"Error toggling preset for arg '{args}'");

                //if (IsReader && avm.OleFormatInfos.IsReaderDefault) {
                //    // initial custom preset, before toggling create snapshot of defaults to use for toggling
                //    await Task.WhenAll(
                //        MpAvClipboardHandlerCollectionViewModel.Instance.EnabledReaders
                //        .Select(x => avm.OleFormatInfos.AddAppOlePresetViewModelByPresetIdAsync(x.PresetId)));
                //} else if (IsWriter && avm.OleFormatInfos.IsWriterDefault) {
                //    // initial custom preset, before toggling create snapshot of defaults to use for toggling
                //    await Task.WhenAll(
                //        MpAvClipboardHandlerCollectionViewModel.Instance.EnabledWriters
                //        .Select(x => avm.OleFormatInfos.AddAppOlePresetViewModelByPresetIdAsync(x.PresetId)));
                //}


                if (avm.OleFormatInfos.GetAppOleFormatInfoByPresetId(PresetId) is MpAvAppOlePresetViewModel aofivm) {
                    // format exists, remove
                    await avm.OleFormatInfos.RemoveAppOlePresetViewModelByPresetIdAsync(PresetId);
                } else {
                    await avm.OleFormatInfos.AddAppOlePresetViewModelByPresetIdAsync(PresetId);
                }

            });


        #endregion
    }
}
