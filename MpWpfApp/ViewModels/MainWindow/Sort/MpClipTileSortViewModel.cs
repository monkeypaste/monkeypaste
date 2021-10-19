using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using MonkeyPaste;

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

        public bool IsSortDescending { get; set; } = true;

        #endregion

        #region Events

        public event EventHandler<MpContentSortType> OnSortTypeChanged;
        public event EventHandler<bool> OnIsDescendingChanged;

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
                        if (SelectedSortType.SortType != MpContentSortType.Manual) {
                            var manualSort = SortTypes.Where(x => x.SortType == MpContentSortType.Manual).FirstOrDefault();
                            if (MpTagTrayViewModel.Instance.SelectedTagTile.IsSudoTag) {
                                manualSort.IsVisible = false;
                            } else {
                                manualSort.IsVisible = true;
                            }
                            PerformSelectedSortCommand.Execute(null);
                            OnSortTypeChanged?.Invoke(this, SelectedSortType.SortType);
                        }
                        break;
                    case nameof(IsSortDescending):
                        OnIsDescendingChanged?.Invoke(this, IsSortDescending);
                        break;
                }
            };

            OnViewModelLoaded();
        }

        public void SetToManualSort() {
            SelectedSortType = SortTypes.Where(x => x.SortType == MpContentSortType.Manual).FirstOrDefault();
            SelectedSortType.IsVisible = true;
            //if (IsSortDescending) {
            //    ToggleSortOrderCommand.Execute(null);
            //}
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

        public ICommand PerformSelectedSortCommand => new AsyncRelayCommand(
                async () => {
                    await MpClipTrayViewModel.Instance.RefreshTiles();
                },
                () => { 
                    return !MpMainWindowViewModel.IsMainWindowLoading; 
                });
          
        #endregion
    }
}
