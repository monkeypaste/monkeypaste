using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchCriteriaOptionViewModel : 
        MpViewModelBase<MpAvSearchCriteriaOptionViewModel>,
        MpITreeItemViewModel, 
        MpIHoverableViewModel {
        #region Private Variables

        #endregion

        #region Interfaces

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpITreeItemViewModel Implementation

        public bool IsExpanded { get; set; } = true;
        public MpITreeItemViewModel ParentTreeItem => Parent;
        public IEnumerable<MpITreeItemViewModel> Children => Items;

        #endregion

        #endregion


        #region Properties

        #region View Models

        public MpAvSearchCriteriaItemViewModel HostCriteriaItem { get; set; }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> Items { get; set; } = new ObservableCollection<MpAvSearchCriteriaOptionViewModel>();

        public MpAvSearchCriteriaOptionViewModel SelectedItem {
            get => Items.FirstOrDefault(x => x.IsSelected);
            set {
                Items.ForEach(x => x.IsSelected = false);
                if(value != null) {
                    value.IsSelected = true;
                }
            }
        }

        #endregion

        #region State

        public int SelectedItemIdx {
            get {
                if(SelectedItem == null) {
                    return -1;
                }
                return Items.IndexOf(SelectedItem);
            }
        }

        public bool IsDropDownOpen { get; set; }
        public bool IsVisible {
            get {
                if(Parent == null || !Parent.IsDropDownOpen) {
                    return true;
                }
                // hide blank opt from drop down
                return Label != MpAvSearchCriteriaItemViewModel.DEFAULT_OPTION_LABEL;
            }
        }
        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsAnyBusy);

        public bool HasChildren => Items.Count > 0;

        public bool IsSelected { get; set; } = false;

        public Type ItemsOptionType { get; set; }

        public object SelectedItemPathObj {
            get {
                if(ItemsOptionType == null || SelectedItemIdx < 0) {
                    return null;
                }
                try {
                    string selected_item_name = Enum.GetNames(ItemsOptionType)[SelectedItemIdx];
                    if (Enum.TryParse(ItemsOptionType, selected_item_name, out var enumObj)) {
                        return enumObj;
                    }
                } catch(Exception ex) {
                    MpConsole.WriteTraceLine($"Error converting selected item to enum type.", ex);
                }
                Debugger.Break();
                return null;
                
            }
        }

        public bool IsValid => string.IsNullOrEmpty(ValidationText);

        #region Multi Value IsValid

        public bool IsValid1 { get; set; } = true;
        public bool IsValid2 { get; set; } = true;
        public bool IsValid3 { get; set; } = true;
        public bool IsValid4 { get; set; } = true;

        public IEnumerable<bool> IsValids =>
            new bool[] { IsValid1, IsValid2, IsValid3, IsValid4 };
        #endregion

        #endregion

        #region Appearance

        public string Label { get; set; }

        public string ValidationText { get; set; }

        #endregion

        #region Model

        #region Multi Values 

        private string[] _values;
        public string[] Values {
            get {
                if (_values == null) {
                    _values = Enumerable.Repeat(string.Empty, 4).ToArray();
                }
                return _values;
            }
            set {
                if(_values != value) {
                    _values = value;
                    OnPropertyChanged(nameof(Values));
                }
            }
        }
        public string Value1 {
            get => Values[0];
            set {
                if (Values[0] != value) {
                    Values[0] = value;
                    OnPropertyChanged(nameof(Value1));
                    OnPropertyChanged(nameof(Values));
                }
            }
        }
        public string Value2 {
            get => Values[1];
            set {
                if (Values[1] != value) {
                    Values[1] = value;
                    OnPropertyChanged(nameof(Value2));
                    OnPropertyChanged(nameof(Values));
                }
            }
        }
        public string Value3 {
            get => Values[2];
            set {
                if (Values[2] != value) {
                    Values[2] = value;
                    OnPropertyChanged(nameof(Value3));
                    OnPropertyChanged(nameof(Values));
                }
            }
        }
        public string Value4 {
            get => Values[3];
            set {
                if (Values[3] != value) {
                    Values[3] = value;
                    OnPropertyChanged(nameof(Value4));
                    OnPropertyChanged(nameof(Values));
                }
            }
        }
        #endregion

        private string _value;
        public string Value { 
            get {
                if(FilterValue.IsMultiValue()) {
                    if(Values.All(x=>string.IsNullOrEmpty(x))) {
                        return string.Empty;
                    }
                    return Values.ToCsv(MpCsvFormatProperties.DefaultBase64Value);
                }
                return _value;
            }
            set {
                if(_value != value) {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        
        }


        public bool IsCheckable => 
            UnitType.HasFlag(MpSearchCriteriaUnitFlags.Text) && 
            !UnitType.HasFlag(MpSearchCriteriaUnitFlags.RegEx);

        public bool IsChecked { get; set; }
        public bool IsChecked2 { get; set; }

        public bool IsEnabled { get; set; } = true;

        public bool IsLeafOption 
            => Items == null || Items.Count == 0;
        public MpSearchCriteriaUnitFlags UnitType { get; set; }

        public MpContentQueryBitFlags FilterValue { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvSearchCriteriaOptionViewModel() : base(null) { }

        public MpAvSearchCriteriaOptionViewModel(MpAvSearchCriteriaItemViewModel host,MpAvSearchCriteriaOptionViewModel parent) : base(parent) {            
            PropertyChanged += MpSearchParameterViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            HostCriteriaItem = host;
        }

        public async Task InitializeAsync(string opt_path, int idx) {
            IsBusy = true;

            await Task.Delay(1);
            bool? opt_checked = null;
            int cur_opt_sel_idx = 0;
            Type enumType = null;

            var opt_parts = opt_path.Split("|");
            if (opt_parts.Length < 2) {
                throw new Exception($"Criteria option part parse error, each part needs full enum type and value part seperated by '|'");
            }

            // GET ENUM TYPE
            string enumFullTypeName = opt_parts[0];
            string enumAssemblyName = string.Join(".", enumFullTypeName.SplitNoEmpty(".").SkipLast(1));

            var enumAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == enumAssemblyName);
            if (enumAssembly == null) {
                throw new Exception($"Error cannot find assembly for type '{enumFullTypeName}'.");
            }

            try {
                enumType = enumAssembly.GetType(enumFullTypeName);
                string enum_value = opt_parts[1];
                cur_opt_sel_idx = Enum.GetNames(enumType).IndexOf(enum_value);
            }
            catch (Exception ex) {
                throw new Exception($"Error finding type '{enumFullTypeName}'.", ex);
            }

            // SET FILTER FLAG
            string filter_name;
            if (opt_parts.Length > 3) {
                // HANDLE LEAF
                var vals = opt_parts[2].ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value);
                if (vals.Count > 1) {
                    vals.ForEach((x, idx) => Values[idx] = x);
                } else if (vals.Count > 0) {
                    Value = vals.FirstOrDefault();
                } else {
                    Value = string.Empty;
                }

                opt_checked = bool.Parse(opt_parts[3]);

                filter_name = opt_parts[4];
            } else {
                //Values.ForEach(x => x = null);
                filter_name = opt_parts[2];
            }
            IsChecked = opt_checked.IsTrue();
            FilterValue = filter_name.ToEnum<MpContentQueryBitFlags>();

            // SET CUR SELECTION
            if (cur_opt_sel_idx >= Items.Count) {
                MpConsole.WriteTraceLine($"Error criteria path mismatch option vm '{this}' cannot select idx '{cur_opt_sel_idx}' only has '{Items.Count}' options. Ignoring and selecting default");
                cur_opt_sel_idx = Items.Count > 0 ? 0 : -1;
            }

            ItemsOptionType = enumType;
            SelectedItem = cur_opt_sel_idx >= 0 ? Items[cur_opt_sel_idx] : null;

            while (Items.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private void MpSearchParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedItem):
                    Items.ForEach(x => x.IsSelected = x == SelectedItem);
                    break;
                case nameof(IsSelected):
                    HostCriteriaItem.OnPropertyChanged(nameof(HostCriteriaItem.Items));
                    HostCriteriaItem.OnPropertyChanged(nameof(HostCriteriaItem.IsInputVisible));
                    //if((Items == null || Items.Count == 0) && !string.IsNullOrEmpty(Value)) {
                    //    if(UnitType != MpSearchCriteriaUnitFlags.EnumerableValue) {
                    //        // what's the unit in this case?
                    //        Debugger.Break();
                    //    }
                    //    OnPropertyChanged(nameof(Value));
                    //    // leaf enumerable value
                    //}

                    OnPropertyChanged(nameof(Value));
                    break;
                case nameof(Value1):
                case nameof(Value2):
                case nameof(Value3):
                case nameof(Value4):
                    OnPropertyChanged(nameof(Values));
                    break;
                case nameof(Values):
                case nameof(Value):
                    if(HostCriteriaItem == null || IsBusy) {
                        // should only be busy during initial load 
                        // where collection handles ntf from adv search opened msg
                        break;
                    }
                    if(Validate()) {
                        // only notify when values are valid
                        HostCriteriaItem.NotifyValueChanged(this);
                    }
                    
                    break;
                case nameof(IsBusy):
                    if(Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(IsAnyBusy));
                    break;
                case nameof(IsDropDownOpen):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsVisible)));
                    break;
                case nameof(ValidationText):
                    if(IsValid) {
                        IsValids.ForEach(x => x = true);
                    }
                    break;
            }
        }

        private bool Validate() {
            ValidationText = string.Empty;

            if(string.IsNullOrEmpty(Value)) {
                return true;
            }

            if(UnitType.IsUnsignedNumeric()) {
                string non_numeric_msg = "Value must contain only numbers or '.-'";
                var notNumRegEx = MpRegEx.RegExLookup[MpRegExType.Is_NOT_Number];


                Tuple<double, double> value_range = UnitType.GetNumericBounds();

                string out_of_range_msg = $"Value must be from {value_range.Item1} to {value_range.Item2}";
                if (FilterValue.IsMultiValue()) {
                    // multi-value 
                    var is_not_number_idxs = Values.Where(x => notNumRegEx.IsMatch(x)).Select(x=>Values.IndexOf(x)).ToList();
                    if(is_not_number_idxs.Count > 0) {
                        // non-number characters
                        IsValids.ForEach((x, idx) => x = is_not_number_idxs.Contains(idx));
                        ValidationText = non_numeric_msg;
                    } else {
                        // out-of-range values
                        try {
                            var numeric_vals = Values.Select(x => double.Parse(x)).ToList();
                            var out_of_range_idxs = numeric_vals.Where(x => x < value_range.Item1 || x > value_range.Item2).Select(x => numeric_vals.IndexOf(x)).ToList();
                            if(out_of_range_idxs.Count > 0) {
                                IsValids.ForEach((x, idx) => x = out_of_range_idxs.Contains(idx));
                                ValidationText = out_of_range_msg;
                            }
                        } catch(Exception ex) {
                            MpConsole.WriteTraceLine($"Error converting values '{Value}'.", ex);
                            ValidationText = "Unknown error";
                        }
                    }
                } else {
                    // single value
                    bool is_not_number = notNumRegEx.IsMatch(Value);
                    if (is_not_number) {
                        // non-number characters
                        ValidationText = non_numeric_msg;
                    } else {
                        // out-of-range values
                        try {
                            double numeric_val = double.Parse(Value);
                            if(numeric_val < value_range.Item1 || numeric_val > value_range.Item2) {
                                ValidationText = out_of_range_msg;
                            }
                        } catch (Exception ex) {
                            MpConsole.WriteTraceLine($"Error converting values '{Value}'.", ex);
                            ValidationText = "Unknown error";
                        }
                    }
                }
            } else if(UnitType.HasFlag(MpSearchCriteriaUnitFlags.Hex)) {
                string invalid_hex_msg = "Must be a hex (6 or 8 value) string starting with '#'";
                if(!Value.IsStringHexColor()) {
                    ValidationText = invalid_hex_msg;
                }
            }

            return IsValid;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            //OnPropertyChanged(nameof(Items));
        }


        #endregion
    }
}
