using FFImageLoading.Helpers.Exif;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static SQLite.SQLite3;

namespace MonkeyPaste {
    public class MpDataModelProvider : MpSingleton<MpDataModelProvider> {
        #region Private Variables
        private IList<MpCopyItem> _lastResult;

        private List<int> _allFetchedAndSortedCopyItemIds = new List<int>();
        #endregion

        #region Properties

        public MpIQueryInfo QueryInfo { get; set; }

        public int TotalItems => _allFetchedAndSortedCopyItemIds.Count;

        #endregion

        #region Constructor

        public MpDataModelProvider() { }

        #endregion

        #region Public Methods

        public void Init(MpIQueryInfo queryInfo) {
            QueryInfo = queryInfo;
            MpDb.Instance.OnItemUpdated += Instance_OnItemUpdated;
            MpDb.Instance.OnItemDeleted += Instance_OnItemDeleted;
        }

        public void ResetQuery() {
            _allFetchedAndSortedCopyItemIds.Clear();
            _lastResult = new List<MpCopyItem>();
        }


        #region MpQueryInfo Fetch Methods

        public async Task QueryForTotalCount() {
            string allRootIdQuery = GetQueryForCount();
            _allFetchedAndSortedCopyItemIds = await MpDb.Instance.QueryScalarsAsync<int>(allRootIdQuery);
            _allFetchedAndSortedCopyItemIds = _allFetchedAndSortedCopyItemIds.Distinct().ToList();
            QueryInfo.TotalItemsInQuery = _allFetchedAndSortedCopyItemIds.Count;
        }

        public async Task<IList<MpCopyItem>> FetchCopyItemRangeAsync(int startIndex, int count, Dictionary<int, int> manualSortOrderLookup = null) {
            var fetchRange = _allFetchedAndSortedCopyItemIds.GetRange(startIndex, count);
            var items = await GetCopyItemsByIdList(fetchRange);
            if(items.Count == 0 && startIndex + count < _allFetchedAndSortedCopyItemIds.Count) {
                MpConsole.WriteTraceLine("Bad data detected for ids: " + string.Join(",", fetchRange));
            }
            return items;
        }

        #endregion

        #region View Queries

