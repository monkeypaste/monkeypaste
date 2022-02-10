using MonkeyPaste;
using MonkeyPaste.Plugin;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSliderParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Private Variables
        //private string _defaultValue;
        #endregion

        #region Properties

        #region View Models
        //public override MpAnalyticItemParameterValueViewModel CurrentValueViewModel => new MpAnalyticItemParameterValueViewModel() { Value = this.Value.ToString() };
        #endregion

        #region Model

        public override string CurrentValue { get; set; } = string.Empty;

        //public override string DefaultValue => _defaultValue;

        public double SliderValue {
            get {
                if(CurrentValue == null || Parameter == null) {
                    return 0;
                }
                
                switch(Parameter.ValueType) {
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
                    OnPropertyChanged(nameof(SliderValue));
                }
            }
        }

        public double Min {
            get {
                if(Parameter == null) {
                    return double.MinValue;
                }
                var minCond = Parameter.Values.FirstOrDefault(x => x.IsMinimum);
                if (minCond != null) {
                    try {
                        return Convert.ToDouble(minCond.Value);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Minimum val: {minCond.Value} could not conver to int, exception: {ex}");                        
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
                var maxCond = Parameter.Values.FirstOrDefault(x => x.IsMaximum);
                if (maxCond != null) {
                    try {
                        return Convert.ToDouble(maxCond.Value);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Minimum val: {maxCond.Value} could not conver to int, exception: {ex}");
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
                if(string.IsNullOrEmpty(Parameter.FormatInfo)) {
                    return (Max - Min) / 10;
                }
                return Convert.ToDouble(Parameter.FormatInfo);
            }
        }

        #endregion
        

        #endregion

        #region Constructors

        public MpSliderParameterViewModel() : base(null) { }

        public MpSliderParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aip) {
            IsBusy = true;

            Parameter = aip;

            //if (Parameter == null || Parameter.Values == null) {
            //    ResetToDefault();
            //} else {
            //    MpAnalyticItemParameterValue defVal = Parameter.Values.FirstOrDefault(x => x.IsDefault);
            //    if (defVal != null) {
            //        _defaultValue = defVal.Value;
            //    } else {
            //        _defaultValue = "0";
            //    }
            //    ResetToDefault();
            //}
            ResetToDefault();

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
