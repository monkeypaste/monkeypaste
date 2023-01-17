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
        #endregion

        #region Constructor
        #endregion

        #region Public Methods

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
            string query = null;
            if(string.IsNullOrEmpty(machineName)) {
                query = $"select * from MpUserDevice where e_MpUserDeviceType=?";
            } else {
                query = $"select * from MpUserDevice where MachineName=? and e_MpUserDeviceType=?";
            }
            
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

        #region MpTransactionSource
        public static async Task<MpTransactionSource> GetCopyItemSourceByMembersAsync(int citid, MpTransactionSourceType sourceType, int sourceObjId) {
            string query = $"select * from MpTransactionSource where fk_MpCopyItemTransactionId=? and e_MpTransactionSourceType=? and fk_SourceObjId=?";
            var result = await MpDb.QueryAsync<MpTransactionSource>(query, citid, sourceType.ToString(), sourceObjId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<List<MpTransactionSource>> GetCopyItemSources(int ciid) {
            string query = $"select * from MpTransactionSource where fk_MpCopyItemTransactionId in (select pk_MpCopyItemTransactionId from MpCopyItemTransaction where fk_MpCopyItemId=?)";
            var result = await MpDb.QueryAsync<MpTransactionSource>(query,ciid);
            return result;
        }
        
        public static async Task<List<MpTransactionSource>> GetCopyItemTransactionSourcesAsync(int citid) {
            string query = $"select * from MpTransactionSource where fk_MpCopyItemTransactionId=?";
            var result = await MpDb.QueryAsync<MpTransactionSource>(query,citid);
            return result;
        }
        #endregion

        #region MpCopyItem

        public static async Task<List<MpCopyItem>> GetCopyItemsBySourceTypeAndIdAsync(
            MpTransactionSourceType sourceType, 
            int sourceObjId) {
            string query = 
                $"select fk_MpCopyItemId from MpTransactionSource where e_MpTransactionSourceType=='{sourceType.ToString()}' and fk_SourceObjId = ?";
            var result = await MpDb.QueryAsync<MpCopyItem>(query, sourceObjId);
            return result;
        }


        public static async Task<List<MpCopyItem>> GetCopyItemsByIdListAsync(List<int> ciida) {
            //if(ciida.Count == 0) {
            //    return new List<MpCopyItem>();
            //}
            //string whereStr = string.Join(" or ", ciida.Select(x => string.Format(@"pk_MpCopyItemId={0}", x)));
            //string query = $"select * from MpCopyItem where {whereStr}";
            //var result = await MpDb.QueryAsync<MpCopyItem>(query);
            //return result.OrderBy(x=>ciida.IndexOf(x.Id)).ToList();
            string query = $"select * from MpCopyItem where pk_MpCopyItemId in ({string.Join(",", ciida.Select(x => "?"))})";
            var result = await MpDb.QueryAsync<MpCopyItem>(query,ciida.Cast<object>().ToArray());
            return result.OrderBy(x => ciida.IndexOf(x.Id)).ToList();
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


        #endregion 

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

        #region MpCopyItemAnnotation

        public static async Task<List<MpCopyItemAnnotation>> GetCopyItemAnnotationsAsync(int ciid) {
            string query = @"select * from MpCopyItemAnnotation where fk_MpCopyItemId=?";
            var result = await MpDb.QueryAsync<MpCopyItemAnnotation>(query,ciid);
            return result;
        }

        #endregion


        #region MpCopyItemTransaction

        public static async Task<List<MpCopyItemTransaction>> GetCopyItemTransactionsByCopyItemIdAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItemTransaction where fk_MpCopyItemId=?");
            var result = await MpDb.QueryAsync<MpCopyItemTransaction>(query, ciid);
            return result;
        }

        public static async Task<List<MpCopyItemTransaction>> GetCopyItemTransactionsByTransactionTypeIdPairAsync(IEnumerable<KeyValuePair<MpTransactionSourceType,int>> trans_lookup) {
            if (trans_lookup == null || trans_lookup.Count() == 0) {
                return new List<MpCopyItemTransaction>();
            }
            string whereStr = string.Join(" or ", trans_lookup.Select(x => $"(e_MpCopyItemTransactionType='{x.Key}' AND fk_TransactionObjId={x.Value})"));
            string query = $"select * from MpCopyItemTransaction where {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItemTransaction>(query, whereStr);
            return result;
        }

        #endregion


        #region MpISourceRef

        public static async Task<MpISourceRef> GetSourceRefByTransactionTypeAndSourceIdAsync(
            MpTransactionSourceType sourceType, int sourceId) {
            MpISourceRef source_ref;
            switch (sourceType) {
                case MpTransactionSourceType.App:
                    source_ref = await MpDataModelProvider.GetItemAsync<MpApp>(sourceId);
                    break;
                case MpTransactionSourceType.Url:
                    source_ref = await MpDataModelProvider.GetItemAsync<MpUrl>(sourceId);
                    break;
                case MpTransactionSourceType.AnalyzerPreset:
                    source_ref = await MpDataModelProvider.GetItemAsync<MpPluginPreset>(sourceId);
                    break;
                case MpTransactionSourceType.CopyItem:
                    source_ref = await MpDataModelProvider.GetItemAsync<MpCopyItem>(sourceId);
                    break;
                default:
                    throw new Exception($"Unknown source type: '{sourceType}'");
            }
            return source_ref;
        }
        public static async Task<List<MpISourceRef>> GetSourceRefsByCopyItemTransactionIdAsync(int citid) {
            var cit_sources = await GetCopyItemTransactionSourcesAsync(citid);
            List<MpISourceRef> result = new List<MpISourceRef>();
            foreach(var cit_source in cit_sources) {
                var source_ref = await GetSourceRefByTransactionTypeAndSourceIdAsync(cit_source.CopyItemSourceType, cit_source.SourceObjId);
                result.Add(source_ref);
            }
            return result;
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

        public static async Task<MpPluginPreset> GetPluginPresetByPresetGuidAsync(string preset_guid) {
            string query = $"select * from MpPluginPreset where MpPluginPresetGuid=?";
            var result = await MpDb.QueryAsync<MpPluginPreset>(query, preset_guid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }
        public static async Task<List<MpPluginPreset>> GetPluginPresetsByPluginGuidAsync(string aguid) {
            string query = $"select * from MpPluginPreset where PluginGuid=?";
            var result = await MpDb.QueryAsync<MpPluginPreset>(query, aguid);
            return result;
        }

        public static async Task<List<MpPluginPresetParameterValue>> GetAllParameterHostValuesAsync(MpParameterHostType hostType,int paramHostId) {
            string query = $"select * from MpPluginPresetParameterValue where e_MpParameterHostType=? and fk_ParameterHostId=?";
            var result = await MpDb.QueryAsync<MpPluginPresetParameterValue>(query,hostType.ToString(), paramHostId);
            return result;
        }


        public static async Task<MpPluginPresetParameterValue> GetParameterValueAsync(MpParameterHostType hostType, int paramHostId, string paramId) {
            string query = $"select * from MpPluginPresetParameterValue where e_MpParameterHostType=? and fk_ParameterHostId=? and ParamId=?";
            var result = await MpDb.QueryAsync<MpPluginPresetParameterValue>(query,hostType.ToString(), paramHostId, paramId);

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
            string query = $"select * from MpAction where e_MpActionType=?";
            var result = await MpDb.QueryAsync<MpAction>(query, MpActionType.Trigger.ToString());
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
