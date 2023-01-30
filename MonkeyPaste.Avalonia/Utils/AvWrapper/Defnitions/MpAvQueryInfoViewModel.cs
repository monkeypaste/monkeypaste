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
        MpViewModelBase, MpIQueryInfo, MpIQueryResultProvider, MpIJsonObject {
        #region Private Variables
        [JsonIgnore]
        private ObservableCollection<MpIQueryInfoValueProvider> _valueProviders = new ObservableCollection<MpIQueryInfoValueProvider>();


        [JsonIgnore]
        private MpQueryPageTools _pageTools;
        #endregion

        #region Statics

        public static MpIQueryResultProvider Parse(string lastQueryInfoStr) {
            if(!string.IsNullOrWhiteSpace(lastQueryInfoStr) && 
                !lastQueryInfoStr.StartsWith("[") && !lastQueryInfoStr.StartsWith("{")) {
                try {
                    int queryTagId = int.Parse(lastQueryInfoStr);
                    if(queryTagId != 0) {
                        return new MpAvSearchCriteriaItemCollectionViewModel(queryTagId);
                    }
                }
                catch {

                }
            }
            var result = JsonConvert.DeserializeObject<MpAvQueryInfoViewModel>(lastQueryInfoStr);
            return result;
        }

        #endregion

        #region Properties     

        #region MpIQueryResultProvider Implementation

        [JsonIgnore]
        public MpIDbIdCollection PageTools => _pageTools;
        [JsonIgnore]
        public IEnumerable<MpIQueryInfoValueProvider> ValueProviders => _valueProviders;
        [JsonIgnore]
        public int TotalAvailableItemsInQuery => _pageTools.AllQueryIds.Count;

        public async Task QueryForTotalCountAsync() {
            var result = await MpContentQuery.QueryAllAsync(this, false);
            _pageTools.AllQueryIds.Clear();
            _pageTools.AllQueryIds.AddRange(result);
        }

        public async Task<List<MpCopyItem>> FetchIdsByQueryIdxListAsync(List<int> copyItemQueryIdxList) {
            var fetchRootIds = _pageTools.AllQueryIds
                                .Select((val, idx) => (val, idx))
                                .Where(x => copyItemQueryIdxList.Contains(x.idx))
                                .Select(x => x.val).ToList();
            var items = await MpDataModelProvider.GetCopyItemsByIdListAsync(fetchRootIds);
            return items;
        }


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

                    _pageTools.AllQueryIds.Clear();
                    MpMessenger.SendGlobal(MpMessageType.QueryChanged);
                } else {
                    MpMessenger.SendGlobal(MpMessageType.SubQueryChanged);

                }
            });
        }

        #endregion

        #region MpIQueryInfo Implementation

        public bool IsDescending { get; set; } = true;
        public MpContentSortType SortType { get; set; } = MpContentSortType.CopyDateTime;
        public int TagId { get; set; } = MpTag.HelpTagId;
        public string SearchText { get; set; } = string.Empty;

        public MpContentQueryBitFlags FilterFlags { get; set; } = MpContentQueryBitFlags.Content | MpContentQueryBitFlags.TextType | MpContentQueryBitFlags.ImageType | MpContentQueryBitFlags.FileType;
        public MpTextQueryType TextFlags { get; set; } = MpTextQueryType.None;
        public MpDateTimeQueryType TimeFlags { get; set; } = MpDateTimeQueryType.None;
        public MpLogicalQueryType NextJoinType { get; set; }

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
            _pageTools = new MpQueryPageTools();
            PropertyChanged += MpAvQueryInfoViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods
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
