﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpComboBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models
        //public MpAnalyticItemParameterValueViewModel DefaultValueViewModel { get; set; }

        public virtual ObservableCollection<MpComboBoxParameterValueViewModel> Items { get; set; } = new ObservableCollection<MpComboBoxParameterValueViewModel>();


        public virtual MpComboBoxParameterValueViewModel SelectedItem {
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

        public override string DefaultValue => Items.FirstOrDefault(x => x.IsDefault)?.Value;


        #endregion

        #endregion

        #region Constructors

        public MpComboBoxParameterViewModel() : base () { }

        public MpComboBoxParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aip, MpAnalyticItemPresetParameterValue aipv) { 
            IsBusy = true;
            
            Parameter = aip;

            Items.Clear();

            foreach (var paramVal in Parameter.values) {
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

            IsBusy = false;
        }


        public async Task<MpComboBoxParameterValueViewModel> CreateAnalyticItemParameterValueViewModel(
            int idx,
            MpAnalyticItemParameterValue valueSeed) {
            var naipvvm = new MpComboBoxParameterValueViewModel(this);
            naipvvm.PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
            await naipvvm.InitializeAsync(idx, valueSeed);
            return naipvvm;
        }

        #endregion

        #region Protected Methods
        #endregion
    }
}