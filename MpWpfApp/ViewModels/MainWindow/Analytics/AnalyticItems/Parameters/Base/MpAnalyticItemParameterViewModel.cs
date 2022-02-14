using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using SQLite;
using Windows.Foundation.Collections;

namespace MpWpfApp {
    public abstract class MpAnalyticItemParameterViewModel : 
        MpViewModelBase<MpAnalyticItemPresetViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel {
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
                if (Parameter != null && !string.IsNullOrEmpty(Parameter.description)) {
                    return Parameter.description;
                }
                return null;
            }
        }

        #endregion

        #region State

        public bool IsHovering { get; set; } = false;

        public bool IsValid => string.IsNullOrEmpty(ValidationMessage);

        public bool IsSelected { get; set; } = false;

        #endregion

        #region Model

        public bool IsVisible {
            get {
                if (Parameter == null) {
                    return false;
                }
                return Parameter.isVisible;
            }
        }

        public virtual string CurrentValue { get; set; }

        public virtual string DefaultValue { 
            get {
                if (Parameter == null || Parameter.values.All(x=>x.isDefault == false)) {
                    return string.Empty;
                }
                return Parameter.values.FirstOrDefault(x => x.isDefault).value;
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
                if (CurrentValue != "0" && CurrentValue != "1") {
                    throw new Exception("Cannot convert value " + CurrentValue + " to boolean");
                }
                return CurrentValue == "1";
            }
            set {
                if (BoolValue != value) {
                    CurrentValue = value ? "1" : "0";
                    OnPropertyChanged(nameof(BoolValue));
                    OnPropertyChanged(nameof(CurrentValue));
                }
            }
        }

        public int ParamEnumId {
            get {
                if (Parameter == null) {
                    return 0;
                }
                return Parameter.enumId;
            }
        }

        public bool IsReadOnly {
            get {
                if (Parameter == null) {
                    return false;
                }
                return Parameter.isReadOnly;
            }
        }

        public bool IsRequired {
            get {
                if (Parameter == null) {
                    return false;
                }
                return Parameter.isRequired;
           }
        }

        public string Label {
            get {
                if (Parameter == null) {
                    return string.Empty;
                }
                if(string.IsNullOrEmpty(Parameter.label)) {
                    return Parameter.label;
                }
                return Parameter.label;
            }
        }

        public string FormatInfo {
            get {
                if (Parameter == null) {
                    return string.Empty;
                }
                return Parameter.formatInfo;
            }
        }

        public MpAnalyticItemParameterFormat Parameter { get; protected set; }

        #endregion

        #endregion

        #region Events

        public event EventHandler OnValidate;

        #endregion

        #region Constructors

        public MpAnalyticItemParameterViewModel() : base(null) { }

        public MpAnalyticItemParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public abstract Task InitializeAsync(MpAnalyticItemParameterFormat aip);
        
        public async Task<MpComboBoxParameterValueViewModel> CreateAnalyticItemParameterValueViewModel(int idx, MpAnalyticItemParameterValue valueSeed) {
            var naipvvm = new MpComboBoxParameterValueViewModel(this);
            naipvvm.PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
            await naipvvm.InitializeAsync(idx, valueSeed);
            return naipvvm;
        }

        public void ResetToDefault() {
            CurrentValue = DefaultValue;
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
                case nameof(ValidationMessage):
                    OnPropertyChanged(nameof(IsValid));
                    break;
            }
            //Validate();
        }

        protected virtual void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            OnValidate?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}
