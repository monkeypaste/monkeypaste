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
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Date", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Application", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Title", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Content", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Type", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Usage", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Manual", null));
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

        private MpSortTypeComboBoxItemViewModel _selectedSortType;
        public MpSortTypeComboBoxItemViewModel SelectedSortType {
            get {
                return _selectedSortType;
            }
            set {
                if (_selectedSortType != value) {
                    _selectedSortType = value;
                    OnPropertyChanged_old(nameof(SelectedSortType));
                }
            }
        }
        #endregion

        #region Properties
        private bool _isSorting = false;
        public bool IsSorting {
            get {
                return _isSorting;
            }
            set {
                if(_isSorting != value) {
                    _isSorting = value;
                    OnPropertyChanged_old(nameof(IsSorting));
                }
            }
        }

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
                    OnPropertyChanged_old(nameof(AscSortOrderButtonImageVisibility));
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
                    OnPropertyChanged_old(nameof(DescSortOrderButtonImageVisibility));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpClipTileSortViewModel() : base(null) {
            //must be set before property changed registered for loading order
            SelectedSortType = SortTypes[0];

            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SelectedSortType):
                        if(SelectedSortType == null) {
                            break;
                        }
                        if (SelectedSortType.Name != "Manual") {
                            SelectedSortType.IsVisible = false;
                            PerformSelectedSort();
                        } 
                        break;
                }
            };
        }
        public void ClipTileSort_Loaded(object sender, RoutedEventArgs e) {
            PerformSelectedSortCommand.Execute(null);
        }
        public string GetSortTypeAsMemberPath() {
            return ConvertSortTypeToMemberPath(SelectedSortType.Name);
        }

        public void SetToManualSort() {
            SelectedSortType = SortTypes.Where(x => x.Header == "Manual").FirstOrDefault();
            SelectedSortType.IsVisible = true;
            if(IsSortDescending) {
                ToggleSortOrder();
            }            
        }
        #endregion

        #region Private Methods        
        private string ConvertSortTypeToMemberPath(string sortType) {
            switch (sortType) {
                case "Manual":
                    return null;
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

        private RelayCommand _performSelectedSortCommand;
        public ICommand PerformSelectedSortCommand {
            get {
                if(_performSelectedSortCommand == null) {
                    _performSelectedSortCommand = new RelayCommand(PerformSelectedSort);
                }
                return _performSelectedSortCommand;
            }
        }
        private void PerformSelectedSort() {
            if (MpMainWindowViewModel.IsMainWindowLoading) {
                return;
            }

            IsSorting = true;
            var ct = MpClipTrayViewModel.Instance;
            //var sw = new Stopwatch();
            //sw.Start();

            //ct.ClearClipSelection();
            //ListSortDirection sortDir = AscSortOrderButtonImageVisibility == Visibility.Visible ? ListSortDirection.Ascending : ListSortDirection.Descending;

            //cvs.SortDescriptions.Clear();
            //cvs.SortDescriptions.Add(new SortDescription(sortBy, sortDir));
            //ct.ClipTileViewModels.Sort(x => x[GetSortTypeAsMemberPath()], sortDir == ListSortDirection.Descending);

            ct.RefreshClips(IsSortDescending, SelectedSortType.Name);
            //ct.Refresh();
            //sw.Stop();
            //MonkeyPaste.MpConsole.WriteLine("Sort for " + ct.VisibileClipTiles.Count + " items: " + sw.ElapsedMilliseconds + " ms");
            //ct.ResetClipSelection();
            IsSorting = false;
        }
        #endregion
    }
}
