using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
//using Newtonsoft.Json;
//using SQLite;

namespace MonkeyPaste.Avalonia {
    public class MpAvParameterViewModelBase :
        MpViewModelBase<MpViewModelBase>,
        MpITreeItemViewModel,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpITooltipInfoViewModel {
        #region Private Variables
        protected string _lastValue = string.Empty;
        #endregion

        #region Interfaces

        #region MpITreeItemViewModel Implementation

        public IEnumerable<MpITreeItemViewModel> Children => null;

        public MpITreeItemViewModel ParentTreeItem => Parent as MpITreeItemViewModel;
        public bool IsExpanded { get; set; }

        #endregion

        #region MpPluginRequestItemFormat Implementation

        //object MpIParameterKeyValuePair.paramId => ParamId;
        //string MpIParameterKeyValuePair.value => CurrentValue;


        #endregion

        #region MpITooltipInfoViewModel Implementation

        public object Tooltip => Description;

        #endregion

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; } = false;

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; } = false;

        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #endregion

        #region Properties

        #region View Models

        #endregion

        #region Business Logic

        public string ValidationMessage { get; set; } = string.Empty;

        #endregion

        #region Appearance


        public string ParameterTooltipText {
            get {
                if (!IsValid) {
                    return ValidationMessage;
                }
                if (ParameterFormat != null && !string.IsNullOrEmpty(ParameterFormat.description)) {
                    return ParameterFormat.description;
                }
                return null;
            }
        }

        #endregion

        #region State

        public bool IsRememberChecked { get; set; }
        public override bool HasModelChanged =>
            CurrentValue != _lastValue && CanSetModelValue();

        public bool HasDescription => !string.IsNullOrEmpty(Description);

        public bool IsValid => string.IsNullOrEmpty(ValidationMessage);

        public bool IsActionParameter { get; set; } = false;

        public object CurrentTypedValue {
            get {
                switch (UnitType) {
                    case MpParameterValueUnitType.Bool:
                        return BoolValue;
                    case MpParameterValueUnitType.ActionComponentId:
                    case MpParameterValueUnitType.CollectionComponentId:
                    case MpParameterValueUnitType.AnalyzerComponentId:
                    case MpParameterValueUnitType.ContentPropertyPathTypeComponentId:
                    case MpParameterValueUnitType.Integer:
                        return IntValue;
                    case MpParameterValueUnitType.Decimal:
                        return DoubleValue;
                    default:
                        return CurrentValue;
                }
            }
            set {
                if (CurrentTypedValue != value) {
                    CurrentValue = value == null ? null : value.ToString();
                    OnPropertyChanged(nameof(CurrentTypedValue));
                }
            }
        }

        public double DoubleValue {
            get {
                return CurrentValue.ParseOrConvertToDouble(0);
            }
            set {
                if (DoubleValue != value) {
                    CurrentValue = value.ToString();
                    OnPropertyChanged(nameof(DoubleValue));
                    OnPropertyChanged(nameof(CurrentValue));
                }
            }
        }

        public int IntValue {
            get {
                return CurrentValue.ParseOrConvertToInt(0);
            }
            set {
                if (IntValue != value) {
                    CurrentValue = value.ToString();
                    OnPropertyChanged(nameof(IntValue));
                    OnPropertyChanged(nameof(CurrentValue));
                }
            }
        }

        public bool BoolValue {
            get {
                return CurrentValue.ParseOrConvertToBool(false);
            }
            set {
                if (BoolValue != value) {
                    CurrentValue = value ? "True" : "False";
                    OnPropertyChanged(nameof(BoolValue));
                    OnPropertyChanged(nameof(CurrentValue));
                }
            }
        }

        #endregion

        #region Model

        #region Control Settings

        public bool IsConfirmRemember {
            get {
                if (ParameterFormat == null) {
                    return false;
                }
                return ParameterFormat.confirmToRemember;
            }
        }

        public bool IsVisible {
            get {
                if (ParameterFormat == null) {
                    return false;
                }
                return ParameterFormat.isVisible;
            }
        }

        public bool IsReadOnly {
            get {
                if (ParameterFormat == null) {
                    return false;
                }
                return ParameterFormat.isReadOnly;
            }
        }

        public bool IsRequired {
            get {
                if (ParameterFormat == null) {
                    return false;
                }
                return ParameterFormat.isRequired;
            }
        }
        //Text Box
        public int MinLength {
            get {
                if (ParameterFormat == null) {
                    return 0;
                }
                return ParameterFormat.minLength;
            }
        }

        public int MaxLength {
            get {
                if (ParameterFormat == null) {
                    return int.MaxValue;
                }
                return ParameterFormat.maxLength;
            }
        }


        public string Pattern {
            get {
                if (ParameterFormat == null) {
                    return null;
                }
                return ParameterFormat.pattern;
            }
        }
        public string PatternInfo {
            get {
                if (ParameterFormat == null) {
                    return null;
                }
                return ParameterFormat.patternInfo;
            }
        }
        //Slider
        public double Minimum {
            get {
                if (ParameterFormat == null) {
                    return double.MinValue;
                }
                return ParameterFormat.minimum;
            }
        }

        public double Maximum {
            get {
                if (ParameterFormat == null) {
                    return double.MaxValue;
                }
                return ParameterFormat.maximum;
            }
        }

        public int Precision {
            get {
                if (ParameterFormat == null) {
                    return 2;
                }
                return ParameterFormat.precision;
            }
        }

        public MpParameterValueUnitType UnitType {
            get {
                if (ParameterFormat == null) {
                    return MpParameterValueUnitType.None;
                }
                if (ParameterFormat.unitType == MpParameterValueUnitType.None) {
                    switch (ParameterFormat.controlType) {
                        case MpParameterControlType.CheckBox:
                            return MpParameterValueUnitType.Bool;
                        case MpParameterControlType.Slider:
                            return MpParameterValueUnitType.Decimal;
                        case MpParameterControlType.Button:
                        case MpParameterControlType.PasswordBox:
                        case MpParameterControlType.TextBox:
                        case MpParameterControlType.List:
                        case MpParameterControlType.ComboBox:
                            return MpParameterValueUnitType.PlainText;
                        case MpParameterControlType.MultiSelectList:
                        case MpParameterControlType.EditableList:
                            return MpParameterValueUnitType.DelimitedPlainText;
                        case MpParameterControlType.FileChooser:
                        case MpParameterControlType.DirectoryChooser:
                            return MpParameterValueUnitType.FileSystemPath;

                    }
                }
                return ParameterFormat.unitType;
            }
        }

        public List<string> DefaultValues {
            get {
                if (ParameterFormat == null || ParameterFormat.values == null) {
                    return new List<string>();
                }
                return ParameterFormat.values.Where(x => x.isDefault).Select(x => x.value).ToList();
            }
        }


        #endregion

        #region Db

        private string _currentValue = string.Empty;
        public virtual string CurrentValue {
            get {
                if (PresetValueModel == null) {
                    return string.Empty;
                }
                return _currentValue;
                //if (IsConfirmRemember && !IsRememberChecked) {
                //    return _currentValue;
                //}
                //return PresetValueModel.Value;
            }
            set {
                if (CurrentValue != value) {
                    _currentValue = value;
                    if (CanSetModelValue()) {
                        PresetValueModel.Value = _currentValue;
                    }
                    OnPropertyChanged(nameof(CurrentValue));
                }
            }
        }

        public int ParameterValueId {
            get {
                if (PresetValueModel == null) {
                    return 0;
                }
                return PresetValueModel.Id;
            }
        }

        public MpParameterValue PresetValueModel { get; set; }

        #endregion

        #region Plugin (MpPluginParameterFormat)

        public object ParamId {
            get {
                if (ParameterFormat == null) {
                    return null;
                }
                return ParameterFormat.paramId;
            }
        }

        public bool IsSharedValue {
            get {
                if (ParameterFormat == null) {
                    return false;
                }
                return ParameterFormat.isSharedValue;
            }
        }
        public bool IsExecuteParameter {
            get {
                if (ParameterFormat == null) {
                    return false;
                }
                return ParameterFormat.isExecuteParameter;
            }
        }

        public MpCsvFormatProperties CsvProps =>
            ParameterFormat == null ? null : ParameterFormat.CsvProps;

        public string Label {
            get {
                if (ParameterFormat == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(ParameterFormat.label)) {
                    return ParameterFormat.label;
                }
                return ParameterFormat.label;
            }
        }

        public MpParameterControlType ControlType {
            get {
                if (ParameterFormat == null) {
                    return MpParameterControlType.None;
                }
                return ParameterFormat.controlType;
            }
        }

        public string Description {
            get {
                if (ParameterFormat == null) {
                    return string.Empty;
                }
                return ParameterFormat.description;
            }
        }

        private MpParameterFormat _paramFormat;
        public MpParameterFormat ParameterFormat {
            get {
                if (Parent is MpIParameterHostViewModel phvm) {
                    return phvm.ComponentFormat.parameters.FirstOrDefault(x => x.paramId == PresetValueModel.ParamId);
                }
                return _paramFormat;

            }
            set {
                if (Parent is MpIParameterHostViewModel) {
                    throw new Exception("Error, param format should not be set for plugin parameters");
                }
                if (_paramFormat != value) {
                    _paramFormat = value;
                    OnPropertyChanged(nameof(ParameterFormat));
                }
            }
        }
        #endregion

        #endregion

        #endregion

        #region Events

        public event EventHandler OnValidate;

        #endregion

        #region Constructors

        public MpAvParameterViewModelBase() : this(null) { }

        //public MpAvParameterViewModelBase(MpIParameterHostViewModel parent) : this(parent as MpViewModelBase) { }
        public MpAvParameterViewModelBase(MpViewModelBase parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterViewModel_PropertyChanged;

        }

        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpParameterValue aipv) {
            bool wasBusy = IsBusy;
            IsBusy = true;

            PresetValueModel = aipv;
            _currentValue = PresetValueModel == null ? string.Empty : PresetValueModel.Value;
            if (IsConfirmRemember) {
                IsRememberChecked = !string.IsNullOrEmpty(CurrentValue);
            } else {
                IsRememberChecked = false;
            }

            SetLastValue(CurrentValue);

            OnPropertyChanged(nameof(CurrentValue));
            OnPropertyChanged(nameof(IsExecuteParameter));

            await Task.Delay(1);

            IsBusy = wasBusy;
        }

        public bool Validate() {
            bool was_valid = IsValid;
            OnValidate?.Invoke(this, new EventArgs());
            if (was_valid != IsValid &&
                Parent is MpAvAnalyticItemPresetViewModel aipvm) {
                aipvm.Parent.UpdateCanExecute();
            }
            return IsValid;
        }
        #endregion

        #region Protected Methods
        protected virtual void MpAnalyticItemParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasModelChanged):
                case nameof(CurrentValue):
                    MpISaveOrCancelableViewModel socvm = null;
                    if (Parent is MpISaveOrCancelableViewModel) {
                        // analyzers
                        socvm = Parent as MpISaveOrCancelableViewModel;
                    } else if (Parent is MpAvHandledClipboardFormatViewModel hcfvm) {
                        socvm = hcfvm.Items.FirstOrDefault(x => x.Items.Any(x => x.ParameterValueId == ParameterValueId));
                    }
                    if (socvm != null) {

                        socvm.OnPropertyChanged(nameof(socvm.CanSaveOrCancel));
                    }
                    Validate();
                    break;
                case nameof(ValidationMessage):
                    if (!string.IsNullOrEmpty(ValidationMessage)) {

                        MpConsole.WriteLine($"Validation Msg Changed to '{ValidationMessage}'");
                    }
                    OnPropertyChanged(nameof(IsValid));
                    break;
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                    }
                    break;
                case nameof(IsRememberChecked):
                    if (!IsRememberChecked) {
                        //
                        SetLastValue(string.Empty);
                    }
                    OnPropertyChanged(nameof(HasModelChanged));
                    break;
            }
            //Validate();
        }

        protected bool CanSetModelValue() {
            if (IsConfirmRemember) {
                if (IsRememberChecked) {
                    return true;
                }
                // when marked as not remember but model has value need to clear it for uncheck to persist
                return
                    PresetValueModel != null &&
                    !string.IsNullOrEmpty(PresetValueModel.Value);
            }
            return true;
        }

        protected virtual void SetLastValue(object value) {
            if (!CanSetModelValue()) {
                _lastValue = string.Empty;
                return;
            }
            _lastValue = value == null ? null : value.ToString();
        }
        protected virtual void RestoreLastValue() {
            if (!CanSetModelValue()) {
                return;
            }
            CurrentValue = _lastValue;
        }

        #region Db Event Handlers

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpParameterValue pv &&
                pv.Id == ParameterValueId &&
                pv.Value != CurrentValue) {
                if (IsSharedValue) {
                    // this param is persistent but not the instance that was updated
                    Dispatcher.UIThread.Post(async () => {
                        await InitializeAsync(pv);
                    });
                }
            }
        }
        #endregion

        #endregion

        #region Private Methods

        protected virtual void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            bool is_core_loaded = Mp.Services != null &&
                     Mp.Services.StartupState != null &&
                     Mp.Services.StartupState.IsCoreLoaded;
            if (!is_core_loaded) {
                return;
            }
            OnValidate?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Commands

        public ICommand RestoreLastValueCommand => new MpCommand(
            () => {
                RestoreLastValue();
            },
            () => {
                return HasModelChanged;
            });

        public ICommand SaveCurrentValueCommand => new MpCommand<object>(
            (args) => {
                SetLastValue(CurrentValue);
                OnPropertyChanged(nameof(HasModelChanged));
                if (args != null) {
                    // skip write (for pref's at least)
                    return;
                }
                Task.Run(async () => {
                    string value_to_write = CurrentValue;
                    if (IsConfirmRemember && !IsRememberChecked) {
                        // always clear unremember values
                        value_to_write = string.Empty;
                    }
                    //IsBusy = true;
                    if (IsSharedValue && Parent is MpIParameterHostViewModel phvm) {
                        // update all references to persistent param with this new value 
                        var all_param_refs =
                            await MpDataModelProvider.GetAllParameterValueInstancesForPluginAsync(ParamId.ToString(), phvm.PluginGuid);

                        all_param_refs.ForEach(x => x.Value = value_to_write);
                        await Task.WhenAll(all_param_refs.Select(x => x.WriteToDatabaseAsync()));
                    } else {
                        PresetValueModel.Value = value_to_write;
                        await PresetValueModel.WriteToDatabaseAsync();
                    }
                    return;
                });
            },
            (args) => {
                //return HasModelChanged;
                return true;
            });

        #endregion
    }
}
