using MonkeyPaste;
using MonkeyPaste.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpMultiSelectListBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpEnumerableParameterValueViewModel> Items { get; set; } = new ObservableCollection<MpEnumerableParameterValueViewModel>();

        public virtual IList<MpEnumerableParameterValueViewModel> SelectedItems {
            get => Items.Where(x => x.IsSelected).ToList();
            set => Items.ForEach(x => x.IsSelected = value.Contains(x));
        }
        
        #endregion
        
        #region State

        #endregion

        #region Model

        public override string CurrentValue => string.Join(",", SelectedItems.Select(x => x.Value));

        public override string DefaultValue => Items.FirstOrDefault(x => x.IsDefault)?.Value;

        #endregion

        #endregion

        #region Constructors

        public MpMultiSelectListBoxParameterViewModel() : base () { }

        public MpMultiSelectListBoxParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

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
                naipvvm.IsSelected = aipv.Value.Contains(paramVal.value);
                Items.Add(naipvvm);
            }

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(CurrentValue));
            OnPropertyChanged(nameof(DefaultValue));

            OnPropertyChanged(nameof(SelectedItems));

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
