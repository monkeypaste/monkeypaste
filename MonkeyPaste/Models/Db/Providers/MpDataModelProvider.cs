using FFImageLoading.Helpers.Exif;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static SQLite.SQLite3;

namespace MonkeyPaste {
    public class MpDataModelProvider : MpSingleton<MpDataModelProvider> {
        #region Private Variables
        private IList<MpCopyItem> _lastResult;


        #endregion

        #region Properties
        public List<MpIQueryInfo> QueryInfos { get; private set; } = new List<MpIQueryInfo>();
        public MpIQueryInfo QueryInfo {
            get {
                if(QueryInfos.Count > 0) {
                    return QueryInfos.OrderBy(x => x.SortOrderIdx).ToList()[0];
                }
                return null;
            }
        }

        public ObservableCollection<int> AllFetchedAndSortedCopyItemIds { get; private set; } = new ObservableCollection<int>();

        public int TotalTilesInQuery => AllFetchedAndSortedCopyItemIds.Count;

        #endregion

        #region Constructor

        public MpDataModelProvider() { }

        #endregion

        #region Public Methods

        public void Init(MpIQueryInfo queryInfo) {
            QueryInfos.Add(queryInfo);
            MpDb.Instance.OnItemUpdated += Instance_OnItemUpdated;
            MpDb.Instance.OnItemDeleted += Instance_OnItemDeleted;
        }

        public void ResetQuery() {
            AllFetchedAndSortedCopyItemIds.Clear();
            _lastResult = new List<MpCopyItem>();
        }

        #region MpQueryInfo Fetch Methods
                

