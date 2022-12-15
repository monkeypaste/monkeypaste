using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpContentQuery {
        public static async Task<List<int>> QueryAllAsync(
            MpIQueryInfo head_qi, 
            IEnumerable<int> tagIds,
            IEnumerable<int> ci_idsToOmit) {
            ci_idsToOmit = ci_idsToOmit == null ? new List<int>() : ci_idsToOmit;

            List<int> totalIds = null;
            string viewQueryStr = string.Empty;
            //AvailableQueryCopyItemIds.Clear();


            MpIQueryInfo prev_qi = null;
            for (var qi = head_qi; qi != null; qi = qi.Next) {
                //if (prev_qi != null) {
                //    qi.IsDescending = prev_qi.IsDescending;
                //    qi.SortType = prev_qi.SortType;
                //    qi.TagId = prev_qi.TagId;
                //}
                //if (qi.FilterFlags.HasFlag(MpContentFilterType.Tag) && i > 0) {
                //    if (qi.FilterFlags.HasFlag(MpContentFilterType.Regex)) {
                //        qi.TagId = MpTag.AllTagId;
                //    } else {
                //        qi.TagId = Convert.ToInt32(qi.SearchText);
                //    }
                //}
                
                var qi_result = await PerformContentQueryAsync(qi, tagIds);
                if (totalIds == null) {
                    totalIds = qi_result.ToList();
                } else if (prev_qi != null) {
                    if(prev_qi.NextJoinType == MpLogicalFilterFlagType.And) {
                        // only allow results if both this and previous had match
                        totalIds = totalIds.Where(x => qi_result.Contains(x)).ToList();
                    } else {
                        // compound results
                        totalIds.AddRange(qi_result);
                        totalIds.Distinct();
                    }
                }
            }
            return totalIds.Where(x => !ci_idsToOmit.Contains(x)).ToList();
        } 

        private static async Task<List<int>> PerformContentQueryAsync(MpIQueryInfo qi, IEnumerable<int> tagIds) {
            string qi_root_id_query_str = ConvertQueryToSql(qi, tagIds);

            MpConsole.WriteLine("Current DataModel Query: " + qi_root_id_query_str);

            var result = await MpDb.QueryScalarsAsync<int>(qi_root_id_query_str);
            return result.Distinct().ToList();
        }

        private static string ConvertQueryToSql(MpIQueryInfo qi, IEnumerable<int> tagIds) {
            string query = "select RootId from MpSortableCopyItem_View";
            string tagClause = string.Empty;
            string sortClause = string.Empty;
            List<string> types = new List<string>();
            List<string> filters = new List<string>();

            if (qi.TagId != MpTag.AllTagId) {
                // NOTE ignoring tagIds for all is just to optimize since they're all included anyway
                string tag_where_stmt = string.Join(" or ", tagIds.Select(x => $"fk_MpTagId={x}"));
                tagClause = string.Format(
                    @"RootId in 
                    (select distinct pk_MpCopyItemId from MpCopyItem where pk_MpCopyItemId in 
		                (select fk_MpCopyItemId from MpCopyItemTag where {0}))",
                    tag_where_stmt);
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
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("ItemMetaData"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentFilterType.DeviceName)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("DeviceName"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentFilterType.DeviceType)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("DeviceType"), searchOp, escapeStr, searchText, escapeClause));
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
                types.Add(string.Format(@"e_MpCopyItemType='{0}'", MpCopyItemType.Text.ToString()));
            }
            if (qi.FilterFlags.HasFlag(MpContentFilterType.FileType)) {
                types.Add(string.Format(@"e_MpCopyItemType='{0}'", MpCopyItemType.FileList.ToString()));
            }
            if (qi.FilterFlags.HasFlag(MpContentFilterType.ImageType)) {
                types.Add(string.Format(@"e_MpCopyItemType='{0}'", MpCopyItemType.Image.ToString()));
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
                    sortClause = string.Format(@"order by {0}", "e_MpCopyItemType");
                    break;
                case MpContentSortType.UsageScore:
                    sortClause = string.Format(@"order by {0}", "UsageScore");
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


        private static string CaseFormat(string fieldOrSearchText) {
            //if (QueryInfo.FilterFlags.HasFlag(MpContentFilterType.CaseSensitive)) {
            //    return string.Format(@"UPPER({0})", fieldOrSearchText);
            //}
            return fieldOrSearchText;
        }
    }
}
