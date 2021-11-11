using MonkeyPaste;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSliderParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        #region View Models
        //public override MpAnalyticItemParameterValueViewModel CurrentValueViewModel => new MpAnalyticItemParameterValueViewModel() { Value = this.Value.ToString() };
        #endregion

        #region Model

        public double SliderValue {
            get {
                if(ValueViewModels.Count == 0) {
                    return 0;
                }

                switch(ValueViewModels[0].AnalyticItemParameterValue.ParameterValueType) {
                    case MpAnalyticItemParameterValueUnitType.Integer:
                        return CurrentValueViewModel.IntValue;
                    case MpAnalyticItemParameterValueUnitType.Decimal:
                        return Math.Round(CurrentValueViewModel.DoubleValue,2);
                    default:
                        return CurrentValueViewModel.DoubleValue;
                }
            }
            set {
                if(SliderValue != value) {
                    CurrentValueViewModel.Value = value.ToString();
                    OnPropertyChanged(nameof(SliderValue));
                }
            }
        }

        public double Min {
            get {
                var minVm = ValueViewModels.FirstOrDefault(x => x.IsMinimum);
                if(minVm == null) {
                    return 0;
                }
                return minVm.DoubleValue;
            }
        }

        public double Max {
            get {
                var maxVm = ValueViewModels.FirstOrDefault(x => x.IsMaximum);
                if (maxVm == null) {
                    return 1;
                }
                return maxVm.DoubleValue;
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

        //public double Value {
        //    get {
        //        if(Parameter == null) {
        //            return 0;
        //        }
        //        if(!string.IsNullOrWhiteSpace(Parameter.InputValue)) {
        //            return Convert.ToDouble(Parameter.InputValue);
        //        }
        //        return Convert.ToDouble(Parameter.DefaultValue);
        //    }
        //    set {
        //        if(Parameter != null && Parameter.InputValue.ToString() != value.ToString()) {
        //            Parameter.InputValue = value.ToString();
        //            OnPropertyChanged(nameof(Value));
        //        }
        //    }
        //}

        #endregion
        

        public override bool IsValid => true;

        #endregion

        #region Constructors

        public MpSliderParameterViewModel() : base() { }

        public MpSliderParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameter aip) {
            IsBusy = true;

            Parameter = aip;

            ValueViewModels.Clear();

            foreach (var valueSeed in Parameter.ValueSeeds) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(ValueViewModels.Count, valueSeed);
                ValueViewModels.Add(naipvvm);
            }

            MpAnalyticItemParameterValueViewModel defVal = ValueViewModels.FirstOrDefault(x => x.IsDefault);
            if (defVal != null) {
                defVal.IsSelected = true;
            }

            OnPropertyChanged(nameof(ValueViewModels));

            IsBusy = false;
        }

        #endregion
    }
}
