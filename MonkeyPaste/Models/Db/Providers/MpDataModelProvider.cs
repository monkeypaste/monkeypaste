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

    /// <summary>
    /// Represents a provider of collection details.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    public interface MpIItemsProvider<T> {
        /// <summary>
        /// Fetches the total number of items available.
        /// </summary>
        /// <returns></returns>
        int FetchCopyItemCount();

        /// <summary>
        /// Fetches a range of items.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The number of items to fetch.</param>
        /// <returns></returns>
        IList<T> FetchCopyItemRange(int startIndex, int count);


    }


    public class MpDataModelProvider : MpIItemsProvider<MpCopyItem> {
        #region Singleton Definition
        private static readonly Lazy<MpDataModelProvider> _Lazy = new Lazy<MpDataModelProvider>(() => new MpDataModelProvider());
        public static MpDataModelProvider Instance { get { return _Lazy.Value; } }
        
        private MpDataModelProvider() {
            //MpDb.Instance.OnItemAdded += Instance_OnItemAdded;
            //MpDb.Instance.OnItemUpdated += Instance_OnItemUpdated;
            //MpDb.Instance.OnItemDeleted += Instance_OnItemDeleted;

            //var sl = MpDb.Instance.GetItems<MpSource>();
            //_sourceLookup = new Dictionary<int, MpSource>();
            //sl.ForEach(x => _sourceLookup.Add(x.Id, x));
        }

        public void Init(MpQueryInfo queryInfo, int pageSize) {
            QueryInfo = queryInfo;
            QueryInfo.PageSize = pageSize;
        }

        #endregion

        #region Private Variables


        //private Dictionary<int, MpSource> _sourceLookup;
        #endregion

        #region Properties

        public MpQueryInfo QueryInfo { get; set; }

        #endregion

        #region Public Methods



        #region Select queries

        public async Task<int> GetTotalCopyItemCountAsync() {
            string query = "select count(pk_MpCopyItemId) from MpCopyItem";
            var result = await MpDb.Instance.QueryScalarAsync<int>(query);
            return result;
        }

        public int GetTotalCopyItemCount() {
            string query = "select count(pk_MpCopyItemId) from MpCopyItem";
            var result = MpDb.Instance.QueryScalar<int>(query);
            return result;
        }

        public async Task<int> GetRecentCopyItemCountAsync() {
            string query = @"select count(*) from MpCopyItem 
                                where pk_MpCopyItemId in
	                                (select pk_MpCopyItemId from MpCopyItem where fk_ParentCopyItemId = 0 and pk_MpCopyItemId in (
	                                select pci.pk_MpCopyItemId from MpCopyItem aci
	                                inner join MpCopyItem pci 
	                                ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
	                                order by aci.CopyDateTime) limit ?)
                                or fk_ParentCopyItemId in 
	                                (select pk_MpCopyItemId from MpCopyItem where fk_ParentCopyItemId = 0 and pk_MpCopyItemId in (
	                                select pci.pk_MpCopyItemId from MpCopyItem aci
	                                inner join MpCopyItem pci 
	                                ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
	                                order by aci.CopyDateTime) limit ?)";
            var result = await MpDb.Instance.QueryScalarAsync<int>(query, MpPreferences.Instance.MaxRecentClipItems, MpPreferences.Instance.MaxRecentClipItems);
            return result;
        }

        public int GetRecentCopyItemCount() {
            string query = @"select count(*) from MpCopyItem 
                                where pk_MpCopyItemId in
	                                (select pk_MpCopyItemId from MpCopyItem where fk_ParentCopyItemId = 0 and pk_MpCopyItemId in (
	                                select pci.pk_MpCopyItemId from MpCopyItem aci
	                                inner join MpCopyItem pci 
	                                ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
	                                order by aci.CopyDateTime) limit ?)
                                or fk_ParentCopyItemId in 
	                                (select pk_MpCopyItemId from MpCopyItem where fk_ParentCopyItemId = 0 and pk_MpCopyItemId in (
	                                select pci.pk_MpCopyItemId from MpCopyItem aci
	                                inner join MpCopyItem pci 
	                                ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
	                                order by aci.CopyDateTime) limit ?)";
            var result = MpDb.Instance.QueryScalar<int>(query, MpPreferences.Instance.MaxRecentClipItems, MpPreferences.Instance.MaxRecentClipItems);
            return result;
        }

        public List<MpCopyItem> GetRecentItems() {
            string query = @"select * from MpCopyItem where fk_ParentCopyItemId = 0 and pk_MpCopyItemId in (
                                select pci.pk_MpCopyItemId from MpCopyItem aci
                                inner join MpCopyItem pci 
                                ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
                                order by aci.CopyDateTime) limit ?";
            var result = MpDb.Instance.Query<MpCopyItem>(query, MpPreferences.Instance.MaxRecentClipItems);

            return result;
        }

        public async Task<List<MpCopyItem>> GetRecentItemsAsync() {
            string query = @"select * from MpCopyItem where fk_ParentCopyItemId = 0 and pk_MpCopyItemId in (
                                select pci.pk_MpCopyItemId from MpCopyItem aci
                                inner join MpCopyItem pci 
                                ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
                                order by aci.CopyDateTime) limit ?";
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query, MpPreferences.Instance.MaxRecentClipItems);

            return result;
        }

        public async Task<List<int>> GetCopyItemIdsForTagAsync(int tagId) {
            string query = string.Format(@"select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0}", tagId);
            var result = await MpDb.Instance.QueryScalarsAsync<int>(query);
            return result;
        }

        public async Task<List<MpCopyItem>> GetCopyItemsForTagAsync(int tagId) {
            string query = string.Format(@"select * from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=?)", tagId);
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query,tagId);
            return result;
        }

        public List<int> GetCopyItemIdsForTag(int tagId) {
            string query = string.Format(@"select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0}", tagId);
            var result = MpDb.Instance.QueryScalars<int>(query);
            return result;
        }


        public async Task<int> GetTagItemCountAsync(int tagId) {
            string query = string.Format(@"select count(pk_MpCopyItemId) from MpCopyItem where pk_MpCopyItemId in 
                                           (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0})", tagId);
            var result = await MpDb.Instance.QueryScalarAsync<int>(query);
            return result;
        }

        public int GetTagItemCount(int tagId) {
            string query = string.Format(@"select count(pk_MpCopyItemId) from MpCopyItem where pk_MpCopyItemId in 
                                           (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0})", tagId);
            var result = MpDb.Instance.QueryScalar<int>(query);
            return result;
        }

        public async Task<List<MpCopyItem>> GetCompositeChildrenAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItem where fk_ParentCopyItemId={0} order by CompositeSortOrderIdx", ciid);
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public List<MpCopyItem> GetCompositeChildren(int ciid) {
            string query = string.Format(
                @"select * from MpCopyItem where fk_ParentCopyItemId={0} order by CompositeSortOrderIdx", ciid);
            var result = MpDb.Instance.Query<MpCopyItem>(query);
            return result;
        }

        public async Task<int> GetCompositeChildCountAsync(int ciid) {
            string query = string.Format(@"select count(*) from MpCopyItem where fk_ParentCopyItemId={0} order by CompositeSortOrderIdx", ciid);
            var result = await MpDb.Instance.QueryScalarAsync<int>(query);
            return result;
        }

        public async Task<List<MpCopyItemTemplate>> GetTemplatesAsync(int ciid) {
            string query = string.Format(
                @"select * from MpCopyItemTemplate where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTemplate>(query);
            return result;
        }

        public List<MpCopyItemTemplate> GetTemplates(int ciid) {
            string query = string.Format(
                @"select * from MpCopyItemTemplate where fk_MpCopyItemId={0}", ciid);
            var result = MpDb.Instance.Query<MpCopyItemTemplate>(query);
            return result;
        }

        public async Task<List<MpShortcut>> GetCopyItemShortcutsAsync(int ciid) {
            string query = string.Format(
                @"select * from MpShortcut where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.Instance.QueryAsync<MpShortcut>(query);
            return result;
        }

        public List<MpShortcut> GetCopyItemShortcuts(int ciid) {
            string query = string.Format(
                @"select * from MpShortcut where fk_MpCopyItemId={0}", ciid);
            var result = MpDb.Instance.Query<MpShortcut>(query);
            return result;
        }

        public async Task<List<MpShortcut>> GetTagShortcutsAsync(int tid) {
            string query = string.Format(
                @"select * from MpShortcut where fk_MpTagId={0}", tid);
            var result = await MpDb.Instance.QueryAsync<MpShortcut>(query);
            return result;
        }

        public List<MpShortcut> GetTagShortcuts(int tid) {
            string query = string.Format(
                @"select * from MpShortcut where fk_MpTagId={0}", tid);
            var result = MpDb.Instance.Query<MpShortcut>(query);
            return result;
        }

        public async Task<List<MpShortcut>> GetAllShortcuts() {
            string query = @"select * from MpShortcut";
            var result = await MpDb.Instance.QueryAsync<MpShortcut>(query);
            return result;
        }

        public async Task<List<MpCopyItem>> GetCopyItemsByData(string text) {
            string query = $"select * from MpCopyItem where ItemData=?";
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

        public bool IsAppRejected(string path) {
            string query = $"select * from MpApp where SourcePath=?";
            var result = MpDb.Instance.Query<MpApp>(query, path);
            if (result == null || result.Count == 0) {
                return false;
            }
            return result[0].IsAppRejected;
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
            var result = await MpDb.Instance.QueryAsync<MpSource>(query, appId,urlId);
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

        public int FetchCopyItemCount() {
            int count = 0;
            switch(QueryInfo.TagId) {
                case MpTag.AllTagId:
                    count = GetTotalCopyItemCount();
                    break;
                case MpTag.RecentTagId:
                    count = GetRecentCopyItemCount();
                    break;
                default:
                    count = GetTagItemCount(QueryInfo.TagId);
                    break;
            }

            return count;
        }

        public async Task<int> FetchCopyItemCountAsync() {
            string totalCountQuery = GetFetchQuery(0, 0, true);
            int count = await MpDb.Instance.QueryScalarAsync<int>(totalCountQuery);
            return count;
        }

        public IList<MpCopyItem> FetchCopyItemRange(int startIndex, int count, Dictionary<int, int> manualSortOrderLookup = null) {
            string query = GetFetchQuery(startIndex, count);
            var result = MpDb.Instance.Query<MpCopyItem>(query);
            return result;
        }

        public async Task<IList<MpCopyItem>> FetchCopyItemRangeAsync(int startIndex, int count, Dictionary<int, int> manualSortOrderLookup = null) {
            string query = GetFetchQuery(startIndex, count);
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public IList<MpCopyItem> FetchCopyItemRange(int startIndex, int count) {
            return FetchCopyItemRange(startIndex, count, null);
        }

        #endregion

        #region Private Methods

        private string GetFetchQuery(int startIndex,int count, bool queryForTotalCount = false) {
            int tagId = QueryInfo.TagId;
            string descStr = QueryInfo.IsDescending ? "DESC" : "ASC";
            string sortStr = Enum.GetName(typeof(MpContentSortType), QueryInfo.SortType);
            var st = QueryInfo.SortType;
            if (st == MpContentSortType.Source ||
                st == MpContentSortType.UsageScore ||
                st == MpContentSortType.Manual) {
                if (st != MpContentSortType.Manual) {
                    MpConsole.WriteLine("Ignoring unimplemented sort type: " + sortStr + " and sorting by id...");
                }
                sortStr = "pk_MpCopyItemId";
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

            string query = string.Empty;

            switch (tagId) {
                case MpTag.RecentTagId:
                    query = string.Format(@"select {4} from MpCopyItem where fk_ParentCopyItemId = 0 and pk_MpCopyItemId in (
                                            select pci.pk_MpCopyItemId from MpCopyItem aci
                                            inner join MpCopyItem pci  
                                            ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
                                            order by aci.{0} {1}) order by {0} {1} limit {2} offset {3}",
                                           sortStr, descStr, count, startIndex,selectToken);
                    break;
                case MpTag.AllTagId:
                    query = string.Format(@"select {4} from MpCopyItem where fk_ParentCopyItemId = 0 
                                            order by {0} {1} limit {2} offset {3}",
                                           sortStr, descStr, count, startIndex,selectToken);
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

        //private void Instance_OnItemAdded(object sender, MpDbModelBase e) {
        //    if(e is MpSource s) {
        //        if(!_sourceLookup.ContainsKey(s.Id)) {
        //            _sourceLookup.Add(s.Id, s);
        //        }
        //    }
        //}

        //private void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
        //    if (e is MpApp a) {
        //        var sa = _sourceLookup.Where(x => x.Value.AppId == a.Id).FirstOrDefault();
        //        if (sa.Value != null) {
        //            _sourceLookup[sa.Key].App = a;
        //        }
        //    } else if (e is MpUrl url) {
        //        var sa = _sourceLookup.Where(x => x.Value.UrlId == url.Id).FirstOrDefault();
        //        if (sa.Value != null) {
        //            _sourceLookup[sa.Key].Url = url;
        //        }
        //    }
        //}

        //private void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
        //    if (e is MpSource s) {
        //        if (_sourceLookup.ContainsKey(s.Id)) {
        //            _sourceLookup.Remove(s.Id);
        //        }
        //    }
        //}

        #endregion

        #endregion
    }
}
