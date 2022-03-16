using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Media;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using SQLite;
using Windows.Foundation.Collections;

namespace MpWpfApp {
    public class MpAnalyticItemParameterViewModelBase : 
        MpViewModelBase<MpAnalyticItemPresetViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpITooltipInfoViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        #endregion

        #region Business Logic

        public string ValidationMessage { get; set; } = string.Empty;

        #endregion

        #region Appearance

        public Brush ParameterBorderBrush {
            get {
                if (IsValid) {
                    return Brushes.Transparent;
                }
                return Brushes.Red;
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

        public bool IsHovering { get; set; } = false;

        public bool IsValid => string.IsNullOrEmpty(ValidationMessage);

        public bool IsSelected { get; set; } = false;

        #endregion

        #region MpITooltipInfoViewModel Implementation

        public object Tooltip => Description;

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

        public MpAnalyticItemParameterValueUnitType UnitType {
            get {
                if (ParameterFormat == null) {
                    return MpAnalyticItemParameterValueUnitType.None;
                }
                if(ParameterFormat.unitType == MpAnalyticItemParameterValueUnitType.None) {
                    switch(ParameterFormat.controlType) {
                        case MpAnalyticItemParameterControlType.CheckBox:
                            return MpAnalyticItemParameterValueUnitType.Bool;
                        case MpAnalyticItemParameterControlType.Slider:
                            return MpAnalyticItemParameterValueUnitType.Decimal;
                        case MpAnalyticItemParameterControlType.TextBox:
                        case MpAnalyticItemParameterControlType.List:
                        case MpAnalyticItemParameterControlType.ComboBox:
                            return MpAnalyticItemParameterValueUnitType.PlainText;
                        case MpAnalyticItemParameterControlType.MultiSelectList:
                        case MpAnalyticItemParameterControlType.EditableList:
                            return MpAnalyticItemParameterValueUnitType.DelimitedPlainText;
                        case MpAnalyticItemParameterControlType.FileChooser:
                        case MpAnalyticItemParameterControlType.DirectoryChooser:
                            return MpAnalyticItemParameterValueUnitType.FileSystemPath;

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

        public virtual string CurrentValue {
            get {
                if(PresetValue == null) {
                    return string.Empty;
                }

                return PresetValue.Value.TrimTrailingLineEndings();
            }
            set {
                if(CurrentValue != value) {
                    PresetValue.Value = value;
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
                    MpConsole.WriteTraceLine(ex);
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
                    MpConsole.WriteTraceLine(ex);
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

        public int ParamEnumId {
            get {
                if (ParameterFormat == null) {
                    return 0;
                }
                return ParameterFormat.paramId;
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

        public MpAnalyticItemParameterControlType ControlType {
            get {
                if(ParameterFormat == null) {
                    return MpAnalyticItemParameterControlType.None;
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

        public int ParameterValueId {
            get {
                if(PresetValue == null) {
                    return 0;
                }
                return PresetValue.Id;
            }
        }

        public MpAnalyticItemParameterFormat ParameterFormat { 
            get {
                if(PresetValue == null) {
                    return null;
                }
                return PresetValue.ParameterFormat;
            }
        }

        public MpAnalyticItemPresetParameterValue PresetValue { get; set; }

        #endregion

        #endregion

        #region Events

        public event EventHandler OnValidate;

        #endregion

        #region Constructors

        public MpAnalyticItemParameterViewModelBase() : base(null) { }

        public MpAnalyticItemParameterViewModelBase(MpAnalyticItemPresetViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpAnalyticItemPresetParameterValue aipv) {
            bool wasBusy = IsBusy;
            IsBusy = true;

            PresetValue = aipv;

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
                            await PresetValue.WriteToDatabaseAsync();
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
                        MpConsole.WriteLine(Parent.Parent.Title+" "+Parent.Label+" "+ ValidationMessage);
                    }
                    OnPropertyChanged(nameof(IsValid));
                    break;

            }
            //Validate();
        }

        protected virtual void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(!MpBootstrapperViewModelBase.IsLoaded) {
                return;
            }
            OnValidate?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}
