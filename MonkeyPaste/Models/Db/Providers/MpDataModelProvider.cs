using FFImageLoading.Helpers.Exif;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using System.Data.Common;

namespace MonkeyPaste {
    
    public static class MpDataModelProvider  {
        #region Private Variables
        #endregion

        #region Properties

        
        //public static List<MpIQueryInfo> QueryInfos { get; private set; } = new List<MpIQueryInfo>();

        //public static MpIQueryInfo QueryInfo {
        //    get {
        //        //if(QueryInfos.Count > 0) {
        //        //    return QueryInfos.OrderBy(x => x.SortOrderIdx).ToList()[0];
        //        //}
        //        //return null;
        //        return MpPlatformWrapper.Services.QueryInfo;
        //    }
        //}

        //public static List<int> AvailableQueryCopyItemIds { get; private set; } = new List<int>();


        //public static int TotalTilesInQuery => AvailableQueryCopyItemIds.Count;

        #endregion

        #region Constructor


        //public static void Init() {
        //    QueryInfos.Clear();
        //    QueryInfos.Add(MpPlatformWrapper.Services.QueryInfo);
        //    ResetQuery();
        //}



        #endregion

        #region Public Methods

        //public static void ResetQuery() {
        //    AvailableQueryCopyItemIds.Clear();
        //}

        #region Select queries

        #region Table Query

        public static async Task<List<T>> GetItemsAsync<T>() where T : new() {
            string table_name = typeof(T).ToString().Replace("MonkeyPaste.", string.Empty);
            string query = $"select * from {table_name}";
            var result = await MpDb.QueryAsync<T>(query);
            return result;
        }

        public static List<T> GetItems<T>() where T : new() {
            string table_name = typeof(T).ToString().Replace("MonkeyPaste.", string.Empty);
            string query = $"select * from {table_name}";
            var result = MpDb.Query<T>(query);
            return result;
        }

        #endregion

        #region Id Query

        public static async Task<T> GetItemAsync<T>(int id) where T : new() {
            string table_name = typeof(T).ToString().Replace("MonkeyPaste.", string.Empty);
            string pk_name = $"pk_{table_name}Id";
            string query = $"select * from {table_name} where {pk_name}=?";
            var result = await MpDb.QueryAsync<T>(query, id);
            if(result == null || result.Count == 0) {
                return default;
            }
            return result[0];
        }

        public static T GetItem<T>(int id) where T : new() {
            string table_name = typeof(T).ToString().Replace("MonkeyPaste.", string.Empty);
            string pk_name = $"pk_{table_name}Id";
            string query = $"select * from {table_name} where {pk_name}=?";
            var result = MpDb.Query<T>(query, id);
            if (result == null || result.Count == 0) {
                return default;
            }
            return result[0];
        }

        #endregion

        #region MpSortableCopyItem_View (PropertyPath Queries)

        public static async Task<T> GetSortableCopyItemViewPropertyAsync<T>(int ciid,string propertyName) {
            string query = "select ? from MpSortableCopyItem_View where pk_MpCopyItemId=?";
            var result = await MpDb.QueryScalarAsync<T>(query, propertyName, ciid);
            return result;
        }

        #endregion

        #region MpUserDevice

