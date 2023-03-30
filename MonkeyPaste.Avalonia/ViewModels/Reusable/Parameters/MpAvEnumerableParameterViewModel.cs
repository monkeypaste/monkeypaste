using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvEnumerableParameterViewModel : MpAvParameterViewModelBase {
        #region Private Variables
        private List<MpAvEnumerableParameterValueViewModel> _lastSelectedValues;
        #endregion

        #region Properties

        #region View Models

        public virtual ObservableCollection<MpAvEnumerableParameterValueViewModel> Items { get; set; } = new ObservableCollection<MpAvEnumerableParameterValueViewModel>();

        private MpAvEnumerableParameterValueViewModel _selectedItem;
        public virtual MpAvEnumerableParameterValueViewModel SelectedItem {
            get {
                if (IsMultiValue) {
                    return SelectedItems.FirstOrDefault();
                }
                return _selectedItem;
            }
            set {
                if (IsMultiValue) {
                    MpDebug.Break("Need to set items not item, this is multivalue");
                    return;
                }
                if (SelectedItem != value) {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }
        //public virtual MpAvEnumerableParameterValueViewModel SelectedItem {
        //    get => Items.FirstOrDefault(x => x.IsSelected);
        //    set {
        //        if (SelectedItem != value) {
        //            if (value == null) {
        //                Items.ForEach(x => x.IsSelected = false);
        //            } else {
        //                Items.ForEach(x => x.IsSelected = x.ValueIdx == value.ValueIdx);
        //            }
        //            OnPropertyChanged(nameof(SelectedItems));
        //            OnPropertyChanged(nameof(SelectedItem));
        //            OnPropertyChanged(nameof(CurrentValue));
        //        }
        //    }

        private ObservableCollection<MpAvEnumerableParameterValueViewModel> _selectedItems;
        public virtual ObservableCollection<MpAvEnumerableParameterValueViewModel> SelectedItems {
            get {
                if (_selectedItems == null) {
                    _selectedItems = new ObservableCollection<MpAvEnumerableParameterValueViewModel>();
                }
                if (!IsMultiValue) {
                    if (!_selectedItems.Contains(SelectedItem)) {
                        _selectedItems.Clear();
                        if (SelectedItem != null) {
                            _selectedItems.Add(SelectedItem);
                        }
                    }
                }
                return _selectedItems;
            }
        }
        //public virtual IList<MpAvEnumerableParameterValueViewModel> SelectedItems {
        //    get {
        //        if (ControlType == MpParameterControlType.EditableList) {
        //            return Items;
        //        }
        //        return Items.Where(x => x.IsSelected).ToList();
        //    }
        //    set {
        //        if (SelectedItems != value) {
        //            if (value == null) {
        //                Items.ForEach(x => x.IsSelected = false);
        //            } else {
        //                Items.ForEach(x => x.IsSelected = value.Contains(x));
        //            }

        //            OnPropertyChanged(nameof(SelectedItems));
        //            OnPropertyChanged(nameof(SelectedItem));
        //            OnPropertyChanged(nameof(CurrentValue));
        //        }
        //    }
        //}

        #endregion

        #region State

        public override bool HasModelChanged //=>
                                             //SelectedItems.Difference(_lastSelectedValues).Count() > 0; //{
          {
            get {
                if (IsParameterDropDownOpen) {
                    return false;
                }
                var selected_vals = SelectedItems.Select(x => x.Value);
                var last_vals = _lastSelectedValues.Select(x => x.Value);
                if (selected_vals.Count() != last_vals.Count()) {
                    // HACK attempt to work around SelectedItem comboBox bug
                    return true;
                }
                var diffs = selected_vals.Difference(last_vals);
                if (diffs.Any()) {
                    return true;
                }
                return false;
            }
        }
        public bool IsParameterDropDownOpen { get; set; }

        public MpCsvFormatProperties CsvProperties {
            get {
                // NOTE since enumerable param's are stored as single csv string
                // to avoid any escape/encoding issues (esp with json!) when multiple values maybe present
                // all values are stored base64 encoded and decoded at runtime

                // NOTE2 since values maybe predefined MpCsvFormatProperties detects base64 when decoding
                // so a caveat here is 
                //return ControlType == MpParameterControlType.List ?
                //    MpCsvFormatProperties.Default : MpCsvFormatProperties.DefaultBase64Value;
                if (ParameterFormat == null) {
                    return MpCsvFormatProperties.Default;
                }
                return ParameterFormat.CsvProps;
            }
        }

        public int SelectedItemIdx {
            get {
                return Items.IndexOf(SelectedItem);
            }
            set {
                if (SelectedItemIdx != value) {
                    Items.ForEach((x, idx) => x.IsSelected = value == idx);
                    OnPropertyChanged(nameof(SelectedItemIdx));
                }
            }
        }

        public string SelectedValue =>
            SelectedItem == null ? string.Empty : SelectedItem.Value;
        #endregion

        #region Model

        #region Db
        #endregion

        #endregion

        #endregion

        #region Constructors

        public MpAvEnumerableParameterViewModel() : this(null) { }

        public MpAvEnumerableParameterViewModel(MpViewModelBase parent) : base(parent) {
            PropertyChanged += MpEnumerableParameterViewModel_PropertyChanged;
        }



        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;
            await base.InitializeAsync(aipv);
            if (_selectedItems == null) {
                // only do on first init, needs model so can't be in constructor
                SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
            }
            Items.Clear();

            if (aipv.ParameterHostId == 30 && aipv.ParamId == "1") {

            }

            List<string> selectedValues = new List<string>();

            if (!string.IsNullOrEmpty(PresetValueModel.Value)) {
                selectedValues = PresetValueModel.Value.ToListFromCsv(CsvProperties);
            } else {
                selectedValues = DefaultValues;
            }

            foreach (var paramVal in ParameterFormat.values) {
                int selectedIdx = selectedValues.IndexOf(paramVal.value);
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, paramVal.label, paramVal.value, selectedIdx >= 0);
                if (selectedIdx >= 0) {
                    selectedValues.RemoveAt(selectedIdx);
                }
                Items.Add(naipvvm);
            }

            // NOTE this secondary add is very editable lists where values wouldn't be found in parameter format..
            // reverse selected values to retain order (valueIdx increment maybe unnecessary, don't remember why its necessary but this retains order)
            selectedValues.Reverse();
            foreach (var selectValueStr in selectedValues) {
                if (ControlType != MpParameterControlType.EditableList) {
                    MpDebug.Break("whys this goin down?");
                }
                // for new values add them to front of Items
                Items.ForEach(x => x.ValueIdx++);
                //for new values from preset add and select
                var nsaipvvm = await CreateAnalyticItemParameterValueViewModel(0, selectValueStr, selectValueStr, true);
                Items.Insert(0, nsaipvvm);
            }


            if (Items.All(x => x.IsSelected == false) && Items.Count > 0) {
                Items[0].IsSelected = true;
            }

            OnPropertyChanged(nameof(Items));
            CurrentValue = SelectedItems.Select(x => x.Value).ToList().ToCsv(CsvProperties);

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
            SetLastValue(SelectedItems);

            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(SelectedItems));
            OnPropertyChanged(nameof(SelectedItemIdx));

            Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));

            //Selection.Source = SelectedItems;

            IsBusy = false;
        }


        public async Task<MpAvEnumerableParameterValueViewModel> CreateAnalyticItemParameterValueViewModel(
            int idx, string label, string value, bool isSelected) {
            var naipvvm = new MpAvEnumerableParameterValueViewModel(this);
            naipvvm.PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
            await naipvvm.InitializeAsync(idx, label, value, isSelected);
            return naipvvm;
        }

        #endregion

        #region Protected Methods

        protected override void SetLastValue(object value) {
            if (value is string val_str) {
                // for single selectables
                var epvvm = new MpAvEnumerableParameterValueViewModel() {
                    Value = val_str
                };
                SetLastValue(new[] { epvvm });
            } else if (value is IEnumerable<MpAvEnumerableParameterValueViewModel> val_vml) {
                _lastSelectedValues = val_vml.ToList();
            } else {
                _lastSelectedValues = new List<MpAvEnumerableParameterValueViewModel>();
            }
        }

        protected override void RestoreLastValue() {
            if (_lastSelectedValues is IEnumerable<MpAvEnumerableParameterValueViewModel> val_vml) {
                SelectedItems.Clear();
                SelectedItems.AddRange(val_vml);//= new // val_vml.ToList();
            } else {
                SelectedItems.Clear();
            }
        }

        #endregion

        #region Private Methods
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            //OnPropertyChanged(nameof(CurrentValue));
            //HasModelChanged = true;
        }

        private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(SelectedItem));
        }
        private void MpEnumerableParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(CurrentValue):
                    //SelectValueCommand.Execute(CurrentValue);
                    break;
                case nameof(HasModelChanged):
                case nameof(SelectedItem):
                case nameof(SelectedItems):
                case nameof(Items):
                    if (Parent is MpISaveOrCancelableViewModel socvm) {
                        socvm.OnPropertyChanged(nameof(socvm.CanSaveOrCancel));
                    }
                    break;
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

                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, string.Empty, string.Empty, false);
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
