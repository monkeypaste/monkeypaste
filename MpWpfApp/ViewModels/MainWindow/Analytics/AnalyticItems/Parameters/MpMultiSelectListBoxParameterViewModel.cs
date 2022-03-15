using MonkeyPaste;
using MonkeyPaste.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpMultiSelectListBoxParameterViewModel : MpEnumerableParameterViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models
        #endregion
        
        #region State

        #endregion

        #region Model


        #endregion

        #endregion

        #region Constructors

        public MpMultiSelectListBoxParameterViewModel() : base () { }

        public MpMultiSelectListBoxParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemPresetParameterValue aipv) {
            IsBusy = true;

            //Parameter = aipf;
            PresetValue = aipv;

            Items.Clear();
            if (!string.IsNullOrEmpty(PresetValue.Value)) {
                ParameterFormat.values.ForEach(x => x.isDefault = false);

                var presetValParts = PresetValue.Value.Split(new string[] { "," }, StringSplitOptions.None).ToList();
                for (int i = 0; i < presetValParts.Count; i++) {
                    string presetValStr = presetValParts[i];
                    var paramVal = ParameterFormat.values.FirstOrDefault(x => x.value == presetValStr);
                    if (paramVal == null) {
                        paramVal = new MpAnalyticItemParameterValueFormat() {
                            isDefault = true,
                            label = presetValStr,
                            value = presetValStr
                        };
                        if (i >= ParameterFormat.values.Count) {
                            ParameterFormat.values.Add(paramVal);
                        }
                    } else {
                        paramVal.isDefault = true;
                    }                    
                }
            }

            foreach (var paramVal in ParameterFormat.values) {
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