        public static async Task<MpUserDevice> GetUserDeviceByGuidAsync(string guid) {
            string query = $"select * from MpUserDevice where MpUserDeviceGuid=?";
            var result = await MpDb.QueryAsync<MpUserDevice>(query, guid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<MpUserDevice> GetUserDeviceByMembersAsync(string machineName, MpUserDeviceType deviceType) {
            string query = $"select * from MpUserDevice where MachineName=? and PlatformTypeId=?";
            var result = await MpDb.QueryAsync<MpUserDevice>(query, machineName, deviceType.ToString());
            if (result == null || result.Count == 0) {
                return null;
            }
            if(result.Count > 1) {
                // this should only be 1
                Debugger.Break();
            }
            return result[0];
        }

        #endregion MpUserDevice

        #region DbImage

        public static async Task<MpDbImage> GetDbImageByBase64StrAsync(string text64) {
            string query = $"select pk_MpDbImageId from MpDbImage where ImageBase64=?";
            var result = await MpDb.QueryAsync<MpDbImage>(query, text64);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static MpDbImage GetDbImageById(int dbImgId) {
            string query = $"select * from MpDbImage where pk_MpDbImageId=?";
            var result = MpDb.Query<MpDbImage>(query, dbImgId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }
        #endregion

        

        #region MpIcon


        public static async Task<MpIcon> GetIconByImageStrAsync(string text64) {
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

        public static async Task<MpIcon> GetIconByImageStr2Async(string text64) {
            string query = $"select pk_MpDbImageId from MpDbImage where ImageBase64='{text64}'";
            int iconImgId = await MpDb.QueryScalarAsync<int>(query, null);
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

        public static async Task<string> GetIconHexColorAsync(int iconId, int hexColorIdx = 0) {
            string hexFieldStr = string.Format(@"HexColor{0}", Math.Min(hexColorIdx + 1, 5));
            string query = $"select {hexFieldStr} from MpIcon where pk_MpIconId=?";
            string result = await MpDb.QueryScalarAsync<string>(query, iconId);
            return result;
        }

        #endregion MpIcon

        #region MpApp

        public static async Task<MpApp> GetAppByPathAsync(string path, string args, int deviceId) {
            List<MpApp> result;
            if(args == null) {
                string query = $"select * from MpApp where LOWER(AppPath)=? and Arguments is NULL and fk_MpUserDeviceId=?";
                result = await MpDb.QueryAsync<MpApp>(query, path.ToLower(), deviceId);
            } else {
                string query = $"select * from MpApp where LOWER(AppPath)=? and Arguments=? and fk_MpUserDeviceId=?";
                result = await MpDb.QueryAsync<MpApp>(query, path.ToLower(), args, deviceId);
            }
            
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<bool> IsAppRejectedAsync(string path, int deviceId) {
            string query = $"select count(*) from MpApp where LOWER(AppPath)=? and IsAppRejected=1 and fk_MpUserDeviceId=?";
            var result = await MpDb.QueryScalarAsync<int>(query, path.ToLower(), deviceId);
            return result > 0;
        }

        //public static bool IsAppRejected(string path) {
        //    string query = $"select count(*) from MpApp where SourcePath=? and IsAppRejected=1";
        //    var result = MpDb.QueryScalar<int>(query, path);
        //    return result > 0;
        //}

        #endregion MpApp

        #region MpAppInteropSetting 

        public static async Task<List<MpAppClipboardFormatInfo>> GetAppClipboardFormatInfosByAppIdAsync(int appId) {
            string query = $"select * from MpAppClipboardFormatInfo where fk_MpAppId=?";
            var result = await MpDb.QueryAsync<MpAppClipboardFormatInfo>(query,appId);
            return result;
        }

        #endregion MpAppInteropSetting

        #region MpAppPasteShortcut
        
        public static async Task<MpAppPasteShortcut> GetAppPasteShortcutAsync(int appId) {
            string query = $"select * from MpAppPasteShortcut where fk_MpAppId=?";
            var result = await MpDb.QueryAsync<MpAppPasteShortcut>(query, appId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion MpAppPasteShortcut

        #region MpUrl

        public static async Task<MpUrl> GetUrlByPathAsync(string url) {
            string query = $"select * from MpUrl where UrlPath=?";
            var result = await MpDb.QueryAsync<MpUrl>(query, url);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<List<MpUrl>> GetAllUrlsByDomainNameAsync(string domain) {
            string query = $"select * from MpUrl where UrlDomainPatn=?";
            var result = await MpDb.QueryAsync<MpUrl>(query, domain.ToLower());
            return result;
        }

        #endregion MpUrl

        #region MpCopyItemSource
        public static async Task<MpCopyItemSource> GetCopyItemSourceByMembersAsync(int ciid, MpCopyItemSourceType sourceType, int sourceObjId) {
            string query = $"select * from MpCopyItemSource where fk_MpCopyItemId=? and e_MpCopyItemSourceType=? and fk_SourceObjId=?";
            var result = await MpDb.QueryAsync<MpCopyItemSource>(query, ciid, sourceType.ToString(), sourceObjId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<List<MpCopyItemSource>> GetCopyItemSources(int ciid) {
            string query = $"select * from MpCopyItemSource where fk_MpCopyItemId=?";
            var result = await MpDb.QueryAsync<MpCopyItemSource>(query,ciid);
            return result;
        }
        #endregion

        #region MpSource

        public static async Task<List<MpSource>> GetAllSourcesByAppIdAsync(int appId) {
            string query = $"select * from MpSource where fk_MpAppId=?";
            var result = await MpDb.QueryAsync<MpSource>(query, appId);
            return result;
        }

        public static async Task<List<MpSource>> GetAllSourcesByUrlIdAsync(int urlId) {
            string query = $"select * from MpSource where fk_MpUrlId=?";
            var result = await MpDb.QueryAsync<MpSource>(query, urlId);
            return result;
        }

        public static async Task<MpSource> GetSourceByMembersAsync(int appId, int urlId, int copyItemId = 0) {
            string query = $"select * from MpSource where fk_MpAppId=? and fk_MpUrlId=? and fk_MpCopyItemId=?";
            var result = await MpDb.QueryAsync<MpSource>(query, appId, urlId,copyItemId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<MpSource> GetSourceByGuidAsync(string guid) {
            string query = $"select * from MpSource where MpSourceGuid=?";
            var result = await MpDb.QueryAsync<MpSource>(query, guid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion MpSource

        #region MpCopyItem

        public static async Task<List<MpCopyItem>> GetCopyItemsByAppIdAsync(int appId) {
            var sl = await GetAllSourcesByAppIdAsync(appId);

            string whereStr = string.Join(" or ", sl.Select(x => string.Format(@"fk_MpSourceId={0}", x.Id)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public static async Task<List<MpCopyItem>> GetCopyItemsByUrlIdAsync(int urlId) {
            List<MpSource> sl = await GetAllSourcesByUrlIdAsync(urlId);
            string whereStr = string.Join(" or ", sl.Select(x => string.Format(@"fk_MpSourceId={0}", x.Id)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public static async Task<List<MpCopyItem>> GetCopyItemsByUrlDomainAsync(string domainStr) {
            var urll = await GetAllUrlsByDomainNameAsync(domainStr);
            List<MpSource> sl = new List<MpSource>();
            foreach(var url in urll) {
                var ssl = await GetAllSourcesByUrlIdAsync(url.Id);
                sl.AddRange(ssl);
            }
            sl = sl.Distinct().ToList();
            string whereStr = string.Join(" or ", sl.Select(x => string.Format(@"fk_MpSourceId={0}", x.Id)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public static async Task<List<MpCopyItem>> GetCopyItemsByIdListAsync(List<int> ciida) {
            if(ciida.Count == 0) {
                return new List<MpCopyItem>();
            }
            string whereStr = string.Join(" or ", ciida.Select(x => string.Format(@"pk_MpCopyItemId={0}", x)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result.OrderBy(x=>ciida.IndexOf(x.Id)).ToList();
        }


        public static async Task<MpCopyItem> GetCopyItemByGuidAsync(string cig) {
            string query = "select * from MpCopyItem where MpCopyItemGuid=?";
            var result = await MpDb.QueryAsync<MpCopyItem>(query,cig);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<List<MpCopyItem>> GetCopyItemsByGuidsAsync(string[] cigl) {
            if(cigl.Length == 0) {
                return new List<MpCopyItem>();
            }
            string whereStr = string.Join(" or ", cigl.Select(x => string.Format(@"MpCopyItemGuid='{0}'", x)));
            string query = $"select * from MpCopyItem where {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result.OrderBy(x => cigl).ToList();
        }

        public static async Task<MpCopyItem> GetCopyItemByIdAsync(int ciid) {
            // NOTE this is used to safely try to get item instead of GetItemAsync 
            // which crashes if the key doesn't exist...
            string query = "select * from MpCopyItem where pk_MpCopyItemId=?";
            var result = await MpDb.QueryAsync<MpCopyItem>(query, ciid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<MpCopyItem> GetCopyItemByDataAsync(string text) {
            string query = "select * from MpCopyItem where ItemData=?";
            var result = await MpDb.QueryAsync<MpCopyItem>(query, text);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }
        public static async Task<List<MpCopyItem>> GetMostRecentCopyItems(int count, MpCopyItemType[] excludedFormats) {
            string whereClause = excludedFormats == null || excludedFormats.Length == 0 ?
                string.Empty :
                "where " + string.Join(" and ", excludedFormats.Select(x => "fk_MpCopyItemTypeId <> " + ((int)(x)).ToString()));

            string query = $"select * from MpCopyItem {whereClause} order by CopyDateTime DESC limit ?";
            var result = await MpDb.QueryAsync<MpCopyItem>(query, count);
            return result;
        }
        public static async Task<MpCopyItem> GetMostRecentCopyItem(MpCopyItemType[] excludedFormats) {
            var result = await GetMostRecentCopyItems(1,excludedFormats);
            if(result.Count > 0) {
                return result[0];
            }
            return null;
        }
        public static async Task<int> GetTotalCopyItemCountAsync() {
            string query = "select count(pk_MpCopyItemId) from MpCopyItem";
            var result = await MpDb.QueryScalarAsync<int>(query);
            return result;
        }


        #endregion MpCopyItem

        #region MpTextTemplate
        public static Task<List<string>> ParseTextTemplateGuidsByCopyItemIdAsync(MpCopyItem ci) {
            var textTemplateGuids = new List<string>();
            if (ci == null) {
                return Task.FromResult(textTemplateGuids);
            }
            var encodedTemplateGuids = MpRegEx.RegExLookup[MpRegExType.EncodedTextTemplate].Matches(ci.ItemData);
            if (encodedTemplateGuids.Count == 0) {
                return Task.FromResult(textTemplateGuids);
            }
            foreach (Match encodedTemplateGuid in encodedTemplateGuids) {
                var guidMatch = MpRegEx.RegExLookup[MpRegExType.Guid].Match(encodedTemplateGuid.Value);
                textTemplateGuids.Add(guidMatch.Value);
            }
            textTemplateGuids = textTemplateGuids.Distinct().ToList();
            return Task.FromResult(textTemplateGuids);
        }

        public static async Task<List<MpTextTemplate>> GetTextTemplatesByType(IEnumerable<MpTextTemplateType> templateTypes) {
            string whereStr;
            if (templateTypes == null || templateTypes.Count() == 0) {
                whereStr = string.Empty;
            } else {
                string where_clause = string.Join(" or ", templateTypes.Select(x => string.Format(@"LOWER(TemplateTypeStr)=LOWER('{0}')", x)));
                whereStr = $"where {where_clause}";
            }
            string query = $"select * from MpTextTemplate {whereStr}";
            var result = await MpDb.QueryAsync<MpTextTemplate>(query, whereStr);
            return result;
        }

        public static async Task<MpTextTemplate> GetTextTemplateByGuidAsync(string guid) {
            string query = @"select * from MpTextTemplate where MpTextTemplateGuid=?";
            var result = await MpDb.QueryAsync<MpTextTemplate>(query, guid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }
        public static async Task<int> GetTextTemplateIdByGuidAsync(string tguid) {
            string query = $"select pk_MpTextTemplateId from MpTextTemplate where MpTextTemplateGuid=?";
            var result = await MpDb.QueryScalarAsync<int>(query, tguid);
            return result;
        }

        public static async Task<List<MpTextTemplate>> GetTextTemplatesByGuidsAsync(List<string> guids) {
            if(guids == null || guids.Count == 0) {
                return new List<MpTextTemplate>();
            }
            string whereStr = string.Join(" or ", guids.Select(x => string.Format(@"MpTextTemplateGuid='{0}'", x)));
            string query = $"select * from MpTextTemplate where {whereStr}";
            var result = await MpDb.QueryAsync<MpTextTemplate>(query, whereStr);
            return result;
        }

        public static async Task<MpTextTemplate> GetTextTemplateByNameAsync(int ciid, string templateName) {
            string query = @"select * from MpTextTemplate where fk_MpCopyItemId=? and TemplateName=?";
            var result = await MpDb.QueryAsync<MpTextTemplate>(query,ciid,templateName);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }
        public static async Task<List<MpCopyItem>> GetCopyItemsByTextTemplateGuid(string tguid) {
            string whereStr = $"where CopyItemData LIKE '%templateguid=''{tguid}''%'";
            string query = $"select * from MpCopyItem {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result;
        }
        #endregion

        #region MpTextAnnotation

        public static async Task<List<MpTextAnnotation>> GetTextAnnotationsAsync(int ciid) {
            string query = @"select * from MpTextAnnotation where fk_MpCopyItemId=?";
            var result = await MpDb.QueryAsync<MpTextAnnotation>(query);
            return result;
        }

        public static async Task<MpTextAnnotation> GetTextAnnotationByDataAsync(int ciid, int citid, string label, string matchValue, string description) {
            string query = @"select * from MpTextAnnotation where fk_MpCopyItemId=? and fk_MpCopyItemTransactionId=? and Label=? and MatchValue=? and Description=?";
            var result = await MpDb.QueryAsync<MpTextAnnotation>(query, ciid, citid,label,matchValue, description);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion

        #region MpImageAnnotation

        public static async Task<List<MpImageAnnotation>> GetImageAnnotationsByCopyItemIdAsync(int ciid) {
            string query = string.Format(@"select * from MpImageAnnotation where fk_MpCopyItemId=?");
            var result = await MpDb.QueryAsync<MpImageAnnotation>(query,ciid);
            return result;
        }

        public static async Task<MpImageAnnotation> GetImageAnnotationByDataAsync(int ciid,double x,double y,double w,double h,double s,string label,string description,string c) {
            string query = string.Format(@"select * from MpImageAnnotation where fk_MpCopyItemId=? and X=? and Y=? and Width=? and Height=? and Score=? and Label=? and Description=? and HexColor=?");
            var result = await MpDb.QueryAsync<MpImageAnnotation>(query, ciid,x,y,w,h,s,label,description,c);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion

        #region MpCopyItemTransaction

        public static async Task<List<MpCopyItemTransaction>> GetCopyItemTransactionsByCopyItemIdAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItemTransaction where fk_MpCopyItemId=?");
            var result = await MpDb.QueryAsync<MpCopyItemTransaction>(query, ciid);
            return result;
        }

        public static async Task<List<MpCopyItemTransaction>> GetCopyItemTransactionsByTransactionTypeIdPairAsync(IEnumerable<KeyValuePair<MpCopyItemTransactionType,int>> trans_lookup) {
            if (trans_lookup == null || trans_lookup.Count() == 0) {
                return new List<MpCopyItemTransaction>();
            }
            string whereStr = string.Join(" or ", trans_lookup.Select(x => $"(e_MpCopyItemTransactionType='{x.Key}' AND fk_CopyItemTransactionObjectId={x.Value}"));
            string query = $"select * from MpCopyItemTransaction where {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItemTransaction>(query, whereStr);
            return result;
        }

        #endregion

        #region MpIPluginPresetTransaction

        public static async Task<List<MpIPluginPresetTransaction>> GetPluginPresetTransactionsByCopyItemId(int ciid) {
            List<MpIPluginPresetTransaction> ci_transactions = new List<MpIPluginPresetTransaction>();
            var citl = await GetCopyItemTransactionsByCopyItemIdAsync(ciid);
            foreach(var cit in citl) {
                MpIPluginPresetTransaction ppt = null;
                switch (cit.CopyItemTransactionType) {
                    case MpCopyItemTransactionType.Http:
                        ppt = await GetItemAsync<MpHttpTransaction>(cit.CopyItemTransactionObjId);
                        break;
                    case MpCopyItemTransactionType.Dll:
                        ppt = await GetItemAsync<MpDllTransaction>(cit.CopyItemTransactionObjId);
                        break;
                    case MpCopyItemTransactionType.Cli:
                        ppt = await GetItemAsync<MpCliTransaction>(cit.CopyItemTransactionObjId);
                        break;
                }
                if(ppt == null) {
                    continue;
                }
                ci_transactions.Add(ppt);
            }
            return ci_transactions;
        }

        public static async Task<List<MpIPluginPresetTransaction>> GetPluginPresetTransactionsByPresetId(int ppid) {
            List<MpIPluginPresetTransaction> preset_transactions = new List<MpIPluginPresetTransaction>();

            var clil = await GetCliTransactionsByPresetIdAsync(ppid);
            if(clil != null && clil.Count > 0) {
                preset_transactions.AddRange(clil);
            }
            var httpl = await GetHttpTransactionsByPresetIdAsync(ppid);
            if (httpl != null && httpl.Count > 0) {
                preset_transactions.AddRange(httpl);
            }
            var dlll = await GetDllTransactionsByPresetIdAsync(ppid);
            if (dlll != null && dlll.Count > 0) {
                preset_transactions.AddRange(dlll);
            }
            return preset_transactions;
        }

        #endregion

        #region MpCliTransaction

        public static async Task<List<MpCliTransaction>> GetCliTransactionsByPresetIdAsync(int pid) {
            string query = string.Format(@"select * from MpCliTransaction where fk_MpAnalyticItemPresetId=?");
            var result = await MpDb.QueryAsync<MpCliTransaction>(query,pid);
            return result;
        }
        #endregion

        #region MpHttpTransaction

        public static async Task<List<MpHttpTransaction>> GetHttpTransactionsByPresetIdAsync(int pid) {
            string query = string.Format(@"select * from MpHttpTransaction where fk_MpAnalyticItemPresetId=?");
            var result = await MpDb.QueryAsync<MpHttpTransaction>(query, pid);
            return result;
        }
        #endregion

        #region MpDllTransaction

        public static async Task<List<MpDllTransaction>> GetDllTransactionsByPresetIdAsync(int pid) {
            string query = string.Format(@"select * from MpDllTransaction where fk_MpAnalyticItemPresetId=?");
            var result = await MpDb.QueryAsync<MpDllTransaction>(query, pid);
            return result;
        }
        #endregion

        #region MpISourceRef

        public static async Task<MpISourceRef> GetSourceRefByCopyItemTransactionIdAsync(int citid) {
            var ci_trans = await GetItemAsync<MpCopyItemTransaction>(citid);
            if (ci_trans != null) {

                switch (ci_trans.CopyItemTransactionType) {
                    case MpCopyItemTransactionType.Http:
                        var http_tran = await GetItemAsync<MpHttpTransaction>(ci_trans.CopyItemTransactionObjId);
                        if (http_tran != null) {
                            var url = await GetItemAsync<MpUrl>(http_tran.UrlId);
                            return url;
                        }
                        break;
                    case MpCopyItemTransactionType.Cli:
                        var cli_tran = await GetItemAsync<MpCliTransaction>(ci_trans.CopyItemTransactionObjId);
                        if (cli_tran != null) {
                            var app = await GetItemAsync<MpApp>(cli_tran.AppId);
                            return app;
                        }
                        break;
                    case MpCopyItemTransactionType.Dll:
                        var dll_tran = await GetItemAsync<MpDllTransaction>(ci_trans.CopyItemTransactionObjId);
                        if (dll_tran != null) {
                            var app = await GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
                            return app;
                        }
                        break;
                }

            }
            return null;
        }
        #endregion

        #region MpCopyItemTag
        public static async Task<List<int>> GetCopyItemIdsForTagAsync(int tagId) {
            string query = string.Format(@"select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId={0}", tagId);
            var result = await MpDb.QueryScalarsAsync<int>(query);
            return result;
        }

        public static async Task<List<int>> GetTagIdsForCopyItemAsync(int ciid) {
            string query = string.Format(@"select fk_MpTagId from MpCopyItemTag where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.QueryScalarsAsync<int>(query);
            return result;
        }

        public static List<int> GetTagIdsForCopyItem(int ciid) {
            string query = string.Format(@"select fk_MpTagId from MpCopyItemTag where fk_MpCopyItemId={0}", ciid);
            var result = MpDb.QueryScalars<int>(query);
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

        public static async Task<List<string>> GetTagColorsForCopyItemAsync(int ciid) {
            string query = @"select HexColor from MpTag where pk_MpTagId in (select fk_MpTagId from MpCopyItemTag where fk_MpCopyItemId = ?)";
            var result = await MpDb.QueryScalarsAsync<string>(query,ciid);
            return result;
        }
        

        public static async Task<List<MpCopyItemTag>> GetCopyItemTagsForCopyItemAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItemTag where fk_MpCopyItemId={0}", ciid);
            var result = await MpDb.QueryAsync<MpCopyItemTag>(query);
            return result;
        }

        public static async Task<bool> IsTagLinkedWithCopyItemAsync(int tagId, int copyItemId) {
            string query = $"select count(*) from MpCopyItemTag where fk_MpTagId=? and fk_MpCopyItemId=?";
            var result = await MpDb.QueryScalarAsync<int>(query, tagId, copyItemId);
            return result > 0;
        }

        public static bool IsTagLinkedWithCopyItem(int tagId, int copyItemId) {
            // returns true if tag is linked
            // returns false if tag is not linked and no children are linked
            // returns null if not linked and 1 or more children are linked 

            string query = $"select count(*) from MpCopyItemTag where fk_MpTagId=? and fk_MpCopyItemId=?";
            var result = MpDb.QueryScalar<int>(query, tagId, copyItemId);
            return result > 0;
        }

        #endregion MpCopyItemTag        

        #region MpTag

        public static async Task<List<MpTag>> GetChildTagsAsync(int tagId) {
            string query = "select * from MpTag where fk_ParentTagId=?";
            var result = await MpDb.QueryAsync<MpTag>(query,tagId);
            return result;
        }

        public static async Task<int> GetChildTagCountAsync(int tagId) {
            string query = "select count(pk_MpTagId) from MpTag where fk_ParentTagId=?";
            var result = await MpDb.QueryScalarAsync<int>(query, tagId);
            return result;
        }

        #endregion

        #region MpShortcut

        public static async Task<MpShortcut> GetShortcutAsync(string shortcutTypeName, string commandParameter = null) {
            List<MpShortcut> result;
           
            if (commandParameter == null) {
                string query = string.Format(@"select * from MpShortcut where ShortcutTypeName=? and CommandParameter is NULL");
                result = await MpDb.QueryAsync<MpShortcut>(query, shortcutTypeName);
            } else {
                string query = string.Format(@"select * from MpShortcut where ShortcutTypeName=? and CommandParameter=?");
                result = await MpDb.QueryAsync<MpShortcut>(query, shortcutTypeName, commandParameter);
            }
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<string> GetShortcutKeystringAsync(string shortcutTypeName, string commandParameter = null) {
            string result;
            if (commandParameter == null) {
                string query = string.Format(@"select KeyString from MpShortcut where ShortcutTypeName=? and CommandParameter is NULL");
                result = await MpDb.QueryScalarAsync<string>(query, shortcutTypeName);
            } else {
                string query = string.Format(@"select KeyString from MpShortcut where ShortcutTypeName=? and CommandParameter=?");
                result = await MpDb.QueryScalarAsync<string>(query, shortcutTypeName, commandParameter);
            }

            return result;
        }

        public static string GetShortcutKeystring(string shortcutTypeName, string commandParameter = null) {
            string result;
            if(commandParameter == null) {
                string query = string.Format(@"select KeyString from MpShortcut where ShortcutTypeName=? and CommandParameter is NULL");
                result = MpDb.QueryScalar<string>(query, shortcutTypeName);
            } else {
                string query = string.Format(@"select KeyString from MpShortcut where ShortcutTypeName=? and CommandParameter=?");
                result = MpDb.QueryScalar<string>(query, shortcutTypeName, commandParameter);
            }
            return result;
        }

        public static async Task<List<MpShortcut>> GetAllShortcutsAsync() {
            string query = @"select * from MpShortcut";
            var result = await MpDb.QueryAsync<MpShortcut>(query);
            return result;
        }
        #endregion

        #region MpPasteToAppPath



        #endregion

        #region MpAnalytic Item
        public static async Task<int> GetPluginPresetCountByPluginGuidAsync(string aguid) {
            string query = $"select count(*) from MpPluginPreset where PluginGuid=?";
            var result = await MpDb.QueryScalarAsync<int>(query, aguid);
            return result;
        }
        public static async Task<List<MpPluginPreset>> GetPluginPresetsByPluginGuidAsync(string aguid) {
            string query = $"select * from MpPluginPreset where PluginGuid=?";
            var result = await MpDb.QueryAsync<MpPluginPreset>(query, aguid);
            return result;
        }

        public static async Task<List<MpPluginPresetParameterValue>> GetPluginPresetValuesByPresetIdAsync(int presetId) {
            string query = $"select * from MpPluginPresetParameterValue where fk_MpPluginPresetId=?";
            var result = await MpDb.QueryAsync<MpPluginPresetParameterValue>(query, presetId);
            return result;
        }


        public static async Task<MpPluginPresetParameterValue> GetPluginPresetValueAsync(int presetId, string paramId) {
            string query = $"select * from MpPluginPresetParameterValue where fk_MpPluginPresetId=? and ParamId=?";
            var result = await MpDb.QueryAsync<MpPluginPresetParameterValue>(query, presetId, paramId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion 

        #region MpDbLog

        public static async Task<MpDbLog> GetDbLogByIdAsync(int DbLogId) {
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

        public static async Task<MpSyncHistory> GetSyncHistoryByDeviceGuidAsync(string dg) {
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

        public static async Task<List<MpAction>> GetAllTriggerActionsAsync() {
            string query = $"select * from MpAction where e_MpActionTypeId=? and fk_ActionObjId != ?";
            var result = await MpDb.QueryAsync<MpAction>(query, (int)MpActionType.Trigger,(int)MpTriggerType.ParentOutput);
            return result;
        }

        public static async Task<int> GetChildActionCountAsync(int actionId) {
            string query = $"select count(pk_MpActionId) from MpAction where fk_ParentActionId=?";
            var result = await MpDb.QueryScalarAsync<int>(query, actionId);
            return result;
        }

        public static async Task<List<MpAction>> GetChildActionsAsync(int actionId) {
            string query = $"select * from MpAction where fk_ParentActionId=?";
            var result = await MpDb.QueryAsync<MpAction>(query, actionId);
            return result;
        }

        public static async Task<MpAction> GetActionByLabelAsync(string label) {
            string query = string.Format(@"select * from MpAction where Label=?");
            var result = await MpDb.QueryAsync<MpAction>(query, label);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }
        #endregion

        #region MpContentToken

        public static async Task<MpContentToken> GetTokenAsync(int copyItemId, int actionId, string matchData) {
            string query = string.Format(@"select * from MpContentToken where fk_MpCopyItemId=? and fk_MpActionId=? and MatchData=?");
            var result = await MpDb.QueryAsync<MpContentToken>(query, copyItemId, actionId,matchData);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<List<MpContentToken>> GetTokenByActionIdAsync(int actionId) {
            string query = $"select * from MpContentToken where fk_MpActionId=?";
            var result = await MpDb.QueryAsync<MpContentToken>(query, actionId);
            return result;
        }

        public static async Task<List<MpContentToken>> GetTokenByCopyItemIdAsync(int copyItemId) {
            string query = $"select * from MpContentToken where fk_MpCopyItemId=?";
            var result = await MpDb.QueryAsync<MpContentToken>(query, copyItemId);
            return result;
        }

        #endregion

        #region MpDataObject

        public static async Task<bool> IsDataObjectContainFormatAsync(int doid, string formatName) {
            string query = string.Format(@"select count(pk_MpDataObjectItemId) from MpDataObjectItem where fk_MpDataObjectId=? and ItemFormat=?");
            int result = await MpDb.QueryScalarAsync<int>(query, doid,formatName);
            return result > 0;
        }

        public static async Task<List<MpDataObjectItem>> GetDataObjectItemsByDataObjectId(int dobjid) {
            string query = "select * from MpDataObjectItem where fk_MpDataObjectId=?";
            var result = await MpDb.QueryAsync<MpDataObjectItem>(query, dobjid);
            return result;
        }

        public static async Task<List<MpDataObjectItem>> GetDataObjectItemsForFormatByDataObjectId(int dobjid, string format) {
            string query = "select * from MpDataObjectItem where fk_MpDataObjectId=? and ItemFormat=?";
            var result = await MpDb.QueryAsync<MpDataObjectItem>(query, dobjid,format);
            return result;
        }

        #endregion

        #region MpUserSearch


        public static async Task<List<MpSearchCriteriaItem>> GetCriteriaItemsByUserSearchId(int usid) {
            string query = "select * from MpSearchCriteriaItem where fk_MpUserSearchId=?";
            var result = await MpDb.QueryAsync<MpSearchCriteriaItem>(query, usid);
            return result;
        }

        #endregion

        #endregion

        #endregion

    }
}
