using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using MonkeyPaste;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common; 
//using Newtonsoft.Json;
//using SQLite;

namespace MonkeyPaste.Avalonia {
    public class MpAvPluginParameterViewModelBase : 
        MpViewModelBase<MpIPluginComponentViewModel>,
        MpITreeItemViewModel,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpITooltipInfoViewModel,
        MpIParameterKeyValuePair {
        #region Private Variables

        #endregion

        #region Properties

        #region MpITreeItemViewModel Implementation

        public IEnumerable<MpITreeItemViewModel> Children => null;

        public MpITreeItemViewModel ParentTreeItem => Parent as MpITreeItemViewModel;
        public bool IsExpanded { get; set; }
        #endregion

        #region View Models

        #endregion

        #region MpIParameterKeyValuePair Implementation

        #region MpIJsonObject Implementation

        string MpIJsonObject.SerializeJsonObject() {
            return MpJsonObject.SerializeObject(this);
        }
        #endregion

        string MpIParameterKeyValuePair.paramName => ParamName;
        string MpIParameterKeyValuePair.value => CurrentValue;
        

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

        #region Business Logic

        public string ValidationMessage { get; set; } = string.Empty;

        #endregion

        #region Appearance

        public string ParameterBorderBrush {
            get {
                if (IsValid) {
                    return MpSystemColors.Transparent;
                }
                return MpSystemColors.Red;
            }
        }


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

        public bool HasDescription => !string.IsNullOrEmpty(Description);


        public bool IsValid => string.IsNullOrEmpty(ValidationMessage);


        // TODO I forget why this is needed but was used to hide trigger stuff from query path selector popup
        public bool IsActionParameter { get; set; } = false;
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

        public MpPluginParameterValueUnitType UnitType {
            get {
                if (ParameterFormat == null) {
                    return MpPluginParameterValueUnitType.None;
                }
                if(ParameterFormat.unitType == MpPluginParameterValueUnitType.None) {
                    switch(ParameterFormat.controlType) {
                        case MpPluginParameterControlType.CheckBox:
                            return MpPluginParameterValueUnitType.Bool;
                        case MpPluginParameterControlType.Slider:
                            return MpPluginParameterValueUnitType.Decimal;
                        case MpPluginParameterControlType.PasswordBox:
                        case MpPluginParameterControlType.TextBox:
                        case MpPluginParameterControlType.List:
                        case MpPluginParameterControlType.ComboBox:
                            return MpPluginParameterValueUnitType.PlainText;
                        case MpPluginParameterControlType.MultiSelectList:
                        case MpPluginParameterControlType.EditableList:
                            return MpPluginParameterValueUnitType.DelimitedPlainText;
                        case MpPluginParameterControlType.FileChooser:
                        case MpPluginParameterControlType.DirectoryChooser:
                            return MpPluginParameterValueUnitType.FileSystemPath;

                    }
                }
                return ParameterFormat.unitType;
            }
        }

        public List<string> DefaultValues {
            get {
                if(ParameterFormat == null || ParameterFormat.values == null) {
                    return new List<string>();
                }
                return ParameterFormat.values.Where(x => x.isDefault).Select(x => x.value).ToList();
            }
        }


        #endregion

        #region Db

        public virtual string CurrentValue {
            get {
                if(PresetValueModel == null) {
                    return string.Empty;
                }

                return PresetValueModel.Value.TrimTrailingLineEnding();
            }
            set {
                if(CurrentValue != value) {
                    PresetValueModel.Value = value; 
                    HasModelChanged = true;      
                    OnPropertyChanged(nameof(CurrentValue));
                }
            }
        }

        public double DoubleValue {
            get {
                if (string.IsNullOrWhiteSpace(CurrentValue)) {
                    return 0;
                }
                try {
                    return Convert.ToDouble(CurrentValue);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Cannot convert "+CurrentValue+" to double ",ex);
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

        public string ParamName {
            get {
                if (ParameterFormat == null) {
                    return null;
                }
                //if(string.IsNullOrEmpty(ParameterFormat.paramName)) {
                //    ParameterFormat.paramName = 
                //}
                return ParameterFormat.paramName;
            }
        }

        public string Label {
            get {
                if (ParameterFormat == null) {
                    return string.Empty;
                }
                if(string.IsNullOrEmpty(ParameterFormat.label)) {
                    return ParameterFormat.label;
                }
                return ParameterFormat.label;
            }
        }

        public MpPluginParameterControlType ControlType {
            get {
                if(ParameterFormat == null) {
                    return MpPluginParameterControlType.None;
                }
                return ParameterFormat.controlType;
            }
        }

        public string Description {
            get {
                if(ParameterFormat == null) {
                    return string.Empty;
                }
                return ParameterFormat.description;
            }
        }


        public MpPluginParameterFormat ParameterFormat {
            get {
                if (Parent == null || PresetValueModel == null) {
                    return null;
                }
                //AnalyzerFormat.parameters.FirstOrDefault(y => y.paramName == x.ParamName)
                return Parent.ComponentFormat.parameters.FirstOrDefault(x=>x.paramName == PresetValueModel.ParamName);
            }
        }
        #endregion

        #endregion

        #endregion

        #region Events

        public event EventHandler OnValidate;

        #endregion

        #region Constructors

        public MpAvPluginParameterViewModelBase() : base(null) { }

        public MpAvPluginParameterViewModelBase(MpIPluginComponentViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpPluginPresetParameterValue aipv) {
            bool wasBusy = IsBusy;
            IsBusy = true;

            PresetValueModel = aipv;

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

        #endregion

        #region Private Methods

        private void MpAnalyticItemParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(HasModelChanged):
                    if(HasModelChanged) {
                        Task.Run(async () => {
                            await PresetValueModel.WriteToDatabaseAsync();
                            HasModelChanged = false;
                            //if(this is MpComboBoxParameterViewModel cbpvm) {
                            //    cbpvm.Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                            //    cbpvm.Items.ForEach(x => x.HasModelChanged = false);
                            //}
                        });
                    }
                    break;
                case nameof(ValidationMessage):
                    if(!string.IsNullOrEmpty(ValidationMessage)) {
                        dynamic pp = Parent;
                        
                        MpConsole.WriteLine($"Validation Msg Changed for ['{pp.Parent.Title}' {pp.Label}]: {ValidationMessage}");
                    }
                    OnPropertyChanged(nameof(IsValid));
                    break;
                case nameof(IsSelected):
                    if(IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                    }
                    break;
            }
            //Validate();
        }

        protected virtual void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(!MpBootstrapperViewModelBase.IsCoreLoaded) {
                return;
            }
            OnValidate?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
