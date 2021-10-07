using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpCopyItemSource {

        public static async Task<List<int>> QueryForIds(
            int tagId,
            MpClipTileSortType sortType,
            bool isDescending,
            Dictionary<int, int> manualSortOrderLookup = null) {

            var citm = MpDb.Instance.GetTableMapping("MpCopyItem");
            string sortColumn = citm.FindColumnWithPropertyName(Enum.GetName(typeof(MpClipTileSortType), sortType)).Name;
            string isDescStr = isDescending ? "DESC" : "ASC";
            string query = string.Empty;

            switch (tagId) {
                case MpTag.RecentTagId:
                    int maxRecentCount = MpPreferences.Instance.MaxRecentClipItems;
                    query = $"select Id from MpCopyItem where fk_ParentCopyItemId = 0 orderby ModifiedDateTime {isDescStr} limit ?";
                    break;
                case MpTag.AllTagId:
                    query = $"select Id from MpCopyItem where fk_ParentCopyItemId = 0 orderby {sortColumn} {isDescStr}";
                    break;
                default:
                    
                    break;
            }

            var result = await MpDb.Instance.QueryAsync("MpCopyItem",query);
            return result.Cast<int>().ToList();
        }

        public static async Task<List<MpCopyItem>> GetPageAsync(
            int tagId,
            int start,
            int count,
            MpClipTileSortType sortType,
            bool isDescending,
            Dictionary<int, int> manualSortOrderLookup = null) {
            MpCopyItem dummyCi = new MpCopyItem();
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
                case MpClipTileSortType.CopyDateTime:
                    result = result.OrderBy(x => x.GetType().GetProperty(nameof(x.CopyDateTime)).GetValue(x))
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    break;
                case MpClipTileSortType.ItemType:
                    result = result.OrderBy(x => x.GetType().GetProperty(nameof(x.ItemType)).GetValue(x))
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    break;
                // TODO add rest of sort types
                case MpClipTileSortType.Manual:
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
    }
}
