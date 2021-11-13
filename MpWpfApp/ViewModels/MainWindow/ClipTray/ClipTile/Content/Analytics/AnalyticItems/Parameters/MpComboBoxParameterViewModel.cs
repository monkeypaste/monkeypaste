using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpComboBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        #region View Models
        public MpAnalyticItemParameterValueViewModel DefaultValueViewModel { get; set; }
        #endregion

        #region State
        public override bool HasChanged => CurrentValueViewModel != DefaultValueViewModel;
        #endregion

        #region Model

        public override bool IsValid {
            get {
                if(!IsRequired) {
                    return true;
                }
                return CurrentValueViewModel != null && !string.IsNullOrEmpty(CurrentValueViewModel.Value);
            }
        }
        
        #endregion

        #endregion

        #region Constructors

        public MpComboBoxParameterViewModel() : base () { }

        public MpComboBoxParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameter aip) {
            IsBusy = true;

            Parameter = aip;

            ValueViewModels.Clear();

            foreach(var valueSeed in Parameter.ValueSeeds) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(ValueViewModels.Count, valueSeed);
                ValueViewModels.Add(naipvvm);
            }

            MpAnalyticItemParameterValueViewModel defVal = ValueViewModels.FirstOrDefault(x => x.IsDefault);
            if (defVal != null) {
                defVal.IsSelected = true;
            } else if(ValueViewModels.Count > 0) {
                ValueViewModels[0].IsSelected = true;
            }
            DefaultValueViewModel = CurrentValueViewModel;

            OnPropertyChanged(nameof(ValueViewModels));

            IsBusy = false;
        }

        public override void SetValue(string newValue) {
            var valueVm = ValueViewModels.FirstOrDefault(x => x.Value == newValue);
            if(valueVm == null) {
                throw new Exception($"Param {Label} does not have a '{newValue}' value");
            }
            ValueViewModels.ForEach(x => x.IsSelected = false);
            valueVm.IsSelected = true;
        }

        #endregion
    }
}
