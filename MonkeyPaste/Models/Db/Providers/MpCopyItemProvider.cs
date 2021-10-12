using FFImageLoading.Helpers.Exif;
using MonkeyPaste;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

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
        int FetchCount();

        /// <summary>
        /// Fetches a range of items.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The number of items to fetch.</param>
        /// <returns></returns>
        IList<T> FetchRange(int startIndex, int count);


    }


    public class MpCopyItemProvider : MpIItemsProvider<MpCopyItem> {
        #region Singleton Definition
        private static readonly Lazy<MpCopyItemProvider> _Lazy = new Lazy<MpCopyItemProvider>(() => new MpCopyItemProvider());
        public static MpCopyItemProvider Instance { get { return _Lazy.Value; } }
        
        private MpCopyItemProvider() {
            MpDb.Instance.OnItemAdded += Instance_OnItemAdded;
            MpDb.Instance.OnItemUpdated += Instance_OnItemUpdated;
            MpDb.Instance.OnItemDeleted += Instance_OnItemDeleted;

            var sl = MpDb.Instance.GetItems<MpSource>();
            _sourceLookup = new Dictionary<int, MpSource>();
            sl.ForEach(x => _sourceLookup.Add(x.Id, x));

            _queryInfo = new MpQueryInfo();
        }

        #endregion

        #region Private Variables
        private MpQueryInfo _queryInfo;

        private Dictionary<int, MpSource> _sourceLookup;
        #endregion

        #region Properties

        #endregion

        #region Public Methods

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


        public async Task<List<int>> QueryForIds(
            int tagId,
            MpContentSortType sortType,
            bool isDescending,
            Dictionary<int, int> manualSortOrderLookup = null) {

            var citm = MpDb.Instance.GetTableMapping("MpCopyItem");
            string sortColumn = citm.FindColumnWithPropertyName(Enum.GetName(typeof(MpContentSortType), sortType)).Name;
            string isDescStr = isDescending ? "DESC" : "ASC";
            string query = string.Empty;

            switch (tagId) {
                case MpTag.RecentTagId:
                    int maxRecentCount = MpPreferences.Instance.MaxRecentClipItems;
                    query = $"select pk_MpCopyItemId from MpCopyItem where fk_ParentCopyItemId = 0 order by CopyDateTime {isDescStr} limit {maxRecentCount}";
                    break;
                case MpTag.AllTagId:
                    query = $"select pk_MpCopyItemId from MpCopyItem where fk_ParentCopyItemId = 0 order by {sortColumn} {isDescStr}";
                    break;
                default:
                    
                    break;
            }

            var result = await MpDb.Instance.QueryScalarsAsync<int>(query);
            return result;
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

        public void SetQueryInfo(MpQueryInfo info) {
            _queryInfo = info;
        }

        public int FetchCount() {
            int count = 0;
            switch(_queryInfo.TagId) {
                case MpTag.AllTagId:
                    count = GetTotalCopyItemCount();
                    break;
                case MpTag.RecentTagId:
                    count = GetRecentCopyItemCount();
                    break;
                default:

                    break;
            }

            return count;
        }

        public IList<MpCopyItem> FetchRange(int startIndex, int count, Dictionary<int, int> manualSortOrderLookup = null) {
            int tagId = _queryInfo.TagId;
            string descStr = _queryInfo.IsDescending ? "DESC" : "ASC";
            string sortStr = Enum.GetName(typeof(MpContentSortType), _queryInfo.SortType);
            var st = _queryInfo.SortType;
            if (st == MpContentSortType.Source ||
                st == MpContentSortType.UsageScore ||
                st == MpContentSortType.Manual) {
                if(st != MpContentSortType.Manual) {
                    MpConsole.WriteLine("Ignoring unimplemented sort type: " + sortStr + " and sorting by id...");
                }
                sortStr = "pk_MpCopyItemId";
            }
            
            string query = string.Empty;

            switch (tagId) {
                case MpTag.RecentTagId:
                    query = string.Format(@"select * from MpCopyItem where fk_ParentCopyItemId = 0 and pk_MpCopyItemId in (
                                            select pci.pk_MpCopyItemId from MpCopyItem aci
                                            inner join MpCopyItem pci 
                                            ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
                                            order by aci.{0} {1}) limit {2} offset {3}",
                                           sortStr, descStr,count,startIndex);
                    break;
                case MpTag.AllTagId:
                    query = string.Format(@"select * from MpCopyItem where fk_ParentCopyItemId = 0 
                                            order by {0} {1}) limit {2} offset {3}",
                                           sortStr, descStr, count, startIndex);
                    break;
                default:
                    query = string.Format(@"select * from MpCopyItem where pk_MpCopyItemId in 
                                            (select distinct
	                                            case fk_ParentCopyItemId
		                                            when 0
			                                            then pk_MpCopyItemId
		                                            ELSE
			                                            fk_ParentCopyItemId
	                                            end
	                                            from MpCopyItem where pk_MpCopyItemId in 
                                                (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0}))
                                           order by {0} {1} limit {2} offset {3}",
                                           sortStr, descStr, count, startIndex);
                    break;
            }

            var result = MpDb.Instance.Query<MpCopyItem>(query);
            result.ForEach(x => x.Source = _sourceLookup[x.SourceId]);

            return result;
        }

        public IList<MpCopyItem> FetchRange(int startIndex, int count) {
            return FetchRange(startIndex, count, null);
        }

        #endregion

        #region Private Methods

        #region Db Event Handlers

        private void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpSource s) {
                if(!_sourceLookup.ContainsKey(s.Id)) {
                    _sourceLookup.Add(s.Id, s);
                }
            }
        }

        private void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpApp a) {
                var sa = _sourceLookup.Where(x => x.Value.AppId == a.Id).FirstOrDefault();
                if (sa.Value != null) {
                    _sourceLookup[sa.Key].App = a;
                }
            } else if (e is MpUrl url) {
                var sa = _sourceLookup.Where(x => x.Value.UrlId == url.Id).FirstOrDefault();
                if (sa.Value != null) {
                    _sourceLookup[sa.Key].Url = url;
                }
            }
        }

        private void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpSource s) {
                if (_sourceLookup.ContainsKey(s.Id)) {
                    _sourceLookup.Remove(s.Id);
                }
            }
        }

        #endregion

        #endregion
    }
}
