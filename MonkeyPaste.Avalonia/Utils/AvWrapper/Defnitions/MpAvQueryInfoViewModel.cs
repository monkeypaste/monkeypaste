using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;
using Newtonsoft.Json;

namespace MonkeyPaste.Avalonia {
    public class MpAvQueryInfoViewModel : MpViewModelBase, MpIQueryInfo, MpIJsonObject {

        #region Statics

        public static void Init(string lastQueryInfoStr) {
            _current = JsonConvert.DeserializeObject<MpAvQueryInfoViewModel>(lastQueryInfoStr);
        }

        [JsonIgnore]
        private static MpAvQueryInfoViewModel _current;
        public static MpAvQueryInfoViewModel Current => _current;
        #endregion

        #region Properties
        
        [JsonIgnore]
        public ObservableCollection<MpIQueryInfoProvider> InfoProviders { get; private set; } = new ObservableCollection<MpIQueryInfoProvider>();

        #region MpIQueryInfo Implementation

        //public int TotalItemsInQuery { get; set; } = 0;
        public bool IsDescending { get; set; } = true;
        public MpContentSortType SortType { get; set; } = MpContentSortType.CopyDateTime;
        public int TagId { get; set; } = 0;
        public string SearchText { get; set; } = string.Empty;

        public MpContentFilterType FilterFlags { get; set; } = MpContentFilterType.None;
        public MpTextFilterFlagType TextFlags { get; set; } = MpTextFilterFlagType.None;
        public MpTimeFilterFlagType TimeFlags { get; set; } = MpTimeFilterFlagType.None;
        public MpLogicalFilterFlagType NextJoinType { get; set; }

        public MpIQueryInfo Next { get; set; }

        public int PageSize { get; set; } = 0;

        public int SortOrderIdx { get; set; } = 0;

        public void RegisterProvider(MpIQueryInfoProvider qip) {
            if(InfoProviders.Contains(qip)) {
                MpConsole.WriteLine("Ignoring duplicate query info provider registration");
                return;
            }
            InfoProviders.Add(qip);
        }

        public void NotifyQueryChanged(bool isFilterSortOrSearch = true) {
            //SupressPropertyChangedNotification = true;

            //IsDescending = MpAvClipTileSortViewModel.Instance.IsSortDescending;
            //SortType = MpAvClipTileSortViewModel.Instance.SelectedSortType;
            TagId = MpAvTagTrayViewModel.Instance.SelectedItem.TagId;
            SearchText = MpAvSearchBoxViewModel.Instance.SearchText;
            //TotalItemsInQuery = MpDataModelProvider.TotalTilesInQuery;

            // NOTE not sure why this isn't set so maybe bad
            //FilterFlags = MpContentFilterType.TextType | MpContentFilterType.FileType | MpContentFilterType.ImageType; //MpSearchBoxViewModel.Instance.FilterType;
            FilterFlags = MpAvSearchBoxViewModel.Instance.FilterType;

            MpPrefViewModel.Instance.LastQueryInfoJson = SerializeJsonObject();

            //var qi = MpDataModelProvider.QueryInfo;
            MpDataModelProvider.QueryInfos.Clear();

            //qi.FilterFlags = FilterFlags;//MpSearchBoxViewModel.Instance.FilterType;
            MpDataModelProvider.QueryInfos.Add(this);
            // MpSearchBoxViewModel.Instance.CriteriaItems.OrderBy(x => x.SortOrderIdx).ForEach(x => MpDataModelProvider.QueryInfos.Add(x.ToQueryInfo()));

            MpPlatformWrapper.Services.MainThreadMarshal.RunOnMainThread(() => {
                if (isFilterSortOrSearch) {
                    MpMessenger.SendGlobal(MpMessageType.QueryChanged);
                } else {
                    MpMessenger.SendGlobal(MpMessageType.SubQueryChanged);
                }
            });
        }
        #endregion


        #region MpIJsonObject Implementation

        public string SerializeJsonObject() {
            return MpJsonObject.SerializeObject(this);
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvQueryInfoViewModel() {
            PropertyChanged += MpAvQueryInfoViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public void JoinWithNext(MpIQueryInfo next, MpLogicalFilterFlagType joinType) {
            Next = next;
            NextJoinType = joinType;
        }
        
        

        #endregion

        #region Private Methods

        private void MpAvQueryInfoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(SupressPropertyChangedNotification) {
                MpConsole.WriteLine("Hey! QueryInfo still receives suppressed property changes");
                return;
            }

            if(this == Current) {
                // persist current query to pref json
                //MpPrefViewModel.Instance.LastQueryInfoJson = SerializeJsonObject();
            }
        }

        #endregion
    }
}
