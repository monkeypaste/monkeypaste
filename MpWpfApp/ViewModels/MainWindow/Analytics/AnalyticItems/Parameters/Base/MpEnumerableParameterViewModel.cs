﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpEnumerableParameterViewModel : MpAnalyticItemParameterViewModelBase {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public virtual ObservableCollection<MpEnumerableParameterValueViewModel> Items { get; set; } = new ObservableCollection<MpEnumerableParameterValueViewModel>();

        public virtual MpEnumerableParameterValueViewModel SelectedItem {
            get => Items.FirstOrDefault(x => x.IsSelected);
            set {
                if (SelectedItem != value) {
                    if (value == null) {
                        Items.ForEach(x => x.IsSelected = false);
                    } else {
                        Items.ForEach(x => x.IsSelected = x.ValueIdx == value.ValueIdx);
                    }
                    OnPropertyChanged(nameof(SelectedItems));
                    OnPropertyChanged(nameof(SelectedItem));
                    OnPropertyChanged(nameof(CurrentValue));
                }
            }
        }

        public virtual IList<MpEnumerableParameterValueViewModel> SelectedItems {
            get {
                if(ParameterFormat.controlType == MpAnalyticItemParameterControlType.EditableList) {
                    return Items;
                }
                return Items.Where(x => x.IsSelected).ToList();
            }
            set //=> Items.ForEach(x => x.IsSelected = value.Contains(x));
                {
                if (SelectedItems != value) {
                    if (value == null) {
                        Items.ForEach(x => x.IsSelected = false);
                    } else {
                        Items.ForEach(x => x.IsSelected = value.Contains(x));
                    }

                    OnPropertyChanged(nameof(SelectedItems));
                    OnPropertyChanged(nameof(SelectedItem));
                    OnPropertyChanged(nameof(CurrentValue));
                }
            }
        }

        #endregion

        #region State

        #endregion

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpEnumerableParameterViewModel() : base () { }

        public MpEnumerableParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) {
            //PropertyChanged += MpEnumerableParameterViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemPresetParameterValue aipv) { 
            IsBusy = true;

            await base.InitializeAsync(aipv);

            Items.Clear();


            List<string> selectedValues = new List<string>();

            if(!string.IsNullOrEmpty(PresetValue.Value)) {
                selectedValues = PresetValue.Value.ToListFromCsv();                
            } else {
                selectedValues = DefaultValues;
            }


            foreach (var paramVal in ParameterFormat.values) {
                int selectedIdx = selectedValues.IndexOf(paramVal.value);
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, paramVal.label,paramVal.value,selectedIdx >= 0);                
                if(selectedIdx >= 0) {
                    selectedValues.RemoveAt(selectedIdx);
                }
                Items.Add(naipvvm);
            }

            foreach(var selectValueStr in selectedValues) {
                // for new values add them to front of Items
                Items.ForEach(x => x.ValueIdx++);
                //for new values from preset add and select
                var nsaipvvm = await CreateAnalyticItemParameterValueViewModel(0, selectValueStr, selectValueStr, true);
                Items.Insert(0, nsaipvvm);
            }

            if (Items.All(x=>x.IsSelected == false) && Items.Count > 0) {
                Items[0].IsSelected = true;
            }

            OnPropertyChanged(nameof(Items));
            CurrentValue = SelectedItems.Select(x => x.Value).ToList().ToCsv();


            //Items.CollectionChanged += Items_CollectionChanged;

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            IsBusy = false;
        }


        public async Task<MpEnumerableParameterValueViewModel> CreateAnalyticItemParameterValueViewModel(
            int idx, string label, string value, bool isSelected) {
            var naipvvm = new MpEnumerableParameterValueViewModel(this);
            naipvvm.PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
            await naipvvm.InitializeAsync(idx, label,value,isSelected);
            return naipvvm;
        }

        #endregion

        #region Private Methods
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            //OnPropertyChanged(nameof(CurrentValue));
            //HasModelChanged = true;
        }

        private void MpEnumerableParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch(e.PropertyName) {
            //    case nameof(SelectedItem):
            //    case nameof(SelectedItems):
            //    case nameof(Items):
            //        OnPropertyChanged(nameof(CurrentValue));
            //        HasModelChanged = true;
            //        break;
            //}
        }

        #endregion

        #region Commands

        public ICommand AddValueCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;

                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, string.Empty,string.Empty,false);
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
                 if (idxToRemove >= 0) {
                     if (Items.Count == 1) {
                         Items[0].Value = string.Empty;
                     } else {
                         Items.RemoveAt(idxToRemove);
                     }
                 }

                 OnPropertyChanged(nameof(Items));

                 if (Items.Count > 0) {
                     idxToRemove = Math.Max(0, idxToRemove - 1);
                     Items.ForEach(x => x.IsSelected = Items.IndexOf(x) == idxToRemove);
                 }
                 OnPropertyChanged(nameof(SelectedItem));

                 HasModelChanged = true;
                 IsBusy = false;
             }, (args) => Items.Count > 0);

        #endregion
    }
}