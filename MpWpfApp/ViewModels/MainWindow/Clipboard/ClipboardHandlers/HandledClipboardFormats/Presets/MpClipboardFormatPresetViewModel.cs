using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpClipboardFormatPresetViewModel : 
        MpSelectorViewModelBase<MpHandledClipboardFormatViewModel,MpAnalyticItemParameterViewModelBase>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpISidebarItemViewModel,
        MpIUserIconViewModel,
        MpITreeItemViewModel {

        #region Properties

        #region MpISelectableViewModel Implementation
        public bool IsSelected { get; set; }

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISidebarItemViewModel Implemntation

        public double SidebarWidth { get; set; } = MpMeasurements.Instance.DefaultAnalyzerPresetPanelWidth;
        public double DefaultSidebarWidth => MpMeasurements.Instance.DefaultAnalyzerPresetPanelWidth;
        public bool IsSidebarVisible { get; set; } = false;
        public MpISidebarItemViewModel NextSidebarItem => null;
        public MpISidebarItemViewModel PreviousSidebarItem => MpClipboardHandlerCollectionViewModel.Instance;

        #endregion

        #region MpITreeItemViewModel Implementation

        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem { get; }
        public ObservableCollection<MpITreeItemViewModel> Children { get; }

        #endregion

        #region State

        public bool IsAllValid => Items.All(x => x.IsValid);
        
        #endregion

        #region Model
        public string FullName {
            get {
                if (Preset == null || Parent == null) {
                    return string.Empty;
                }
                return $"{Parent.Title}/{Label}";
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
                if(IsDefault != value) {
                    Preset.IsDefault = value;
                    OnPropertyChanged(nameof(IsDefault));
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

        public string AnalyzerPluginGuid {
            get {
                if (Preset == null) {
                    return string.Empty;
                }
                return Preset.AnalyzerPluginGuid;
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

        public MpAnalyticItemPreset Preset { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpClipboardFormatPresetViewModel(MpHandledClipboardFormatViewModel parent) : base(parent) {
            PropertyChanged += MpClipboardFormatPresetViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpAnalyticItemPreset aip) {
            IsBusy = true;

            Items.Clear();

            Preset = aip;//await MpDb.GetItemAsync<MpAnalyticItemPreset>(aip.Id);

            var presetValues = await MpDataModelProvider.GetAnalyticItemPresetValuesByPresetId(PresetId);
            foreach (var paramFormat in Preset.ClipboardFormat.parameters) {
                if (!presetValues.Any(x => x.ParamId == paramFormat.paramId)) {
                    string paramVal = string.Empty;
                    if (paramFormat.values != null && paramFormat.values.Count > 0) {
                        if (paramFormat.values.Any(x => x.isDefault)) {
                            paramVal = paramFormat.values.Where(x => x.isDefault).Select(x => x.value).ToList().ToCsv();
                        } else {
                            paramVal = paramFormat.values[0].value;
                        }
                    }
                    var newPresetVal = await MpAnalyticItemPresetParameterValue.Create(
                        presetId: Preset.Id,
                        paramEnumId: paramFormat.paramId,
                        value: paramVal,
                        format: paramFormat);

                    presetValues.Add(newPresetVal);
                }
            }
            presetValues.ForEach(x => x.ParameterFormat = Preset.ClipboardFormat.parameters.FirstOrDefault(y => y.paramId == x.ParamId));

            foreach (var paramVal in presetValues) {
                var naipvm = await CreateParameterViewModel(paramVal);
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
        public async Task<MpAnalyticItemParameterViewModelBase> CreateParameterViewModel(MpAnalyticItemPresetParameterValue aipv) {
            MpAnalyticItemParameterViewModelBase naipvm = null;

            switch (aipv.ParameterFormat.controlType) {
                case MpPluginParameterControlType.List:
                case MpPluginParameterControlType.MultiSelectList:
                case MpPluginParameterControlType.EditableList:
                case MpPluginParameterControlType.ComboBox:
                    naipvm = new MpEnumerableParameterViewModel(this);
                    break;
                case MpPluginParameterControlType.PasswordBox:
                case MpPluginParameterControlType.TextBox:
                    naipvm = new MpTextBoxParameterViewModel(this);
                    break;
                case MpPluginParameterControlType.CheckBox:
                    naipvm = new MpCheckBoxParameterViewModel(this);
                    break;
                case MpPluginParameterControlType.Slider:
                    naipvm = new MpSliderParameterViewModel(this);
                    break;
                case MpPluginParameterControlType.DirectoryChooser:
                case MpPluginParameterControlType.FileChooser:
                    naipvm = new MpFileChooserParameterViewModel(this);
                    break;
                default:
                    throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpPluginParameterControlType), aipv.ParameterFormat.controlType));
            }
            naipvm.OnValidate += ParameterViewModel_OnValidate;


            await naipvm.InitializeAsync(aipv);

            return naipvm;
        }

        #endregion

        #region Protected Methods

        protected virtual void ParameterViewModel_OnValidate(object sender, EventArgs e) {
            var aipvm = sender as MpAnalyticItemParameterViewModelBase;
            if (aipvm.IsRequired && string.IsNullOrWhiteSpace(aipvm.CurrentValue)) {
                aipvm.ValidationMessage = $"{aipvm.Label} is required";
            } else {
                aipvm.ValidationMessage = string.Empty;
            }
            Parent.Validate();
        }

        #endregion

        #region Private Methods

        private void MpClipboardFormatPresetViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsDefault):
                    Parent.Parent.Parent.ToggleFormatPresetIsDefaultCommand.Execute(this);
                    break;
            }
        }

        #endregion
    }
}
