using MonkeyPaste;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace MpWpfApp {
    public class MpTextBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        #region State

        public override bool IsValid {
            get {
                if(!IsRequired) {
                    return true;
                }
                if(CurrentValueViewModel == null || string.IsNullOrEmpty(CurrentValueViewModel.Value)) {
                    return false;
                }
                var minCond = ValueViewModels.FirstOrDefault(x => x.IsMinimum);
                if(minCond != null) {
                    if(CurrentValueViewModel.Value.Length < minCond.IntValue) {
                        return false;
                    }
                }
                var maxCond = ValueViewModels.FirstOrDefault(x => x.IsMaximum);
                if (maxCond != null) {
                    if (CurrentValueViewModel.Value.Length > maxCond.IntValue) {
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(FormatInfo)) {
                    if(CurrentValueViewModel.Value.IndexOfAny(FormatInfo.ToCharArray()) != -1) {
                        return false;
                    }
                }
                return true;
            }
        }
        #endregion

        //public string InputValue {
        //    get {
        //        if (Parameter == null) {
        //            return string.Empty;
        //        }
        //        if(string.IsNullOrEmpty(Parameter.ValueCsv) && 
        //           !string.IsNullOrEmpty(Parameter.DefaultValue)) {
        //            Parameter.ValueCsv = Parameter.DefaultValue;
        //        }
        //        return Parameter.ValueCsv;
        //    }
        //    set {
        //        if (Parameter != null && Parameter.ValueCsv != value) {
        //            Parameter.ValueCsv = value;
        //            OnPropertyChanged(nameof(InputValue));
        //        }
        //    }
        //}

        //public override MpAnalyticItemParameterValueViewModel CurrentValueViewModel => new MpAnalyticItemParameterValueViewModel() { Value = InputValue };

        #endregion

        #region Constructors

        public MpTextBoxParameterViewModel() : base() { }

        public MpTextBoxParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

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
            } else if (ValueViewModels.Count > 0) {
                ValueViewModels[0].IsSelected = true;
            } else {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(
                    ValueViewModels.Count,
                    new MpAnalyticItemParameterValue() {
                        ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                        AnalyticItemParameter = Parameter,
                        AnalyticItemParameterValueId = Parameter.Id,
                        IsDefault = true
                    });

                ValueViewModels.Add(naipvvm);
                naipvvm.IsSelected = true;
            }

            OnPropertyChanged(nameof(ValueViewModels));

            IsBusy = false;
        }

        #endregion
    }
}