        public string GetQueryForCount() {
            string query = "select RootId from MpSortableCopyItem_View";
            string tagClause = string.Empty;
            string sortClause = string.Empty;
            List<string> filters = new List<string>();

            if (QueryInfo.TagId != MpTag.AllTagId) {
                tagClause = string.Format(
                    @"RootId in 
                    (select 
		                case fk_ParentCopyItemId
			                when 0
				                then pk_MpCopyItemId
			                ELSE
				                fk_ParentCopyItemId
		                end
		                from MpCopyItem where pk_MpCopyItemId in 
		                (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0}))",
                    QueryInfo.TagId);
            }
            if (!string.IsNullOrEmpty(QueryInfo.SearchText)) {
                if (QueryInfo.FilterFlags.HasFlag(MpContentFilterType.CaseSensitive)) {

                }
                string searchText = QueryInfo.SearchText;

                string escapeStr = string.Empty;
                if (searchText.Contains('%')) {
                    searchText = searchText.Replace("%", @"\%");
                    escapeStr = @" ESCAPE '\'";
                }
                if (QueryInfo.FilterFlags.HasFlag(MpContentFilterType.Title)) {
                    filters.Add(string.Format(@"{0} like '%{1}%'{2}", CaseFormat("Title"), searchText, escapeStr));
                }
                if (QueryInfo.FilterFlags.HasFlag(MpContentFilterType.Text) ||
                    QueryInfo.FilterFlags.HasFlag(MpContentFilterType.File)) {
                    filters.Add(string.Format(@"{0} like '%{1}%'{2}", CaseFormat("ItemData"), searchText, escapeStr));
                }
                if (QueryInfo.FilterFlags.HasFlag(MpContentFilterType.Url)) {
                    filters.Add(string.Format(@"{0} like '%{1}%'{2}", CaseFormat("UrlPath"), searchText, escapeStr));
                }
                if (QueryInfo.FilterFlags.HasFlag(MpContentFilterType.AppName)) {
                    filters.Add(string.Format(@"{0} like '%{1}%'{2}", CaseFormat("AppName"), searchText, escapeStr));
                }
                if (QueryInfo.FilterFlags.HasFlag(MpContentFilterType.AppPath)) {
                    filters.Add(string.Format(@"{0} like '%{1}%'{2}", CaseFormat("AppPath"), searchText, escapeStr));
                }
                if (QueryInfo.FilterFlags.HasFlag(MpContentFilterType.Meta)) {
                    filters.Add(string.Format(@"{0} like '%{1}%'{2}", CaseFormat("ItemDescription"), searchText, escapeStr));
                }
            }
            switch (QueryInfo.SortType) {
                case MpContentSortType.CopyDateTime:
                    sortClause = string.Format(@"order by {0}", "CopyDateTime");
                    break;
                case MpContentSortType.Source:
                    sortClause = string.Format(@"order by {0}", "SourcePath");
                    break;
                case MpContentSortType.Title:
                    sortClause = string.Format(@"order by {0}", "Title");
                    break;
                case MpContentSortType.ItemData:
                    sortClause = string.Format(@"order by {0}", "ItemData");
                    break;
                case MpContentSortType.ItemType:
                    sortClause = string.Format(@"order by {0}", "fk_MpCopyItemTypeId");
                    break;
                case MpContentSortType.UsageScore:
                    sortClause = string.Format(@"order by {0}", "UsageScore");
                    break;
                case MpContentSortType.Manual:

                    break;
            }
            if (QueryInfo.IsDescending) {
                sortClause += " DESC";
            }
            if (!string.IsNullOrEmpty(tagClause)) {
                query += " where " + tagClause;
                if (filters.Count > 0) {
                    query += " and (";
                    query += string.Join(" or ", filters) + ")";
                }
            } else if (filters.Count > 0) {
                query += " where ";
                query += string.Join(" or ", filters);
            }
            //query += " group by RootId";
            query += " " + sortClause;
            return query;
        }

        private string CaseFormat(string fieldOrSearchText) {
            //if (QueryInfo.FilterFlags.HasFlag(MpContentFilterType.CaseSensitive)) {
            //    return string.Format(@"UPPER({0})", fieldOrSearchText);
            //}
            return fieldOrSearchText;
        }
        #endregion

        #region Select queries

        #region MpUserDevice

