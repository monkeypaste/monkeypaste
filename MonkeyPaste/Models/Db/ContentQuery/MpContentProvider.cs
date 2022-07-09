using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpContentProvider {
        #region Private Variables
        private static IList<MpCopyItem> _lastResult;

        //private static List<int> _manualQueryIds;
        //private static List<MpSize> _manualQuerySizes;

        private static MpIncrementalContentProvider _incrementalProvider;
        private static MpRandomContentProvider _randomProvider;

        #endregion

        #region Properties

        #endregion

        #region Constructor


        public static void Init() {
            if(_incrementalProvider == null) {
                _incrementalProvider = new MpIncrementalContentProvider();
            }
            if(_randomProvider == null) {
                _randomProvider = new MpRandomContentProvider();
            }
            ResetQuery();
        }


        #endregion

        #region Public Methods

        public static void ResetQuery() {
            AllFetchedAndSortedCopyItemIds.Clear();
            AllFetchedAndSortedTileOffsets.Clear();

            _lastResult = new List<MpCopyItem>();
        }

        public static void SetManualQuery(List<int> copyItemIds) {
            _manualQueryIds = copyItemIds;

            QueryInfo.NotifyQueryChanged();
        }

        public static void UnsetManualQuery() {
            _manualQueryIds = null;
            QueryInfo.NotifyQueryChanged();
        }

        #region MpQueryInfo Fetch Methods      

        public static async Task<double> Requery(double minWidth, double maxWidth, double marginWidth) {
            string viewQueryStr = await QueryForTotalCount();
            //double totalWidth = await QueryForOffsetPositions(viewQueryStr, minWidth, maxWidth, marginWidth);
            //return totalWidth;
            return 0;
        }

        public static async Task<double> QueryForOffsetPositions(string viewQueryStr, double minWidth, double maxWidth, double marginWidth) {
            AllFetchedAndSortedTileOffsets.Clear();

            string minWhenStr = $"when ItemWidth < {minWidth} then {minWidth} + {marginWidth}";
            string maxWhenStr = $"when ItemWidth > {maxWidth} then {maxWidth} + {marginWidth}";
            string elseStr = $"else ItemWidth + {marginWidth}";

            string caseStr = $"case {minWhenStr} {maxWhenStr} {elseStr} END";

            int rootIdStartIdx = viewQueryStr.IndexOf("RootId");
            string pre = viewQueryStr.Substring(0, rootIdStartIdx);
            string post = viewQueryStr.Substring(rootIdStartIdx + "RootId".Length);
            string itemWidthQueryStr = pre + caseStr + post;

            var widths = await MpDb.QueryScalarsAsync<double>(itemWidthQueryStr);

            double curOffset = 0.0d;
            for (int i = 0; i < widths.Count; i++) {
                if (i == 0) {
                    AllFetchedAndSortedTileOffsets.Add(0);
                    continue;
                }
                curOffset += widths[i - 1];
                AllFetchedAndSortedTileOffsets.Add(curOffset);
            }

            //string totalWidthQueryStr = pre + $"SUM({caseStr})" + post;

            //double totalWidth = await MpDb.QueryScalarAsync<double>(totalWidthQueryStr);
            //return totalWidth;
            return AllFetchedAndSortedTileOffsets[AllFetchedAndSortedTileOffsets.Count - 1] + widths[widths.Count - 1];
        }

        public static async Task<string> QueryForTotalCount() {
            string viewQueryStr = string.Empty;
            AllFetchedAndSortedCopyItemIds.Clear();

            if (_manualQueryIds != null) {
                foreach (var copyItemId in _manualQueryIds) {
                    AllFetchedAndSortedCopyItemIds.Add(copyItemId);
                }
                QueryInfo.TotalItemsInQuery = AllFetchedAndSortedCopyItemIds.Count;
                return viewQueryStr;
            }
            MpLogicalFilterFlagType lastLogicFlag = MpLogicalFilterFlagType.None;

            for (int i = 0; i < QueryInfos.Count; i++) {
                var qi = QueryInfos[i];
                if (i > 0) {
                    qi.IsDescending = QueryInfos[i - 1].IsDescending;
                    qi.SortType = QueryInfos[i - 1].SortType;
                    qi.TagId = QueryInfos[i - 1].TagId;
                }
                if (qi.FilterFlags.HasFlag(MpContentFilterType.Tag) && i > 0) {
                    if (qi.FilterFlags.HasFlag(MpContentFilterType.Regex)) {
                        qi.TagId = MpTag.AllTagId;
                    } else {
                        qi.TagId = Convert.ToInt32(qi.SearchText);
                    }
                }
                if (qi.LogicFlags != MpLogicalFilterFlagType.None) {
                    lastLogicFlag = qi.LogicFlags;
                    continue;
                }
                string allRootIdQuery = GetQueryForCount(i);
                viewQueryStr = allRootIdQuery;
                string totalWidthQuery = allRootIdQuery.Replace("RootId", "SUM(ItemWidth)");

                MpConsole.WriteTraceLine("Current DataModel Query: " + allRootIdQuery);

                var idl = await MpDb.QueryScalarsAsync<int>(allRootIdQuery);
                var curIds = new ObservableCollection<int>(idl.Distinct());

                var totalIds = new List<int>();
                if (i == 0) {
                    AllFetchedAndSortedCopyItemIds = curIds;
                } else if (lastLogicFlag == MpLogicalFilterFlagType.And) {
                    foreach (var allId in AllFetchedAndSortedCopyItemIds) {
                        if (curIds.Contains(allId)) {
                            totalIds.Add(allId);
                        }
                    }
                } else {
                    //same as or
                    totalIds.AddRange(AllFetchedAndSortedCopyItemIds);
                    totalIds.AddRange(curIds);
                    totalIds = totalIds.Distinct().ToList();
                }

                if (totalIds.Count > 0) {
                    AllFetchedAndSortedCopyItemIds = new ObservableCollection<int>(idl.Distinct());
                }
            }

            QueryInfo.TotalItemsInQuery = AllFetchedAndSortedCopyItemIds.Count;
            return viewQueryStr;
        }

        public static async Task<List<MpCopyItem>> FetchCopyItemRangeAsync(
            int startIndex,
            int count) {
            var fetchRange = AllFetchedAndSortedCopyItemIds.GetRange(startIndex, count);
            var items = await MpDataModelFetcher.GetCopyItemsByIdListAsync(fetchRange);
            if (items.Count == 0 && startIndex + count < AllFetchedAndSortedCopyItemIds.Count) {
                MpConsole.WriteTraceLine("Bad data detected for ids: " + string.Join(",", fetchRange));
            }
            return items;
        }

        public static async Task<List<MpCopyItem>> FetchCopyItemsByQueryIdxListAsync(
            List<int> copyItemQueryIdxList) {
            var fetchRootIds = AllFetchedAndSortedCopyItemIds
                                .Select((val, idx) => (val, idx))
                                .Where(x => copyItemQueryIdxList.Contains(x.idx))
                                .Select(x => x.val).ToList();
            var items = await MpDataModelFetcher.GetCopyItemsByIdListAsync(fetchRootIds);
            return items;
        }

        #endregion

        #region View Queries


        public static string GetQueryForCount(int qiIdx = 0) {
            var qi = QueryInfos[qiIdx];
            string query = "select RootId from MpSortableCopyItem_View";
            string tagClause = string.Empty;
            string sortClause = string.Empty;
            List<string> types = new List<string>();
            List<string> filters = new List<string>();

            if (qi.TagId != MpTag.AllTagId) {
                tagClause = string.Format(
                    @"RootId in 
                    (select pk_MpCopyItemId from MpCopyItem where pk_MpCopyItemId in 
		                (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0}))",
                    qi.TagId);
            }
            if (!string.IsNullOrEmpty(qi.SearchText)) {
                string searchOp = "like";
                string escapeStr = "";
                if (qi.FilterFlags.HasFlag(MpContentFilterType.CaseSensitive)) {
                    searchOp = "=";
                } else if (qi.FilterFlags.HasFlag(MpContentFilterType.Regex)) {
                    searchOp = "REGEXP";
                } else {
                    escapeStr = "%";
                }
                string searchText = qi.SearchText;

                string escapeClause = string.Empty;
                if (searchOp == "like" && searchText.Contains('%')) {
                    searchText = searchText.Replace("%", @"\%");
                    escapeClause = @" ESCAPE '\'";
                }
                if (searchText.Contains("'")) {
                    searchText = searchText.Replace("'", "''");
                }
                if (qi.FilterFlags.HasFlag(MpContentFilterType.Title)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("Title"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentFilterType.Content)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("ItemData"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentFilterType.Url)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("UrlPath"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentFilterType.UrlTitle)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("UrlTitle"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentFilterType.AppName)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("AppName"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentFilterType.AppPath)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("AppPath"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentFilterType.Meta)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("ItemDescription"), searchOp, escapeStr, searchText, escapeClause));
                }

                if (qi.FilterFlags.HasFlag(MpContentFilterType.Time)) {
                    if (qi.TimeFlags.HasFlag(MpTimeFilterFlagType.After)) {
                        searchOp = ">";
                    } else if (qi.TimeFlags.HasFlag(MpTimeFilterFlagType.Before)) {
                        searchOp = "<";
                    } else {
                        searchOp = "=";
                    }
                    searchText = DateTime.Parse(searchText).Ticks.ToString();
                    filters.Add(string.Format(@"{0} {1} {2}", CaseFormat("CopyDateTime"), searchOp, searchText));
                }
            }
            if (qi.FilterFlags.HasFlag(MpContentFilterType.TextType)) {
                types.Add(string.Format(@"fk_MpCopyItemTypeId={0}", (int)MpCopyItemType.Text));
            }
            if (qi.FilterFlags.HasFlag(MpContentFilterType.FileType)) {
                types.Add(string.Format(@"fk_MpCopyItemTypeId={0}", (int)MpCopyItemType.FileList));
            }
            if (qi.FilterFlags.HasFlag(MpContentFilterType.ImageType)) {
                types.Add(string.Format(@"fk_MpCopyItemTypeId={0}", (int)MpCopyItemType.Image));
            }

            switch (qi.SortType) {
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

            if (!string.IsNullOrEmpty(sortClause)) {
                sortClause = qi.IsDescending ? sortClause + " DESC" : sortClause;
            }

            if (!string.IsNullOrEmpty(tagClause)) {
                query += " where " + tagClause;
                if (filters.Count > 0) {
                    query += " and (";
                    query += string.Join(" or ", filters) + ")";
                }
                if (types.Count > 0) {
                    query += " and (";
                    query += string.Join(" or ", types) + ")";
                }
            } else if (filters.Count > 0) {
                query += " where ";
                if (types.Count > 0) {
                    query += "(";
                }
                query += string.Join(" or ", filters);
                if (types.Count > 0) {
                    query += ") and (";
                    query += string.Join(" or ", types) + ")";
                }
            } else if (types.Count > 0) {
                query += " where ";
                query += string.Join(" or ", types);
            }


            query += " " + sortClause;
            return query;
        }

        #endregion

        #endregion


        #region Private Methods
        private static string CaseFormat(string fieldOrSearchText) {
            //if (QueryInfo.FilterFlags.HasFlag(MpContentFilterType.CaseSensitive)) {
            //    return string.Format(@"UPPER({0})", fieldOrSearchText);
            //}
            return fieldOrSearchText;
        }

        #endregion
    }
}
