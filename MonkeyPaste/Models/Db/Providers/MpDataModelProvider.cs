using FFImageLoading.Helpers.Exif;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static SQLite.SQLite3;
using static System.Net.Mime.MediaTypeNames;

namespace MonkeyPaste {
    public static class MpDataModelProvider {
        #region Private Variables
        private static IList<MpCopyItem> _lastResult;


        #endregion

        #region Properties
        public static List<MpIQueryInfo> QueryInfos { get; private set; } = new List<MpIQueryInfo>();

        public static MpIQueryInfo QueryInfo {
            get {
                if(QueryInfos.Count > 0) {
                    return QueryInfos.OrderBy(x => x.SortOrderIdx).ToList()[0];
                }
                return null;
            }
        }

        public static ObservableCollection<int> AllFetchedAndSortedCopyItemIds { get; private set; } = new ObservableCollection<int>();

        public static int TotalTilesInQuery => AllFetchedAndSortedCopyItemIds.Count;

        public static int TotalItemCount { get; set; } = 0;

        #endregion

        #region Constructor


        public static void Init(MpIQueryInfo queryInfo) {
            QueryInfos.Add(queryInfo);
        }


        #endregion

        #region Public Methods


        public static void ResetQuery() {
            AllFetchedAndSortedCopyItemIds.Clear();
            _lastResult = new List<MpCopyItem>();
        }

        #region MpQueryInfo Fetch Methods                

        public static async Task QueryForTotalCount() {
            AllFetchedAndSortedCopyItemIds.Clear();
            MpLogicalFilterFlagType lastLogicFlag = MpLogicalFilterFlagType.None;

            for (int i = 0;i < QueryInfos.Count;i++) {
                var qi = QueryInfos[i];
                if(i > 0) {
                    qi.IsDescending = QueryInfos[i - 1].IsDescending;
                    qi.SortType = QueryInfos[i - 1].SortType;
                    qi.TagId = QueryInfos[i - 1].TagId;
                }
                if(qi.FilterFlags.HasFlag(MpContentFilterType.Tag) && i > 0) {
                    if(qi.FilterFlags.HasFlag(MpContentFilterType.Regex)) {
                        qi.TagId = MpTag.AllTagId;
                    } else {
                        qi.TagId = Convert.ToInt32(qi.SearchText);
                    }
                }
                if(qi.LogicFlags != MpLogicalFilterFlagType.None) {
                    lastLogicFlag = qi.LogicFlags;
                    continue;
                }
                string allRootIdQuery = GetQueryForCount(i);
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

                if(totalIds.Count > 0) {
                    AllFetchedAndSortedCopyItemIds = new ObservableCollection<int>(idl.Distinct());
                }
            }
            
            QueryInfo.TotalItemsInQuery = AllFetchedAndSortedCopyItemIds.Count;
        }