        public async Task<MpUserDevice> GetUserDeviceByGuid(string guid) {
            string query = $"select * from MpUserDevice where MpUserDeviceGuid=?";
            var result = await MpDb.Instance.QueryAsync<MpUserDevice>(query, guid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion MpUserDevice

        #region MpIcon

        public async Task<MpIcon> GetIconByImageStr(string text64) {
            string query = $"select pk_MpDbImageId from MpDbImage where ImageBase64=?";
            int iconImgId = await MpDb.Instance.QueryScalarAsync<int>(query, text64);
            if (iconImgId <= 0) {
                return null;
            }
            query = $"select * from MpIcon where fk_IconDbImageId=?";
            var result = await MpDb.Instance.QueryAsync<MpIcon>(query, iconImgId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion MpIcon

        #region MpApp

        public async Task<MpApp> GetAppByPath(string path) {
            string query = $"select * from MpApp where SourcePath=?";
            var result = await MpDb.Instance.QueryAsync<MpApp>(query, path);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<bool> IsAppRejectedAsync(string path) {
            string query = $"select count(*) from MpApp where SourcePath=? and IsAppRejected=1";
            var result = await MpDb.Instance.QueryScalarAsync<int>(query, path);
            return result > 0;
        }

        //public bool IsAppRejected(string path) {
        //    string query = $"select count(*) from MpApp where SourcePath=? and IsAppRejected=1";
        //    var result = MpDb.Instance.QueryScalar<int>(query, path);
        //    return result > 0;
        //}

        #endregion MpApp

        #region MpUrl

        public async Task<MpUrl> GetUrlByPath(string url) {
            string query = $"select * from MpUrl where UrlPath=?";
            var result = await MpDb.Instance.QueryAsync<MpUrl>(query, url);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion MpUrl

        #region MpSource

        public async Task<MpSource> GetSourceByMembers(int appId, int urlId) {
            string query = $"select * from MpSource where fk_MpAppId=? and fk_MpUrlId=?";
            var result = await MpDb.Instance.QueryAsync<MpSource>(query, appId, urlId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<MpSource> GetSourceByGuid(string guid) {
            string query = $"select * from MpSource where MpSourceGuid=?";
            var result = await MpDb.Instance.QueryAsync<MpSource>(query, guid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion MpSource

        #region MpCopyItem

        public async Task<List<MpCopyItem>> GetCopyItemsByIdList(List<int> ciida) {
            string whereStr = string.Join(" or ", ciida.Select(x => string.Format(@"pk_MpCopyItemId={0}", x)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query);
            return result.OrderBy(x=>ciida.IndexOf(x.Id)).ToList();
        }

        public async Task<List<MpCopyItem>> GetCopyItemsByData(string text) {
            string query = "select * from MpCopyItem where ItemData=?";
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query, text);
            return result;
        }

        public async Task<int> GetTotalCopyItemCountAsync() {
            string query = "select count(pk_MpCopyItemId) from MpCopyItem";
            var result = await MpDb.Instance.QueryScalarAsync<int>(query);
            return result;
        }

        public async Task<int> GetRecentCopyItemCountAsync() {
            string query = GetFetchQuery(0, 0, true, MpTag.RecentTagId, true);
            var result = await MpDb.Instance.QueryScalarAsync<int>(query);
            return result;
        }

        public async Task<List<MpCopyItem>> GetCompositeChildrenAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItem where fk_ParentCopyItemId={0} order by CompositeSortOrderIdx", ciid);
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public async Task<int> GetCompositeChildCountAsync(int ciid) {
            string query = string.Format(@"select count(*) from MpCopyItem where fk_ParentCopyItemId={0} order by CompositeSortOrderIdx", ciid);
            var result = await MpDb.Instance.QueryScalarAsync<int>(query);
            return result;
        }

        #endregion MpCopyItem

        #region MpCopyItemTemplate

        public async Task<List<MpCopyItemTemplate>> GetTemplatesAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItemTemplate where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTemplate>(query);
            return result;
        }

        public async Task<MpCopyItemTemplate> GetTemplateByNameAsync(int ciid, string templateName) {
            // NOTE may need to use '?' below
            string query = string.Format(@"select * from MpCopyItemTemplate where fk_MpCopyItemId={0} and TemplateName=?", ciid);
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTemplate>(query,templateName);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion

        #region MpCopyItemTag

        public async Task<List<int>> GetCopyItemIdsForTagAsync(int tagId) {
            string query = string.Format(@"select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0}", tagId);
            var result = await MpDb.Instance.QueryScalarsAsync<int>(query);
            return result;
        }

        public async Task<int> GetCopyItemCountForTagAsync(int tagId) {
            string query = string.Format(@"select count(pk_MpCopyItemId) from MpCopyItem where pk_MpCopyItemId in 
                                           (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0})", tagId);
            var result = await MpDb.Instance.QueryScalarAsync<int>(query);
            return result;
        }

        public async Task<List<MpCopyItem>> GetCopyItemsForTagAsync(int tagId) {
            string query = string.Format(@"select * from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0})", tagId);
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public async Task<MpCopyItemTag> GetCopyItemTagForTagAsync(int ciid, int tagId) {
            string query = string.Format(@"select * from MpCopyItemTag where fk_MpCopyItemId={0} and fk_MpTagId={1}", ciid, tagId);
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTag>(query);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<List<MpCopyItemTag>> GetCopyItemTagsForTagAsync(int tagId) {
            string query = string.Format(@"select * from MpCopyItemTag where fk_MpTagId={0}", tagId);
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTag>(query);
            return result;
        }

        public async Task<List<MpCopyItemTag>> GetCopyItemTagsForCopyItemAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItemTag where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTag>(query);
            return result;
        }

        public async Task<bool> IsCopyItemInRecentTag(int copyItemId) {
            string query = GetFetchQuery(0, 0, true, MpTag.RecentTagId, true, copyItemId);
            var result = await MpDb.Instance.QueryScalarAsync<int>(query);
            return result > 0;
        }

        public async Task<bool> IsTagLinkedWithCopyItem(int tagId, int copyItemId) {
            string query = $"select count(*) from MpCopyItemTag where fk_MpTagId=? and fk_MpCopyItemId=?";
            var result = await MpDb.Instance.QueryScalarAsync<int>(query, tagId, copyItemId);
            return result > 0;
        }

        #endregion MpCopyItemTag        

        #region MpShortcut

        public async Task<List<MpShortcut>> GetAllShortcuts() {
            string query = @"select * from MpShortcut";
            var result = await MpDb.Instance.QueryAsync<MpShortcut>(query);
            return result;
        }

        public async Task<List<MpShortcut>> GetCopyItemShortcutsAsync(int ciid) {
            string query = string.Format(@"select * from MpShortcut where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.Instance.QueryAsync<MpShortcut>(query);
            return result;
        }

        public async Task<List<MpShortcut>> GetTagShortcutsAsync(int tid) {
            string query = string.Format(@"select * from MpShortcut where fk_MpTagId={0}", tid);
            var result = await MpDb.Instance.QueryAsync<MpShortcut>(query);
            return result;
        }

        #endregion

        #region MpPasteToAppPath



        #endregion

        #region MpAnalytic Item

        public async Task<int> GetAnalyticItemCount() {
            string query = $"select count(pk_MpAnalyticItemId) from MpAnalyticItem";
            var result = await MpDb.Instance.QueryScalarAsync<int>(query);
            return result;
        }

        public async Task<MpAnalyticItem> GetAnalyticItemByEndpoint(string endPoint) {
            string query = $"select * from MpAnalyticItem where EndPoint=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItem>(query, endPoint);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<List<MpAnalyticItemPreset>> GetAllQuickActionAnalyzers() {
            string query = $"select * from MpAnalyticItemPreset where IsQuickAction=1";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemPreset>(query);
            return result;
        }

        public async Task<List<MpAnalyticItemPreset>> GetAllShortcutAnalyzers() {
            string query = $"select * from MpAnalyticItemPreset where pk_MpAnalyticItemPresetId in (select fk_MpAnalyticItemPresetId from MpShortcut where fk_MpAnalyticItemPresetId > 0)";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemPreset>(query);
            return result;
        }

        public async Task<MpAnalyticItemPreset> GetAnalyzerPresetById(int aipid) {
            string query = $"select * from MpAnalyticItemPreset where pk_MpAnalyticItemPresetId=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemPreset>(query,aipid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<MpAnalyticItem> GetAnalyticItemByTitle(string title) {
            string query = $"select * from MpAnalyticItem where Title=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItem>(query, title);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<MpAnalyticItemPreset> GetAnalyticItemPresetByLabel(int aiid, string label) {
            string query = $"select * from MpAnalyticItemPreset where fk_MpAnalyticItemId=? and Label=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemPreset>(query, aiid,label);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<MpAnalyticItemPresetParameterValue> GetAnalyticItemPresetValue(int presetid, int paramEnumId) {
            string query = $"select * from MpAnalyticItemPresetParameterValue where fk_MpAnalyticItemPresetId=? and ParameterEnumId=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemPresetParameterValue>(query, presetid, paramEnumId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<MpAnalyticItemParameter> GetAnalyticItemParameterByKey(int analyticItemId, string key) {
            string query = $"select * from MpAnalyticItemParameter where Key=? and fk_MpAnalyticItemId=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemParameter>(query, key,analyticItemId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<List<MpAnalyticItemParameterValue>> GetAnalyticItemParameterValuesByParamId(int paramId) {
            string query = $"select * from MpAnalyticItemParameterValue where fk_MpAnalyticItemParameterId=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemParameterValue>(query, paramId);
            return result;
        }

        #endregion MpAnalyticItem

        #region MpDbLog

        public async Task<MpDbLog> GetDbLogById(int DbLogId) {
            string query = string.Format(@"select * from MpDbLog where Id=?");
            var result = await MpDb.Instance.QueryAsync<MpDbLog>(query, DbLogId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<List<MpDbLog>> GetDbLogsByGuidAsync(string dboGuid, DateTime fromDateUtc) {
            string query = string.Format(@"select * from MpDbLog where DbObjectGuid=? and LogActionDateTime>?");
            var result = await MpDb.Instance.QueryAsync<MpDbLog>(query, dboGuid, fromDateUtc);
            return result;
        }

        #endregion

        #region MpSyncHistory

        public async Task<MpSyncHistory> GetSyncHistoryByDeviceGuid(string dg) {
            string query = string.Format(@"select * from MpSyncHistory where OtherClientGuid=?");
            var result = await MpDb.Instance.QueryAsync<MpSyncHistory>(query,dg);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion

        #endregion

        #endregion

        private string GetFetchQuery(int startIndex, int count, bool queryForTotalCount = false, int forceTagId = -1, bool ignoreSearchStr = false, int forceCheckCopyItemId = -1) {
            int tagId = forceTagId > 0 ? forceTagId : QueryInfo.TagId;
            string descStr = QueryInfo.IsDescending ? "DESC" : "ASC";
            string sortStr = Enum.GetName(typeof(MpContentSortType), QueryInfo.SortType);
            string searchStr = string.IsNullOrWhiteSpace(QueryInfo.SearchText) ? null : QueryInfo.SearchText;
            searchStr = ignoreSearchStr ? null : searchStr;
            var st = QueryInfo.SortType;

            if (st == MpContentSortType.Source ||
                st == MpContentSortType.UsageScore ||
                st == MpContentSortType.Manual) {
                if (st != MpContentSortType.Manual) {
                    MpConsole.WriteLine("Ignoring unimplemented sort type: " + sortStr + " and sorting by id...");
                }
                sortStr = "pk_MpCopyItemId";
            }


            string checkCopyItemToken = string.Empty;
            if (forceCheckCopyItemId > 0) {
                checkCopyItemToken = $" and pk_MpCopyItemId={forceCheckCopyItemId}";
                queryForTotalCount = true;
            }

            string selectToken = "*";
            if (queryForTotalCount) {
                startIndex = 0;
                count = int.MaxValue;
                selectToken = "count(pk_MpCopyItemId)";
            }

            if (tagId == MpTag.RecentTagId) {
                startIndex = 0;
                count = MpPreferences.Instance.MaxRecentClipItems;
            }


            string query;

            switch (tagId) {
                case MpTag.RecentTagId:
                    if (queryForTotalCount) {
                        query = string.Format(@"select count(*) from MpCopyItem 
                                                    where (pk_MpCopyItemId in
	                                                    (select pk_MpCopyItemId from MpCopyItem where fk_ParentCopyItemId = 0 and pk_MpCopyItemId in (
	                                                    select pci.pk_MpCopyItemId from MpCopyItem aci
	                                                    inner join MpCopyItem pci 
	                                                    ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
	                                                    order by aci.CopyDateTime) limit {0})
                                                    or fk_ParentCopyItemId in 
	                                                    (select pk_MpCopyItemId from MpCopyItem where fk_ParentCopyItemId = 0 and pk_MpCopyItemId in (
	                                                    select pci.pk_MpCopyItemId from MpCopyItem aci
	                                                    inner join MpCopyItem pci 
	                                                    ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
	                                                    order by aci.CopyDateTime) limit {0})){1}", count, checkCopyItemToken);
                    } else {
                        query = string.Format(@"select {4} from MpCopyItem where fk_ParentCopyItemId = 0 and pk_MpCopyItemId in (
                                            select pci.pk_MpCopyItemId from MpCopyItem aci
                                            inner join MpCopyItem pci  
                                            ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
                                            order by aci.{0} {1}) order by {0} {1} limit {2} offset {3}",
                                           sortStr, descStr, count, startIndex, selectToken);
                    }
                    break;
                case MpTag.AllTagId:
                    if (searchStr == null) {
                        query = string.Format(@"select {4} from MpCopyItem where fk_ParentCopyItemId = 0 
                                            order by {0} {1} limit {2} offset {3}",
                                          sortStr, descStr, count, startIndex, selectToken);
                    } else {
                        query = string.Format(@"select {4} from MpCopyItem where pk_MpCopyItemId in 
                                            (select distinct
                                                case fk_ParentCopyItemId
                                                    when 0
                                                        then pk_MpCopyItemId
                                                    ELSE
                                                        fk_ParentCopyItemId
                                                end
                                                from MpCopyItem where ItemData like '%{5}%')
                                            order by {0} {1} limit {2} offset {3}",
                                            sortStr, descStr, count, startIndex, selectToken, searchStr);
                    }
                    break;
                default:
                    query = string.Format(@"select {5} from MpCopyItem where pk_MpCopyItemId in 
                                            (select distinct
	                                            case fk_ParentCopyItemId
		                                            when 0
			                                            then pk_MpCopyItemId
		                                            ELSE
			                                            fk_ParentCopyItemId
	                                            end
	                                            from MpCopyItem where pk_MpCopyItemId in 
                                                (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={4}))
                                           order by {0} {1} limit {2} offset {3}",
                                           sortStr, descStr, count, startIndex, tagId, selectToken);
                    break;
            }
            return query;
        }

        public async Task<List<MpCopyItem>> GetPageAsync(
            int tagId,
            int start,
            int count,
            MpContentSortType sortType,
            bool isDescending,
            Dictionary<int, int> manualSortOrderLookup = null) {
            List<MpCopyItem> result = await MpDb.Instance.GetItemsAsync<MpCopyItem>();

            switch (tagId) {
                case MpTag.RecentTagId:
                    result = result.Where(x => x.CompositeParentCopyItemId == 0)
                                 .OrderByDynamic(isDescending, x => x.CopyDateTime)
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    return result;
                case MpTag.AllTagId:
                    result = result.Where(x => x.CompositeParentCopyItemId == 0).ToList();
                    break;
                default:
                    var citl = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();
                    if (isDescending) {
                        result = (from value in
                                    (from ci in result
                                     from cit in citl
                                     where ci.Id == cit.CopyItemId &&
                                         tagId == cit.TagId
                                     select new { ci, cit })
                                  orderby value.cit.CopyItemSortIdx descending
                                  select value.ci)
                                                  .Where(x => x.CompositeParentCopyItemId == 0)
                                                 .ToList();
                    } else {
                        result = (from value in
                                    (from ci in result
                                     from cit in citl
                                     where ci.Id == cit.CopyItemId &&
                                         tagId == cit.TagId
                                     select new { ci, cit })
                                  orderby value.cit.CopyItemSortIdx ascending
                                  select value.ci)
                                                  .Where(x => x.CompositeParentCopyItemId == 0)
                                                 .ToList();
                    }
                    break;
            }
            switch (sortType) {
                case MpContentSortType.CopyDateTime:
                    result = result.OrderBy(x => x.GetType().GetProperty(nameof(x.CopyDateTime)).GetValue(x))
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    break;
                case MpContentSortType.ItemType:
                    result = result.OrderBy(x => x.GetType().GetProperty(nameof(x.ItemType)).GetValue(x))
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    break;
                // TODO add rest of sort types
                case MpContentSortType.Manual:
                    if (manualSortOrderLookup == null) {
                        result = result.Take(count).Skip(start).ToList();
                    } else {
                        int missingCount = 0;
                        var missingItems = new List<MpCopyItem>();
                        foreach (var ci in result) {
                            if (manualSortOrderLookup.ContainsKey(ci.Id)) {
                                ci.ManualSortIdx = manualSortOrderLookup[ci.Id];
                            } else {
                                missingCount++;
                                if (isDescending) {
                                    ci.ManualSortIdx = manualSortOrderLookup.Min(x => x.Value) - missingCount;
                                } else {
                                    ci.ManualSortIdx = manualSortOrderLookup.Max(x => x.Value) + missingCount;
                                }

                            }
                        }
                        result = result.OrderByDynamic(isDescending, x => x.ManualSortIdx).Take(count).Skip(start).ToList();
                    }
                    break;
            }
            return result;
        }

        public async Task RemoveQueryItem(int copyItemId) {
            if (_allFetchedAndSortedCopyItemIds.Contains(copyItemId)) {
                int newParentId = 0;
                var ccil = await GetCompositeChildrenAsync(copyItemId);
                for (int i = 0; i < ccil.Count; i++) {
                    if (i == 0) {
                        newParentId = ccil[i].Id;
                        ccil[i].CompositeParentCopyItemId = 0;
                    } else {
                        ccil[i].CompositeParentCopyItemId = newParentId;
                    }
                    ccil[i].CompositeSortOrderIdx = i;
                    await ccil[i].WriteToDatabaseAsync();
                }
                if (newParentId > 0) {
                    _allFetchedAndSortedCopyItemIds[_allFetchedAndSortedCopyItemIds.IndexOf(copyItemId)] = newParentId;
                } else {
                    _allFetchedAndSortedCopyItemIds.Remove(copyItemId);
                }
            }
        }
        public void MoveOrInsertQueryItem(int copyItemId, int newIdx) {
            if (_allFetchedAndSortedCopyItemIds.Contains(copyItemId)) {
                int oldIdx = _allFetchedAndSortedCopyItemIds.IndexOf(copyItemId);
                if (newIdx > oldIdx) {
                    newIdx--;
                }
                _allFetchedAndSortedCopyItemIds.Remove(copyItemId);

            }

            if (newIdx >= _allFetchedAndSortedCopyItemIds.Count) {
                _allFetchedAndSortedCopyItemIds.Add(newIdx);
            } else {
                _allFetchedAndSortedCopyItemIds.Insert(newIdx, copyItemId);
            }
        }

        private  void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if(e is MpCopyItem ci) {
                //if(_allFetchedAndSortedCopyItemIds.Contains(ci.Id)) {
                //    int newParentId = 0;
                //    var ccil = await GetCompositeChildrenAsync(ci.Id);
                //    for (int i = 0; i < ccil.Count; i++) {
                //        if (i == 0) {
                //            newParentId = ccil[i].Id;
                //            ccil[i].CompositeParentCopyItemId = 0;
                //        } else {
                //            ccil[i].CompositeParentCopyItemId = newParentId;
                //        }
                //        ccil[i].CompositeSortOrderIdx = i;
                //        await ccil[i].WriteToDatabaseAsync();
                //    }
                //    if (newParentId > 0) {
                //        _allFetchedAndSortedCopyItemIds[_allFetchedAndSortedCopyItemIds.IndexOf(ci.Id)] = newParentId;
                //    } else {
                //        _allFetchedAndSortedCopyItemIds.Remove(ci.Id);
                //    }

                //    QueryInfo.NotifyQueryChanged(false);
                //}
            }
        }

        private  void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
            }
        }
    }
}
