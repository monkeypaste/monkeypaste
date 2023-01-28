using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsAnyBusy);

        public bool HasChildren => Items.Count > 0;

        public bool IsSelected { get; set; } = false;

        #endregion

        #region Appearance

        public string Label { get; set; }

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

        public string Value { get; set; }


        public bool IsCheckable => UnitType.HasFlag(MpSearchCriteriaUnitFlags.Text) && !UnitType.HasFlag(MpSearchCriteriaUnitFlags.RegEx);

        public bool IsChecked { get; set; }

        public bool IsEnabled { get; set; } = true;

        public MpSearchCriteriaUnitFlags UnitType { get; set; }

        public MpContentQueryBitFlags FilterValue { get; private set; }

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
                Type enumType = enumAssembly.GetType(enumFullTypeName);
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
                if(vals.Count > 1) {
                    vals.ForEach((x, idx) => Values[idx] = x);
                } else if(vals.Count > 0) {
                    Value = vals.FirstOrDefault();
                } else {
                    Value = string.Empty;
                }

                opt_checked = bool.Parse(opt_parts[3]);

                filter_name = opt_parts[4];
            } else {
                Values.ForEach(x => x = null);
                filter_name = opt_parts[2];
            }
            IsChecked = opt_checked.IsTrue();
            FilterValue = filter_name.ToEnum<MpContentQueryBitFlags>();

            // SET CUR SELECTION
            if (cur_opt_sel_idx >= Items.Count) {
                MpConsole.WriteTraceLine($"Error criteria path mismatch option vm '{this}' cannot select idx '{cur_opt_sel_idx}' only has '{Items.Count}' options. Ignoring and selecting default");
                cur_opt_sel_idx = Items.Count > 0 ? 0 : -1;
            }

            SelectedItem = cur_opt_sel_idx >= 0 ? Items[cur_opt_sel_idx] : null;

            while(Items.Any(x=>x.IsAnyBusy)) {
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
                    //if(Parent != null && IsSelected && Parent.SelectedItem != this) {
                        
                    //}
                    //if(!IsSelected) {
                    //    Items.ForEach(x => x.IsSelected = false);
                    //    OnPropertyChanged(nameof(SelectedItem));
                    //}
                    if(!IsSelected) {
                        // clear unselected sub-trees
                        Items.Clear();
                    }

                    HostCriteriaItem.OnPropertyChanged(nameof(HostCriteriaItem.Items));
                    HostCriteriaItem.OnPropertyChanged(nameof(HostCriteriaItem.IsInputVisible));
                    break;
                case nameof(Value1):
                case nameof(Value2):
                case nameof(Value3):
                case nameof(Value4):
                    OnPropertyChanged(nameof(Values));
                    //Value = string.Format(@"{0},{1},{2}.{3}", Value1, Value2, Value3, Value4);
                    break;
                case nameof(Values):
                case nameof(Value):
                    if(HostCriteriaItem == null || IsBusy) {
                        // should only be busy during initial load 
                        // where collection handles ntf from adv search opened msg
                        break;
                    }
                    HostCriteriaItem.NotifyValueChanged();
                    break;
                case nameof(IsBusy):
                    if(Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(IsAnyBusy));
                    break;
            }
        }



        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            //OnPropertyChanged(nameof(Items));
        }


        #endregion
    }
}
