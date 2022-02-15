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
        public virtual ObservableCollection<MpComboBoxParameterValueViewModel> SelectedViewModels { get; set; } = new ObservableCollection<MpComboBoxParameterValueViewModel>();
        
        #endregion
        
        #region State

        #endregion

        #region Model

        public override string CurrentValue {
            get => string.Join(",", SelectedViewModels.Select(x => x.Value));
            set {
                if(CurrentValue != value) {
                    Items.ForEach(x => x.IsSelected = false);
                    if (value != null) {
                        var ncvvm = Items.FirstOrDefault(x => x.Value == value);
                        if (ncvvm == null) {
                            throw new Exception("Cannot set combobox to: " + value);
                        }
                        ncvvm.IsSelected = true;
                    }
                    OnPropertyChanged(nameof(CurrentValue));
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public override string DefaultValue => Items.FirstOrDefault(x => x.IsDefault)?.Value;

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

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aipf, MpAnalyticItemPresetParameterValue aipv) {
            IsBusy = true;

            Parameter = aipf;

            Items.Clear();

            foreach (var paramVal in Parameter.values) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, paramVal);
                naipvvm.IsSelected = aipv.Value.Contains(paramVal.value);
                Items.Add(naipvvm);
            }

            foreach (var spv in Items.Where(x => x.IsDefault)) {
                SelectedViewModels.Add(spv);
            }

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(CurrentValue));
            OnPropertyChanged(nameof(DefaultValue));

            OnPropertyChanged(nameof(SelectedViewModels));


            IsBusy = false;


        }

        #endregion

        #region Protected Methods
        #endregion
    }
}
