using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;
using System.Windows.Input;
//using Newtonsoft.Json;
//using SQLite;

namespace MonkeyPaste.Avalonia {
    public class MpAvParameterViewModelBase :
        MpViewModelBase<MpIParameterHostViewModel>,
        MpITreeItemViewModel,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpITooltipInfoViewModel {
        #region Private Variables
        private string _lastValue;
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

        public override bool HasModelChanged => CurrentValue != _lastValue;

        public bool HasDescription => !string.IsNullOrEmpty(Description);

        public bool IsValid => string.IsNullOrEmpty(ValidationMessage);

        public bool IsActionParameter { get; set; } = false;

        public double DoubleValue {
            get {
                if (string.IsNullOrWhiteSpace(CurrentValue)) {
                    return 0;
                }
                try {
                    return Convert.ToDouble(CurrentValue);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Cannot convert " + CurrentValue + " to double ", ex);
                    return 0;
                }
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
                if (string.IsNullOrWhiteSpace(CurrentValue)) {
                    return 0;
                }
                try {
                    return Convert.ToInt32(DoubleValue);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Cannot convert " + CurrentValue + " to int ", ex);
                    return 0;
                }
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
                if (string.IsNullOrWhiteSpace(CurrentValue)) {
                    return false;
                }
                //if (CurrentValue.ToLower() != "false" && CurrentValue.ToLower() != "true") {
                //    throw new Exception("Cannot convert value " + CurrentValue + " to boolean");
                //}
                return CurrentValue.ToLower() == "true" || CurrentValue.ToLower() == "1";
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


        public string IllegalCharacters {
            get {
                if (ParameterFormat == null) {
                    return null;
                }
                return ParameterFormat.illegalCharacters;
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

        public virtual string CurrentValue {
            get {
                if (PresetValueModel == null) {
                    return string.Empty;
                }

                return PresetValueModel.Value;//.TrimTrailingLineEnding();
            }
            set {
                if (CurrentValue != value) {
                    AddUndo(CurrentValue, value, $"{Label} Changed");
                    PresetValueModel.Value = value;
                    //HasModelChanged = true;
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

        public MpPluginPresetParameterValue PresetValueModel { get; set; }

        #endregion

        #region Plugin (MpPluginParameterFormat)

        public object ParamId {
            get {
                if (ParameterFormat == null) {
                    return null;
                }
                //if(string.IsNullOrEmpty(ParameterFormat.paramName)) {
                //    ParameterFormat.paramName = 
                //}
                return ParameterFormat.paramId;
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


        public MpParameterFormat ParameterFormat {
            get {
                if (Parent == null || PresetValueModel == null) {
                    return null;
                }
                //AnalyzerFormat.parameters.FirstOrDefault(y => y.paramName == x.ParamName)
                return Parent.ComponentFormat.parameters.FirstOrDefault(x => x.paramId == PresetValueModel.ParamId);
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

        public MpAvParameterViewModelBase(MpIParameterHostViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpPluginPresetParameterValue aipv) {
            bool wasBusy = IsBusy;
            IsBusy = true;

            PresetValueModel = aipv;

            SetLastValue(CurrentValue);

            OnPropertyChanged(nameof(CurrentValue));

            await Task.Delay(1);

            IsBusy = wasBusy;
        }

        public bool Validate() {
            OnValidate?.Invoke(this, new EventArgs());
            return IsValid;
        }
        #endregion

        #region Protected Methods

        protected virtual void SetLastValue(object value) {
            _lastValue = value == null ? null : value.ToString();
        }
        
        protected virtual void RestoreLastValue() {
            CurrentValue = _lastValue;
        }

        #endregion

        #region Private Methods

        private void MpAnalyticItemParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                //case nameof(CurrentValue):
                //case nameof(LastValue):
                case nameof(HasModelChanged):
                case nameof(CurrentValue):
                    if(Parent is MpISaveOrCancelableViewModel socvm) {
                        // analyzers
                        socvm.OnPropertyChanged(nameof(socvm.CanSaveOrCancel));
                    } else {
                        // for action parameters
                        //SaveCurrentValueCommand.Execute(null);
                    }
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
            }
            //Validate();
        }

        protected virtual void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (!MpBootstrapperViewModelBase.IsCoreLoaded) {
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
        
        public ICommand SaveCurrentValueCommand => new MpCommand(
            () => {
                SetLastValue(CurrentValue);
                OnPropertyChanged(nameof(HasModelChanged));
                Task.Run(async () => {
                    //IsBusy = true;
                    await PresetValueModel.WriteToDatabaseAsync();
                    //IsBusy = false;
                });
            },
            () => {
                return HasModelChanged;
            });

        #endregion
    }
}
