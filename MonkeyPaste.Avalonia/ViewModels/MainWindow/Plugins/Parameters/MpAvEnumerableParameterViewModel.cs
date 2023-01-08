using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using Avalonia.Controls.Selection;
using Avalonia.Controls;

namespace MonkeyPaste.Avalonia {
    public class MpAvEnumerableParameterViewModel : MpAvPluginParameterViewModelBase {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public virtual ObservableCollection<MpAvEnumerableParameterValueViewModel> Items { get; set; } = new ObservableCollection<MpAvEnumerableParameterValueViewModel>();

        public virtual MpAvEnumerableParameterValueViewModel SelectedItem {
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
        public virtual IList<MpAvEnumerableParameterValueViewModel> SelectedItems {
            get {
                if (ControlType == MpPluginParameterControlType.EditableList) {
                    return Items;
                }
                return Items.Where(x => x.IsSelected).ToList();
            }
            set {
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

        public bool IsParameterDropDownOpen { get; set; }

        public MpCsvFormatProperties CsvProperties {
            get {
                // NOTE since enumerable param's are stored as single csv string
                // to avoid any escape/encoding issues (esp with json!) when multiple values maybe present
                // all values are stored base64 encoded and decoded at runtime

                // NOTE2 since values maybe predefined MpCsvFormatProperties detects base64 when decoding
                // so a caveat here is 
                return ControlType == MpPluginParameterControlType.List ?
                    MpCsvFormatProperties.Default : MpCsvFormatProperties.DefaultBase64Value;
            }
        }
        #endregion

        #region Model

        #region Db

        //public override string CurrentValue {
        //    get {
        //        if (PresetValueModel == null) {
        //            return string.Empty;
        //        }

        //        return CsvProperties.DecodeValue(PresetValueModel.Value);
        //    }
        //    set {
        //        if (CurrentValue != value) {
        //            PresetValueModel.Value = CsvProperties.EncodeValue(value);
        //            HasModelChanged = true;
        //            OnPropertyChanged(nameof(CurrentValue));
        //        }
        //    }
        //}
        #endregion

        #endregion

        #endregion

        #region Constructors

        public MpAvEnumerableParameterViewModel() : this(null) { }

        public MpAvEnumerableParameterViewModel(MpIPluginComponentViewModel parent) : base(parent) {
            PropertyChanged += MpEnumerableParameterViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpPluginPresetParameterValue aipv) { 
            IsBusy = true;

            await base.InitializeAsync(aipv);

            Items.Clear();
            if (PresetValueModel.Id == 979) {
                Debugger.Break();
            }

            List<string> selectedValues = new List<string>();

            if(!string.IsNullOrEmpty(PresetValueModel.Value)) {
                selectedValues = PresetValueModel.Value.ToListFromCsv(CsvProperties);                
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

            // reverse selected values to retain order (valueIdx increment maybe unnecessary, don't remember why its necessary but this retains order)
            selectedValues.Reverse();
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
            CurrentValue = SelectedItems.Select(x => x.Value).ToList().ToCsv(CsvProperties);

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(SelectedItems));

            Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
            
            //Selection.Source = SelectedItems;

            IsBusy = false;
        }


        public async Task<MpAvEnumerableParameterValueViewModel> CreateAnalyticItemParameterValueViewModel(
            int idx, string label, string value, bool isSelected) {
            var naipvvm = new MpAvEnumerableParameterValueViewModel(this);
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
            switch (e.PropertyName) {
                //case nameof(SelectedItem):
                //case nameof(SelectedItems):
                //case nameof(Items):
                //    OnPropertyChanged(nameof(CurrentValue));
                //    HasModelChanged = true;
                //    break;
                case nameof(IsParameterDropDownOpen):
                    MpAvMainWindowViewModel.Instance.IsAnyDropDownOpen = IsParameterDropDownOpen;
                    break;
            }
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
                 var epvvm = args as MpAvEnumerableParameterValueViewModel;

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
