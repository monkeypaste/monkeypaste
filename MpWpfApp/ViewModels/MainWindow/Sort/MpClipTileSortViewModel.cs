using DataGridAsyncDemoMVVM.filtersort;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpClipTileSortViewModel : MpViewModelBase {
        #region View Models
        private ObservableCollection<MpSortTypeComboBoxItemViewModel> _sortTypes = new ObservableCollection<MpSortTypeComboBoxItemViewModel>();
        public ObservableCollection<MpSortTypeComboBoxItemViewModel> SortTypes {
            get {
                if (_sortTypes.Count == 0) {
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Date", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Application", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Title", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Content", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Type", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Usage", null));
                }
                return _sortTypes;
            }
            set {
                if (_sortTypes != value) {
                    _sortTypes = value;
                    OnPropertyChanged(nameof(SortTypes));
                }
            }
        }

        private MpSortTypeComboBoxItemViewModel _selectedSortType;
        public MpSortTypeComboBoxItemViewModel SelectedSortType {
            get {
                return _selectedSortType;
            }
            set {
                if (_selectedSortType != value) {
                    _selectedSortType = value;
                    OnPropertyChanged(nameof(SelectedSortType));
                }
            }
        }
        #endregion

        #region Properties
        public bool IsSortDescending {
            get {
                return DescSortOrderButtonImageVisibility == Visibility.Visible;
            }
        }

        private Visibility _ascSortOrderButtonImageVisibility = Visibility.Collapsed;
        public Visibility AscSortOrderButtonImageVisibility {
            get {
                return _ascSortOrderButtonImageVisibility;
            }
            set {
                if (_ascSortOrderButtonImageVisibility != value) {
                    _ascSortOrderButtonImageVisibility = value;
                    OnPropertyChanged(nameof(AscSortOrderButtonImageVisibility));
                }
            }
        }

        private Visibility _descSortOrderButtonImageVisibility = Visibility.Visible;
        public Visibility DescSortOrderButtonImageVisibility {
            get {
                return _descSortOrderButtonImageVisibility;
            }
            set {
                if (_descSortOrderButtonImageVisibility != value) {
                    _descSortOrderButtonImageVisibility = value;
                    OnPropertyChanged(nameof(DescSortOrderButtonImageVisibility));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpClipTileSortViewModel() : base() {
            //must be set before property changed registered for loading order
            SelectedSortType = SortTypes[0];

            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SelectedSortType):
                        PerformSelectedSort();
                        break;
                }
            };
        }
        public void ClipTileSort_Loaded(object sender, RoutedEventArgs e) {
        }
        #endregion

        #region Private Methods
        private void PerformSelectedSort() {
            var sort = new MemberPathSortingDirection();
            sort.MemberPath = ConvertSortTypeToMemberPath(SelectedSortType.Header);
            sort.SortDirection = IsSortDescending ? System.ComponentModel.ListSortDirection.Descending : System.ComponentModel.ListSortDirection.Ascending;
            MainWindowViewModel.ClipTrayViewModel.SortCommand.Execute(sort);
        }
        private string ConvertSortTypeToMemberPath(string sortType) {
            switch (sortType) {
                case "Date":
                    return "CopyItemCreatedDateTime";
                case "Application":
                    return "CopyItemAppId";
                case "Title":
                    return "CopyItemTitle";
                case "Content":
                    return "CopyItemPlainText";
                case "Type":
                    return "CopyItemType";
                case "Usage":
                    return "CopyItemUsageScore";
                default:
                    return "CopyItemCreatedDateTime";
            }
        }
        #endregion

        #region Commands
        private RelayCommand _toggleSortOrderCommand;
        public ICommand ToggleSortOrderCommand {
            get {
                if (_toggleSortOrderCommand == null) {
                    _toggleSortOrderCommand = new RelayCommand(ToggleSortOrder);
                }
                return _toggleSortOrderCommand;
            }
        }
        private void ToggleSortOrder() {
            if (AscSortOrderButtonImageVisibility == Visibility.Visible) {
                AscSortOrderButtonImageVisibility = Visibility.Collapsed;
                DescSortOrderButtonImageVisibility = Visibility.Visible;
            } else {
                AscSortOrderButtonImageVisibility = Visibility.Visible;
                DescSortOrderButtonImageVisibility = Visibility.Collapsed;
            }
            PerformSelectedSort();
        }
        #endregion
    }
}
