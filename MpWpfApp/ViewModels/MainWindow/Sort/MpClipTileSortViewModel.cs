using DataGridAsyncDemoMVVM.filtersort;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpClipTileSortViewModel : MpViewModelBase<object> {
        #region Singleton Definition
        private static readonly Lazy<MpClipTileSortViewModel> _Lazy = new Lazy<MpClipTileSortViewModel>(() => new MpClipTileSortViewModel());
        public static MpClipTileSortViewModel Instance { get { return _Lazy.Value; } }

        public void Init() { }
        #endregion
        #region View Models
        private ObservableCollection<MpSortTypeComboBoxItemViewModel> _sortTypes = new ObservableCollection<MpSortTypeComboBoxItemViewModel>();
        public ObservableCollection<MpSortTypeComboBoxItemViewModel> SortTypes {
            get {
                if (_sortTypes.Count == 0) {
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Date", "default"));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Application", "CopyItemAppId"));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Title", "CopyItemTitle"));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Content", "CopyItemPlainText"));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Type", "CopyItemType"));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Usage", "CopyItemUsageScore"));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Manual", "Manual",false));
                }
                return _sortTypes;
            }
            set {
                if (_sortTypes != value) {
                    _sortTypes = value;
                    OnPropertyChanged_old(nameof(SortTypes));
                }
            }
        }

        public MpSortTypeComboBoxItemViewModel SelectedSortType { get; set; }
        #endregion

        #region Properties

        public bool IsSortDescending { get; set; } = true;

        #endregion

        #region Public Methods
        public MpClipTileSortViewModel() : base(null) {
            //must be set before property changed registered for loading order
            SelectedSortType = SortTypes[0];

            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SelectedSortType):
                        if (SelectedSortType == null) {
                            break;
                        }
                        if (SelectedSortType.Name != "Manual") {
                            SelectedSortType.IsVisible = false;
                            PerformSelectedSortCommand.Execute(null);
                        }
                        break;
                }
            };
        }
        public void ClipTileSort_Loaded(object sender, RoutedEventArgs e) {
            PerformSelectedSortCommand.Execute(null);
        }

        public void SetToManualSort() {
            SelectedSortType = SortTypes.Where(x => x.Header == "Manual").FirstOrDefault();
            SelectedSortType.IsVisible = true;
            if (IsSortDescending) {
                ToggleSortOrderCommand.Execute(null);
            }
        }
        #endregion

        #region Private Methods        
        #endregion

        #region Commands
        public ICommand ToggleSortOrderCommand => new RelayCommand(
            () => {
                IsSortDescending = !IsSortDescending;
                PerformSelectedSortCommand.Execute(null);
            });

        public ICommand PerformSelectedSortCommand => new RelayCommand(
                () => MpClipTrayViewModel.Instance.RefreshTiles()
                ,
                () => { return MpMainWindowViewModel.IsMainWindowLoading; 
            });
          
        #endregion
    }
}