        public async Task QueryForTotalCount() {
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
                var idl = await MpDb.Instance.QueryScalarsAsync<int>(allRootIdQuery);
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

        public async Task<IList<MpCopyItem>> FetchCopyItemRangeAsync(int startIndex, int count, Dictionary<int, int> manualSortOrderLookup = null) {
            var fetchRange = AllFetchedAndSortedCopyItemIds.GetRange(startIndex, count);
            var items = await GetCopyItemsByIdList(fetchRange); 
            if(items.Count == 0 && startIndex + count < AllFetchedAndSortedCopyItemIds.Count) {
                MpConsole.WriteTraceLine("Bad data detected for ids: " + string.Join(",", fetchRange));
            }
            return items;
        }

        #endregion

        #region View Queries

        public string GetQueryForCount(int qiIdx = 0) {
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
                types.Add(string.Format(@"fk_MpCopyItemTypeId={0}", (int)MpCopyItemType.RichText));
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
            if (qi.IsDescending) {
                sortClause += " DESC";
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

        private string CaseFormat(string fieldOrSearchText) {
            //if (QueryInfo.FilterFlags.HasFlag(MpContentFilterType.CaseSensitive)) {
            //    return string.Format(@"UPPER({0})", fieldOrSearchText);
            //}
            return fieldOrSearchText;
        }
        #endregion

        #region Select queries

        #region MpUserDevice

        public async Task<MpUserDevice> GetUserDeviceByGuid(string guid) {
            string query = $"select * from MpUserDevice where MpUserDeviceGuid=?";
            var result = await MpDb.Instance.QueryAsync<MpUserDevice>(query, guid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion MpUserDevice

        #region DbImage

        public async Task<MpDbImage> GetDbImageByBase64Str(string text64) {
            string query = $"select pk_MpDbImageId from MpDbImage where ImageBase64=?";
            var result = await MpDb.Instance.QueryAsync<MpDbImage>(query, text64);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion

        #region MpIcon

        public async Task<MpIcon> GetIconByImageStr(string text64) {
            string query = $"select pk_MpDbImageId from MpDbImage where ImageBase64=?";
            int iconImgId = await MpDb.Instance.QueryScalarAsync<int>(query, text64);
            if (iconImgId <= 0) {
                return null;
            }
            query = $"select * from MpIcon where fk_IconDbImageId=?";
            var result = await MpDb.Instance.QueryAsync<MpIcon>(query, iconImgId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion MpIcon

        #region MpApp

        public async Task<MpApp> GetAppByPath(string path) {
            string query = $"select * from MpApp where SourcePath=?";
            var result = await MpDb.Instance.QueryAsync<MpApp>(query, path);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<bool> IsAppRejectedAsync(string path) {
            string query = $"select count(*) from MpApp where SourcePath=? and IsAppRejected=1";
            var result = await MpDb.Instance.QueryScalarAsync<int>(query, path);
            return result > 0;
        }

        //public bool IsAppRejected(string path) {
        //    string query = $"select count(*) from MpApp where SourcePath=? and IsAppRejected=1";
        //    var result = MpDb.Instance.QueryScalar<int>(query, path);
        //    return result > 0;
        //}

        #endregion MpApp

        #region MpUrl

        public async Task<MpUrl> GetUrlByPath(string url) {
            string query = $"select * from MpUrl where UrlPath=?";
            var result = await MpDb.Instance.QueryAsync<MpUrl>(query, url);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<List<MpUrl>> GetAllUrlsByDomainName(string domain) {
            string query = $"select * from MpUrl where UrlDomainPatn=?";
            var result = await MpDb.Instance.QueryAsync<MpUrl>(query, domain.ToLower());
            return result;
        }

        #endregion MpUrl

        #region MpSource

        public async Task<List<MpSource>> GetAllSourcesByAppId(int appId) {
            string query = $"select * from MpSource where fk_MpAppId=?";
            var result = await MpDb.Instance.QueryAsync<MpSource>(query, appId);
            return result;
        }

        public async Task<List<MpSource>> GetAllSourcesByUrlId(int urlId) {
            string query = $"select * from MpSource where fk_MpUrlId=?";
            var result = await MpDb.Instance.QueryAsync<MpSource>(query, urlId);
            return result;
        }

        public async Task<MpSource> GetSourceByMembers(int appId, int urlId) {
            string query = $"select * from MpSource where fk_MpAppId=? and fk_MpUrlId=?";
            var result = await MpDb.Instance.QueryAsync<MpSource>(query, appId, urlId);
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

        #endregion MpSource

        #region MpCopyItem

        public async Task<List<MpCopyItem>> GetCopyItemsByAppId(int appId) {
            var sl = await GetAllSourcesByAppId(appId);

            string whereStr = string.Join(" or ", sl.Select(x => string.Format(@"fk_MpSourceId={0}", x.Id)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public async Task<List<MpCopyItem>> GetCopyItemsByUrlId(int urlId) {
            List<MpSource> sl = await GetAllSourcesByUrlId(urlId);
            string whereStr = string.Join(" or ", sl.Select(x => string.Format(@"fk_MpSourceId={0}", x.Id)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public async Task<List<MpCopyItem>> GetCopyItemsByUrlDomain(string domainStr) {
            var urll = await GetAllUrlsByDomainName(domainStr);
            List<MpSource> sl = new List<MpSource>();
            foreach(var url in urll) {
                var ssl = await GetAllSourcesByUrlId(url.Id);
                sl.AddRange(ssl);
            }
            sl = sl.Distinct().ToList();
            string whereStr = string.Join(" or ", sl.Select(x => string.Format(@"fk_MpSourceId={0}", x.Id)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public async Task<List<MpCopyItem>> GetCopyItemsByIdList(List<int> ciida) {
            string whereStr = string.Join(" or ", ciida.Select(x => string.Format(@"pk_MpCopyItemId={0}", x)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query);
            return result.OrderBy(x=>ciida.IndexOf(x.Id)).ToList();
        }

        public async Task<MpCopyItem> GetCopyItemByData(string text) {
            string query = "select * from MpCopyItem where ItemData=?";
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query, text);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

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

        #endregion MpCopyItem

        #region MpCopyItemTemplate

        public async Task<List<MpCopyItemTemplate>> GetTemplatesAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItemTemplate where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTemplate>(query);
            return result;
        }

        public async Task<MpCopyItemTemplate> GetTemplateByNameAsync(int ciid, string templateName) {
            // NOTE may need to use '?' below
            string query = string.Format(@"select * from MpCopyItemTemplate where fk_MpCopyItemId={0} and TemplateName=?", ciid);
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTemplate>(query,templateName);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion

        #region MpDetectedImageObject

        public async Task<List<MpDetectedImageObject>> GetDetectedImageObjects(int ciid) {
            string query = string.Format(@"select * from MpDetectedImageObject where fk_MpCopyItemId=?");
            var result = await MpDb.Instance.QueryAsync<MpDetectedImageObject>(query,ciid);
            return result;
        }

        #endregion

        #region MpCopyItemTag

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

        public async Task<List<string>> GetTagColorsForCopyItem(int ciid) {
            string query = @"select HexColor from MpTag where pk_MpTagId in (select fk_MpTagId from MpCopyItemTag where fk_MpCopyItemId = ?)";
            var result = await MpDb.Instance.QueryScalarsAsync<string>(query,ciid);
            return result;
        }
        

        public async Task<List<MpCopyItemTag>> GetCopyItemTagsForCopyItemAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItemTag where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTag>(query);
            return result;
        }

        public async Task<bool> IsCopyItemInRecentTag(int copyItemId) {
            string query = GetFetchQuery(0, 0, true, MpTag.RecentTagId, true, copyItemId);
            var result = await MpDb.Instance.QueryScalarAsync<int>(query);
            return result > 0;
        }

        public async Task<bool> IsTagLinkedWithCopyItem(int tagId, int copyItemId) {
            string query = $"select count(*) from MpCopyItemTag where fk_MpTagId=? and fk_MpCopyItemId=?";
            var result = await MpDb.Instance.QueryScalarAsync<int>(query, tagId, copyItemId);
            return result > 0;
        }

        #endregion MpCopyItemTag        

        #region MpTag

        public async Task<List<MpTag>> GetChildTagsAsync(int tagId) {
            string query = "select * from MpTag where fk_ParentTagId=?";
            var result = await MpDb.Instance.QueryAsync<MpTag>(query,tagId);
            return result;
        }

        #endregion

        #region MpTagProperty

        public async Task<List<MpTagProperty>> GetTagPropertiesById(int tagId) {
            string query = "select * from MpTagProperty where fk_MpTagId=?";
            var result = await MpDb.Instance.QueryAsync<MpTagProperty>(query,tagId);
            return result;
        }

        #endregion

        #region MpShortcut

        public async Task<MpShortcut> GetShortcut(int ciid, int tagId, int aiid) {
            string query = string.Format(@"select * from MpShortcut where fk_MpCopyItemId=? and fk_MpTagId=? and fk_MpAnalyticItemPresetId=?");
            var result = await MpDb.Instance.QueryAsync<MpShortcut>(query,ciid,tagId,aiid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<MpShortcut> GetShortcut(MpShortcutType shortcutType, int commandId) {
            string query = string.Format(@"select * from MpShortcut where e_ShortcutTypeId=? and fk_MpCommandId=?");
            var result = await MpDb.Instance.QueryAsync<MpShortcut>(query, (int)shortcutType, commandId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<List<MpShortcut>> GetAllShortcuts() {
            string query = @"select * from MpShortcut";
            var result = await MpDb.Instance.QueryAsync<MpShortcut>(query);
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

        #endregion

        #region MpPasteToAppPath



        #endregion

        #region MpAnalytic Item

        public async Task<int> GetAnalyticItemCount() {
            string query = $"select count(pk_MpAnalyticItemId) from MpAnalyticItem";
            var result = await MpDb.Instance.QueryScalarAsync<int>(query);
            return result;
        }

        public async Task<MpAnalyticItem> GetAnalyticItemByEndpoint(string endPoint) {
            string query = $"select * from MpAnalyticItem where EndPoint=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItem>(query, endPoint);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<List<MpAnalyticItemPreset>> GetAllQuickActionAnalyzers() {
            string query = $"select * from MpAnalyticItemPreset where IsQuickAction=1";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemPreset>(query);
            return result;
        }

        public async Task<List<MpAnalyticItemPreset>> GetAllShortcutAnalyzers() {
            string query = $"select * from MpAnalyticItemPreset where pk_MpAnalyticItemPresetId in (select fk_MpAnalyticItemPresetId from MpShortcut where fk_MpAnalyticItemPresetId > 0)";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemPreset>(query);
            return result;
        }

        public async Task<MpAnalyticItemPreset> GetAnalyzerPresetById(int aipid) {
            string query = $"select * from MpAnalyticItemPreset where pk_MpAnalyticItemPresetId=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemPreset>(query,aipid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<MpAnalyticItem> GetAnalyticItemByTitle(string title) {
            string query = $"select * from MpAnalyticItem where Title=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItem>(query, title);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<MpAnalyticItemPreset> GetAnalyticItemPresetByLabel(int aiid, string label) {
            string query = $"select * from MpAnalyticItemPreset where fk_MpAnalyticItemId=? and Label=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemPreset>(query, aiid,label);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<MpAnalyticItemPreset> GetAnalyticItemDefaultPreset(int aiid) {
            string query = $"select * from MpAnalyticItemPreset where fk_MpAnalyticItemId=? and b_IsDefault=1";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemPreset>(query, aiid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<List<MpAnalyticItemPreset>> GetAnalyticItemPresetsById(int aiid) {
            string query = $"select * from MpAnalyticItemPreset where fk_MpAnalyticItemId=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemPreset>(query, aiid);
            return result;
        }

        public async Task<MpAnalyticItemPresetParameterValue> GetAnalyticItemPresetValue(int presetid, int paramEnumId) {
            string query = $"select * from MpAnalyticItemPresetParameterValue where fk_MpAnalyticItemPresetId=? and ParameterEnumId=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemPresetParameterValue>(query, presetid, paramEnumId);
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

        public async Task<List<MpAnalyticItemParameterValue>> GetAnalyticItemParameterValuesByParamId(int paramId) {
            string query = $"select * from MpAnalyticItemParameterValue where fk_MpAnalyticItemParameterId=?";
            var result = await MpDb.Instance.QueryAsync<MpAnalyticItemParameterValue>(query, paramId);
            return result;
        }

        #endregion MpAnalyticItem

        #region MpDbLog

        public async Task<MpDbLog> GetDbLogById(int DbLogId) {
            string query = string.Format(@"select * from MpDbLog where Id=?");
            var result = await MpDb.Instance.QueryAsync<MpDbLog>(query, DbLogId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public async Task<List<MpDbLog>> GetDbLogsByGuidAsync(string dboGuid, DateTime fromDateUtc) {
            string query = string.Format(@"select * from MpDbLog where DbObjectGuid=? and LogActionDateTime>?");
            var result = await MpDb.Instance.QueryAsync<MpDbLog>(query, dboGuid, fromDateUtc);
            return result;
        }

        #endregion

        #region MpSyncHistory

        public async Task<MpSyncHistory> GetSyncHistoryByDeviceGuid(string dg) {
            string query = string.Format(@"select * from MpSyncHistory where OtherClientGuid=?");
            var result = await MpDb.Instance.QueryAsync<MpSyncHistory>(query,dg);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion

        #region MpUserSearch


        #endregion

        #endregion

        #endregion

        private string GetFetchQuery(int startIndex, int count, bool queryForTotalCount = false, int forceTagId = -1, bool ignoreSearchStr = false, int forceCheckCopyItemId = -1) {
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
                count = MpPreferences.Instance.MaxRecentClipItems;
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

        public async Task<MpCopyItem> RemoveQueryItem(int copyItemId) {
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

        public void MoveQueryItem(int copyItemId, int newIdx) {
            if (!AllFetchedAndSortedCopyItemIds.Contains(copyItemId)) {
                throw new Exception("Query does not contain item " + copyItemId);
            }
            int oldIdx = AllFetchedAndSortedCopyItemIds.IndexOf(copyItemId);
            AllFetchedAndSortedCopyItemIds.Move(oldIdx, newIdx);
            MpConsole.WriteLine($"QueryItem {copyItemId} moved from [{oldIdx}] to [{newIdx}]");
        }

        public void InsertQueryItem(int copyItemId, int newIdx) {
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

        private  void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if(e is MpCopyItem ci) {
                //if(_allFetchedAndSortedCopyItemIds.Contains(ci.Id)) {
                //    int newParentId = 0;
                //    var ccil = await GetCompositeChildrenAsync(ci.Id);
                //    for (int i = 0; i < ccil.Count; i++) {
                //        if (i == 0) {
                //            newParentId = ccil[i].Id;
                //            ccil[i].CompositeParentCopyItemId = 0;
                //        } else {
                //            ccil[i].CompositeParentCopyItemId = newParentId;
                //        }
                //        ccil[i].CompositeSortOrderIdx = i;
                //        await ccil[i].WriteToDatabaseAsync();
                //    }
                //    if (newParentId > 0) {
                //        _allFetchedAndSortedCopyItemIds[_allFetchedAndSortedCopyItemIds.IndexOf(ci.Id)] = newParentId;
                //    } else {
                //        _allFetchedAndSortedCopyItemIds.Remove(ci.Id);
                //    }

                //    QueryInfo.NotifyQueryChanged(false);
                //}
            }
        }

        private  void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
            }
        }
    }
}
