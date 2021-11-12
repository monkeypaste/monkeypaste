using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpComboBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        #region View Models

        //public ObservableCollection<MpAnalyticItemParameterValueViewModel> ValueViewModels { get; set; } = new ObservableCollection<MpAnalyticItemParameterValueViewModel>();

        #endregion

        #region State
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

            OnPropertyChanged(nameof(ValueViewModels));

            IsBusy = false;
        }

        #endregion
    }
}
