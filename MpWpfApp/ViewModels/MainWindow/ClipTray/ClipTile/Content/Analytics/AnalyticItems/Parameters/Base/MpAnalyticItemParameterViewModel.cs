using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using MonkeyPaste;
using SQLite;
using Windows.Foundation.Collections;

namespace MpWpfApp {
    public abstract class MpAnalyticItemParameterViewModel : MpAnalyticItemComponentViewModel {
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
                if (Parameter != null && !string.IsNullOrEmpty(Parameter.Description)) {
                    return Parameter.Description;
                }
                return null;
            }
        }

        #endregion

        #region State

        public bool HasChanged => CurrentValue != DefaultValue && !IsBusy;//=> ValueViewModels.Any(x => x.HasChanged);

        public bool IsHovering { get; set; } = false;

        public bool IsValid => string.IsNullOrEmpty(ValidationMessage);

        #endregion

        #region Model

        public abstract string CurrentValue { get; set; }

        public virtual string DefaultValue { get; set; }

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
                return Parameter.EnumId;
            }
        }

        public bool IsRequired {
            get {
                if (Parameter == null) {
                    return false;
                }
                return Parameter.IsParameterRequired;
           }
        }

        public string Label {
            get {
                if (Parameter == null) {
                    return string.Empty;
                }
                if(string.IsNullOrEmpty(Parameter.Label)) {
                    return Parameter.Label;
                }
                return Parameter.Label;
            }
        }

        public string FormatInfo {
            get {
                if (Parameter == null) {
                    return string.Empty;
                }
                return Parameter.FormatInfo;
            }
        }

        public MpAnalyticItemParameter Parameter { get; protected set; }


        #endregion

        #endregion

        #region Events

        public event EventHandler OnValidate;

        #endregion

        #region Constructors

        public MpAnalyticItemParameterViewModel() : base(null) { }

        public MpAnalyticItemParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public abstract Task InitializeAsync(MpAnalyticItemParameter aip);// {
        
        public async Task<MpAnalyticItemParameterValueViewModel> CreateAnalyticItemParameterValueViewModel(int idx, MpAnalyticItemParameterValue valueSeed) {
            var naipvvm = new MpAnalyticItemParameterValueViewModel(this);
            naipvvm.PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
            await naipvvm.InitializeAsync(idx, valueSeed);
            return naipvvm;
        }

        public void ResetToDefault() {
            //MpAnalyticItemParameterValueViewModel defVal = ValueViewModels.FirstOrDefault(x => x.IsDefault);
            //if (defVal != null) {
            //    defVal.IsSelected = true;
            //} else if (ValueViewModels.Count > 0) {
            //    ValueViewModels[0].IsSelected = true;
            //}
            CurrentValue = DefaultValue;
        }

        public void SetValueFromPreset(string newValue) {
            DefaultValue = CurrentValue = newValue;
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
                case nameof(HasChanged):
                    Parent.OnPropertyChanged(nameof(Parent.HasAnyChanged));
                    break;
                case nameof(ValidationMessage):
                    OnPropertyChanged(nameof(IsValid));
                    break;
            }
            OnValidate?.Invoke(this, new EventArgs());
        }

        protected virtual void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            OnValidate?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}
