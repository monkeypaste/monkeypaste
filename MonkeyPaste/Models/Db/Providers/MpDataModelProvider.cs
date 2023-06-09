using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using Xamarin.Forms;

namespace MonkeyPaste {

    public static class MpDataModelProvider {
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

        #region Guid Query

        public static async Task<int> GetItemIdByGuidAsync<T>(string guid) where T : new() {
            string table_name = typeof(T).ToString().Replace("MonkeyPaste.", string.Empty);
            string pk_name = $"pk_{table_name}Id";
            string guid_name = $"{table_name}Guid";
            string query = $"select {pk_name} from {table_name} where {guid_name}=?";
            var result = await MpDb.QueryScalarAsync<int>(query, guid);
            return result;
        }

        public static async Task<T> GetItemAsync<T>(string guid) where T : new() {
            int item_id = await GetItemIdByGuidAsync<T>(guid);
            var result = await GetItemAsync<T>(item_id);
            return result;
        }
        #endregion

        #region Id Query

        public static async Task<T> GetItemAsync<T>(int id) where T : new() {
            string table_name = typeof(T).ToString().Replace("MonkeyPaste.", string.Empty);
            string pk_name = $"pk_{table_name}Id";
            string query = $"select * from {table_name} where {pk_name}=?";
            var result = await MpDb.QueryAsync<T>(query, id);
            if (result == null || result.Count == 0) {
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

        #region MpContentQueryView_simple (PropertyPath Queries)

        public static async Task<T> GetSortableCopyItemViewPropertyAsync<T>(int ciid, string propertyName) {
            string query = "select ? from MpContentQueryView_simple where pk_MpCopyItemId=?";
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

        public static async Task<MpUserDevice> GetUserDeviceByMembersAsync(
            string machineName,
            MpUserDeviceType deviceType) {
            string query;
            if (string.IsNullOrEmpty(machineName)) {
                query = $"select * from MpUserDevice where e_MpUserDeviceType=?";
            } else {
                query = $"select * from MpUserDevice where MachineName=? and e_MpUserDeviceType=?";
            }

            var result = await MpDb.QueryAsync<MpUserDevice>(query, machineName, deviceType.ToString());
            if (result == null || result.Count == 0) {
                return null;
            }
            if (result.Count > 1) {
                // this should only be 1
                Debugger.Break();
            }
            return result[0];
        }

        #endregion MpUserDevice

        #region DbImage

        public static async Task<string> GetDbImageBase64ByIconIdAsync(int iconId) {
            string query = $"select ImageBase64 from MpDbImage where pk_MpDbImageId=(select fk_IconDbImageId from MpIcon where pk_MpIconId=?)";
            var result = await MpDb.QueryScalarAsync<string>(query, iconId);
            return result;
        }

        public static async Task<MpDbImage> GetDbImageByBase64StrAsync(string text64) {
            string query = $"select pk_MpDbImageId from MpDbImage where ImageBase64=?";
            var result = await MpDb.QueryAsync<MpDbImage>(query, text64);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion        

        #region MpIcon


        public static async Task<MpIcon> GetIconByImageStrAsync(string text64) {
            string query = $"select * from MpIcon where fk_IconDbImageId=(select pk_MpDbImageId from MpDbImage where ImageBase64=?)";
            var result = await MpDb.QueryAsync<MpIcon>(query, text64);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }


        #endregion MpIcon

        #region MpApp

        public static async Task<MpApp> GetAppByMembersAsync(string path, string args, int deviceId) {
            List<MpApp> result;
            string query = $"select * from MpApp where LOWER(AppPath)=? and Arguments=? and fk_MpUserDeviceId=?";
            result = await MpDb.QueryAsync<MpApp>(query, path.ToLower(), args, deviceId);
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
            var result = await MpDb.QueryAsync<MpAppClipboardFormatInfo>(query, appId);
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

        public static async Task<List<MpTransactionSource>> GetCopyItemSourcesAsync(int ciid) {
            string query = $"select * from MpTransactionSource where fk_MpCopyItemTransactionId in (select pk_MpCopyItemTransactionId from MpCopyItemTransaction where fk_MpCopyItemId=?)";
            var result = await MpDb.QueryAsync<MpTransactionSource>(query, ciid);
            return result;
        }

        public static async Task<List<MpTransactionSource>> GetCopyItemTransactionSourcesAsync(int citid) {
            string query = $"select * from MpTransactionSource where fk_MpCopyItemTransactionId=?";
            var result = await MpDb.QueryAsync<MpTransactionSource>(query, citid);
            return result;
        }
        #endregion

        #region MpCopyItem

        public static async Task<List<MpCopyItem>> GetCopyItemsBySourceTypeAndIdAsync(
            MpTransactionSourceType sourceType,
            int sourceObjId) {
            string query =
                $"select * from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTransaction where pk_MpCopyItemTransactionId IN (select fk_MpCopyItemTransactionId from MpTransactionSource where e_MpTransactionSourceType=? and fk_SourceObjId=?))";
            var result = await MpDb.QueryAsync<MpCopyItem>(query, sourceType.ToString(), sourceObjId);
            return result;
        }


        public static async Task<List<MpCopyItem>> GetCopyItemsByIdListAsync(List<int> ciida) {
            string query = $"select * from MpCopyItem where pk_MpCopyItemId in ({string.Join(",", ciida.Select(x => "?"))})";
            var result = await MpDb.QueryAsync<MpCopyItem>(query, ciida.Cast<object>().ToArray());
            return result.OrderBy(x => ciida.IndexOf(x.Id)).ToList();
        }

        public static async Task<List<MpCopyItem>> GetCopyItemsByDataObjectIdListAsync(List<int> dobidl) {
            string query = $"select * from MpCopyItem where pk_MpCopyItemId in (select distinct pk_MpCopyItemId from MpCopyItem where fk_MpDataObjectId in ({string.Join(",", dobidl.Select(x => "?"))}))";
            var result = await MpDb.QueryAsync<MpCopyItem>(query, dobidl.Cast<object>().ToArray());
            return result.ToList();
        }

        public static async Task<MpCopyItem> GetCopyItemByDataAsync(string text) {
            string query = "select * from MpCopyItem where ItemData=?";
            var result = await MpDb.QueryAsync<MpCopyItem>(query, text);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<MpCopyItem> GetCopyItemByDataObjectIdAsync(int doid) {
            string query = "select * from MpCopyItem where fk_MpDataObjectId=?";
            var result = await MpDb.QueryAsync<MpCopyItem>(query, doid);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }
        public static async Task<int> GetTotalCopyItemCountAsync(IEnumerable<int> ciids_to_omit) {
            string exclude_trash_clause = string.Empty;
            if (ciids_to_omit != null && ciids_to_omit.Any()) {
                exclude_trash_clause = $" WHERE pk_MpCopyItemId NOT IN ({string.Join(",", ciids_to_omit)})";
            }
            string query = $"select count(pk_MpCopyItemId) from MpCopyItem{exclude_trash_clause}";
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
            if (templateTypes == null || !templateTypes.Any()) {
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
            if (guids == null || guids.Count == 0) {
                return new List<MpTextTemplate>();
            }
            string whereStr = string.Join(" or ", guids.Select(x => string.Format(@"MpTextTemplateGuid='{0}'", x)));
            string query = $"select * from MpTextTemplate where {whereStr}";
            var result = await MpDb.QueryAsync<MpTextTemplate>(query, whereStr);
            return result;
        }

        public static async Task<MpTextTemplate> GetTextTemplateByNameAsync(int ciid, string templateName) {
            string query = @"select * from MpTextTemplate where fk_MpCopyItemId=? and TemplateName=?";
            var result = await MpDb.QueryAsync<MpTextTemplate>(query, ciid, templateName);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }
        public static async Task<List<MpCopyItem>> GetCopyItemsByTextTemplateGuid(string tguid) {
            string like_match = $"%templateguid=\"{tguid}\"%";
            string query = $"select * from MpCopyItem where ItemData LIKE ?";
            var result = await MpDb.QueryAsync<MpCopyItem>(query, like_match);
            return result;
        }
        #endregion

        #region MpCopyItemAnnotation

        public static async Task<List<MpCopyItemAnnotation>> GetCopyItemAnnotationsAsync(int ciid) {
            string query = @"select * from MpCopyItemAnnotation where fk_MpCopyItemId=?";
            var result = await MpDb.QueryAsync<MpCopyItemAnnotation>(query, ciid);
            return result;
        }

        #endregion

        #region MpCopyItemTransaction

        public static async Task<List<MpCopyItemTransaction>> GetCopyItemTransactionsByCopyItemIdAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItemTransaction where fk_MpCopyItemId=?");
            var result = await MpDb.QueryAsync<MpCopyItemTransaction>(query, ciid);
            return result;
        }

        public static async Task<List<MpCopyItemTransaction>> GetCopyItemTransactionsByTransactionTypeIdPairAsync(IEnumerable<KeyValuePair<MpTransactionSourceType, int>> trans_lookup) {
            if (trans_lookup == null || !trans_lookup.Any()) {
                return new List<MpCopyItemTransaction>();
            }
            string whereStr = string.Join(" or ", trans_lookup.Select(x => $"(e_MpCopyItemTransactionType='{x.Key}' AND fk_TransactionObjId={x.Value})"));
            string query = $"select * from MpCopyItemTransaction where {whereStr}";
            var result = await MpDb.QueryAsync<MpCopyItemTransaction>(query, whereStr);
            return result;
        }

        public static async Task<int> GetCopyItemSourceDeviceIdAsync(int ciid) {
            string query = $"select fk_MpUserDeviceId from MpCopyItemTransaction where fk_MpCopyItemId=? and TransactionLabel=?";
            var result = await MpDb.QueryScalarsAsync<int>(query, ciid, MpTransactionType.Created.ToString());
            MpDebug.Assert(result.Count == 1, $"CopyItem w/ id {ciid} has zero or multiple create transactions, should only have 1");
            if (result.Any()) {
                return result[0];
            }
            return 0;
        }

        #endregion

        #region MpISourceRef

        public static async Task<MpISourceRef> GetSourceRefBySourceTypeAndSourceIdAsync(
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
                case MpTransactionSourceType.UserDevice:
                    source_ref = await MpDataModelProvider.GetItemAsync<MpUserDevice>(sourceId);
                    break;
                default:
                    throw new Exception($"Unknown source type: '{sourceType}'");
            }
            return source_ref;
        }
        public static async Task<List<MpISourceRef>> GetSourceRefsByCopyItemTransactionIdAsync(int citid) {
            var cit_sources = await GetCopyItemTransactionSourcesAsync(citid);
            List<MpISourceRef> result = new List<MpISourceRef>();
            foreach (var cit_source in cit_sources) {
                var source_ref = await GetSourceRefBySourceTypeAndSourceIdAsync(cit_source.CopyItemSourceType, cit_source.SourceObjId);
                result.Add(source_ref);
            }
            return result;
        }

        #endregion

        #region MpCopyItemTag
        public static async Task<List<int>> GetCopyItemIdsForTagAsync(int tagId) {
            string query = "select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=?";
            var result = await MpDb.QueryScalarsAsync<int>(query, tagId);
            return result;
        }

        public static async Task<List<int>> GetTagIdsForCopyItemAsync(int ciid) {
            string query = "select fk_MpTagId from MpCopyItemTag where fk_MpCopyItemId=?";
            var result = await MpDb.QueryScalarsAsync<int>(query, ciid);
            return result;
        }

        public static async Task<int> GetTotalCopyItemCountForTagAndAllDescendantsAsync(int tid, IEnumerable<int> ciids_to_omit) {
            if (tid == MpTag.AllTagId) {
                return await GetTotalCopyItemCountAsync(ciids_to_omit);
            }
            string excluded_ciid_clause = string.Empty;
            if (ciids_to_omit != null && ciids_to_omit.Any()) {
                excluded_ciid_clause = string.Join(",", ciids_to_omit);
            }
            string query = string.Format(
                @"WITH RECURSIVE
                      tag_descendant(n) AS (
                        VALUES({0})
                        UNION 
                        SELECT MpTag.pk_MpTagId FROM MpTag, tag_descendant
                         WHERE fk_ParentTagId=tag_descendant.n
                      )
                    SELECT COUNT(DISTINCT fk_MpCopyItemId) FROM MpCopyItemTag WHERE fk_MpCopyItemId NOT IN ({1}) AND fk_MpTagId IN
                    (SELECT {0} UNION ALL SELECT MpTag.pk_MpTagId FROM MpTag
                     WHERE fk_ParentTagId IN tag_descendant AND fk_ParentTagId > 0);", tid, excluded_ciid_clause);
            var result = await MpDb.QueryScalarAsync<int>(query);
            return result;
        }

        public static async Task<List<MpCopyItem>> GetAllCopyItemsForTagAndAllDescendantsAsync(int tid, IEnumerable<int> ciids_to_omit) {
            if (tid == MpTag.AllTagId) {
                var all_result = await GetItemsAsync<MpCopyItem>();
                if (ciids_to_omit != null && ciids_to_omit.Any()) {
                    all_result = all_result.Where(x => !ciids_to_omit.Contains(x.Id)).ToList();
                }
                return all_result;
            }

            string excluded_ciid_clause = string.Empty;
            if (ciids_to_omit != null && ciids_to_omit.Any()) {
                excluded_ciid_clause = string.Join(",", ciids_to_omit);
            }

            // NOTE sqlite parameterized CTE is too confusing and this has no unsantized inputs so ignoring
            string query = string.Format(
@"SELECT * FROM MpCopyItem WHERE pk_MpCopyItemId IN(
WITH RECURSIVE
  tag_descendant(n) AS (
	VALUES({0})
	UNION 
	SELECT MpTag.pk_MpTagId FROM MpTag, tag_descendant
	 WHERE fk_ParentTagId=tag_descendant.n
  )
SELECT DISTINCT fk_MpCopyItemId FROM MpCopyItemTag WHERE fk_MpCopyItemId NOT IN ({1}) AND fk_MpTagId IN
(SELECT {0} UNION ALL SELECT MpTag.pk_MpTagId FROM MpTag
 WHERE fk_ParentTagId IN tag_descendant AND fk_ParentTagId > 0))", tid, excluded_ciid_clause);
            var result = await MpDb.QueryAsync<MpCopyItem>(query);
            return result;
        }

        public static List<int> GetTagIdsForCopyItem(int ciid) {
            string query = "select fk_MpTagId from MpCopyItemTag where fk_MpCopyItemId=?";
            var result = MpDb.QueryScalars<int>(query, ciid);
            return result;
        }

        public static async Task<int> GetCopyItemCountForTagAsync(int tagId) {
            string query = @"select count(pk_MpCopyItemTagId) from MpCopyItemTag where fk_MpTagId=?";
            var result = await MpDb.QueryScalarAsync<int>(query, tagId);
            return result;
        }

        public static async Task<List<MpCopyItem>> GetCopyItemsForTagAsync(int tagId) {
            string query = "select * from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=?)";
            var result = await MpDb.QueryAsync<MpCopyItem>(query, tagId);
            return result;
        }

        public static async Task<MpCopyItemTag> GetCopyItemTagForTagAsync(int ciid, int tagId) {
            string query = "select * from MpCopyItemTag where fk_MpCopyItemId=? and fk_MpTagId=?";
            var result = await MpDb.QueryAsync<MpCopyItemTag>(query, ciid, tagId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<List<MpCopyItemTag>> GetCopyItemTagsForTagAsync(int tagId) {
            string query = "select * from MpCopyItemTag where fk_MpTagId=?";
            var result = await MpDb.QueryAsync<MpCopyItemTag>(query, tagId);
            return result;
        }

        public static async Task<List<string>> GetTagColorsForCopyItemAsync(int ciid) {
            string query = @"select HexColor from MpTag where pk_MpTagId in (select fk_MpTagId from MpCopyItemTag where fk_MpCopyItemId = ?)";
            var result = await MpDb.QueryScalarsAsync<string>(query, ciid);
            return result;
        }


        public static async Task<List<MpCopyItemTag>> GetCopyItemTagsForCopyItemAsync(int ciid) {
            string query = "select * from MpCopyItemTag where fk_MpCopyItemId=?";
            var result = await MpDb.QueryAsync<MpCopyItemTag>(query, ciid);
            return result;
        }

        public static async Task<bool> IsTagLinkedWithCopyItemAsync(int tagId, int copyItemId) {
            string query = $"select count(*) from MpCopyItemTag where fk_MpTagId=? and fk_MpCopyItemId=?";
            var result = await MpDb.QueryScalarAsync<int>(query, tagId, copyItemId);
            return result > 0;
        }

        public static bool IsTagLinkedWithCopyItem(int tagId, int copyItemId) {
            string query = $"select count(*) from MpCopyItemTag where fk_MpTagId=? and fk_MpCopyItemId=?";
            var result = MpDb.QueryScalar<int>(query, tagId, copyItemId);
            return result > 0;
        }

        #endregion MpCopyItemTag        

        #region MpTag

        public static async Task<List<MpTag>> GetChildTagsAsync(int parentTagId) {
            string query = "select * from MpTag where fk_ParentTagId=?";
            var result = await MpDb.QueryAsync<MpTag>(query, parentTagId);
            return result;
        }

        public static async Task<int> GetChildTagCountAsync(int parentTagId) {
            string query = "select count(pk_MpTagId) from MpTag where fk_ParentTagId=?";
            var result = await MpDb.QueryScalarAsync<int>(query, parentTagId);
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
            if (commandParameter == null) {
                string query = string.Format(@"select KeyString from MpShortcut where ShortcutTypeName=? and CommandParameter is NULL");
                result = MpDb.QueryScalar<string>(query, shortcutTypeName);
            } else {
                string query = string.Format(@"select KeyString from MpShortcut where ShortcutTypeName=? and CommandParameter=?");
                result = MpDb.QueryScalar<string>(query, shortcutTypeName, commandParameter);
            }
            return result;
        }

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

        public static async Task<List<MpParameterValue>> GetAllParameterHostValuesAsync(MpParameterHostType hostType, int paramHostId) {
            string query = $"select * from MpParameterValue where e_MpParameterHostType=? and fk_ParameterHostId=?";
            var result = await MpDb.QueryAsync<MpParameterValue>(query, hostType.ToString(), paramHostId);
            return result;
        }

        public static async Task<List<MpParameterValue>> GetAllParameterValueInstancesForPluginAsync(string paramId, string pluginGuid) {
            string query = $"select * from MpParameterValue where e_MpParameterHostType='Preset' and ParamId=? and fk_ParameterHostId in (select pk_MpPluginPresetId from MpPluginPreset where PluginGuid=?)";
            var result = await MpDb.QueryAsync<MpParameterValue>(query, paramId, pluginGuid);
            return result;
        }


        public static async Task<MpParameterValue> GetParameterValueAsync(MpParameterHostType hostType, int paramHostId, string paramId) {
            string query = $"select * from MpParameterValue where e_MpParameterHostType=? and fk_ParameterHostId=? and ParamId=?";
            var result = await MpDb.QueryAsync<MpParameterValue>(query, hostType.ToString(), paramHostId, paramId);

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
            var result = await MpDb.QueryAsync<MpSyncHistory>(query, dg);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

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
            string query = "select * from MpContentToken where fk_MpCopyItemId=? and fk_MpActionId=? and MatchData=?";
            var result = await MpDb.QueryAsync<MpContentToken>(query, copyItemId, actionId, matchData);
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
            int result = await MpDb.QueryScalarAsync<int>(query, doid, formatName);
            return result > 0;
        }

        public static async Task<List<MpDataObjectItem>> GetDataObjectItemsByDataObjectIdAsync(int dobjid) {
            string query = "select * from MpDataObjectItem where fk_MpDataObjectId=?";
            var result = await MpDb.QueryAsync<MpDataObjectItem>(query, dobjid);
            return result;
        }

        public static async Task<List<MpDataObjectItem>> GetDataObjectItemsForFormatByDataObjectIdAsync(int dobjid, string format) {
            string query = "select * from MpDataObjectItem where fk_MpDataObjectId=? and ItemFormat=?";
            var result = await MpDb.QueryAsync<MpDataObjectItem>(query, dobjid, format);
            return result;
        }

        public static async Task<List<MpDataObjectItem>> GetDataObjectItemsForFormatByDataAsync(string format, string data) {
            string query = "select * from MpDataObjectItem where ItemFormat=? and ItemData=?";
            var result = await MpDb.QueryAsync<MpDataObjectItem>(query, format, data);
            return result;
        }

        #endregion

        #region MpSearchCriteriaItem

        public static async Task<List<MpSearchCriteriaItem>> GetCriteriaItemsByTagIdAsync(int tid) {
            string query = "select * from MpSearchCriteriaItem where fk_MpTagId=?";
            var result = await MpDb.QueryAsync<MpSearchCriteriaItem>(query, tid);
            return result;
        }

        public static async Task<int> GetCriteriaItemCountByTagIdAsync(int tid) {
            string query = "select COUNT(pk_MpSearchCriteriaItemId) from MpSearchCriteriaItem where fk_MpTagId=?";
            var result = await MpDb.QueryScalarAsync<int>(query, tid);
            return result;
        }

        #endregion

        #endregion



        #endregion

    }
}
