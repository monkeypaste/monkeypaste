using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using MonkeyPaste;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpClipTileSortViewModel : MpViewModelBase, MpIAsyncSingletonViewModel<MpClipTileSortViewModel> {
        #region View Models
        private ObservableCollection<MpSortTypeComboBoxItemViewModel> _sortTypes = new ObservableCollection<MpSortTypeComboBoxItemViewModel>();
        public ObservableCollection<MpSortTypeComboBoxItemViewModel> SortTypes {
            get {
                if (_sortTypes.Count == 0) {
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Copy Date", MpContentSortType.CopyDateTime));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Application", MpContentSortType.Source));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Title", MpContentSortType.Title));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Content", MpContentSortType.ItemData));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Type", MpContentSortType.ItemType));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Usage", MpContentSortType.UsageScore));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel(this,"Manual", MpContentSortType.Manual, false));
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

        public MpSortTypeComboBoxItemViewModel SelectedSortType { get; set; }
        #endregion

        #region Properties

        public string SortImagePath {
            get {
                if(SelectedSortType == null) {
                    return Application.Current.Resources["DecendingIcon"] as string;
                }
                if(IsSortDescending) { 
                    if(SelectedSortType.SortType == MpContentSortType.Manual) {
                        return Application.Current.Resources["DecendingIcon_Disabled"] as string;
                    }
                    return Application.Current.Resources["DecendingIcon"] as string;
                }
                if (SelectedSortType.SortType == MpContentSortType.Manual) {
                    return Application.Current.Resources["AscendingIcon_Disabled"] as string;
                }
                return Application.Current.Resources["AscendingIcon"] as string;
            }
        }

        public bool IsManualSort {
            get {
                if(SelectedSortType == null) {
                    return false;
                }
                return SelectedSortType.SortType == MpContentSortType.Manual;
            }
        }
        public bool IsSortDescending { get; set; } = true;

        public bool IsReseting { get; private set; } = false;
        #endregion

        #region Events
        #endregion

        #region Constructors

        private static MpClipTileSortViewModel _instance;
        public static MpClipTileSortViewModel Instance => _instance ?? (_instance = new MpClipTileSortViewModel());


        public MpClipTileSortViewModel() : base(null) {
            PropertyChanged += MpClipTileSortViewModel_PropertyChanged;
        }


        public async Task Init() {
            await Task.Delay(1);
            ResetToDefault(true);
        }

        #endregion

        #region Public Methods

        public void SetToManualSort() {
            SelectedSortType = SortTypes.Where(x => x.SortType == MpContentSortType.Manual).FirstOrDefault();
            SelectedSortType.IsVisible = true;
        }

        public void ResetToDefault(bool suppressNotifyQueryChanged = false) {
            IsReseting = true;

            SelectedSortType = SortTypes[0];
            IsSortDescending = true;

            IsReseting = false;

            if(!suppressNotifyQueryChanged) {
                MpDataModelProvider.QueryInfo.NotifyQueryChanged();
            }
        }
        #endregion

        #region Private Methods       
        private void MpClipTileSortViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectedSortType):
                    if (SelectedSortType == null) {
                        break;
                    }
                    if (SelectedSortType.SortType != MpContentSortType.Manual) {
                        var manualSort = SortTypes.Where(x => x.SortType == MpContentSortType.Manual).FirstOrDefault();
                        manualSort.IsVisible = false;
                        if (!IsReseting) {
                            MpDataModelProvider.QueryInfo.NotifyQueryChanged();
                        }
                    }
                    OnPropertyChanged(nameof(SortImagePath));
                    OnPropertyChanged(nameof(IsManualSort));
                    break;
                case nameof(IsSortDescending):
                    if (!IsReseting) {
                        MpDataModelProvider.QueryInfo.NotifyQueryChanged();
                    }
                    OnPropertyChanged(nameof(SortImagePath));
                    OnPropertyChanged(nameof(IsManualSort));
                    break;
            }
            
        }
        #endregion

        #region Commands
        public ICommand ToggleSortOrderCommand => new RelayCommand(
            () => {
                IsSortDescending = !IsSortDescending;
            },!IsManualSort);
        #endregion
    }
}
