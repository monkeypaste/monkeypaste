using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using Newtonsoft.Json;

namespace MonkeyPaste.Avalonia {
    public class MpAvQueryInfoViewModel : MpViewModelBase, MpIQueryInfo, MpIJsonObject {
        #region Private Variables
        [JsonIgnore]
        private ObservableCollection<MpIQueryInfoValueProvider> _valueProviders = new ObservableCollection<MpIQueryInfoValueProvider>();

        [JsonIgnore]
        private List<int> _allQueryCopyItemIds = new List<int>();

        [JsonIgnore]
        private IEnumerable<string> _requeryPropertieNames = new string[] {
            nameof(TagId),
        };
        #endregion

        #region Statics

        public static void Init(string lastQueryInfoStr) {
            _current = JsonConvert.DeserializeObject<MpAvQueryInfoViewModel>(lastQueryInfoStr);
        }

        [JsonIgnore]
        private static MpAvQueryInfoViewModel _current;
        public static MpAvQueryInfoViewModel Current => _current;
        #endregion

        #region Properties     

        #region MpIQueryInfo Implementation

        public int TotalAvailableItemsInQuery => _allQueryCopyItemIds.Count;
        public bool IsDescending { get; set; } = true;
        public MpContentSortType SortType { get; set; } = MpContentSortType.CopyDateTime;
        public int TagId { get; set; } = MpTag.HelpTagId;
        public string SearchText { get; set; } = string.Empty;

        public MpContentFilterType FilterFlags { get; set; } = MpContentFilterType.Content | MpContentFilterType.TextType | MpContentFilterType.ImageType | MpContentFilterType.FileType;
        public MpTextFilterFlagType TextFlags { get; set; } = MpTextFilterFlagType.None;
        public MpTimeFilterFlagType TimeFlags { get; set; } = MpTimeFilterFlagType.None;
        public MpLogicalFilterFlagType NextJoinType { get; set; }

        public MpIQueryInfo Next { get; set; }

        public int SortOrderIdx { get; set; } = 0;

        #endregion

        #region MpIJsonObject Implementation

        public string SerializeJsonObject() {
            return MpJsonConverter.SerializeObject(this);
        }

        #endregion

        #endregion

        #region Events
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

        #region Query Methods
        public async Task QueryForTotalCountAsync(IEnumerable<int> ci_idsToOmit, IEnumerable<int> tagIds) { // = null) {
            var result = await MpContentQuery.QueryAllAsync(this, tagIds, ci_idsToOmit);
            _allQueryCopyItemIds.Clear();
            _allQueryCopyItemIds.AddRange(result);
        }

        public async Task<List<MpCopyItem>> FetchCopyItemsByQueryIdxListAsync(List<int> copyItemQueryIdxList) {
            var fetchRootIds = _allQueryCopyItemIds
                                .Select((val, idx) => (val, idx))
                                .Where(x => copyItemQueryIdxList.Contains(x.idx))
                                .Select(x => x.val).ToList();
            var items = await MpDataModelProvider.GetCopyItemsByIdListAsync(fetchRootIds);
            return items;
        }
        #endregion

        #region Result Interaction
        
        public int GetItemId(int queryIdx) {
            if(queryIdx < 0 || queryIdx >= _allQueryCopyItemIds.Count) {
                return -1;
            }
            return _allQueryCopyItemIds[queryIdx];
        }

        public int GetItemOffsetIdx(int itemId) {
            return _allQueryCopyItemIds.IndexOf(itemId);
        }

        public void InsertId(int idx, int id) {
            if(idx < 0 || idx > _allQueryCopyItemIds.Count) {
                // bad idx
                Debugger.Break();
                return;
            }
            if(idx == _allQueryCopyItemIds.Count) {
                _allQueryCopyItemIds.Add(id);
            } else {
                _allQueryCopyItemIds.Insert(idx, id);
            }
        }
        public bool RemoveItemId(int itemId) {
            bool was_removed = _allQueryCopyItemIds.Remove(itemId);
            return was_removed;
        }
        public bool RemoveIdx(int queryIdx) {
            if (queryIdx < 0 || queryIdx >= _allQueryCopyItemIds.Count) {
                return false;
            }
            _allQueryCopyItemIds.RemoveAt(queryIdx);
            return true;
        }
        #endregion

        #region Value Provider Interaction

        public void RestoreProviderValues() {
            foreach(var vp in _valueProviders) {
                vp.Source.SetPropertyValue(vp.SourcePropertyName, this.GetPropertyValue(vp.QueryValueName));
            }
        }

        public void RegisterProvider(MpIQueryInfoValueProvider qip) {
            if(!qip.Source.GetType().GetProperty(qip.SourcePropertyName).CanWrite) {
                // needs to allow reset
                Debugger.Break();
            }
            if (_valueProviders.Contains(qip)) {
                MpConsole.WriteLine("Ignoring duplicate query info provider registration");
                return;
            }
            _valueProviders.Add(qip);
        }

        public void NotifyQueryChanged(bool forceRequery = false) {
            Dispatcher.UIThread.Post(() => {

                bool has_query_changed = RefreshQuery();

                if (has_query_changed || forceRequery) {
                    MpPrefViewModel.Instance.LastQueryInfoJson = SerializeJsonObject();

                    _allQueryCopyItemIds.Clear();
                    MpMessenger.SendGlobal(MpMessageType.QueryChanged);
                } else {
                    MpMessenger.SendGlobal(MpMessageType.SubQueryChanged);

                }


                //var qi = MpDataModelProvider.QueryInfo;

                //qi.FilterFlags = FilterFlags;//MpSearchBoxViewModel.Instance.FilterType;
                //MpDataModelProvider.QueryInfos.Add(this);
                // MpSearchBoxViewModel.Instance.CriteriaItems.OrderBy(x => x.SortOrderIdx).ForEach(x => MpDataModelProvider.QueryInfos.Add(x.ToQueryInfo()));
            });
        }

        #endregion

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


        private bool RefreshQuery() {
            bool hasChanged = false;
            foreach (var vp in _valueProviders) {
                object provided_value = vp.Source.GetPropertyValue(vp.SourcePropertyName);
                object cur_value = this.GetPropertyValue(vp.QueryValueName);
                if(!cur_value.Equals(provided_value)) {
                    hasChanged = true;
                }
                this.SetPropertyValue(vp.QueryValueName, provided_value);
            }
            return hasChanged;
        }

        #endregion
    }
}
