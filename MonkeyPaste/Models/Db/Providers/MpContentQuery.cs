using Google.Apis.PeopleService.v1.Data;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    public static class MpContentQuery {
        private static MpIQueryInfo _cur_qi;
        public static async Task<List<int>> QueryAllAsync(MpIQueryInfo head_qi, bool isAdvanced) {
            List<int> result_ids = null;
            MpLogicalQueryType join_type = MpLogicalQueryType.None;
            int idx = 0;
            for (MpIQueryInfo qi = head_qi; qi != null; qi = qi.Next) {
                _cur_qi = qi;
                IEnumerable<int> qi_tag_ids =
                    MpPlatform.Services.TagQueryTools.GetSelfAndAllDescendantsTagIds(qi.TagId);

                var qi_result = await PerformContentQueryAsync(qi, qi_tag_ids,isAdvanced,idx++);
                switch(join_type) {
                    case MpLogicalQueryType.None:
                        // initial case
                        result_ids = qi_result.ToList();
                        break;
                    case MpLogicalQueryType.And:
                        // only allow results if both this and previous had match
                        result_ids = result_ids.Where(x => qi_result.Contains(x)).ToList();
                        break;
                    case MpLogicalQueryType.Or:
                        // compound results
                        result_ids.AddRange(qi_result);
                        break;
                    case MpLogicalQueryType.Not:
                        // remove current result from total
                        result_ids = result_ids.Where(x => !qi_result.Contains(x)).ToList();
                        break;
                }
                result_ids.Distinct();
                join_type = qi.NextJoinType;
            }
            _cur_qi = null;

            IEnumerable<int> ci_idsToOmit =
                MpPlatform.Services.ContentQueryTools.GetOmittedContentIds();

            return result_ids.Where(x => !ci_idsToOmit.Contains(x)).ToList();
        } 

        private static async Task<List<int>> PerformContentQueryAsync(MpIQueryInfo qi, IEnumerable<int> tagIds, bool isAdvanced, int idx) {
            string qi_root_id_query_str = ConvertQueryToSql(qi, tagIds, isAdvanced, out string pq, out var args); 

            MpConsole.WriteLine($"Current DataModel Query ({idx}): " + qi_root_id_query_str);

            var result = await MpDb.QueryScalarsAsync<int>(qi_root_id_query_str);
            var result2 = await MpDb.QueryScalarsAsync<int>(pq, args);
            var diff = result.Distinct().ToList().Difference(result2.Distinct().ToList());
            if(diff.Count() > 0) {
                // what's differnt?
                Debugger.Break();
            }

            return result.Distinct().ToList();
        }


        private static string ConvertQueryToSql(MpIQueryInfo qi, IEnumerable<int> tagIds, bool isAdvanced, out string pq, out object[] args) {
            string sortClause = string.Empty;
            List<string> types = new List<string>();
            List<string> filters = new List<string>();
            string searchOp = "LIKE";
            string searchOp2 = null;
            string escapeStr = "";

            MpContentQueryBitFlags qf = qi.QueryFlags;
            string mv = qi.MatchValue.CaseFormat(true);

            // FILTERS

            if (!string.IsNullOrEmpty(mv)) {
                if (qf.HasFlag(MpContentQueryBitFlags.CaseSensitive)) {
                    searchOp = "GLOB";
                } else if (qf.HasFlag(MpContentQueryBitFlags.Regex)) {
                    searchOp = "REGEXP";
                } else {
                    escapeStr = "%";
                }
                if(qf.HasFlag(MpContentQueryBitFlags.WholeWord)) {
                    searchOp = "REGEXP";
                }

                string escapeClause = string.Empty;
                if (searchOp == "LIKE" && mv.Contains('%')) {
                    mv = mv.Replace("%", @"\%");
                    escapeClause = @" ESCAPE '\'";
                }
                if (mv.Contains("'")) {
                    mv = mv.Replace("'", "''");
                }
                string fmv = qf.GetStringMatchValue(searchOp, mv);
                // <Field> <op> '<mv>'
                if (qf.HasFlag(MpContentQueryBitFlags.Title)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("Title"), searchOp, escapeStr, fmv, escapeClause));
                }
                if (qf.HasFlag(MpContentQueryBitFlags.Content)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("ItemData"), searchOp, escapeStr, fmv, escapeClause));
                }
                if (qf.HasFlag(MpContentQueryBitFlags.Url)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("UrlPath"), searchOp, escapeStr, fmv, escapeClause));
                }
                if (qf.HasFlag(MpContentQueryBitFlags.UrlTitle)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("UrlTitle"), searchOp, escapeStr, fmv, escapeClause));
                }
                if (qf.HasFlag(MpContentQueryBitFlags.AppName)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("AppName"), searchOp, escapeStr, fmv, escapeClause));
                }
                if (qf.HasFlag(MpContentQueryBitFlags.AppPath)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("AppPath"), searchOp, escapeStr, fmv, escapeClause));
                }
                if (qf.HasFlag(MpContentQueryBitFlags.Meta)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("ItemMetaData"), searchOp, escapeStr, fmv, escapeClause));
                }
                if (qf.HasFlag(MpContentQueryBitFlags.DeviceName)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("DeviceName"), searchOp, escapeStr, fmv, escapeClause));
                }
                if (qf.HasFlag(MpContentQueryBitFlags.DeviceType)) {
                    filters.Add(string.Format(@"{0} {1} '{2}{3}{2}'{4}", CaseFormat("DeviceType"), searchOp, escapeStr, fmv, escapeClause));
                }

                if (qf.HasFlag(MpContentQueryBitFlags.Before)) {
                    searchOp = "<";
                    string match_ticks = null;
                    string match_ticks2 = null;
                    if(qf.HasFlag(MpContentQueryBitFlags.Exactly)) {
                        try {
                            //exactly 1 < comp_field < 2

                            var dt = DateTime.Parse(mv);
                            var start_dt = dt.Date;
                            var end_dt = start_dt + TimeSpan.FromDays(0.999999);
                            match_ticks = start_dt.Ticks.ToString();
                            match_ticks2 = end_dt.Ticks.ToString();
                            searchOp = ">";
                            searchOp2 = "<";
                        }
                        catch {
                            searchOp = null;
                        }
                    } else {
                        // all day units
                        try {
                            double today_offset = (DateTime.Now - DateTime.Today).TotalDays;
                            double days = double.Parse(mv);
                            double total_day_offset = days + today_offset;
                            var dt = DateTime.Now - TimeSpan.FromDays(total_day_offset);
                            match_ticks = dt.Ticks.ToString();
                        }
                        catch {
                            searchOp = null;
                        }
                    }
                    if(!string.IsNullOrEmpty(match_ticks)) {
                        string comp_field = null;
                        if(qf.HasFlag(MpContentQueryBitFlags.Created)) {
                            comp_field = "CopyDateTime";
                        } else if (qf.HasFlag(MpContentQueryBitFlags.Modified)) {
                            comp_field = "TransactionDateTime";
                            // filter out TransactionLabel = 'Created'
                            filters.Add(string.Format(@"{0} {1} {2}", CaseFormat(MpTransactionType.Created.ToString()), "!=", match_ticks));
                        } else if (qf.HasFlag(MpContentQueryBitFlags.Pasted)) {
                            comp_field = "PasteDateTime";
                        }

                        if (!string.IsNullOrEmpty(comp_field)) {
                            if(!string.IsNullOrEmpty(match_ticks2)) {
                                //exactly 1 < comp_field < 2
                                filters.Add(string.Format(@"{0} {1} {2}", CaseFormat(comp_field), searchOp2, match_ticks2));
                            } 
                            filters.Add(string.Format(@"{0} {1} {2}", CaseFormat(comp_field), searchOp, match_ticks));
                        }
                    }
                }
                if(qf.HasFlag(MpContentQueryBitFlags.After)) {
                    searchOp = ">";
                }
            }

            // TYPES

            if (qf.HasFlag(MpContentQueryBitFlags.TextType)) {
                types.Add(string.Format(@"e_MpCopyItemType='{0}'", MpCopyItemType.Text.ToString()));
            }
            if (qf.HasFlag(MpContentQueryBitFlags.FileType)) {
                types.Add(string.Format(@"e_MpCopyItemType='{0}'", MpCopyItemType.FileList.ToString()));
            }
            if (qf.HasFlag(MpContentQueryBitFlags.ImageType)) {
                types.Add(string.Format(@"e_MpCopyItemType='{0}'", MpCopyItemType.Image.ToString()));
            }

            // SORT

            switch (qi.SortType) {
                case MpContentSortType.CopyDateTime:
                    sortClause = string.Format(@"ORDER BY {0}", "CopyDateTime");
                    break;
                case MpContentSortType.Source:
                    sortClause = string.Format(@"ORDER BY {0}", "SourcePath");
                    break;
                case MpContentSortType.Title:
                    sortClause = string.Format(@"ORDER BY {0}", "Title");
                    break;
                case MpContentSortType.ItemData:
                    sortClause = string.Format(@"ORDER BY {0}", "ItemData");
                    break;
                case MpContentSortType.ItemType:
                    sortClause = string.Format(@"ORDER BY {0}", "e_MpCopyItemType");
                    break;
                case MpContentSortType.UsageScore:
                    sortClause = string.Format(@"ORDER BY {0}", "UsageScore");
                    break;
            }

            if (!string.IsNullOrEmpty(sortClause)) {
                sortClause = qi.IsDescending ? sortClause + " DESC" : sortClause;
            }

            // SELECT GEN

            string sql_view = isAdvanced ? "MpAdvancedSortableCopyItem_View" : "MpSortableCopyItem_View";

            string query = $"SELECT RootId FROM {sql_view}";
            string tag_where_stmt = $"fk_MpTagId IN ({string.Join(",", tagIds)})";
            string tagClause =
                @$"RootId IN 
                    (SELECT DISTINCT pk_MpCopyItemId FROM MpCopyItem WHERE pk_MpCopyItemId IN 
		                (SELECT fk_MpCopyItemId FROM MpCopyItemTag WHERE {tag_where_stmt}))";


            List<Tuple<string, List<object>>> arg_filters = GetParameterizedFilters(qi);
            List<object> argList = new List<object>();
            if (arg_filters != null) {
                argList = arg_filters.SelectMany(x => x.Item2).ToList();
            }
            string filter_op = isAdvanced ? " AND " : " OR ";
            string param_query = $"{query} where {tagClause}";
            if(arg_filters.Count > 0) {
                param_query += $" AND ({string.Join(filter_op, arg_filters.Select(x=>x.Item1))})";
            }
            if (types.Count > 0) {
                param_query += $" AND ({string.Join(" OR ", types)})";
            }

            param_query += $" {sortClause}";

            if (!string.IsNullOrEmpty(tagClause)) {
                query += " where " + tagClause;
                if (filters.Count > 0) {
                    query += " AND (";
                    query += string.Join(" OR ", filters) + ")";
                }
                if (types.Count > 0) {
                    query += " AND (";
                    query += string.Join(" OR ", types) + ")";
                }
            } else if (filters.Count > 0) {
                query += " where ";
                if (types.Count > 0) {
                    query += "(";
                }
                query += string.Join(" OR ", filters);
                if (types.Count > 0) {
                    query += ") AND (";
                    query += string.Join(" OR ", types) + ")";
                }
            } else if (types.Count > 0) {
                query += " where ";
                query += string.Join(" OR ", types);
            }
            query += " " + sortClause;

            pq = param_query;
            args = argList.ToArray();
            return query;
        }

        #region Helpers

        private static List<Tuple<string,List<object>>> GetParameterizedFilters(MpIQueryInfo qi) {
            List<Tuple<string, List<object>>> arg_filters = new List<Tuple<string, List<object>>>();
            if(string.IsNullOrEmpty(qi.MatchValue)) {
                return arg_filters;
            }

            MpContentQueryBitFlags qf = qi.QueryFlags;
            string mv = qi.MatchValue.CaseFormat(true);

            if (qf.GetStringMatchFieldName() is IEnumerable<string> strFieldNames) {
                // string matching
                foreach(string strFieldName in strFieldNames) {
                    string strOp = qf.GetStringMatchOp();
                    // <Field> <op> '<mv>'
                    string strFilter = $"{strFieldName.CaseFormat()} {strOp} ?";
                    object strParam = qf.GetStringMatchValue(strOp, mv);
                    arg_filters.Add(new Tuple<string, List<object>>(strFilter, new[] { strParam }.ToList()));
                }
            } else {
                if (qf.HasFlag(MpContentQueryBitFlags.Before)) {
                    string tickOp = "<";
                    string tickOp2 = null;
                    string match_ticks = null;
                    string match_ticks2 = null;
                    if (qf.HasFlag(MpContentQueryBitFlags.Exactly)) {
                        try {
                            //exactly 1 < comp_field < 2

                            var dt = DateTime.Parse(mv);
                            var start_dt = dt.Date;
                            var end_dt = start_dt + TimeSpan.FromDays(0.999999);
                            match_ticks = start_dt.Ticks.ToString();
                            match_ticks2 = end_dt.Ticks.ToString();
                            tickOp = ">";
                            tickOp2 = "<";
                        }
                        catch {
                            tickOp = null;
                        }
                    } else {
                        // all day units
                        try {
                            double today_offset = (DateTime.Now - DateTime.Today).TotalDays;
                            double days = double.Parse(mv);
                            double total_day_offset = days + today_offset;
                            var dt = DateTime.Now - TimeSpan.FromDays(total_day_offset);
                            match_ticks = dt.Ticks.ToString();
                        }
                        catch {
                            tickOp = null;
                        }
                    }
                    if (!string.IsNullOrEmpty(match_ticks)) {
                        string comp_field = null;
                        if (qf.HasFlag(MpContentQueryBitFlags.Created)) {
                            comp_field = MpContentQueryBitFlags.Created.ToViewFieldName();
                        } else if (qf.HasFlag(MpContentQueryBitFlags.Modified)) {
                            comp_field = MpContentQueryBitFlags.Modified.ToViewFieldName();
                            // filter out TransactionLabel = 'Created'
                            string strFilter = $"{"TransactionLabel".CaseFormat()} != ?";
                            object strParam = MpTransactionType.Created.ToString();
                            arg_filters.Add(new Tuple<string, List<object>>(strFilter, new[] { strParam }.ToList()));
                        } else if (qf.HasFlag(MpContentQueryBitFlags.Pasted)) {
                            comp_field = MpContentQueryBitFlags.Pasted.ToViewFieldName();
                        }

                        if (!string.IsNullOrEmpty(comp_field)) {
                            if (!string.IsNullOrEmpty(match_ticks2)) {
                                //exactly 1 < comp_field < 2

                                string strFilter2 = $"{comp_field.CaseFormat()} {tickOp2} ?";
                                object strParam2 = match_ticks2;
                                arg_filters.Add(new Tuple<string, List<object>>(strFilter2, new[] { strParam2 }.ToList()));
                            }
                            string strFilter = $"{comp_field.CaseFormat()} {tickOp} ?";
                            object strParam = match_ticks;
                            arg_filters.Add(new Tuple<string, List<object>>(strFilter, new[] { strParam }.ToList()));
                        }
                    }
                }
                if (qf.HasFlag(MpContentQueryBitFlags.After)) {
                    //searchOp = ">";
                }
            }

            return arg_filters;
        }

        private static string CaseFormat(this string fieldOrSearchText, bool isValue = false) {
            if(_cur_qi == null) {
                // hows this called outside of sql gen?
                Debugger.Break();
                return fieldOrSearchText;
            }
            if (!_cur_qi.QueryFlags.HasFlag(MpContentQueryBitFlags.CaseSensitive)) {
                if(isValue) {
                    return fieldOrSearchText.ToUpper();
                }
                return string.Format(@"UPPER({0})", fieldOrSearchText);
            }
            return fieldOrSearchText;
        }



        #endregion
    }
}
