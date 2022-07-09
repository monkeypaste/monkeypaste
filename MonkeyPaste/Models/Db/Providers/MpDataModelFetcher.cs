using FFImageLoading.Helpers.Exif;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System.Text.RegularExpressions;

namespace MonkeyPaste {
    
    public static class MpDataModelFetcher {

        #region Select queries

        #region Base Select Queries

        public static T SelectModel<T>(string query, params object[] args) where T : new() {
            var result = SelectModels<T>(query, args);
            if (result == null || result.Count == 0) {
                return default(T);
            }
            return result[0];
        }

        public static List<T> SelectModels<T>(string query, params object[] args) where T : new() {
            var result = MpDb.Query<T>(query, args);
            return result;
        }

        public static async Task<T> SelectModelAsync<T>(string query, params object[] args) where T : new() {
            var result = await SelectModelsAsync<T>(query, args);
            if (result == null || result.Count == 0) {
                return default(T);
            }
            return result[0];
        }

        public static async Task<List<T>> SelectModelsAsync<T>(string query, params object[] args) where T : new() {
            var result = await MpDb.QueryAsync<T>(query, args);
            return result;
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

        public static MpIcon GetIconById(int iconId) {
            return SelectModel<MpIcon>("select * from MpIcon where pk_MpIconId=?", iconId);
        }

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

        public static async Task<MpApp> GetAppByPathAsync(string path) {
            string query = $"select * from MpApp where LOWER(SourcePath)=?";
            var result = await MpDb.QueryAsync<MpApp>(query, path.ToLower());
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        public static async Task<bool> IsAppRejectedAsync(string path) {
            string query = $"select count(*) from MpApp where LOWER(SourcePath)=? and IsAppRejected=1";
            var result = await MpDb.QueryScalarAsync<int>(query, path.ToLower());
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

        public static async Task<MpSource> GetSourceByMembersAsync(int appId, int urlId, int copyItemTransactionId = 0) {
            string query = $"select * from MpSource where fk_MpAppId=? and fk_MpUrlId=? and fk_MpCopyItemTransactionId=?";
            var result = await MpDb.QueryAsync<MpSource>(query, appId, urlId,copyItemTransactionId);
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
            // NOTE this is used to safely try to get item instead of MpDb.GetItemAsync 
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

        public static async Task<int> GetTotalCopyItemCountAsync() {
            string query = "select count(pk_MpCopyItemId) from MpCopyItem";
            var result = await MpDb.QueryScalarAsync<int>(query);
            return result;
        }


        #endregion MpCopyItem

        #region MpTextToken
        public static List<string> ParseTextTemplateGuidsByCopyItemId(MpCopyItem ci) {
            var textTemplateGuids = new List<string>();
            if (ci == null) {
                return textTemplateGuids;
            }
            var encodedTemplateGuids = MpRegEx.RegExLookup[MpRegExType.EncodedTextTemplate].Matches(ci.ItemData);
            if (encodedTemplateGuids.Count == 0) {
                return textTemplateGuids;
            }
            foreach (Match encodedTemplateGuid in encodedTemplateGuids) {
                var guidMatch = MpRegEx.RegExLookup[MpRegExType.Guid].Match(encodedTemplateGuid.Value);
                textTemplateGuids.Add(guidMatch.Value);
            }
            textTemplateGuids = textTemplateGuids.Distinct().ToList();
            return textTemplateGuids;
        }

        public static async Task<List<MpTextTemplate>> ParseTextTemplatesByCopyItemIdAsync(MpCopyItem ci) {
            if(!ci.ItemData.Contains(MpTextTemplate.TextTemplateOpenTokenRtf)) {
                // pre-pass data because this may be a bottle neck
                return new List<MpTextTemplate>();
            }

            var templateGuids = ParseTextTemplateGuidsByCopyItemId(ci);
            var result = await GetTextTemplatesByGuidsAsync(templateGuids);
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

        #endregion

        #region MpTextAnnotation

        public static async Task<List<MpTextAnnotation>> GetTextAnnotationsAsync(int ciid) {
            string query = @"select * from MpTextAnnotation where fk_MpCopyItemId=?";
            var result = await MpDb.QueryAsync<MpTextAnnotation>(query);
            return result;
        }

        public static async Task<MpTextAnnotation> GetTextAnnotationByDataAsync(int ciid, int sid, string label, string matchValue, string description) {
            string query = @"select * from MpTextAnnotation where fk_MpCopyItemId=? and fk_MpSourceId=? and Label=? and MatchValue=? and Description=?";
            var result = await MpDb.QueryAsync<MpTextAnnotation>(query, ciid, sid,label,matchValue, description);
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

        #region MpImageAnnotation

        public static async Task<List<MpCopyItemTransaction>> GetCopyItemTransactionsByCopyItemIdAsync(int ciid) {
            string query = string.Format(@"select * from MpCopyItemTransaction where fk_MpCopyItemId=?");
            var result = await MpDb.QueryAsync<MpCopyItemTransaction>(query, ciid);
            return result;
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

        #endregion

        #region MpShortcut

        public static async Task<MpShortcut> GetShortcutAsync(int ciid, int tagId, int aiid) {
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

        public static async Task<string> GetShortcutKeystringAsync(MpShortcutType shortcutType, int commandId = 0) {
            string query = string.Format(@"select KeyString from MpShortcut where e_ShortcutTypeId=? and fk_MpCommandId=?");
            var result = await MpDb.QueryScalarAsync<string>(query, (int)shortcutType, commandId);
            return result;
        }

        public static string GetShortcutKeystring(MpShortcutType shortcutType, int commandId = 0) {
            string query = string.Format(@"select KeyString from MpShortcut where e_ShortcutTypeId=? and fk_MpCommandId=?");
            var result = MpDb.QueryScalar<string>(query, (int)shortcutType, commandId);
            return result;
        }

        public static async Task<List<MpShortcut>> GetAllShortcutsAsync() {
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


        public static async Task<MpPluginPresetParameterValue> GetPluginPresetValueAsync(int presetid, int paramEnumId) {
            string query = $"select * from MpPluginPresetParameterValue where fk_MpPluginPresetId=? and ParamId=?";
            var result = await MpDb.QueryAsync<MpPluginPresetParameterValue>(query, presetid, paramEnumId);
            if (result == null || result.Count == 0) {
                return null;
            }
            return result[0];
        }

        #endregion MpAnalyticItem

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

        #region MpBox

        public static async Task<MpBox> GetBoxByTypeAndObjIdAsync(MpBoxType boxType, int objId) {
            string query = string.Format(@"select * from MpBox where e_MpBoxTypeId=? and fk_BoxObjectId=?");
            var result = await MpDb.QueryAsync<MpBox>(query, (int)boxType,objId);
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

        #endregion
    }
}
