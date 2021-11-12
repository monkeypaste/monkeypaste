using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpAnalyticItemParameterValueViewModel : MpViewModelBase<MpAnalyticItemParameterViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region State
        private bool _isInit = false;
        public bool IsInit { 
            get {
                if(Parent != null && Parent.IsInit) {
                    return true;
                }
                return _isInit;
            }
            set {
                if(_isInit != value) {
                    _isInit = value;
                    OnPropertyChanged(nameof(IsInit));
                }
            }
        }

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public int ValueIdx { get; set; } = 0;

        #endregion

        #region Model

        public bool IsDefault {
            get {
                if(AnalyticItemParameterValue == null) {
                    return false;
                }
                return AnalyticItemParameterValue.IsDefault;
            }
        }

        public bool IsMaximum {
            get {
                if (AnalyticItemParameterValue == null) {
                    return false;
                }
                return AnalyticItemParameterValue.IsMaximum;
            }
        }

        public bool IsMinimum {
            get {
                if (AnalyticItemParameterValue == null) {
                    return false;
                }
                return AnalyticItemParameterValue.IsMinimum;
            }
        }

        public MpAnalyticItemParameterValueUnitType ValueUnitType {
            get {
                if (AnalyticItemParameterValue == null) {
                    return MpAnalyticItemParameterValueUnitType.None;
                }
                return AnalyticItemParameterValue.ParameterValueType;
            }
        }

        public string Value {
            get {
                if(AnalyticItemParameterValue == null) {
                    return null;
                }
                return AnalyticItemParameterValue.Value;
            }
            set {
                if(Value != value) {
                    AnalyticItemParameterValue.Value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public double DoubleValue {
            get {
                if (string.IsNullOrWhiteSpace(Value)) {
                    return 0;
                }
                try {
                    return Convert.ToDouble(Value);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                    return 0;
                }
            }
            set {
                if (DoubleValue != value) {
                    Value = value.ToString();
                    OnPropertyChanged(nameof(DoubleValue));
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public int IntValue {
            get {
                if (string.IsNullOrWhiteSpace(Value)) {
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
                    Value = value.ToString();
                    OnPropertyChanged(nameof(IntValue));
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public bool BoolValue {
            get {
                if (string.IsNullOrWhiteSpace(Value)) {
                    return false;
                }
                if (Value != "0" && Value != "1") {
                    throw new Exception("Cannot convert value " + Value + " to boolean");
                }
                return Value == "1";
            }
            set {
                if(BoolValue != value) {
                    Value = value ? "1" : "0";
                    OnPropertyChanged(nameof(BoolValue));
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public MpAnalyticItemParameterValue AnalyticItemParameterValue { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemParameterValueViewModel() : base(null) { }

        public MpAnalyticItemParameterValueViewModel(MpAnalyticItemParameterViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int idx, MpAnalyticItemParameterValue valueSeed) {
            IsInit = true;
            IsBusy = true;

            await Task.Delay(1);

            ValueIdx = idx;
            AnalyticItemParameterValue = valueSeed;

            IsBusy = false;
            IsInit = false;
        }

        public override string ToString() {
            return Value;
        }

        #region Equals Override

        public bool Equals(MpAnalyticItemParameterValueViewModel other) {
            if (other == null)
                return false;

            if (this.Value == other.Value)
                return true;
            else
                return false;
        }

        public override bool Equals(Object obj) {
            if (obj == null)
                return false;

            MpAnalyticItemParameterValueViewModel personObj = obj as MpAnalyticItemParameterValueViewModel;
            if (personObj == null)
                return false;
            else
                return Equals(personObj);
        }

        public override int GetHashCode() {
            return this.Value.GetHashCode();
        }

        public static bool operator ==(MpAnalyticItemParameterValueViewModel person1, MpAnalyticItemParameterValueViewModel person2) {
            if (((object)person1) == null || ((object)person2) == null)
                return Object.Equals(person1, person2);

            return person1.Equals(person2);
        }

        public static bool operator !=(MpAnalyticItemParameterValueViewModel person1, MpAnalyticItemParameterValueViewModel person2) {
            if (((object)person1) == null || ((object)person2) == null)
                return !Object.Equals(person1, person2);

            return !(person1.Equals(person2));
        }

        #endregion

        #endregion

        #region Private Methods

        private void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(Value):
                    if(!IsInit) {
                        Parent.HasChanged = true;
                    }
                    break;
            }

            (Parent.Parent.ExecuteAnalysisCommand as RelayCommand).RaiseCanExecuteChanged();
        }
        #endregion
    }
}
