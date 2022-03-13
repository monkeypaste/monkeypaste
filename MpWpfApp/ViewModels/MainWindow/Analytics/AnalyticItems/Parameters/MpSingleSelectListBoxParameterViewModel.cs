using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpSingleSelectListBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpEnumerableParameterValueViewModel> Items { get; set; } = new ObservableCollection<MpEnumerableParameterValueViewModel>();

        public MpEnumerableParameterValueViewModel SelectedItem {
            get => Items.FirstOrDefault(x => x.IsSelected);
            set => Items.ForEach(x => x.IsSelected = x == value);
        }
        
        #endregion
        
        #region State

        #endregion

        #region Model

        public override string CurrentValue {
            get {
                if(SelectedItem == null) {
                    return string.Empty;
                }
                return SelectedItem.Value;
            }
            set {
                if(CurrentValue != value) {
                    Items.ForEach(x => x.IsSelected = x.Value == value);
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CurrentValue));
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public override string DefaultValue => Items.FirstOrDefault(x => x.IsDefault)?.Value;


        #endregion

        #endregion

        #region Constructors

        public MpSingleSelectListBoxParameterViewModel() : base () { }

        public MpSingleSelectListBoxParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aipf, MpAnalyticItemPresetParameterValue aipv) {
            IsBusy = true;

            Parameter = aipf;
            ParameterValue = aipv;

            Items.Clear();
            if (!string.IsNullOrEmpty(ParameterValue.Value)) {
                Parameter.values.ForEach(x => x.isDefault = false);

                var presetValParts = ParameterValue.Value.Split(new string[] { "," }, StringSplitOptions.None).ToList();
                for (int i = 0; i < presetValParts.Count; i++) {
                    string presetValStr = presetValParts[i];
                    var paramVal = Parameter.values.FirstOrDefault(x => x.value == presetValStr);
                    if (paramVal == null) {
                        paramVal = new MpAnalyticItemParameterValueFormat() {
                            isDefault = true,
                            label = presetValStr,
                            value = presetValStr
                        };
                        if (i >= Parameter.values.Count) {
                            Parameter.values.Add(paramVal);
                        }
                    } else {
                        paramVal.isDefault = true;
                    }                    
                }
            }

            foreach (var paramVal in Parameter.values) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, paramVal);
                Items.Add(naipvvm);
            }

            Items.ForEach(x => x.IsSelected = x.Value == DefaultValue);

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(CurrentValue));
            OnPropertyChanged(nameof(DefaultValue));

            OnPropertyChanged(nameof(SelectedItem));

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

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
