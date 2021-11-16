using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    
    public class MpSearchDetailViewModel : MpViewModelBase<MpSearchBoxViewModel> {
        #region Properties

        #region View Models

        public ObservableCollection<MpTagTileViewModel> SelectedTagTiles { get; set; } = new ObservableCollection<MpTagTileViewModel>();

        public ObservableCollection<MpSearchCriteriaItemViewModel> CriteriaItems { get; set; } = new ObservableCollection<MpSearchCriteriaItemViewModel>();

        #endregion

        #region State

        public bool HasCriteriaItems => CriteriaItems.Count > 0;

        public bool IsSaved => UserSearch != null && UserSearch.Id > 0;

        #endregion

        #region Appearance


        #endregion

        #region Model

        public MpUserSearch UserSearch { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpSearchDetailViewModel() : base(null) { }

        public MpSearchDetailViewModel(MpSearchBoxViewModel parent) : base(parent) {
            CriteriaItems.CollectionChanged += CriteriaItems_CollectionChanged;
        }

        public async Task InitializeAsync(MpUserSearch us) {
            IsBusy = true;

            if(us == null) {
                UserSearch = null;
            } else {
                if (us.CriteriaItems == null || us.CriteriaItems.Count == 0) {
                    us = await MpDb.Instance.GetItemAsync<MpUserSearch>(us.Id);
                }

                UserSearch = us;
                foreach (var ci in UserSearch.CriteriaItems) {
                    var civm = await CreateCriteriaItemViewModel(ci);
                    CriteriaItems.Add(civm);
                }
            }

            OnPropertyChanged(nameof(CriteriaItems));
            OnPropertyChanged(nameof(HasCriteriaItems));

            IsBusy = false;
        }

        public async Task<MpSearchCriteriaItemViewModel> CreateCriteriaItemViewModel(MpSearchCriteriaItem sci) {
            MpSearchCriteriaItemViewModel nscivm = new MpSearchCriteriaItemViewModel(this);
            await nscivm.InitializeAsync(sci);
            return nscivm;
        }

        #endregion

        #region Private Methods 

        private async void CriteriaItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if(IsBusy) {
                return;
            }
            await UpdateCriteriaSortOrder();
        }

        private async Task UpdateCriteriaSortOrder(bool fromModel = false) {
            if (fromModel) {
                CriteriaItems.Sort(x => x.SortOrderIdx);
            } else {
                foreach (var scivm in CriteriaItems) {
                    scivm.SortOrderIdx = CriteriaItems.IndexOf(scivm);
                }
                if (!MpMainWindowViewModel.Instance.IsMainWindowLoading &&
                    IsSaved) {
                    IsBusy = true;

                    foreach (var scivm in CriteriaItems) {
                        await scivm.SearchCriteriaItem.WriteToDatabaseAsync();
                    }

                    IsBusy = false;
                }
            }
        }

        #endregion

        #region Commands

        public ICommand AddSearchCriteriaItemCommand => new RelayCommand(
            async () => {
                MpSearchCriteriaItem nsci = new MpSearchCriteriaItem() {
                    SortOrderIdx = CriteriaItems.Count
                };
                MpSearchCriteriaItemViewModel nscivm = await CreateCriteriaItemViewModel(nsci);
                CriteriaItems.Add(nscivm);
                OnPropertyChanged(nameof(CriteriaItems));
                OnPropertyChanged(nameof(HasCriteriaItems));
            });

        public ICommand RemoveSearchCriteriaItemCommand => new RelayCommand<MpSearchCriteriaItemViewModel>(
            async (scivm) => {
                int scivmIdx = CriteriaItems.IndexOf(scivm);
                CriteriaItems.RemoveAt(scivmIdx);
                if(scivm.SearchCriteriaItem.Id > 0) {
                    await scivm.SearchCriteriaItem.DeleteFromDatabaseAsync();
                }
                UpdateCriteriaSortOrder();
            });

        public ICommand SaveSearchCommand => new RelayCommand(
            async () => {
                await UserSearch.WriteToDatabaseAsync();
            });
        #endregion
    }
}
