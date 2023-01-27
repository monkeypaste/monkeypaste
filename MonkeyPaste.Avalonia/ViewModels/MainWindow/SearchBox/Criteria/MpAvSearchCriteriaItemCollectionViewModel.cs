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

        private MpIQueryInfo _simpleSearchRef;
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

        public IEnumerable<MpAvSearchCriteriaItemViewModel> SortedItems =>
            Items.OrderBy(x => x.SortOrderIdx);

        public MpAvSearchCriteriaItemViewModel SelectedItem { get; set; }

        #endregion

        #region State
        public bool IsExpanded { get; set; }
        public bool HasCriteriaItems => Items.Count > 0;

        public bool IsCriteriaSaved => CurrentQueryTagId > 0;

        public bool IsPendingQuery => HasCriteriaItems && PendingQueryTagId > 0;
        #endregion

        #region Layout

        public double BoundCriteriaListBoxScreenHeight { get; set; }
        public double SearchCriteriaListBoxItemHeight => 60;

        public double LastSearchCriteriaListBoxHeight { get; private set; }

        public double MaxSearchCriteriaListBoxHeight {
            get {

                return SearchCriteriaListBoxItemHeight * Items.Count;
            }
        }

        #endregion

        #region Model

        #region Global Enumerables

        public IEnumerable<MpUserDevice> UserDevices { get; private set; }
        #endregion

        public int PendingQueryTagId { get; private set; }
        public int CurrentQueryTagId { get;private set;}

        #endregion

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public MpAvSearchCriteriaItemCollectionViewModel() : base() {
            PropertyChanged += MpAvSearchCriteriaItemCollectionViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            if(UserDevices == null) {
                Task.Run(async () => {
                    UserDevices = await MpDataModelProvider.GetItemsAsync<MpUserDevice>();
                });
            }
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
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.AdvancedSearchOpened:
                    int default_visible_row_count = 3;
                    double delta_open_height = Math.Min(Items.Count, default_visible_row_count) * SearchCriteriaListBoxItemHeight;

                    MpAvResizeExtension.ResizeByDelta(MpAvMainWindow.Instance, 0, delta_open_height,false);
                    BoundCriteriaListBoxScreenHeight = delta_open_height;

                    if(HasCriteriaItems) {
                        if (_simpleSearchRef == null) {
                            _simpleSearchRef = MpPlatform.Services.QueryInfo;
                        }
                        MpPlatform.Services.QueryInfo = Items.FirstOrDefault();
                        MpPlatform.Services.QueryInfo.NotifyQueryChanged();
                    }

                    break;
                case MpMessageType.AdvancedSearchClosed:
                    double delta_close_height = -BoundCriteriaListBoxScreenHeight;
                    BoundCriteriaListBoxScreenHeight = 0;
                    MpAvResizeExtension.ResizeByDelta(MpAvMainWindow.Instance, 0, delta_close_height, false);
                    if(_simpleSearchRef != null) {
                        MpPlatform.Services.QueryInfo = _simpleSearchRef;
                        _simpleSearchRef = null;

                        MpPlatform.Services.QueryInfo.NotifyQueryChanged();
                    }
                    break;

            }
        }
        private void MpAvSearchCriteriaItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedItem):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    break;
            }
        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {

            //if (IsBusy) {
            //    return;
            //}

            MpMessenger.SendGlobal<MpMessageType>(MpMessageType.SearchCriteriaItemsChanged);
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SortedItems));
            OnPropertyChanged(nameof(HasCriteriaItems));
            OnPropertyChanged(nameof(MaxSearchCriteriaListBoxHeight));

            UpdateCriteriaSortOrder().FireAndForgetSafeAsync(this);



           // double delta_height = 0;
           // if(LastSearchCriteriaListBoxHeight == 0 && Items.Count > 0) {
           //     // initial open
                
           // } else {
           //     if(Items.Count == 0) {
           //         // on close
           //         delta_height = -LastSearchCriteriaListBoxHeight;
           //     } else {
           //         delta_height = BoundCriteriaListBoxScreenHeight - LastSearchCriteriaListBoxHeight;
           //         if (BoundCriteriaListBoxScreenHeight + delta_height > MaxSearchCriteriaListBoxHeight) {
           //             delta_height = MaxSearchCriteriaListBoxHeight - BoundCriteriaListBoxScreenHeight;
           //         }
           //     }
           // }

            
           // //if (Math.Abs(delta_height) > 0.1) {
           //     MpAvMainWindowViewModel.Instance.WindowResizeCommand.Execute(new MpPoint(0, delta_height));
           //// }

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

        private async Task<IEnumerable<MpSearchCriteriaItem>> ConvertCurrentSearchToAdvanced(int queryTagId) {
            // NOTE this is called at end of provider create so sortIdx is seed for these

            string st = MpAvSearchBoxViewModel.Instance.SearchText;

            var all_filters = MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel.FilterType;
            List<MpContentQueryBitFlags> true_filters =
                MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel.Filters
                .Where(x => !x.IsSeperator && x.IsChecked.IsTrue())
                .Select(x => x.FilterType)
                .ToList();

            MpContentQueryBitFlags[] typeFilters = new[] { MpContentQueryBitFlags.TextType, MpContentQueryBitFlags.ImageType, MpContentQueryBitFlags.FileType };
            if(typeFilters.All(x=>true_filters.Contains(x))) {
                // TODO should probably account for non content type in query as any
                // when all types selected not needed in advanced
                //true_filters.RemoveAll(x=>typeFilters.Contains(x));
            }

            List<MpSearchCriteriaItem> items = new List<MpSearchCriteriaItem>();
            foreach (var ctf in true_filters) {
                string opt_path = 
                    GetSimpleSearchOptionString(
                        ctf, 
                        all_filters, 
                        st);

                var sci = await MpSearchCriteriaItem.CreateAsync(
                    tagId: queryTagId,
                    sortOrderIdx: items.Count,
                    options: opt_path);
                items.Add(sci);
            }
            return items;
        }
        private string GetSimpleSearchOptionString(MpContentQueryBitFlags current_flag, MpContentQueryBitFlags all_flags, string searchText) {
            // format:
            // <opt1>,<opt2>,...

            // opt formats:
            // parent/non match opt:
            // <Enum Module>|<Enum Name>|<MpContentFilterFlag Name>
            // match opt:
            // <Enum Module>|<Enum Name>|<base64 search text>|<is case sensitive>|<MpContentFilterFlag Name>

            searchText = searchText == null ? string.Empty : searchText;
            List<object> opts = new List<object>();
            switch (current_flag) {
                case MpContentQueryBitFlags.Title:
                    opts.Add(MpRootOptionType.Content);
                    opts.Add(MpContentOptionType.Title);
                    opts.Add(MpTextOptionType.Contains);
                    opts.Add(new object[] { MpContentQueryBitFlags.MatchValue, searchText, all_flags.HasFlag(MpContentQueryBitFlags.CaseSensitive) });
                    break;
                case MpContentQueryBitFlags.Content:
                    opts.Add(MpRootOptionType.Content);
                    opts.Add(MpContentOptionType.AnyText);
                    opts.Add(MpTextOptionType.Contains);
                    opts.Add(new object[] { MpContentQueryBitFlags.MatchValue, searchText, all_flags.HasFlag(MpContentQueryBitFlags.CaseSensitive) });
                    break;
                case MpContentQueryBitFlags.TextType:

                    break;
                default:
                    return string.Empty;
            }

            List<string> parts = new List<string>();
            foreach (var opt in opts) {
                string opt_str = string.Empty;
                if (opt is Enum enumOpt) {
                    opt_str = $"{enumOpt.GetType()}|{enumOpt}";
                } else if (opt is object[] matchParts) {
                    // special case for case sensitive
                    if (matchParts[0] is Enum matchEnum) {
                        opt_str += $"{matchEnum.GetType()}|{matchEnum}";
                    }
                    if (matchParts[1] is string match_text) {
                        opt_str += $"|{match_text.ToBase64String()}";
                    }
                    if (matchParts[2] is bool case_val) {
                        opt_str += $"|{case_val}";
                    }
                }
                opt_str += $"|{current_flag}";
                parts.Add(opt_str);
            }
            string result = string.Join(",", parts);
            return result;
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
                //var converted_items = 
                //    await Task.WhenAll(
                //        MpPlatform.Services.QueryInfo.Providers
                //        .OrderBy(x=>x is not MpAvSearchFilterCollectionViewModel)
                //        .Select((x,idx) => x.SaveAsCriteriaItemsAsync(PendingQueryTagId,idx)));
                var converted_items = await ConvertCurrentSearchToAdvanced(PendingQueryTagId);

                // clear search
                MpAvSearchBoxViewModel.Instance.ClearTextCommand.Execute("don't notify, chill");

                IsBusy = false;

                // convert result to criteria rows
                await InitializeAsync(PendingQueryTagId);

                MpMessenger.SendGlobal(MpMessageType.AdvancedSearchOpened);
            }, ()=>!HasCriteriaItems);


        public ICommand ClearCriteriaItemsCommand => new MpAsyncCommand(
            async() => {
                await InitializeAsync(0);
                MpMessenger.SendGlobal(MpMessageType.AdvancedSearchClosed);
            });

        #endregion
    }
}