        public static async Task<IList<MpCopyItem>> FetchCopyItemRangeAsync(int startIndex, int count, Dictionary<int, int> manualSortOrderLookup = null) {
            var fetchRange = AllFetchedAndSortedCopyItemIds.GetRange(startIndex, count);
            var items = await GetCopyItemsByIdList(fetchRange); 
            if(items.Count == 0 && startIndex + count < AllFetchedAndSortedCopyItemIds.Count) {
                MpConsole.WriteTraceLine("Bad data detected for ids: " + string.Join(",", fetchRange));
            }
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
                    (select 
		                case fk_ParentCopyItemId
			                when 0
				                then pk_MpCopyItemId
			                ELSE
				                fk_ParentCopyItemId
		                end
		                from MpCopyItem where pk_MpCopyItemId in 
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

                if(qi.FilterFlags.HasFlag(MpContentFilterType.Time)) {
                    if(qi.TimeFlags.HasFlag(MpTimeFilterFlagType.After)) {
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

            if(!string.IsNullOrEmpty(sortClause)) {
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
                if(types.Count > 0) {
                    query += "(";
                }
                query += string.Join(" or ", filters);
                if (types.Count > 0) {
                    query += ") and (";
                    query += string.Join(" or ", types) + ")";
                }
            } else if(types.Count > 0) {
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
        #endregion

        #region Select queries

        #region MpUserDevice

        public static async Task<MpUserDevice> GetUserDeviceByGuid(string guid) {
            string query = $"select * from MpUserDevice where MpUserDeviceGuid=?";
            var result = await MpDb.QueryAsync<MpUserDevice>(query, guid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion MpUserDevice

        #region DbImage

        public static async Task<MpDbImage> GetDbImageByBase64Str(string text64) {
            string query = $"select pk_MpDbImageId from MpDbImage where ImageBase64=?";
            var result = await MpDb.QueryAsync<MpDbImage>(query, text64);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static MpDbImage GetDbImageById(int dbImgId) {
            var result = MpAsyncHelpers.RunSync<MpDbImage>(() => MpDb.GetItemAsync<MpDbImage>(dbImgId));
            return result;
        }

        #endregion

        #region MpIcon

        public static async Task<MpIcon> GetIconByImageStr(string text64) {
            string query = $"select pk_MpDbImageId from MpDbImage where ImageBase64=?";
            int iconImgId = await MpDb.QueryScalarAsync<int>(query, text64);
            if (iconImgId <= 0) {
                return null;
            }
            query = $"select * from MpIcon where fk_IconDbImageId=?";
            var result = await MpDb.QueryAsync<MpIcon>(query, iconImgId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion MpIcon

        #region MpApp

        public static async Task<MpApp> GetAppByPath(string path) {
            string query = $"select * from MpApp where SourcePath=?";
            var result = await MpDb.QueryAsync<MpApp>(query, path);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<bool> IsAppRejectedAsync(string path) {
            string query = $"select count(*) from MpApp where SourcePath=? and IsAppRejected=1";
            var result = await MpDb.QueryScalarAsync<int>(query, path);
            return result > 0;
        }

        //public static bool IsAppRejected(string path) {
        //    string query = $"select count(*) from MpApp where SourcePath=? and IsAppRejected=1";
        //    var result = MpDb.QueryScalar<int>(query, path);
        //    return result > 0;
        //}

        #endregion MpApp

        #region MpUrl

        public static async Task<MpUrl> GetUrlByPath(string url) {
            string query = $"select * from MpUrl where UrlPath=?";
            var result = await MpDb.QueryAsync<MpUrl>(query, url);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<List<MpUrl>> GetAllUrlsByDomainName(string domain) {
            string query = $"select * from MpUrl where UrlDomainPatn=?";
            var result = await MpDb.QueryAsync<MpUrl>(query, domain.ToLower());
            return result;
        }

        #endregion MpUrl

        #region MpSource

        public static async Task<List<MpSource>> GetAllSourcesByAppId(int appId) {
            string query = $"select * from MpSource where fk_MpAppId=?";
            var result = await MpDb.QueryAsync<MpSource>(query, appId);
            return result;
        }

        public static async Task<List<MpSource>> GetAllSourcesByUrlId(int urlId) {
            string query = $"select * from MpSource where fk_MpUrlId=?";
            var result = await MpDb.QueryAsync<MpSource>(query, urlId);
            return result;
        }

        public static async Task<MpSource> GetSourceByMembers(int appId, int urlId) {
            string query = $"select * from MpSource where fk_MpAppId=? and fk_MpUrlId=?";
            var result = await MpDb.QueryAsync<MpSource>(query, appId, urlId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<MpSource> GetSourceByGuid(string guid) {
            string query = $"select * from MpSource where MpSourceGuid=?";
            var result = await MpDb.QueryAsync<MpSource>(query, guid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion MpSource

        #region MpCopyItem

        public static async Task<List<MpCopyItem>> GetCopyItemsByAppId(int appId) {
            var sl = await GetAllSourcesByAppId(appId);

            string whereStr = string.Join(" or ", sl.Select(x => string.Format(@"fk_MpSourceId={0}", x.Id)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public static async Task<List<MpCopyItem>> GetCopyItemsByUrlId(int urlId) {
            List<MpSource> sl = await GetAllSourcesByUrlId(urlId);
            string whereStr = string.Join(" or ", sl.Select(x => string.Format(@"fk_MpSourceId={0}", x.Id)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public static async Task<List<MpCopyItem>> GetCopyItemsByUrlDomain(string domainStr) {
            var urll = await GetAllUrlsByDomainName(domainStr);
            List<MpSource> sl = new List<MpSource>();
            foreach(var url in urll) {
                var ssl = await GetAllSourcesByUrlId(url.Id);
                sl.AddRange(ssl);
            }
            sl = sl.Distinct().ToList();
            string whereStr = string.Join(" or ", sl.Select(x => string.Format(@"fk_MpSourceId={0}", x.Id)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public static async Task<List<MpCopyItem>> GetCopyItemsByIdList(List<int> ciida) {
            string whereStr = string.Join(" or ", ciida.Select(x => string.Format(@"pk_MpCopyItemId={0}", x)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result.OrderBy(x=>ciida.IndexOf(x.Id)).ToList();
        }

        public static async Task<MpCopyItem> GetCopyItemByData(string text) {
            string query = "select * from MpCopyItem where ItemData=?";
            var result = await MpDb.QueryAsync<MpCopyItem>(query, text);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<int> GetTotalCopyItemCountAsync() {
            string query = "select count(pk_MpCopyItemId) from MpCopyItem";
            var result = await MpDb.QueryScalarAsync<int>(query);
            return result;
        }

        public static async Task<int> GetRecentCopyItemCountAsync() {
            string query = GetFetchQuery(0, 0, true, MpTag.RecentTagId, true);
            var result = await MpDb.QueryScalarAsync<int>(query);
            return result;
        }

        public static async Task<List<MpCopyItem>> GetCompositeChildrenAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItem where fk_ParentCopyItemId={0} order by CompositeSortOrderIdx", ciid);
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public static async Task<int> GetCompositeChildCountAsync(int ciid) {
            string query = string.Format(@"select count(*) from MpCopyItem where fk_ParentCopyItemId={0} order by CompositeSortOrderIdx", ciid);
            var result = await MpDb.QueryScalarAsync<int>(query);
            return result;
        }

        #endregion MpCopyItem

        #region MpTextToken

        public static async Task<List<MpTextToken>> GetTemplatesAsync(int ciid) {
            string query = string.Format(@"select * from MpTextToken where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.QueryAsync<MpTextToken>(query);
            return result;
        }

        public static async Task<MpTextToken> GetTemplateByNameAsync(int ciid, string templateName) {
            // NOTE may need to use '?' below
            string query = string.Format(@"select * from MpTextToken where fk_MpCopyItemId={0} and TemplateName=?", ciid);
            var result = await MpDb.QueryAsync<MpTextToken>(query,templateName);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion

        #region MpDetectedImageObject

        public static async Task<List<MpDetectedImageObject>> GetDetectedImageObjects(int ciid) {
            string query = string.Format(@"select * from MpDetectedImageObject where fk_MpCopyItemId=?");
            var result = await MpDb.QueryAsync<MpDetectedImageObject>(query,ciid);
            return result;
        }

        public static async Task<MpDetectedImageObject> GetDetectedImageObjectByData(int ciid,double x,double y,double w,double h,double s,string label,string description,string c) {
            string query = string.Format(@"select * from MpDetectedImageObject where fk_MpCopyItemId=? and X=? and Y=? and Width=? and Height=? and Score=? and Label=? and Description=? and HexColor=?");
            var result = await MpDb.QueryAsync<MpDetectedImageObject>(query, ciid,x,y,w,h,s,label,description,c);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion

        #region MpCopyItemTag

        public static async Task<List<int>> GetCopyItemIdsForTagAsync(int tagId) {
            string query = string.Format(@"select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0}", tagId);
            var result = await MpDb.QueryScalarsAsync<int>(query);
            return result;
        }

        public static async Task<int> GetCopyItemCountForTagAsync(int tagId) {
            string query = @"select count(pk_MpCopyItemTagId) from MpCopyItemTag where fk_MpTagId=?";
            var result = await MpDb.QueryScalarAsync<int>(query,tagId);
            return result;
        }

        public static async Task<List<MpCopyItem>> GetCopyItemsForTagAsync(int tagId) {
            string query = string.Format(@"select * from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0})", tagId);
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public static async Task<MpCopyItemTag> GetCopyItemTagForTagAsync(int ciid, int tagId) {
            string query = string.Format(@"select * from MpCopyItemTag where fk_MpCopyItemId={0} and fk_MpTagId={1}", ciid, tagId);
            var result = await MpDb.QueryAsync<MpCopyItemTag>(query);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<List<MpCopyItemTag>> GetCopyItemTagsForTagAsync(int tagId) {
            string query = string.Format(@"select * from MpCopyItemTag where fk_MpTagId={0}", tagId);
            var result = await MpDb.QueryAsync<MpCopyItemTag>(query);
            return result;
        }

        public static async Task<List<string>> GetTagColorsForCopyItem(int ciid) {
            string query = @"select HexColor from MpTag where pk_MpTagId in (select fk_MpTagId from MpCopyItemTag where fk_MpCopyItemId = ?)";
            var result = await MpDb.QueryScalarsAsync<string>(query,ciid);
            return result;
        }
        

        public static async Task<List<MpCopyItemTag>> GetCopyItemTagsForCopyItemAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItemTag where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.QueryAsync<MpCopyItemTag>(query);
            return result;
        }

        public static async Task<bool> IsCopyItemInRecentTag(int copyItemId) {
            string query = GetFetchQuery(0, 0, true, MpTag.RecentTagId, true, copyItemId);
            var result = await MpDb.QueryScalarAsync<int>(query);
            return result > 0;
        }

        public static async Task<bool> IsTagLinkedWithCopyItemAsync(int tagId, int copyItemId) {
            string query = $"select count(*) from MpCopyItemTag where fk_MpTagId=? and fk_MpCopyItemId=?";
            var result = await MpDb.QueryScalarAsync<int>(query, tagId, copyItemId);
            return result > 0;
        }

        public static bool IsTagLinkedWithCopyItem(int tagId, int copyItemId) {
            string query = $"select count(*) from MpCopyItemTag where fk_MpTagId=? and fk_MpCopyItemId=?";
            var result = MpAsyncHelpers.RunSync<int>(() => MpDb.QueryScalarAsync<int>(query, tagId, copyItemId));
            return result > 0;
        }

        #endregion MpCopyItemTag        

        #region MpTag

        public static async Task<List<MpTag>> GetChildTagsAsync(int tagId) {
            string query = "select * from MpTag where fk_ParentTagId=?";
            var result = await MpDb.QueryAsync<MpTag>(query,tagId);
            return result;
        }

        #endregion

        #region MpShortcut

        public static async Task<MpShortcut> GetShortcut(int ciid, int tagId, int aiid) {
            string query = string.Format(@"select * from MpShortcut where fk_MpCopyItemId=? and fk_MpTagId=? and fk_MpAnalyticItemPresetId=?");
            var result = await MpDb.QueryAsync<MpShortcut>(query,ciid,tagId,aiid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<MpShortcut> GetShortcutAsync(MpShortcutType shortcutType, int commandId) {
            string query = string.Format(@"select * from MpShortcut where e_ShortcutTypeId=? and fk_MpCommandId=?");
            var result = await MpDb.QueryAsync<MpShortcut>(query, (int)shortcutType, commandId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<string> GetShortcutKeystring(MpShortcutType shortcutType, int commandId = 0) {
            string query = string.Format(@"select KeyString from MpShortcut where e_ShortcutTypeId=? and fk_MpCommandId=?");
            var result = await MpDb.QueryScalarAsync<string>(query, (int)shortcutType, commandId);
            return result;
        }

        public static async Task<List<MpShortcut>> GetAllShortcuts() {
            string query = @"select * from MpShortcut";
            var result = await MpDb.QueryAsync<MpShortcut>(query);
            return result;
        }

        public static async Task<List<MpShortcut>> GetCopyItemShortcutsAsync(int ciid) {
            string query = string.Format(@"select * from MpShortcut where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.QueryAsync<MpShortcut>(query);
            return result;
        }

        public static async Task<List<MpShortcut>> GetTagShortcutsAsync(int tid) {
            string query = string.Format(@"select * from MpShortcut where fk_MpTagId={0}", tid);
            var result = await MpDb.QueryAsync<MpShortcut>(query);
            return result;
        }

        #endregion

        #region MpPasteToAppPath



        #endregion

        #region MpAnalytic Item

        public static async Task<int> GetAnalyticItemCount() {
            string query = $"select DISTINCT count(AnalyzerPluginGuid) from MpAnalyticItemPreset";
            var result = await MpDb.QueryScalarAsync<int>(query);
            return result;
        }

        public static async Task<List<MpAnalyticItemPreset>> GetAllQuickActionAnalyzers() {
            string query = $"select * from MpAnalyticItemPreset where IsQuickAction=1";
            var result = await MpDb.QueryAsync<MpAnalyticItemPreset>(query);
            return result;
        }

        public static async Task<List<MpAnalyticItemPreset>> GetAllShortcutAnalyzers() {
            string query = $"select * from MpAnalyticItemPreset where pk_MpAnalyticItemPresetId in (select fk_MpAnalyticItemPresetId from MpShortcut where fk_MpAnalyticItemPresetId > 0)";
            var result = await MpDb.QueryAsync<MpAnalyticItemPreset>(query);
            return result;
        }

        public static async Task<List<MpAnalyticItemPreset>> GetAnalyticItemPresetsByAnalyzerGuid(string aguid) {
            string query = $"select * from MpAnalyticItemPreset where AnalyzerPluginGuid=?";
            var result = await MpDb.QueryAsync<MpAnalyticItemPreset>(query, aguid);
            return result;
        }

        public static async Task<List<MpAnalyticItemPresetParameterValue>> GetAnalyticItemPresetValuesByPresetId(int presetId) {
            string query = $"select * from MpAnalyticItemPresetParameterValue where fk_MpAnalyticItemPresetId=?";
            var result = await MpDb.QueryAsync<MpAnalyticItemPresetParameterValue>(query, presetId);
            return result;
        }

        public static async Task<MpAnalyticItemPresetParameterValue> GetAnalyticItemPresetValue(int presetid, int paramEnumId) {
            string query = $"select * from MpAnalyticItemPresetParameterValue where fk_MpAnalyticItemPresetId=? and ParameterEnumId=?";
            var result = await MpDb.QueryAsync<MpAnalyticItemPresetParameterValue>(query, presetid, paramEnumId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }


        #endregion MpAnalyticItem

        #region MpDbLog

        public static async Task<MpDbLog> GetDbLogById(int DbLogId) {
            string query = string.Format(@"select * from MpDbLog where Id=?");
            var result = await MpDb.QueryAsync<MpDbLog>(query, DbLogId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<List<MpDbLog>> GetDbLogsByGuidAsync(string dboGuid, DateTime fromDateUtc) {
            string query = string.Format(@"select * from MpDbLog where DbObjectGuid=? and LogActionDateTime>?");
            var result = await MpDb.QueryAsync<MpDbLog>(query, dboGuid, fromDateUtc);
            return result;
        }

        #endregion

        #region MpSyncHistory

        public static async Task<MpSyncHistory> GetSyncHistoryByDeviceGuid(string dg) {
            string query = string.Format(@"select * from MpSyncHistory where OtherClientGuid=?");
            var result = await MpDb.QueryAsync<MpSyncHistory>(query,dg);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion

        #region MpUserSearch


        #endregion

        #region MpAction

        public static async Task<List<MpAction>> GetAllTriggerActions() {
            string query = $"select * from MpAction where e_MpActionTypeId=? and fk_ActionObjId != ?";
            var result = await MpDb.QueryAsync<MpAction>(query, (int)MpActionType.Trigger,(int)MpTriggerType.ParentOutput);
            return result;
        }

        public static async Task<int> GetChildActionCount(int actionId) {
            string query = $"select count(pk_MpActionId) from MpAction where fk_ParentActionId=?";
            var result = await MpDb.QueryScalarAsync<int>(query, actionId);
            return result;
        }

        public static async Task<List<MpAction>> GetChildActions(int actionId) {
            string query = $"select * from MpAction where fk_ParentActionId=?";
            var result = await MpDb.QueryAsync<MpAction>(query, actionId);
            return result;
        }

        public static async Task<MpAction> GetActionByLabel(string label) {
            string query = string.Format(@"select * from MpAction where Label=?");
            var result = await MpDb.QueryAsync<MpAction>(query, label);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }
        #endregion

        #region MpBox

        public static async Task<MpBox> GetBoxByTypeAndObjId(MpBoxType boxType, int objId) {
            string query = string.Format(@"select * from MpBox where e_MpBoxTypeId=? and fk_BoxObjectId=?");
            var result = await MpDb.QueryAsync<MpBox>(query, (int)boxType,objId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion

        #region MpContentToken

        public static async Task<MpContentToken> GetToken(int copyItemId, int actionId, string matchData) {
            string query = string.Format(@"select * from MpContentToken where fk_MpCopyItemId=? and fk_MpActionId=? and MatchData=?");
            var result = await MpDb.QueryAsync<MpContentToken>(query, copyItemId, actionId,matchData);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<List<MpContentToken>> GetTokenByActionId(int actionId) {
            string query = $"select * from MpContentToken where fk_MpActionId=?";
            var result = await MpDb.QueryAsync<MpContentToken>(query, actionId);
            return result;
        }

        public static async Task<List<MpContentToken>> GetTokenByCopyItemId(int copyItemId) {
            string query = $"select * from MpContentToken where fk_MpCopyItemId=?";
            var result = await MpDb.QueryAsync<MpContentToken>(query, copyItemId);
            return result;
        }

        #endregion

        #endregion

        #endregion

        private static string GetFetchQuery(int startIndex, int count, bool queryForTotalCount = false, int forceTagId = -1, bool ignoreSearchStr = false, int forceCheckCopyItemId = -1) {
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
                count = MpPreferences.MaxRecentClipItems;
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

        public static async Task<List<MpCopyItem>> GetPageAsync(
            int tagId,
            int start,
            int count,
            MpContentSortType sortType,
            bool isDescending,
            Dictionary<int, int> manualSortOrderLookup = null) {
            List<MpCopyItem> result = await MpDb.GetItemsAsync<MpCopyItem>();

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
                    var citl = await MpDb.GetItemsAsync<MpCopyItemTag>();
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

        public static async Task<MpCopyItem> RemoveQueryItem(int copyItemId) {
            //returns first child or null
            //return value is used to adjust dropIdx in ClipTrayDrop

            if (!AllFetchedAndSortedCopyItemIds.Contains(copyItemId)) {
                //throw new Exception("Query does not contain item " + copyItemId);
                return null;
            }
            int itemIdx = AllFetchedAndSortedCopyItemIds.IndexOf(copyItemId);
            var ccil = await GetCompositeChildrenAsync(copyItemId);
            if(ccil.Count > 0) {
                for (int i = 0; i < ccil.OrderBy(x=>x.CompositeSortOrderIdx).Count(); i++) {
                    if (i == 0) {
                        ccil[i].CompositeParentCopyItemId = 0;
                        AllFetchedAndSortedCopyItemIds[itemIdx] = ccil[i].Id;
                        MpConsole.WriteLine($"QueryItem {copyItemId} at [{itemIdx}] replaced with first child {ccil[i].Id}");
                    } else {
                        ccil[i].CompositeParentCopyItemId = ccil[0].Id;
                    }
                    ccil[i].CompositeSortOrderIdx = i;
                    await ccil[i].WriteToDatabaseAsync();
                }
            } else {
                AllFetchedAndSortedCopyItemIds.Remove(copyItemId);
            }

            MpConsole.WriteLine($"QueryItem {copyItemId} was removed from [{itemIdx}]");

            return ccil.Count == 0 ? null : ccil[0];
        }

        public static void MoveQueryItem(int copyItemId, int newIdx) {
            if (!AllFetchedAndSortedCopyItemIds.Contains(copyItemId)) {
                throw new Exception("Query does not contain item " + copyItemId);
            }
            int oldIdx = AllFetchedAndSortedCopyItemIds.IndexOf(copyItemId);
            AllFetchedAndSortedCopyItemIds.Move(oldIdx, newIdx);
            MpConsole.WriteLine($"QueryItem {copyItemId} moved from [{oldIdx}] to [{newIdx}]");
        }

        public static void InsertQueryItem(int copyItemId, int newIdx) {
            if (AllFetchedAndSortedCopyItemIds.Contains(copyItemId)) {
                int oldIdx = AllFetchedAndSortedCopyItemIds.IndexOf(copyItemId);
                if (newIdx > oldIdx) {
                    newIdx--;
                }
                AllFetchedAndSortedCopyItemIds.RemoveAt(oldIdx);                
            }
            if(newIdx < 0) {
                throw new Exception($"Idx must be >= 0, was [{newIdx}]");
            }
            if (newIdx > AllFetchedAndSortedCopyItemIds.Count) {
                throw new Exception($"Idx must be < item count ({AllFetchedAndSortedCopyItemIds.Count}), idx was [{newIdx}]");
            }
            if (newIdx == AllFetchedAndSortedCopyItemIds.Count) {
                AllFetchedAndSortedCopyItemIds.Add(copyItemId);
            } else {
                AllFetchedAndSortedCopyItemIds.Insert(newIdx,copyItemId);
            }
            MpConsole.WriteLine($"QueryItem {copyItemId} was inserted at idx [{newIdx}]");            
        }

    }
}
