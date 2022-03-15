using MonkeyPaste;
using MonkeyPaste.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpEditableListBoxParameterViewModel : MpEnumerableParameterViewModel {
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

        public MpEditableListBoxParameterViewModel() : base () { }

        public MpEditableListBoxParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) {
            PropertyChanged += MpListBoxParameterViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aipf, MpAnalyticItemPresetParameterValue aipv) {
            IsBusy = true;

            Parameter = aipf;
            PresetValue = aipv;

            Items.CollectionChanged += Items_CollectionChanged;
            Items.Clear();
            if (!string.IsNullOrEmpty(PresetValue.Value)) {
                //when preset value exists add it to parameter and mark the preset value as default
                ParameterFormat.values.ForEach(x => x.isDefault = false);

                var presetValParts = PresetValue.Value.ToListFromCsv(); //ParameterValue.Value.Split(new string[] { "," }, StringSplitOptions.None).ToList();
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
                Items.Add(naipvvm);
            }

            if(Items.Count > 0) {
                Items[0].IsSelected = true;
            }

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

        private void MpListBoxParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(Items):
                    OnPropertyChanged(nameof(CurrentValue));
                    break;
                case nameof(CurrentValue):
                    HasModelChanged = true;
                    break;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
        }

        
        #endregion

        #region Commands

        public ICommand AddValueCommand => new MpAsyncCommand(
            async() => {
                IsBusy = true;

                var paramVal = new MpAnalyticItemParameterValueFormat() {
                    isDefault = true
                };
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, paramVal);
                Items.Add(naipvvm);

                Items.ForEach(x => x.IsSelected = x == naipvvm);

                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(CurrentValue));
                HasModelChanged = true;

                IsBusy = false;
            });

        public ICommand RemoveValueCommand => new MpCommand<object>(
             (args) => {
                IsBusy = true;
                var epvvm = args as MpEnumerableParameterValueViewModel;
                
                int idxToRemove = Items.IndexOf(epvvm);
                if(idxToRemove >= 0) {
                    if(Items.Count == 1) {
                         Items[0].Value = string.Empty;
                     } else {
                         Items.RemoveAt(idxToRemove);
                     }
                }

                OnPropertyChanged(nameof(Items));

                if(Items.Count > 0) {
                     idxToRemove = Math.Max(0, idxToRemove - 1);
                     Items.ForEach(x => x.IsSelected = Items.IndexOf(x) == idxToRemove);
                }
                OnPropertyChanged(nameof(SelectedItem));

                HasModelChanged = true;
                IsBusy = false;
            },(args)=>Items.Count > 0);

        #endregion
    }
}
