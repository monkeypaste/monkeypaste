using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {

    public class MpSearchCriteriaOptionViewModel : MpViewModelBase<MpSearchCriteriaOptionViewModel> {
        #region Private Variables

        private MpContentFilterType _filterType = MpContentFilterType.None;

        #endregion

        #region Properties

        #region View Models

        public MpSearchCriteriaItemViewModel HostCriteriaItem { get; set; }

        public ObservableCollection<MpSearchCriteriaOptionViewModel> Items { get; set; } = new ObservableCollection<MpSearchCriteriaOptionViewModel>();

        public MpSearchCriteriaOptionViewModel SelectedItem {
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

        public bool HasChildren => Items.Count > 0;

        public bool IsSelected { get; set; } = false;

        #endregion

        #region Appearance

        public string Label { get; set; }

        #endregion

        #region Model

        public string Value { get; set; }

        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Value4 { get; set; }

        public bool IsCheckable => UnitType.HasFlag(MpSearchCriteriaUnitType.Text) && !UnitType.HasFlag(MpSearchCriteriaUnitType.RegEx);

        public bool IsChecked { get; set; }

        public bool IsEnabled { get; set; } = true;

        public MpSearchCriteriaUnitType UnitType { get; set; }

        public MpContentFilterType FilterValue { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpSearchCriteriaOptionViewModel() : base(null) { }

        public MpSearchCriteriaOptionViewModel(MpSearchCriteriaItemViewModel host,MpSearchCriteriaOptionViewModel parent) : base(parent) {            
            PropertyChanged += MpSearchParameterViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            HostCriteriaItem = host;
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

                    HostCriteriaItem.OnPropertyChanged(nameof(HostCriteriaItem.SelectedOptions));
                    HostCriteriaItem.OnPropertyChanged(nameof(HostCriteriaItem.IsInputVisible));
                    break;
                case nameof(Value1):
                case nameof(Value2):
                case nameof(Value3):
                case nameof(Value4):
                    Value = string.Format(@"{0},{1},{2}.{3}", Value1, Value2, Value3, Value4);
                    break;
            }
        }



        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            //OnPropertyChanged(nameof(Items));
        }

        #endregion
    }
}
