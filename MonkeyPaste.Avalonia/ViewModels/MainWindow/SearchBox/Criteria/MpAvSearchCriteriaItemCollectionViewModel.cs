using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchCriteriaItemCollectionViewModel : MpViewModelBase {

        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics

        private static MpAvSearchCriteriaItemCollectionViewModel _instance;
        public static MpAvSearchCriteriaItemCollectionViewModel Instance => _instance ?? (_instance = new MpAvSearchCriteriaItemCollectionViewModel());
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models


        public ObservableCollection<MpAvSearchCriteriaItemViewModel> Items { get; set; } = new ObservableCollection<MpAvSearchCriteriaItemViewModel>();

        #endregion

        #region State
        public bool IsExpanded { get; set; }
        public bool HasCriteriaItems => Items.Count > 0;

        public bool IsCriteriaSaved => CurrentQueryTagId > 0;

        public bool IsPendingQuery => HasCriteriaItems && PendingQueryTagId > 0;
        #endregion

        #region Layout

        public double BoundCriteriaListBoxScreenHeight { get; set; }
        public double SearchCriteriaListBoxItemHeight => 50;

        public double LastSearchCriteriaListBoxHeight { get; private set; }

        public double MaxSearchCriteriaListBoxHeight {
            get {

                return SearchCriteriaListBoxItemHeight * Items.Count;
            }
        }

        #endregion

        #region Model

        public int PendingQueryTagId { get; private set; }
        public int CurrentQueryTagId { get;private set;}

        #endregion

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public MpAvSearchCriteriaItemCollectionViewModel() : base() {
            Items.CollectionChanged += Items_CollectionChanged;
        }


        public async Task InitializeAsync(int currentQueryTagId) {
            IsBusy = true;

            bool hasPending = PendingQueryTagId > 0;
            bool wasPendingRejected = currentQueryTagId != PendingQueryTagId;
            if(hasPending && wasPendingRejected) {
                // if pending was confirmed to save id should have been cleared so delete
                var t = await MpDataModelProvider.GetItemAsync<MpTag>(PendingQueryTagId);
                t.DeleteFromDatabaseAsync().FireAndForgetSafeAsync();
                PendingQueryTagId = 0;
            }

            Items.Clear();
            CurrentQueryTagId = currentQueryTagId;
            if (CurrentQueryTagId > 0) {
                var cil = await MpDataModelProvider.GetCriteriaItemsByTagId(CurrentQueryTagId);
                foreach (var ci in cil) {
                    var civm = await CreateCriteriaItemViewModel(ci);
                    Items.Add(civm);
                }
            }

            while(Items.Any(x=>x.IsBusy)) {
                await Task.Delay(100);
            }
            BoundCriteriaListBoxScreenHeight = MaxSearchCriteriaListBoxHeight;

            IsBusy = false;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private async Task<MpAvSearchCriteriaItemViewModel> CreateCriteriaItemViewModel(MpSearchCriteriaItem sci) {
            MpAvSearchCriteriaItemViewModel nscivm = new MpAvSearchCriteriaItemViewModel(this);
            await nscivm.InitializeAsync(sci);
            return nscivm;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {

            //if (IsBusy) {
            //    return;
            //}

            MpMessenger.SendGlobal<MpMessageType>(MpMessageType.SearchCriteriaItemsChanged);
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(HasCriteriaItems));
            OnPropertyChanged(nameof(MaxSearchCriteriaListBoxHeight));

            UpdateCriteriaSortOrder().FireAndForgetSafeAsync(this);

            double delta_height = BoundCriteriaListBoxScreenHeight - LastSearchCriteriaListBoxHeight;
            if (Math.Abs(delta_height) > 0.1) {
                MpAvMainWindowViewModel.Instance.WindowResizeCommand.Execute(new MpPoint(0, delta_height));
            }

            LastSearchCriteriaListBoxHeight = BoundCriteriaListBoxScreenHeight;
        }

        private async Task UpdateCriteriaSortOrder(bool fromModel = false) {
            if (fromModel) {
                Items.Sort(x => x.SortOrderIdx);
            } else {
                Items.ToList().ForEach((x, idx) => x.SortOrderIdx = idx);
                while(Items.ToList().Any(x=>x.IsBusy)) {
                    await Task.Delay(100);
                }
            }
        }
        #endregion

        #region Commands


        public ICommand AddSearchCriteriaItemCommand => new MpAsyncCommand<object>(
            async (args) => {
                int add_idx = Items.Count;
                if (args is MpAvSearchCriteriaItemViewModel scivm) {
                    add_idx = scivm.SortOrderIdx + 1;
                }
                MpSearchCriteriaItem nsci = new MpSearchCriteriaItem() {
                    SortOrderIdx = Items.Count
                };
                MpAvSearchCriteriaItemViewModel nscivm = await CreateCriteriaItemViewModel(nsci);
                Items.Insert(add_idx, nscivm);
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(HasCriteriaItems));
            });

        public ICommand RemoveSearchCriteriaItemCommand => new MpCommand<object>(
            async (args) => {
                var scivm = args as MpAvSearchCriteriaItemViewModel;
                int scivmIdx = Items.IndexOf(scivm);
                Items.RemoveAt(scivmIdx);
                if (scivm.SearchCriteriaItem.Id > 0) {
                    await scivm.SearchCriteriaItem.DeleteFromDatabaseAsync();
                }
                await UpdateCriteriaSortOrder();
            },
            (args) => args is MpAvSearchCriteriaItemViewModel);

        public ICommand SavePendingQueryCommand => new MpAsyncCommand(
            async() => {
                // NOTE this should only occur for new searches, onced created saving is by HasModelChanged
                var ttrvm = MpAvTagTrayViewModel.Instance;
                int waitTimeMs = MpAvSidebarItemCollectionViewModel.Instance.SelectedItem == ttrvm ? 0 : 500;
                MpAvSidebarItemCollectionViewModel.Instance.SelectSidebarItemCommand.Execute(ttrvm);
                // wait for panel open
                await Task.Delay(waitTimeMs);

                ttrvm.RootGroupTagViewModel.AddNewChildTagCommand.Execute(PendingQueryTagId);
                
                // clear pending flag 
                PendingQueryTagId = 0;
            }, () => PendingQueryTagId > 0);

        public ICommand ConvertQueryToAdvancedSearchCommand => new MpAsyncCommand(
            async() => {
                IsBusy = true;

                // make temporary
                var pending_tag = await MpTag.CreateAsync(
                    tagType: MpTagType.Query);

                PendingQueryTagId = pending_tag.Id;
                CurrentQueryTagId = pending_tag.Id;
                Items.Clear();

                // create snapshot of current filters using provided tagId as param host
                var converted_items = 
                    await Task.WhenAll(
                        MpPlatform.Services.QueryInfo.Providers
                        .OrderBy(x=>x is not MpAvSearchFilterCollectionViewModel)
                        .Select((x,idx) => x.SaveAsCriteriaItemsAsync(PendingQueryTagId,idx)));

                // clear search
                MpAvSearchBoxViewModel.Instance.ClearTextCommand.Execute("don't notify, chill");

                IsBusy = false;

                // convert result to criteria rows
                InitializeAsync(PendingQueryTagId).FireAndForgetSafeAsync(this);
            }, ()=>!HasCriteriaItems);


        public ICommand ClearCriteriaItemsCommand => new MpCommand(
            () => {
                InitializeAsync(0).FireAndForgetSafeAsync(this);
            });

        #endregion
    }
}
