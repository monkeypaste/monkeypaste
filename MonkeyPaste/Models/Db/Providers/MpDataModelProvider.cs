using FFImageLoading.Helpers.Exif;
using MonkeyPaste;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace MonkeyPaste {
    public class MpDataModelProvider {
        #region Singleton Definition
        private static readonly Lazy<MpDataModelProvider> _Lazy = new Lazy<MpDataModelProvider>(() => new MpDataModelProvider());
        public static MpDataModelProvider Instance { get { return _Lazy.Value; } }
        
        private MpDataModelProvider() { }

        public void Init(MpIQueryInfo queryInfo, int pageSize) {
            QueryInfo = queryInfo;
            QueryInfo.PageSize = pageSize;
        }

        #endregion

        #region Private Variables
        #endregion

        #region Properties

        public MpIQueryInfo QueryInfo { get; set; }

        #endregion

        #region Public Methods

        #region Select queries

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

        public async Task<List<MpCopyItemTemplate>> GetTemplatesAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItemTemplate where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTemplate>(query);
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

        public async Task<List<MpShortcut>> GetAllShortcuts() {
            string query = @"select * from MpShortcut";
            var result = await MpDb.Instance.QueryAsync<MpShortcut>(query);
            return result;
        }

        public async Task<List<MpCopyItem>> GetCopyItemsByData(string text) {
            string query = "select * from MpCopyItem where ItemData=?";
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query, text);
            return result;
        }

        public async Task<MpIcon> GetIconByImageStr(string text64) {
            string query = $"select pk_MpDbImageId from MpDbImage where ImageBase64=?";
            int iconImgId = await MpDb.Instance.QueryScalarAsync<int>(query, text64);
            if(iconImgId <= 0) {
                return null;
            }
            query = $"select * from MpIcon where fk_IconDbImageId=?";
            var result = await MpDb.Instance.QueryAsync<MpIcon>(query, iconImgId);
            if(result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<MpApp> GetAppByPath(string path) {
            string query = $"select * from MpApp where SourcePath=?";
            var result = await MpDb.Instance.QueryAsync<MpApp>(query, path);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<bool> IsAppRejectedAsync(string path) {
            string query = $"select count(*) from MpApp where SourcePath=? and IsRejected=1";
            var result = await MpDb.Instance.QueryScalarAsync<int>(query, path);
            return result > 0;
        }

        public bool IsAppRejected(string path) {
            string query = $"select count(*) from MpApp where SourcePath=? and IsRejected=1";
            var result = MpDb.Instance.QueryScalar<int>(query, path);
            return result > 0;
        }

        public async Task<MpUrl> GetUrlByPath(string url) {
            string query = $"select * from MpUrl where UrlPath=?";
            var result = await MpDb.Instance.QueryAsync<MpUrl>(query, url);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<MpUserDevice> GetUserDeviceByGuid(string guid) {
            string query = $"select * from MpUserDevice where MpUserDeviceGuid=?";
            var result = await MpDb.Instance.QueryAsync<MpUserDevice>(query, guid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<MpSource> GetSourceByMembers(int appId,int urlId) {
            string query = $"select * from MpSource where fk_MpAppId=? and fk_MpUrlId=?";
            var result = await MpDb.Instance.QueryAsync<MpSource>(query,appId,urlId);
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

        public async Task<MpAnalyticItem> GetAnalyticItemByEndpoint(string endPoint) {
            string query = $"select * from MpAnalyticItem where EndPoint=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItem>(query, endPoint);
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

        public async Task<bool> IsCopyItemInRecentTag(int copyItemId) {
            string query = GetFetchQuery(0, 0, true, MpTag.RecentTagId, true, copyItemId);
            var result = await MpDb.Instance.QueryScalarAsync<int>(query);
            return result > 0;
        }

        public async Task<bool> IsTagLinkedWithCopyItem(int tagId,int copyItemId) {
            string query = $"select count(*) from MpCopyItemTag where fk_MpTagId=? and fk_MpCopyItemId=?";
            var result = await MpDb.Instance.QueryScalarAsync<int>(query, tagId, copyItemId);
            return result > 0;
        }
        #endregion

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

        public async Task<int> FetchCopyItemCountAsync() {
            string totalCountQuery = GetFetchQuery(0, 0, true, -1, true);
            int count = await MpDb.Instance.QueryScalarAsync<int>(totalCountQuery);
            return count;
        }

        public async Task<IList<MpCopyItem>> FetchCopyItemRangeAsync(int startIndex, int count, Dictionary<int, int> manualSortOrderLookup = null) {
            string query = GetFetchQuery(startIndex, count);
            IList<MpCopyItem> result = await MpDb.Instance.QueryAsync<MpCopyItem>(query);
            return result;
        }

        #endregion

        #region Private Methods

        private string GetFetchQuery(int startIndex,int count, bool queryForTotalCount = false, int forceTagId = -1, bool ignoreSearchStr = false, int forceCheckCopyItemId = -1) {
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

            if(tagId == MpTag.RecentTagId) {
                startIndex = 0;
                count = MpPreferences.Instance.MaxRecentClipItems;
            }


            string query;

            switch (tagId) {
                case MpTag.RecentTagId:
                    if(queryForTotalCount) {
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
	                                                    order by aci.CopyDateTime) limit {0})){1}", count,checkCopyItemToken);
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
                    if(searchStr == null) {
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
                                            sortStr, descStr, count, startIndex, selectToken,searchStr);
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
                                           sortStr, descStr, count, startIndex,tagId,selectToken);
                    break;
            }
            return query;
        }

        #region Db Event Handlers

        #endregion

        #endregion
    }
}
