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

            //if (Parameter.ValueFormats == null) {
            //    await Parent.DeferredCreateParameterValueViewModels(this);
            //} else {

            //}
            var presetVal = Parent.Preset.PresetParameterValues.FirstOrDefault(x => x.ParameterEnumId == Parameter.EnumId);
            string selectedValue = presetVal == null ? string.Empty:presetVal.Value;
            foreach (var valueSeed in Parameter.Values) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(ValueViewModels.Count, valueSeed);
                if(naipvvm.Value == selectedValue) {
                    naipvvm.IsSelected = true;
                }
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
