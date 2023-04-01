using MonkeyPaste.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpContentQuery {
        private static MpIQueryInfo _cur_qi;

        public static async Task<int> QueryForTotalCountAsync(MpIQueryInfo head_qi, IEnumerable<int> idsToOmit) {
            object result = await PeformQueryAsync_internal(head_qi, -1, -1, idsToOmit);
            return (int)result;
        }

        public static async Task<List<MpCopyItem>> FetchItemsAsync(MpIQueryInfo head_qi, int offset, int limit, IEnumerable<int> idsToOmit) {
            object result = await PeformQueryAsync_internal(head_qi, offset, limit, idsToOmit);
            return result as List<MpCopyItem>;
        }



        private static async Task<object> PeformQueryAsync_internal(MpIQueryInfo head_qi, int offset, int limit, IEnumerable<int> ci_idsToOmit) {
            // Item1 = Param Query
            // Item2 = INTERSECT|UNION|EXCEPT
            // Item3 = Params

            List<Tuple<string, string, List<object>>> sub_queries = new List<Tuple<string, string, List<object>>>();
            int idx = 0;
            for (MpIQueryInfo qi = head_qi; qi != null; qi = qi.Next) {
                _cur_qi = qi;
                IEnumerable<int> qi_tag_ids = null;
                if (qi == head_qi && qi.Next != null) {
                    // when adv search is active simp query's tagId is QueryTagId 
                    // to keep RefreshQuery giving accurate results but needs to use AllTag
                    qi_tag_ids = new[] { MpTag.AllTagId };
                } else {
                    qi_tag_ids = Mp.Services.TagQueryTools.GetSelfAndAllDescendantsTagIds(qi.TagId);
                }

                sub_queries.Add(GetContentQuery(qi, qi_tag_ids, ci_idsToOmit, idx++));
            }
            _cur_qi = null;

            var sb = new StringBuilder();
            for (int i = 0; i < sub_queries.Count; i++) {
                var sq = sub_queries[i];
                sb.AppendLine(sq.Item1);
                if (i < sub_queries.Count - 1) {
                    // first query is always simple and its join value is ignored
                    sb.AppendLine(sub_queries[i + 1].Item2);
                }
            }
            string orderBy_clause = $"ORDER BY {head_qi.GetSortField()} {head_qi.GetSortDirection()}";
            sb.AppendLine(orderBy_clause);

            if (offset < 0) {
                // total count query
                string count_query = $"SELECT COUNT(RootId) FROM({sb})";
                int total_count = await MpDb.QueryScalarAsync<int>(count_query, sub_queries.SelectMany(x => x.Item3).ToArray());
                return total_count;
            }

            string inner_query = $"SELECT RootId,{head_qi.GetSortField()} FROM ({sb}) {orderBy_clause}";
            string fetch_query = $"SELECT * FROM MpCopyItem WHERE pk_MpCopyItemId IN (SELECT RootId FROM ({inner_query})) {orderBy_clause} LIMIT {limit} OFFSET {offset}";
            var args = sub_queries.SelectMany(x => x.Item3).ToArray();
            MpConsole.WriteLine($"Current DataModel Query: ");
            MpConsole.WriteLine(MpDb.GetParameterizedQueryString(fetch_query, args));
            var result = await MpDb.QueryAsync<MpCopyItem>(fetch_query, args);

            //string query = $"SELECT RootId FROM({sb})";
            //var result = await MpDb.QueryScalarsAsync<int>(query, sub_queries.SelectMany(x => x.Item3).ToArray());
            //return result.Where(x => !ci_idsToOmit.Contains(x)).Distinct().ToList();
            return result;
        }

        private static Tuple<string, string, List<object>> GetContentQuery(MpIQueryInfo qi, IEnumerable<int> tagIds, IEnumerable<int> ci_idsToOmit, int idx) {

            // Item1 = INTERSECT|UNION|EXCEPT
            // Item2 = Param Query
            // Item3 = Params

            string qi_root_id_query_str = ConvertQueryToSql(qi, tagIds, ci_idsToOmit, out var args);

            string join =
                qi.JoinType == MpLogicalQueryType.Or ?
                    "UNION" :
                    qi.JoinType == MpLogicalQueryType.And ?
                        "INTERSECT" :
                        qi.JoinType == MpLogicalQueryType.Not ?
                            "EXCEPT" :
                            throw new Exception("invalid join type");


            var query_tuple = new Tuple<string, string, List<object>>(qi_root_id_query_str, join, args.ToList());
            return query_tuple;
        }


        private static string ConvertQueryToSql(MpIQueryInfo qi, IEnumerable<int> tagIds, IEnumerable<int> ci_idsToOmit, out object[] args) {
            MpContentQueryBitFlags qf = qi.QueryFlags;
            List<string> types = new List<string>();
            List<string> filters = new List<string>();

            // FILTERS

            List<Tuple<string, List<object>>> arg_filters = GetParameterizedFilters(qi);
            List<object> argList = new List<object>();
            if (arg_filters != null) {
                argList = arg_filters.SelectMany(x => x.Item2).ToList();
            }

            // TYPES

            if (!qf.HasFlag(MpContentQueryBitFlags.TextType) &&
               !qf.HasFlag(MpContentQueryBitFlags.ImageType) &&
               !qf.HasFlag(MpContentQueryBitFlags.FileType)) {
                // NOTE this only can occur in adv search from ui validation in simple
                // so when no types are selected treat as all or there'll be no results
                qf |= MpContentQueryBitFlags.TextType |
                    MpContentQueryBitFlags.ImageType |
                    MpContentQueryBitFlags.FileType;
            }

            if (qf.HasFlag(MpContentQueryBitFlags.TextType)) {
                types.Add("e_MpCopyItemType=?");
                argList.Add(MpCopyItemType.Text.ToString());
            }
            if (qf.HasFlag(MpContentQueryBitFlags.ImageType)) {
                types.Add("e_MpCopyItemType=?");
                argList.Add(MpCopyItemType.Image.ToString());
            }
            if (qf.HasFlag(MpContentQueryBitFlags.FileType)) {
                types.Add("e_MpCopyItemType=?");
                argList.Add(MpCopyItemType.FileList.ToString());
            }

            // WHERE

            string whereClause = string.Empty;
            if (!tagIds.Contains(MpTag.AllTagId)) {
                whereClause = AddWhereCondition(
                    whereClause,
                    @$"RootId IN 
                        (SELECT DISTINCT pk_MpCopyItemId FROM MpCopyItem WHERE pk_MpCopyItemId IN 
		                    (SELECT fk_MpCopyItemId FROM MpCopyItemTag WHERE fk_MpTagId IN 
                                ({string.Join(",", tagIds)})))");
            }

            bool isAdvanced = qi.QueryType == MpQueryType.Advanced;
            if (arg_filters.Count > 0) {
                string filter_op = isAdvanced ? " AND " : " OR ";
                whereClause = AddWhereCondition(whereClause, @$"({string.Join(filter_op, arg_filters.Select(x => x.Item1))})");
            }
            if (types.Count > 0) {
                whereClause = AddWhereCondition(whereClause, @$"({string.Join(" OR ", types)})");
            }
            if (ci_idsToOmit != null && ci_idsToOmit.Any()) {
                whereClause = AddWhereCondition(whereClause, $"({string.Join(" AND ", ci_idsToOmit.Select(x => $"RootId != {x}"))})");
            }

            // SELECT GEN

            string selectClause = $"RootId,{qi.GetSortField()}";
            string query = @$"SELECT {selectClause} FROM MpContentQueryView WHERE {whereClause}";
            args = argList.ToArray();
            return query;
        }

        #region Helpers

        private static string AddWhereCondition(string whereClause, string condition) {
            if (string.IsNullOrEmpty(whereClause)) {
                whereClause = condition;
            } else {
                whereClause = @$"{whereClause} AND {condition}";
            }
            return whereClause;
        }

        private static List<Tuple<string, List<object>>> GetParameterizedFilters(MpIQueryInfo qi) {
            List<Tuple<string, List<object>>> arg_filters = new List<Tuple<string, List<object>>>();
            if (string.IsNullOrEmpty(qi.MatchValue)) {
                return arg_filters;
            }
            MpContentQueryBitFlags qf = qi.QueryFlags;
            string mv = qi.MatchValue.CaseFormat(true);
            arg_filters.AddRange(qf.GetStringMatchOps(mv));
            arg_filters.AddRange(qf.GetDateTimeMatchOps(mv));
            arg_filters.AddRange(qf.GetColorMatchOps(mv));
            arg_filters.AddRange(qf.GetDimensionOps(mv));

            return arg_filters;
        }

        #region Fields

        private static bool IsFieldNumeric(this string fieldName) {
            string low_field_name = fieldName.ToLower();
            switch (low_field_name) {
                case "RootId":
                case "SourceObjId":
                case "CopyDateTime":
                case "LastPasteDateTime":
                case "UsageScore":
                case "TransactionDateTime":
                case "ItemSize1":
                case "ItemSize2":
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region String Match
        private static IEnumerable<Tuple<string, List<object>>> GetStringMatchOps(
            this MpContentQueryBitFlags qf,
            string mv) {
            var ops = new List<Tuple<string, List<object>>>();
            if (qf.GetStringMatchFieldName() is IEnumerable<string> strFieldNames) {
                // string matching
                foreach (string strFieldName in strFieldNames) {
                    string strOp = qf.GetStringMatchOp();
                    // <Field> <op> '<mv>'
                    string strFilter = $"{strFieldName.CaseFormat()} {strOp} ?";
                    object strParam = qf.GetStringMatchValue(strOp, mv);
                    ops.Add(new Tuple<string, List<object>>(strFilter, new[] { strParam }.ToList()));
                }
            }
            return ops;
        }

        #endregion

        #region Dimension Match
        private static IEnumerable<Tuple<string, List<object>>> GetDimensionOps(
            this MpContentQueryBitFlags qf,
            string mv) {
            var ops = new List<Tuple<string, List<object>>>();
            int dim = -1;
            if (string.IsNullOrWhiteSpace(mv) ||
                !int.TryParse(mv, out dim)) {
                return ops;
            }
            string op = qf.GetNumericOperator();
            if (qf.HasFlag(MpContentQueryBitFlags.Width)) {
                ops.Add(new Tuple<string, List<object>>($"ItemSize1 {op} ?", new object[] { dim }.ToList()));
            }
            if (qf.HasFlag(MpContentQueryBitFlags.Height)) {
                ops.Add(new Tuple<string, List<object>>($"ItemSize2 {op} ?", new object[] { dim }.ToList()));
            }
            return ops;
        }

        #endregion

        #region Color Match
        private static IEnumerable<Tuple<string, List<object>>> GetColorMatchOps(
            this MpContentQueryBitFlags qf,
            string mv) {
            var ops = new List<Tuple<string, List<object>>>();
            if (!qf.HasFlag(MpContentQueryBitFlags.Hex) && !qf.HasFlag(MpContentQueryBitFlags.Rgba)) {
                return ops;
            }
            // PIXELCOUNT(ImageBase64,'<mv>')
            var mv_parts = mv.SplitNoEmpty(",").ToList();
            if (mv_parts.Count == 0) {
                return ops;
            }
            if (mv_parts.Count < 2) {
                // when no dist has been provided treat as exact
                mv_parts.Add("0");
            }
            if (qf.HasFlag(MpContentQueryBitFlags.Rgba)) {
                // decode base64 csv
                mv = $"({string.Join(",", mv_parts[0].ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value))})";
            } else {
                mv = mv_parts[0];
            }
            mv += "," + mv_parts[1];

            if (qf.HasFlag(MpContentQueryBitFlags.ItemColor)) {
                ops.Add(new Tuple<string, List<object>>($"HEXMATCH(?,ItemColor) = 0", new object[] { mv }.ToList()));
            } else {
                ops.Add(new Tuple<string, List<object>>($"PIXELCOUNT(?,ItemImageData) > 0", new object[] { mv }.ToList()));
            }

            return ops;
        }

        #endregion

        #region Date Match
        private static IEnumerable<Tuple<string, List<object>>> GetDateTimeMatchOps(
            this MpContentQueryBitFlags qf, string mv) {
            var ops = new List<Tuple<string, List<object>>>();
            ops.AddRange(qf.GetExactlyDateTimeMatchOps(mv));
            ops.AddRange(qf.GetBeforeOrAfterDateTimeMatchOps(mv));
            return ops;
        }

        private static IEnumerable<Tuple<string, List<object>>> GetExactlyDateTimeMatchOps(
            this MpContentQueryBitFlags qf, string mv) {
            var ops = new List<Tuple<string, List<object>>>();
            if (!qf.HasFlag(MpContentQueryBitFlags.Exactly)) {
                return ops;
            }
            // <Field> <op> <mv>
            string tickOp1 = qf.HasFlag(MpContentQueryBitFlags.Before) ? "<" : ">";
            string tickOp2 = qf.HasFlag(MpContentQueryBitFlags.Before) ? ">" : "<";
            string match_ticks1 = null;
            string match_ticks2 = null;
            try {
                //exactly
                // <comp_field> <op1> <ticks1> AND <comp_field> <op2> <ticks2>
                // example (Before)
                // 
                // CreatedDateTime < ticks(05/20/2019) AND CreatedDateTime 

                var dt = DateTime.Parse(mv);
                var start_dt = dt.Date;
                var end_dt = start_dt + TimeSpan.FromDays(0.999999);
                match_ticks1 = start_dt.Ticks.ToString();
                match_ticks2 = end_dt.Ticks.ToString();
            }
            catch {
                tickOp1 = null;
            }
            if (!string.IsNullOrEmpty(match_ticks1)) {
                string comp_field = null;
                if (qf.HasFlag(MpContentQueryBitFlags.Created)) {
                    comp_field = MpContentQueryBitFlags.Created.ToViewFieldName();
                } else if (qf.HasFlag(MpContentQueryBitFlags.Modified)) {
                    comp_field = MpContentQueryBitFlags.Modified.ToViewFieldName();
                    // filter out TransactionLabel = 'Created'
                    string strFilter = $"{"TransactionLabel".CaseFormat()} != ?";
                    object strParam = MpTransactionType.Created.ToString();
                    ops.Add(new Tuple<string, List<object>>(strFilter, new[] { strParam }.ToList()));
                } else if (qf.HasFlag(MpContentQueryBitFlags.Pasted)) {
                    comp_field = MpContentQueryBitFlags.Pasted.ToViewFieldName();
                }

                // <Field> <op> '<mv>'
                if (!string.IsNullOrEmpty(comp_field)) {
                    if (!string.IsNullOrEmpty(match_ticks2)) {
                        //exactly 1 < comp_field < 2

                        string strFilter2 = $"{comp_field} {tickOp2} ?";
                        object strParam2 = match_ticks2;
                        ops.Add(new Tuple<string, List<object>>(strFilter2, new[] { strParam2 }.ToList()));
                    }
                    string strFilter = $"{comp_field} {tickOp1} ?";
                    object strParam = match_ticks1;
                    ops.Add(new Tuple<string, List<object>>(strFilter, new[] { strParam }.ToList()));
                }
            }
            return ops;
        }

        private static IEnumerable<Tuple<string, List<object>>> GetBeforeOrAfterDateTimeMatchOps(
            this MpContentQueryBitFlags qf, string mv) {
            var ops = new List<Tuple<string, List<object>>>();

            if (!qf.HasFlag(MpContentQueryBitFlags.After) &&
                !qf.HasFlag(MpContentQueryBitFlags.Before)) {
                return ops;
            }
            // <Field> <op> <mv>
            string tickOp = qf.HasFlag(MpContentQueryBitFlags.Before) ? "<" : ">";
            string match_ticks = null;
            // all day units
            try {
                //double today_offset = (DateTime.Now - DateTime.Today).TotalDays;
                //double days = double.Parse(mv);
                //double total_day_offset = days + today_offset;
                //var dt = DateTime.Now - TimeSpan.FromDays(total_day_offset);
                //match_ticks = dt.Ticks.ToString();
                double days = double.Parse(mv);
                var match_dt = DateTime.Today - TimeSpan.FromDays(days);
                match_ticks = match_dt.Ticks.ToString();
            }
            catch {
                tickOp = null;
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
                    ops.Add(new Tuple<string, List<object>>(strFilter, new[] { strParam }.ToList()));
                } else if (qf.HasFlag(MpContentQueryBitFlags.Pasted)) {
                    comp_field = MpContentQueryBitFlags.Pasted.ToViewFieldName();
                }

                if (!string.IsNullOrEmpty(comp_field)) {
                    string strFilter = $"{comp_field} {tickOp} ?";
                    object strParam = match_ticks;
                    ops.Add(new Tuple<string, List<object>>(strFilter, new[] { strParam }.ToList()));
                }
            }
            return ops;
        }


        #endregion

        private static string CaseFormat(this string fieldOrSearchText, bool isValue = false) {
            if (_cur_qi == null) {
                // hows this called outside of sql gen?
                Debugger.Break();
                return fieldOrSearchText;
            }
            if (!_cur_qi.QueryFlags.HasFlag(MpContentQueryBitFlags.CaseSensitive)) {
                if (isValue) {
                    return fieldOrSearchText.ToUpper();
                }
                if (fieldOrSearchText.IsFieldNumeric()) {
                    // NOTE UPPER on ticks gives wrong results
                    return fieldOrSearchText;
                }
                return $@"UPPER({fieldOrSearchText})";
            }
            return fieldOrSearchText;
        }

        public static string GetSortField(this MpIQueryInfo qi) {
            switch (qi.SortType) {
                case MpContentSortType.Source:
                    return "SourcePath";
                case MpContentSortType.Title:
                    return "Title";
                case MpContentSortType.ItemData:
                    return "ItemData";
                case MpContentSortType.ItemType:
                    return "e_MpCopyItemType";
                case MpContentSortType.UsageScore:
                    return "UsageScore";
                case MpContentSortType.CopyDateTime:
                    return "CopyDateTime";
                default:
                    return "RootId";
            }
        }

        public static string GetSortDirection(this MpIQueryInfo qi) {
            if (qi.IsDescending) {
                return "DESC";
            }
            return "ASC";
        }
        #endregion
    }
}
