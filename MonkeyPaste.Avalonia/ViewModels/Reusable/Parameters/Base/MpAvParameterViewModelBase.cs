using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
//using Newtonsoft.Json;
//using SQLite;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvParameterViewModelBase :
        MpAvViewModelBase<MpAvViewModelBase>,
        MpITreeItemViewModel,
        MpIFilterMatch,
        MpIAsyncCollectionObject,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIHighlightTextRangesInfoViewModel,
        MpAvIPulseViewModel {
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
        //string MpIParameterKeyValuePair.paramValue => CurrentValue;


        #endregion

        #region MpIHighlightTextRangesInfoViewModel Implementation
        public ObservableCollection<MpTextRange> HighlightRanges { get; set; } = [];
        public int ActiveHighlightIdx { get; set; } = -1;

        #endregion

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; } = false;

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; } = false;

        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region MpIFilterMatch Implementation

        bool MpIFilterMatch.IsFilterMatch(string filter) {
            if (this is not MpIHighlightTextRangesInfoViewModel { } htrivm) {
                return false;
            }
            htrivm.HighlightRanges.Clear();
            if (string.IsNullOrEmpty(filter)) {
                return true;
            }
            if (Label.QueryText(filter, false, false, false) is { } ranges &&
                ranges.Any()) {
                htrivm.HighlightRanges.AddRange(
                    ranges
                .Select(x => new MpTextRange(null, x.Item1, x.Item2)));
            }

            return
                htrivm.HighlightRanges.Any() ||
                Description.ToStringOrEmpty().ToLower().Contains(filter.ToStringOrEmpty().ToLower());

        }
        #endregion
        #endregion

        #region Properties

        #region View Models
        public MpISaveOrCancelableViewModel SaveOrCancelableViewModel {
            get {
                if (Parent is MpISaveOrCancelableViewModel socvm) {
                    return socvm;

                }
                if (Parent is not MpAvHandledClipboardFormatViewModel hcfvm) {
                    return null;
                }
                // find preset using this param val id
                return hcfvm.Items.FirstOrDefault(x => x.Items.Any(x => x.ParameterValueId == ParameterValueId));
            }
        }
        #endregion

        #region Business Logic
        private string _validationMessage = string.Empty;
        public string ValidationMessage {
            get => _validationMessage;
            set {
                if (ValidationMessage != value) {
                    _validationMessage = value;
                    OnPropertyChanged(nameof(ValidationMessage));
                }
            }
        }

        #endregion

        #region Appearance
        public string FullLabel {
            get {
                if (Parent is MpILabelTextViewModel ltvm) {
                    return $"{ltvm.LabelText}[{Label}]";
                }
                return Label;
            }
        }
        #endregion

        #region State
        public int TabIdx {
            get {
                if (Parent is MpIParameterHostViewModel phvm) {
                    return phvm.ComponentFormat.parameters.IndexOf(ParameterFormat);
                }
                return int.MaxValue;
            }
        }
        public virtual bool IsAnyBusy =>
            IsBusy;
        public bool DoFocusPulse { get; set; }

        public bool IsRememberChecked { get; set; }
        public override bool HasModelChanged =>
            CurrentValue != _lastValue && CanSetModelValue();

        public bool HasDescription => !string.IsNullOrEmpty(Description);

        public bool IsValid => string.IsNullOrEmpty(ValidationMessage);

        public bool IsActionParameter =>
            Parent is MpAvActionViewModelBase;
        public bool IsActionParameterAllowLastOutput {
            get {
                if (Parent is not MpAvActionViewModelBase avmb ||
                    !avmb.IsParentActionSupportLastOutput) {
                    return false;
                }
                return true;
            }
        }

        public object CurrentTypedValue {
            get {
                switch (UnitType) {
                    case MpParameterValueUnitType.Date:
                        return DateTimeValue;
                    case MpParameterValueUnitType.Time:
                        return TimeSpanValue;
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

        public TimeSpan TimeSpanValue {
            get {
                return CurrentValue.ParseOrConvertToTimeSpan(0);
            }
            set {
                if (TimeSpanValue != value) {
                    CurrentValue = value.ParseOrConvertToLong().ToString();
                    OnPropertyChanged(nameof(TimeSpanValue));
                    OnPropertyChanged(nameof(CurrentValue));
                }
            }
        }
        public DateTime DateTimeValue {
            get {
                return CurrentValue.ParseOrConvertToDateTime(0);
            }
            set {
                if (DateTimeValue != value) {
                    CurrentValue = value.ParseOrConvertToLong().ToString();
                    OnPropertyChanged(nameof(DateTimeValue));
                    OnPropertyChanged(nameof(CurrentValue));
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
                return ParameterFormat.canRemember;
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

        public decimal Increment {
            get {
                if(ParameterFormat == null) {
                    return 1;
                }
                return ParameterFormat.increment;
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
                        case MpParameterControlType.NumberTicker:
                            return MpParameterValueUnitType.Decimal;
                        case MpParameterControlType.Button:
                        case MpParameterControlType.PasswordBox:
                        case MpParameterControlType.TextBox:
                        case MpParameterControlType.Hyperlink:
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
                if (ParameterValue == null) {
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
                        ParameterValue.Value = _currentValue;
                    }
                    OnPropertyChanged(nameof(CurrentValue));
                }
            }
        }

        public int ParameterHostId {
            get {
                if (ParameterValue == null) {
                    return 0;
                }
                return ParameterValue.ParameterHostId;
            }
        }
        public int ParameterValueId {
            get {
                if (ParameterValue == null) {
                    return 0;
                }
                return ParameterValue.Id;
            }
        }

        public MpParameterValue ParameterValue { get; set; }

        #endregion

        #region Plugin (MpPluginParameterFormat)

        public string ParamId {
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

        private string _label;
        public string Label {
            get {
                if (_label != null) {
                    return _label;
                }
                if (ParameterFormat == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(ParameterFormat.label)) {
                    return ParameterFormat.label;
                }
                return ParameterFormat.label;
            }
            set {
                if (_label != value) {
                    _label = value;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }
        public bool IsEnabled { get; set; } = true;

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
                if (Parent is MpIParameterHostViewModel phvm && phvm.ComponentFormat != null && phvm.ComponentFormat.parameters != null) {
                    return phvm.ComponentFormat.parameters.FirstOrDefault(x => x.paramId == ParameterValue.ParamId);
                }
                return _paramFormat;

            }
            set {
                if (Parent is MpIParameterHostViewModel) {
                    MpDebug.Break("Error, param format should not be set for plugin parameters");
                    return;
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

        public event EventHandler OnParamValidate;
        public event EventHandler OnParamValueChanged;

        #endregion

        #region Constructors

        public MpAvParameterViewModelBase() : this(null) { }

        //public MpAvParameterViewModelBase(MpIParameterHostViewModel parent) : this(parent as MpViewModelBase) { }
        public MpAvParameterViewModelBase(MpAvViewModelBase parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterViewModel_PropertyChanged;

        }

        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpParameterValue aipv) {
            bool wasBusy = IsBusy;
            IsBusy = true;

            ParameterValue = aipv;
            _currentValue = ParameterValue == null ? string.Empty : ParameterValue.Value;
            if (IsConfirmRemember) {
                IsRememberChecked = !string.IsNullOrEmpty(CurrentValue);
            } else {
                IsRememberChecked = false;
            }

            SetLastValue(CurrentValue);

            OnPropertyChanged(nameof(CurrentValue));
            OnPropertyChanged(nameof(IsExecuteParameter));
            OnPropertyChanged(nameof(Description));

            OnParamValidate += MpAnalyticItemParameterViewModel_OnValidate;

            await Task.Delay(1);

            IsBusy = wasBusy;
        }

        public bool Validate() {
            bool was_valid = IsValid;
            OnParamValidate?.Invoke(this, new EventArgs());
            if (was_valid != IsValid &&
                Parent is MpAvAnalyticItemPresetViewModel aipvm) {
                aipvm.Parent.UpdateCanExecute();
            }
            OnPropertyChanged(nameof(IsValid));
            return IsValid;
        }
        public void ClearValidation() {
            ValidationMessage = string.Empty;
            OnPropertyChanged(nameof(IsValid));
        }

        public virtual string GetValidationMessage(bool isExecuting) {
            // NOTE isExecuting is true when analyzer executing or trigger is enabling
            if (IsRequired && string.IsNullOrWhiteSpace(CurrentValue)) {
                if ((isExecuting && IsExecuteParameter) || !IsExecuteParameter) {
                    return string.Format(UiStrings.ParameterInvalidText, Label);
                }
            }

            // TODO add pattern check here
            return string.Empty;
        }

        public async Task SaveParameterAsync() {
            string value_to_write = CurrentValue;
            if (IsConfirmRemember && !IsRememberChecked) {
                // always clear unremember values
                value_to_write = string.Empty;
            }
            //IsBusy = true;
            if (IsSharedValue && Parent is MpIParameterHostViewModel phvm) {
                // update all references to persistent param with this new paramValue 
                var all_param_refs =
                    await MpDataModelProvider.GetAllParameterValueInstancesForPluginAsync(ParamId.ToString(), phvm.PluginGuid);

                all_param_refs.ForEach(x => x.Value = value_to_write);
                await Task.WhenAll(all_param_refs.Select(x => x.WriteToDatabaseAsync()));
            } else {
                ParameterValue.Value = value_to_write;
                await ParameterValue.WriteToDatabaseAsync();
            }
        }
        #endregion

        #region Protected Methods
        protected virtual void MpAnalyticItemParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(Label):
                    OnPropertyChanged(nameof(FullLabel));
                    break;
                case nameof(HasModelChanged):
                case nameof(CurrentValue):
                    MpISaveOrCancelableViewModel socvm = null;
                    if (Parent is MpISaveOrCancelableViewModel) {
                        // analyzers
                        if (e.PropertyName == nameof(CurrentValue) &&
                            Parent is MpAvAnalyticItemPresetViewModel aipvm) {

                            if(aipvm.IsExecuting && IsExecuteParameter ||
                                (MpAvFocusManager.Instance.FocusElement is Control c && 
                                c.GetSelfOrAncestorDataContext<MpAvAnalyticItemPresetViewModel>() == aipvm)) {

                                // automatically save execute param changes WHILE executing 
                                // or user actively editing shared vals
                                SaveCurrentValueCommand.Execute(null);
                            }
                        } else {

                            socvm = Parent as MpISaveOrCancelableViewModel;
                        }

                    } else if (Parent is MpAvHandledClipboardFormatViewModel hcfvm) {
                        socvm = hcfvm.Items.FirstOrDefault(x => x.Items.Any(x => x.ParameterValueId == ParameterValueId));
                    }
                    if (socvm != null) {

                        socvm.OnPropertyChanged(nameof(socvm.CanSaveOrCancel));
                    }
                    if (e.PropertyName == nameof(CurrentValue)) {
                        OnParamValueChanged?.Invoke(this, EventArgs.Empty);
                    }
                    Validate();
                    break;
                case nameof(ValidationMessage):
                    //MpConsole.WriteLine($"'{Label}' Validation Msg Changed to '{ValidationMessage}'");
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
                case nameof(DoFocusPulse):
                    MpAvThemeViewModel.Instance.HandlePulse(this);
                    break;
            }
            //Validate();
        }

        protected virtual void MpAnalyticItemParameterViewModel_OnValidate(object sender, EventArgs e) {
            //if (IsValidationOverrideEnabled) {
            //    return;
            //}
            OnPropertyChanged(nameof(IsValid));
        }
        protected bool CanSetModelValue() {
            if (IsConfirmRemember) {
                if (IsRememberChecked) {
                    return true;
                }
                // when marked as not remember but model has paramValue need to clear it for uncheck to persist
                return
                    ParameterValue != null &&
                    !string.IsNullOrEmpty(ParameterValue.Value);
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

        protected override async void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpParameterValue pv &&
                IsSharedValue &&
                pv.ParamId == ParamId &&
                pv.ParameterHostId == ParameterHostId &&
                pv.Value != CurrentValue) {
                CurrentValue = pv.Value;
                await ParameterValue.WriteToDatabaseAsync();
                Dispatcher.UIThread.Post(async () => {
                    await InitializeAsync(pv);
                });
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
            OnParamValidate?.Invoke(this, new EventArgs());
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
                    await SaveParameterAsync();
                });
            },
            (args) => {
                //return HasModelChanged;
                return true;
            });

        #endregion
    }
}
