using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        IEnumerable<MpAvParameterViewModelBase>
            MpAvIParameterCollectionViewModel.Items => VisibleItems;

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
        public string DropItemTitleHexColor {
            get {
                if (!IsEnabled) {
                    return MpSystemColors.Red;
                }
                return MpSystemColors.limegreen;
            }
        }

        public string DropItemBorderHexColor {
            get {
                if (IsDropItemHovering) {
                    return MpSystemColors.Yellow;
                }
                return MpSystemColors.white;
            }
        }
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
            //MpParameterControlType controlType = ClipboardFormat.parameters.FirstOrDefault(x => x.paramId == aipv.ParamId).controlType;
            //MpAvParameterViewModelBase naipvm = null;

            //switch (controlType) {
            //    case MpParameterControlType.List:
            //    case MpParameterControlType.MultiSelectList:
            //    case MpParameterControlType.EditableList:
            //    case MpParameterControlType.ComboBox:
            //        naipvm = new MpAvEnumerableParameterViewModel(this);
            //        break;
            //    case MpParameterControlType.PasswordBox:
            //    case MpParameterControlType.TextBox:
            //        naipvm = new MpAvTextBoxParameterViewModel(this);
            //        break;
            //    case MpParameterControlType.CheckBox:
            //        naipvm = new MpAvCheckBoxParameterViewModel(this);
            //        break;
            //    case MpParameterControlType.Slider:
            //        naipvm = new MpAvSliderParameterViewModel(this);
            //        break;
            //    case MpParameterControlType.DirectoryChooser:
            //    case MpParameterControlType.FileChooser:
            //        naipvm = new MpAvFileChooserParameterViewModel(this);
            //        break;
            //    default:
            //        throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpParameterControlType), controlType));
            //}
            //naipvm.OnValidate += ParameterViewModel_OnValidate;


            //await naipvm.InitializeAsync(aipv);
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
            if (aipvm.IsRequired && string.IsNullOrWhiteSpace(aipvm.CurrentValue)) {
                aipvm.ValidationMessage = $"{aipvm.Label} is required";
            } else {
                aipvm.ValidationMessage = string.Empty;
            }
            Parent.ValidateParameters();
        }

        #endregion

        #region Private Methods

        private void MpClipboardFormatPresetViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsEnabled):
                    // NOTE Preset should only be able to Read or Write NOT both
                    if (IsReader && IsWriter) {
                        Debugger.Break();
                    }
                    // BUG commented out these root commands, they cause problems and are hard to test w/o 
                    // having the clipboard sidebar in the view so..
                    // NOTE put these commands back and test when clipboard sidebar is back
                    if (IsReader) {
                        //Parent.Parent.Parent.ToggleFormatPresetIsReadEnabledCommand.Execute(this);
                    }
                    if (IsWriter) {
                        //Parent.Parent.Parent.ToggleFormatPresetIsWriteEnabledCommand.Execute(this);
                    }
                    OnPropertyChanged(nameof(DropItemBorderHexColor));
                    OnPropertyChanged(nameof(DropItemTitleHexColor));

                    // this msg is used by dnd helper to update current drag dataobject if dnd in process
                    MpMessenger.SendGlobal(MpMessageType.ClipboardPresetsChanged);
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged && IsAllValid) {
                        Task.Run(async () => {
                            await Preset.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;

                case nameof(IsDropItemHovering):
                    //if (IsDropItemHovering) {
                    //    // on mouse enter toggle current preset enabled/disabled
                    //    IsEnabled = !IsEnabled;
                    //}
                    break;
            }
        }

        //private async Task<IEnumerable<MpParameterValue>> PrepareParameterValueModelsAsync() {
        //    // get all preset values from db
        //    var presetValues = await MpDataModelProvider.GetPluginPresetValuesByPresetIdAsync(PresetId);

        //    //var cbh = Parent.Parent.PluginFormat.clipboardHandler;
        //    //int base_param_idx = 0;
        //    //if(IsReader) {
        //    //    int handler_idx = cbh.readers.IndexOf(Parent.ClipboardPluginFormat);
        //    //    base_param_idx = cbh.readers.Where(x => cbh.readers.IndexOf(x) < handler_idx).SelectMany(x => x.parameters).Count();
        //    //} else {
        //    //    base_param_idx = cbh.readers.Count;
        //    //    int handler_idx = cbh.writers.IndexOf(Parent.ClipboardPluginFormat);                
        //    //    base_param_idx += cbh.writers.Where(x => cbh.writers.IndexOf(x) < handler_idx).SelectMany(x => x.parameters).Count();
        //    //}
        //    // loop through plugin formats parameters and add or replace (if found in db) to the preset values
        //    foreach (var paramFormat in ClipboardFormat.parameters) {
        //        if (!presetValues.Any(x => x.ParamId == paramFormat.paramId)) {
        //            // if no value is found in db for a parameter defined in manifest...

        //            string paramVal = string.Empty;
        //            if (paramFormat.values != null && paramFormat.values.Count > 0) { 
        //                // if parameter has a predefined value (a case when not would be a text box that needs input so its value is empty)

        //                if (paramFormat.values.Any(x => x.isDefault)) {
        //                    // when manifest identifies a value as default choose that for value

        //                    paramVal = paramFormat.values.Where(x => x.isDefault).Select(x => x.value).ToList().ToCsv();
        //                } else {
        //                    // if no default is defined use first available value
        //                    paramVal = paramFormat.values[0].value;
        //                }
        //            }

        //            var newPresetVal = await MpParameterValue.CreateAsync(
        //                presetId: Preset.Id,
        //                paramId: paramFormat.paramId,
        //                value: paramVal
        //                //format: paramFormat
        //                );

        //            presetValues.Add(newPresetVal);
        //        }
        //    }
        //    //presetValues.ForEach(x => x.ParameterFormat = ClipboardFormat.parameters.FirstOrDefault(y => y.paramName == x.ParamName));

        //    return presetValues;
        //}

        #endregion

        #region Commands

        public ICommand TogglePresetIsEnabledCommand => new MpCommand(
            () => {
                IsEnabled = !IsEnabled;
            });
        #endregion
    }
}
