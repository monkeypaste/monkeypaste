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

        public MpComboBoxParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameter aip) {
            IsBusy = true;
            
            Parameter = aip;

            ValueViewModels.Clear();

            //if (Parameter.ValueFormats == null) {
            //    await Parent.DeferredCreateParameterValueViewModels(this);
            //} else {
                
            //}
            foreach (var valueSeed in Parameter.Values) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(ValueViewModels.Count, valueSeed);
                ValueViewModels.Add(naipvvm);
            }

            MpAnalyticItemParameterValueViewModel defVal = ValueViewModels.FirstOrDefault(x => x.IsDefault);
            if (defVal != null) {
                defVal.IsSelected = true;
            } else if(ValueViewModels.Count > 0) {
                ValueViewModels[0].IsSelected = true;
            }
            DefaultValue = CurrentValueViewModel.Value;

            OnPropertyChanged(nameof(ValueViewModels));
            OnPropertyChanged(nameof(CurrentValue));
            OnPropertyChanged(nameof(DefaultValue));

            IsBusy = false;
        }

        //public override void SetValue(string newValue) {
        //    var valueVm = ValueViewModels.FirstOrDefault(x => x.Value == newValue);
        //    if(valueVm == null) {
        //        throw new Exception($"Param {Label} does not have a '{newValue}' value");
        //    }
        //    ValueViewModels.ForEach(x => x.IsSelected = false);
        //    valueVm.IsSelected = true;
        //}

        #endregion

        #region Protected Methods

        //protected override bool Validate() {
        //    if (!IsRequired) {
        //        return true;
        //    }
        //    return CurrentValueViewModel != null && !string.IsNullOrEmpty(CurrentValueViewModel.Value);
        //}
        #endregion
    }
}
