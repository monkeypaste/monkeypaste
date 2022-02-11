using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpMultiSelectComboBoxParameterViewModel : MpComboBoxParameterViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models
        public virtual ObservableCollection<MpAnalyticItemParameterValueViewModel> SelectedViewModels { get; set; } = new ObservableCollection<MpAnalyticItemParameterValueViewModel>();
        
        #endregion
        
        #region State

        #endregion

        #region Model

        public override string CurrentValue {
            get => string.Join(",", SelectedViewModels.Select(x => x.Value));
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

        public bool IsMultiValue {
            get {
                if(Parameter == null) {
                    return false;
                }
                return Parameter.isMultiValue;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpMultiSelectComboBoxParameterViewModel() : base () { }

        public MpMultiSelectComboBoxParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aip) {
            await base.InitializeAsync(aip);
            foreach (var spv in ValueViewModels.Where(x => x.IsDefault)) {
                SelectedViewModels.Add(spv);
            }
            OnPropertyChanged(nameof(SelectedViewModels));
        }

        #endregion

        #region Protected Methods
        #endregion
    }
}
