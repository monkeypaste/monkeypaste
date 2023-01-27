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
    public class MpAvQueryInfoViewModel : 
        MpViewModelBase, MpIQueryInfo, MpIJsonObject {
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

        public static MpAvQueryInfoViewModel Parse(string lastQueryInfoStr) {
            var result = JsonConvert.DeserializeObject<MpAvQueryInfoViewModel>(lastQueryInfoStr);
            return result;
        }

        #endregion

        #region Properties     

        #region MpIQueryInfo Implementation

        IEnumerable<MpIQueryInfoValueProvider> MpIQueryInfo.Providers => _valueProviders;
        public int TotalAvailableItemsInQuery => _allQueryCopyItemIds.Count;
        public bool IsDescending { get; set; } = true;
        public MpContentSortType SortType { get; set; } = MpContentSortType.CopyDateTime;
        public int TagId { get; set; } = MpTag.HelpTagId;
        public string SearchText { get; set; } = string.Empty;

        public MpContentQueryBitFlags FilterFlags { get; set; } = MpContentQueryBitFlags.Content | MpContentQueryBitFlags.TextType | MpContentQueryBitFlags.ImageType | MpContentQueryBitFlags.FileType;
        public MpTextQueryType TextFlags { get; set; } = MpTextQueryType.None;
        public MpDateTimeQueryType TimeFlags { get; set; } = MpDateTimeQueryType.None;
        public MpLogicalQueryType PrevJoinType { get; set; }

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

        public void JoinWithNext(MpIQueryInfo next, MpLogicalQueryType joinType) {
            Next = next;
            PrevJoinType = joinType;
        }

        #region Query Methods
        public async Task QueryForTotalCountAsync(IEnumerable<int> ci_idsToOmit, IEnumerable<int> tagIds) { // = null) {
            var result = await MpContentQuery.QueryAllAsync(this, tagIds, ci_idsToOmit);
            _allQueryCopyItemIds.Clear();
            _allQueryCopyItemIds.AddRange(result);
        }

        public async Task<List<MpCopyItem>> FetchIdsByQueryIdxListAsync(List<int> copyItemQueryIdxList) {
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
            if (queryIdx < 0 || queryIdx >= _allQueryCopyItemIds.Count) {
                return -1;
            }
            return _allQueryCopyItemIds[queryIdx];
        }

        public int GetItemOffsetIdx(int itemId) {
            return _allQueryCopyItemIds.IndexOf(itemId);
        }

        public void InsertId(int idx, int id) {
            if (idx < 0 || idx > _allQueryCopyItemIds.Count) {
                // bad idx
                Debugger.Break();
                return;
            }
            if (idx == _allQueryCopyItemIds.Count) {
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
            foreach (var vp in _valueProviders) {
                vp.Source.SetPropertyValue(vp.SourcePropertyName, this.GetPropertyValue(vp.QueryValueName));
            }
        }

        public void RegisterProvider(MpIQueryInfoValueProvider qip) {
            if (!qip.Source.GetType().GetProperty(qip.SourcePropertyName).CanWrite) {
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
            });
        }

        #endregion

        #endregion

        #region Private Methods

        private void MpAvQueryInfoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (SupressPropertyChangedNotification) {
                MpConsole.WriteLine("Hey! QueryInfo still receives suppressed property changes");
                return;
            }
        }

        private bool RefreshQuery() {
            // set internal properties to current registered values from bound controls
            bool hasChanged = false;
            foreach (var vp in _valueProviders) {
                object provided_value = vp.Source.GetPropertyValue(vp.SourcePropertyName);
                object cur_value = this.GetPropertyValue(vp.QueryValueName);
                if (!cur_value.Equals(provided_value)) {
                    hasChanged = true;
                }
                this.SetPropertyValue(vp.QueryValueName, provided_value);
            }
            return hasChanged;
        }

        #endregion
    }
}
