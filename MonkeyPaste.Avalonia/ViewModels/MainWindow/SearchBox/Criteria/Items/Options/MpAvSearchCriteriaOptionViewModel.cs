using Avalonia.Controls;
using MonkeyPaste.Common;
using MonoMac.Darwin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchCriteriaOptionViewModel :
        MpViewModelBase<MpAvSearchCriteriaOptionViewModel>,
        MpISliderViewModel,
        MpITreeItemViewModel,
        MpIHoverableViewModel {
        #region Private Variables

        #endregion

        #region Interfaces

        #region MpISliderViewModel Implementation

        public double SliderValue {
            get {
                // NOTE presuming this is only needed for color distance
                // and unit value is scaled to 0-255
                if (string.IsNullOrWhiteSpace(Value)) {
                    Value = "0";
                }
                try {
                    double unit_val = double.Parse(Value);
                    return Math.Round(unit_val * byte.MaxValue);
                }
                catch {
                    return 0;
                }
            }
            set {
                if (SliderValue != value) {
                    Value = (value / byte.MaxValue).ToString();
                }
            }
        }
        public double MinValue =>
            0;
        public double MaxValue =>
            MpColorExtensions.MAX_EUCLID_COLOR_DIST;
        public int Precision =>
            0;
        #endregion

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
                if (value != null) {
                    value.IsSelected = true;
                }
            }
        }

        #endregion

        #region State

        public bool IsValueChanging { get; set; }
        public bool IsColorOption =>
            FilterValue.HasFlag(MpContentQueryBitFlags.Rgba) ||
            FilterValue.HasFlag(MpContentQueryBitFlags.Hex);

        public bool IsValueOption =>
            !UnitType.HasFlag(MpSearchCriteriaUnitFlags.Enumerable);

        public bool IsRootMultiValueOption =>
            IsValueOption && Items.Count > 0;

        public int SelectedItemIdx {
            get {
                if (Items == null ||
                    Items.Count == 0) {
                    return -1;
                }
                if (SelectedItem == null) {
                    return 0;
                }
                return Items.IndexOf(SelectedItem);
            }
            set {
                if (SelectedItemIdx != value) {
                    if (value < 0 || value >= Items.Count) {
                        SelectedItem = null;
                    } else {
                        SelectedItem = Items[value];
                    }
                    OnPropertyChanged(nameof(SelectedItemIdx));
                }
            }
        }

        public bool IsDropDownOpen { get; set; }
        public bool IsDropDownItemVisible {
            get {
                if (Parent == null || !Parent.IsDropDownOpen) {
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
                // NOTE unused but maybe useful if
                // dynamic menu is structure is added
                // to determine selection path by reflection
                if (ItemsOptionType == null ||
                    SelectedItemIdx < 0) {
                    return null;
                }
                if (ItemsOptionType.IsEnum) {
                    try {
                        string selected_item_name = Enum.GetNames(ItemsOptionType)[SelectedItemIdx];
                        if (Enum.TryParse(ItemsOptionType, selected_item_name, out var enumObj)) {
                            return enumObj;
                        }
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Error converting selected item to enum type.", ex);
                    }
                } else {
                    // currently will occur on collection or device name
                }

                MpDebug.Break();
                return null;

            }
        }

        public bool IsValid =>
            string.IsNullOrEmpty(ValidationText);

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

        public string UnitLabel { get; set; }
        public string ValidationText { get; set; }

        #endregion

        #region Model

        #region Multi Values 
        public MpCsvFormatProperties CsvFormatProperties { get; set; }

        private string[] _values;
        public string[] Values {
            get {
                if (_values == null) {
                    _values = Enumerable.Repeat(string.Empty, 4).ToArray();
                }
                return _values;
            }
            set {
                if (_values != value) {
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
                if (FilterValue.HasMultiValue()) {
                    if (Values.All(x => string.IsNullOrEmpty(x))) {
                        return string.Empty;
                    }
                    // NOTE changed this from base64 to per opt type, not sure why was base64 but will default to plain now
                    return Values.ToCsv(CsvFormatProperties);
                    //return $"({Value1},{Value2},{Value3},{Value4})";
                }
                return _value;
            }
            set {
                if (_value != value) {
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

        public MpSearchCriteriaUnitFlags UnitType { get; set; }

        public MpContentQueryBitFlags FilterValue { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvSearchCriteriaOptionViewModel() : this(null, null) { }

        public MpAvSearchCriteriaOptionViewModel(MpAvSearchCriteriaItemViewModel host, MpAvSearchCriteriaOptionViewModel parent) : base(parent) {
            // NOTE IsBusy defaults to true since opt doesn't have InitAsync
            //IsBusy = true;
            PropertyChanged += MpSearchParameterViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            HostCriteriaItem = host;
        }

        #endregion

        #region Private Methods

        private void MpSearchParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectedItem):
                    Items.ForEach(x => x.IsSelected = x == SelectedItem);
                    if (HostCriteriaItem == null ||
                        HostCriteriaItem.IsBusy ||
                        IsBusy) {
                        break;
                    }
                    HostCriteriaItem.NotifyOptionOrValueChanged(this);
                    break;
                case nameof(IsSelected):
                    HostCriteriaItem.OnPropertyChanged(nameof(HostCriteriaItem.Items));
                    HostCriteriaItem.OnPropertyChanged(nameof(HostCriteriaItem.IsInputVisible));
                    OnPropertyChanged(nameof(Value));
                    if (!IsSelected) {
                        if (IsRootMultiValueOption) {
                            SelectedItem = null;
                        } else {
                            Value = null;
                        }

                    }
                    break;
                case nameof(Value1):
                case nameof(Value2):
                case nameof(Value3):
                case nameof(Value4):
                    OnPropertyChanged(nameof(Values));
                    break;
                case nameof(Values):
                case nameof(Value):
                case nameof(IsChecked):
                case nameof(IsChecked2):
                case nameof(IsValueChanging):
                    if (!IsSelected) {
                        break;
                    }
                    if (IsRootMultiValueOption) {
                        SelectedItem = Items[0];
                    }

                    if (HostCriteriaItem == null || IsBusy) {
                        // should only be busy during initial load 
                        // where collection handles ntf from adv search opened msg
                        break;
                    }
                    if (Validate() && !IsValueChanging) {
                        // only notify when values are valid
                        HostCriteriaItem.NotifyOptionOrValueChanged(this);
                    }


                    break;
                case nameof(IsBusy):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(IsAnyBusy));
                    break;
                case nameof(IsDropDownOpen):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsDropDownItemVisible)));
                    break;
                case nameof(ValidationText):
                    if (IsValid) {
                        IsValids.ForEach(x => x = true);
                    }
                    break;
            }
        }

        private bool Validate() {
            ValidationText = string.Empty;

            if (string.IsNullOrEmpty(Value)) {
                return true;
            }

            if (UnitType.HasUnsignedNumeric()) {
                string non_numeric_msg = "Value must contain only numbers or '.-'";
                var notNumRegEx = MpRegEx.RegExLookup[MpRegExType.Is_NOT_Number];


                Tuple<double, double> value_range = UnitType.GetNumericBounds();

                string out_of_range_msg = $"Value must be from {value_range.Item1} to {value_range.Item2}";
                if (FilterValue.HasMultiValue()) {
                    // multi-value 
                    var is_not_number_idxs = Values.Where(x => notNumRegEx.IsMatch(x)).Select(x => Values.IndexOf(x)).ToList();
                    if (is_not_number_idxs.Count > 0) {
                        // non-number characters
                        IsValids.ForEach((x, idx) => x = is_not_number_idxs.Contains(idx));
                        ValidationText = non_numeric_msg;
                    } else {
                        // out-of-range values
                        try {
                            var numeric_vals = Values.Select(x => double.Parse(x)).ToList();
                            var out_of_range_idxs = numeric_vals.Where(x => x < value_range.Item1 || x > value_range.Item2).Select(x => numeric_vals.IndexOf(x)).ToList();
                            if (out_of_range_idxs.Count > 0) {
                                IsValids.ForEach((x, idx) => x = out_of_range_idxs.Contains(idx));
                                ValidationText = out_of_range_msg;
                            }
                        }
                        catch (Exception ex) {
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
                            if (numeric_val < value_range.Item1 || numeric_val > value_range.Item2) {
                                ValidationText = out_of_range_msg;
                            }
                        }
                        catch (Exception ex) {
                            MpConsole.WriteTraceLine($"Error converting values '{Value}'.", ex);
                            ValidationText = "Unknown error";
                        }
                    }
                }
            } else if (UnitType.HasFlag(MpSearchCriteriaUnitFlags.Hex)) {
                string invalid_hex_msg = "Must be a hex (6 or 8 value) string starting with '#'";
                if (!Value.IsStringHexColor()) {
                    ValidationText = invalid_hex_msg;
                }
            }

            return IsValid;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            //OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SelectedItemIdx));
        }

        #endregion


        #region Commands 

        public ICommand ShowColorPickerCommand => new MpAsyncCommand<object>(
            async (args) => {
                string selected_color = Value;
                if (FilterValue.HasFlag(MpContentQueryBitFlags.Rgba)) {
                    selected_color = MpColorHelpers.ParseHexFromString(
                        $"({string.Join(",", Values)})");
                }
                Window owner = TopLevel.GetTopLevel(MpAvMainView.Instance) as Window;
                if (HostCriteriaItem.Parent.IsCriteriaWindowOpen) {
                    owner = MpAvWindowManager.AllWindows.FirstOrDefault(x => x.DataContext == HostCriteriaItem.Parent);
                }
                var result = await Mp.Services.CustomColorChooserMenuAsync.ShowCustomColorMenuAsync(
                    selectedColor: selected_color,
                    title: "Pick Match color",
                    owner: owner);

                if (string.IsNullOrEmpty(result)) {
                    // cancel
                    return;
                }
                if (FilterValue.HasFlag(MpContentQueryBitFlags.Rgba)) {
                    var c = new MpColor(result);
                    Values = c.Channels.Select(x => x.ToString()).ToArray();
                } else {
                    Value = result;
                }
            });
        #endregion
    }
}
