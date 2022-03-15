using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpComboBoxParameterViewModel : MpAnalyticItemParameterViewModelBase {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models
        //public MpAnalyticItemParameterValueViewModel DefaultValueViewModel { get; set; }

        public virtual ObservableCollection<MpEnumerableParameterValueViewModel> Items { get; set; } = new ObservableCollection<MpEnumerableParameterValueViewModel>();


        public virtual MpEnumerableParameterValueViewModel SelectedItem {
            get => Items.FirstOrDefault(x => x.IsSelected);
            set {
                if (value != SelectedItem) {
                    Items.ForEach(x => x.IsSelected = false);
                    if (value != null) {
                        value.IsSelected = true;
                    }
                    OnPropertyChanged(nameof(CurrentValue));
                }
            }
        }
        #endregion

        #region State
        
        #endregion

        #region Model

        public override string CurrentValue {
            get => SelectedItem?.Value;
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
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CurrentValue));
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        //public override string DefaultValue => Items.FirstOrDefault(x => x.IsDefault)?.Value;


        #endregion

        #endregion

        #region Constructors

        public MpComboBoxParameterViewModel() : base () { }

        public MpComboBoxParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemPresetParameterValue aipv) { 
            IsBusy = true;
            
            //Parameter = aip;
            PresetValue = aipv;

            Items.Clear();

            foreach (var paramVal in ParameterFormat.values) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, paramVal);
                naipvvm.IsSelected = paramVal.value == aipv.Value;
                Items.Add(naipvvm);
            }

            if (Items.All(x=>x.IsSelected == false) && Items.Count > 0) {
                Items[0].IsSelected = true;
            }

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(CurrentValue));
            OnPropertyChanged(nameof(DefaultValue));

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            IsBusy = false;
        }


        public async Task<MpEnumerableParameterValueViewModel> CreateAnalyticItemParameterValueViewModel(
            int idx,
            MpAnalyticItemParameterValueFormat valueSeed) {
            var naipvvm = new MpEnumerableParameterValueViewModel(this);
            naipvvm.PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
            await naipvvm.InitializeAsync(idx, valueSeed);
            return naipvvm;
        }

        #endregion

        #region Protected Methods
        #endregion
    }
}
