using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpContentQuery {
        public static async Task<List<int>> QueryAllAsync(MpIQueryInfo head_qi, bool isAdvanced) {
            List<int> totalIds = null;
            MpIQueryInfo prev_qi = null;
            int idx = 0;
            for (var qi = head_qi; qi != null; qi = qi.Next) {
                IEnumerable<int> qi_tag_ids =
                    MpPlatform.Services.TagQueryTools.GetSelfAndAllDescendantsTagIds(qi.TagId);

                var qi_result = await PerformContentQueryAsync(qi, qi_tag_ids,isAdvanced,idx++);
                if (totalIds == null) {
                    totalIds = qi_result.ToList();
                } else if (prev_qi != null) {
                    if(prev_qi.NextJoinType == MpLogicalQueryType.And) {
                        // only allow results if both this and previous had match
                        totalIds = totalIds.Where(x => qi_result.Contains(x)).ToList();
                    } else {
                        // compound results
                        totalIds.AddRange(qi_result);
                    }
                    totalIds.Distinct();
                }
            }
            IEnumerable<int> ci_idsToOmit =
                MpPlatform.Services.ContentQueryTools.GetOmittedContentIds();

            return totalIds.Where(x => !ci_idsToOmit.Contains(x)).ToList();
        } 

        private static async Task<List<int>> PerformContentQueryAsync(MpIQueryInfo qi, IEnumerable<int> tagIds, bool isAdvanced, int idx) {
            string qi_root_id_query_str;
            if(isAdvanced) {
                qi_root_id_query_str = ConvertAdvancedQueryToSql(qi, tagIds);

            } else {
                qi_root_id_query_str = ConvertSimpleQueryToSql(qi, tagIds); 
            }           

            MpConsole.WriteLine($"Current DataModel Query ({idx}): " + qi_root_id_query_str);

            var result = await MpDb.QueryScalarsAsync<int>(qi_root_id_query_str);
            return result.Distinct().ToList();
        }

        #region Simple

        private static string ConvertSimpleQueryToSql(MpIQueryInfo qi, IEnumerable<int> tagIds) {
            string query = "select RootId from MpSortableCopyItem_View";
            string tagClause = string.Empty;
            string sortClause = string.Empty;
            List<string> types = new List<string>();
            List<string> filters = new List<string>();
            string tag_where_stmt = $"fk_MpTagId in ({string.Join(",", tagIds)})";
            tagClause =
                @$"RootId in 
                    (select distinct pk_MpCopyItemId from MpCopyItem where pk_MpCopyItemId in 
		                (select fk_MpCopyItemId from MpCopyItemTag where {tag_where_stmt}))";

            if (!string.IsNullOrEmpty(qi.SearchText)) {
                string searchOp = "like";
                string escapeStr = "";
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.CaseSensitive)) {
                    searchOp = "=";
                } else if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.Regex)) {
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
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.Title)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("Title"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.Content)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("ItemData"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.Url)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("UrlPath"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.UrlTitle)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("UrlTitle"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.AppName)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("AppName"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.AppPath)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("AppPath"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.Meta)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("ItemMetaData"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.DeviceName)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("DeviceName"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.DeviceType)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("DeviceType"), searchOp, escapeStr, searchText, escapeClause));
                }

                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.Time)) {
                    if (qi.TimeFlags.HasFlag(MpDateTimeQueryType.After)) {
                        searchOp = ">";
                    } else if (qi.TimeFlags.HasFlag(MpDateTimeQueryType.Before)) {
                        searchOp = "<";
                    } else {
                        searchOp = "=";
                    }
                    searchText = DateTime.Parse(searchText).Ticks.ToString();
                    filters.Add(string.Format(@"{0} {1} {2}", CaseFormat("CopyDateTime"), searchOp, searchText));
                }
            }
            if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.TextType)) {
                types.Add(string.Format(@"e_MpCopyItemType='{0}'", MpCopyItemType.Text.ToString()));
            }
            if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.FileType)) {
                types.Add(string.Format(@"e_MpCopyItemType='{0}'", MpCopyItemType.FileList.ToString()));
            }
            if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.ImageType)) {
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

        #endregion

        #region Advanced

        private static string ConvertAdvancedQueryToSql(MpIQueryInfo qi, IEnumerable<int> tagIds) {
            string query = "select RootId from MpSortableCopyItem_View";
            string tagClause = string.Empty;
            string sortClause = string.Empty;
            List<string> types = new List<string>();
            List<string> filters = new List<string>();
            string tag_where_stmt = $"fk_MpTagId in ({string.Join(",", tagIds)})";
            tagClause =
                @$"RootId in 
                    (select distinct pk_MpCopyItemId from MpCopyItem where pk_MpCopyItemId in 
		                (select fk_MpCopyItemId from MpCopyItemTag where {tag_where_stmt}))";

            if (!string.IsNullOrEmpty(qi.SearchText)) {
                string searchOp = "like";
                string escapeStr = "";
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.CaseSensitive)) {
                    searchOp = "=";
                } else if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.Regex)) {
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
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.Title)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("Title"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.Content)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("ItemData"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.Url)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("UrlPath"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.UrlTitle)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("UrlTitle"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.AppName)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("AppName"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.AppPath)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("AppPath"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.Meta)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("ItemMetaData"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.DeviceName)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("DeviceName"), searchOp, escapeStr, searchText, escapeClause));
                }
                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.DeviceType)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("DeviceType"), searchOp, escapeStr, searchText, escapeClause));
                }

                if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.Time)) {
                    if (qi.TimeFlags.HasFlag(MpDateTimeQueryType.After)) {
                        searchOp = ">";
                    } else if (qi.TimeFlags.HasFlag(MpDateTimeQueryType.Before)) {
                        searchOp = "<";
                    } else {
                        searchOp = "=";
                    }
                    searchText = DateTime.Parse(searchText).Ticks.ToString();
                    filters.Add(string.Format(@"{0} {1} {2}", CaseFormat("CopyDateTime"), searchOp, searchText));
                }
            }
            if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.TextType)) {
                types.Add(string.Format(@"e_MpCopyItemType='{0}'", MpCopyItemType.Text.ToString()));
            }
            if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.FileType)) {
                types.Add(string.Format(@"e_MpCopyItemType='{0}'", MpCopyItemType.FileList.ToString()));
            }
            if (qi.FilterFlags.HasFlag(MpContentQueryBitFlags.ImageType)) {
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

        #endregion

        private static string CaseFormat(string fieldOrSearchText) {
            //if (QueryInfo.FilterFlags.HasFlag(MpContentFilterType.CaseSensitive)) {
            //    return string.Format(@"UPPER({0})", fieldOrSearchText);
            //}
            return fieldOrSearchText;
        }
    }
}
