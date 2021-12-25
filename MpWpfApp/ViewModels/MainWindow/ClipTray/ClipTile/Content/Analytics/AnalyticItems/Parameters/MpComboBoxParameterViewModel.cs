using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpComboBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models
        //public MpAnalyticItemParameterValueViewModel DefaultValueViewModel { get; set; }

        public virtual ObservableCollection<MpAnalyticItemParameterValueViewModel> ValueViewModels { get; set; } = new ObservableCollection<MpAnalyticItemParameterValueViewModel>();

        public virtual MpAnalyticItemParameterValueViewModel CurrentValueViewModel {
            get => ValueViewModels.FirstOrDefault(x => x.IsSelected);
            set {
                if (value != CurrentValueViewModel) {
                    ValueViewModels.ForEach(x => x.IsSelected = false);
                    if (value != null) {
                        value.IsSelected = true;
                    }
                }
            }
        }
        #endregion

        #region State
        
        #endregion

        #region Model

        public override string CurrentValue {
            get => CurrentValueViewModel?.Value;
            set {
                if(CurrentValue != value) {
                    ValueViewModels.ForEach(x => x.IsSelected = false);
                    if (value != null) {
                        var ncvvm = ValueViewModels.FirstOrDefault(x => x.Value == value);
                        if (ncvvm == null) {
                            throw new Exception("Cannot set combobox to: " + value);
                        }
                        ncvvm.IsSelected = true;
                    }
                    OnPropertyChanged(nameof(CurrentValue));
                    OnPropertyChanged(nameof(CurrentValueViewModel));
                }
            }
        }

        public override string DefaultValue => ValueViewModels.FirstOrDefault(x => x.IsDefault)?.Value;

        #endregion

        #endregion

        #region Constructors

        public MpComboBoxParameterViewModel() : base () { }

        public MpComboBoxParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameter aip) {
            IsBusy = true;
            
            Parameter = aip;

            ValueViewModels.Clear();

            foreach (var valueSeed in Parameter.Values) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(ValueViewModels.Count, valueSeed);
                naipvvm.IsSelected = valueSeed.IsDefault;
                ValueViewModels.Add(naipvvm);
            }

            if (ValueViewModels.All(x=>x.IsSelected == false) && ValueViewModels.Count > 0) {
                ValueViewModels[0].IsSelected = true;
            }

            OnPropertyChanged(nameof(ValueViewModels));
            OnPropertyChanged(nameof(CurrentValue));
            OnPropertyChanged(nameof(DefaultValue));

            IsBusy = false;
        }

        #endregion

        #region Protected Methods
        #endregion
    }
}
