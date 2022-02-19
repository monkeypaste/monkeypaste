using MonkeyPaste;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSliderParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Private Variables
        private string _defaultValue;
        #endregion

        #region Properties

        #region View Models
        //public override MpAnalyticItemParameterValueViewModel CurrentValueViewModel => new MpAnalyticItemParameterValueViewModel() { Value = this.Value.ToString() };
        #endregion

        #region Model

        public override string CurrentValue { 
            get {
                if (_currentValue == null || Parameter == null) {
                    return "0";
                }

                switch (Parameter.parameterValueType) {
                    case MpAnalyticItemParameterValueUnitType.Integer:
                        return IntValue.ToString();
                    case MpAnalyticItemParameterValueUnitType.Decimal:
                        return Math.Round(DoubleValue, 2).ToString();
                    default:
                        return DoubleValue.ToString();
                }
            }
            set {
                if(_currentValue != value) {
                    _currentValue = value == null ? "0":value;
                    OnPropertyChanged(nameof(CurrentValue));
                    OnPropertyChanged(nameof(SliderValue));
                }
            }
        }

        public override string DefaultValue => _defaultValue;

        public double SliderValue {
            get {
                if(CurrentValue == null || Parameter == null) {
                    return 0;
                }
                
                switch(Parameter.parameterValueType) {
                    case MpAnalyticItemParameterValueUnitType.Integer:
                        return IntValue;
                    case MpAnalyticItemParameterValueUnitType.Decimal:
                        return Math.Round(DoubleValue,2);
                    default:
                        return DoubleValue;
                }
            }
            set {
                if(SliderValue != value) {
                    CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SliderValue));
                }
            }
        }

        public double Min {
            get {
                if(Parameter == null) {
                    return double.MinValue;
                }
                var minCond = Parameter.values.FirstOrDefault(x => x.isMinimum);
                if (minCond != null) {
                    try {
                        return Convert.ToDouble(minCond.value);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Minimum val: {minCond.value} could not conver to int, exception: {ex}");                        
                    }
                }
                return double.MinValue;
            }
        }

        public double Max {
            get {
                if (Parameter == null) {
                    return double.MaxValue;
                }
                var maxCond = Parameter.values.FirstOrDefault(x => x.isMaximum);
                if (maxCond != null) {
                    try {
                        return Convert.ToDouble(maxCond.value);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Minimum val: {maxCond.value} could not conver to int, exception: {ex}");
                    }
                }
                return 0;
            }
        }

        public double TickFrequency {
            get {
                if(Parameter == null) {
                    return 0.1;
                }
                if(string.IsNullOrEmpty(Parameter.formatInfo)) {
                    return (Max - Min) / 10;
                }
                return Convert.ToDouble(Parameter.formatInfo);
            }
        }

        #endregion        

        #endregion

        #region Constructors

        public MpSliderParameterViewModel() : base(null) { }

        public MpSliderParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aipf, MpAnalyticItemPresetParameterValue aipv) {
            IsBusy = true;

            Parameter = aipf;

            CurrentValue = aipv.Value;
            if (Parameter.values.Any(x => x.isDefault)) {
                _defaultValue = Parameter.values.FirstOrDefault(x => x.isDefault).value = CurrentValue;
            } else {
                _defaultValue = CurrentValue;

            }

            
            OnPropertyChanged(nameof(Min));
            OnPropertyChanged(nameof(Max));
            OnPropertyChanged(nameof(DefaultValue));
            OnPropertyChanged(nameof(SliderValue));
            OnPropertyChanged(nameof(TickFrequency));
            
            await Task.Delay(1);

            IsBusy = false;
        }

        #endregion
    }
}
