using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpUrl :
        MpDbModelBase,
        MpISourceRef,
        MpILabelText,
        MpIUriSource {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpUrlId")]
        public override int Id { get; set; }

        [Column("MpUrlGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpAppId")]
        public int AppId { get; set; }

        [Indexed]
        public string UrlPath { get; set; } = string.Empty;

        public string UrlDomainPath { get; set; } = string.Empty;

        public string UrlTitle { get; set; } = string.Empty;

        public int UrlRejected { get; set; } = 0;

        public int DomainRejected { get; set; } = 0;

        [Column("fk_MpIconId")]
        public int IconId { get; set; } = 0;

        #endregion

        #region Properties

        [Ignore]
        public Guid UrlGuid {
            get {
                if (string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }


        [Ignore]
        public bool IsDomainRejected {
            get {
                return DomainRejected == 1;
            }
            set {
                DomainRejected = value ? 1 : 0;
            }
        }

        [Ignore]
        public bool IsUrlRejected {
            get {
                return IsDomainRejected || UrlRejected == 1;
            }
            set {
                UrlRejected = value ? 1 : 0;
            }
        }

        #endregion

        #region MpILabelText Implementation
        string MpILabelText.LabelText => UrlTitle;

        #endregion

        #region MpIUriSource Implementation
        string MpIUriSource.Uri => UrlPath;
        #endregion


        #region MpISourceRef Implementation
        [Ignore]
        int MpISourceRef.Priority => (int)MpTransactionSourceType.Url;

        [Ignore]
        int MpISourceRef.SourceObjId => Id;

        [Ignore]
        MpTransactionSourceType MpISourceRef.SourceType => MpTransactionSourceType.Url;

        public object IconResourceObj => IconId;
        #endregion

        public static async Task<MpUrl> CreateAsync(
            string urlPath = "",
            string title = "",
            string domain = "",
            int iconId = 0,
            int appId = 0,
            bool suppressWrite = false) {
            var dupCheck = await MpDataModelProvider.GetUrlByPathAsync(urlPath);
            if (dupCheck != null) {
                dupCheck = await MpDataModelProvider.GetItemAsync<MpUrl>(dupCheck.Id);
                dupCheck.WasDupOnCreate = true;
                return dupCheck;
            }

            var newUrl = new MpUrl() {
                UrlGuid = System.Guid.NewGuid(),
                AppId = appId,
                UrlPath = urlPath,
                UrlTitle = title,
                UrlDomainPath = domain
            };

            // NOTE omitting icon fetch since this create only called from builder and it should already exist
            // or there's an error and this will just make it happen again
            //if (iconId == 0) {
            //    string favIconImg64 = await MpUrlHelpers.GetUrlFavIconAsync(urlPath);
            //    MpIcon icon = await Mp.Services.IconBuilder.CreateAsync(
            //            iconBase64: favIconImg64,
            //            suppressWrite: suppressWrite);
            //    iconId = icon.Id;
            //}


            newUrl.IconId = iconId;
            if (newUrl.IconId == 0) {
                newUrl.IconId = MpDefaultDataModelTools.ThisAppIconId;
            }

            if (!suppressWrite) {
                await newUrl.WriteToDatabaseAsync();
            }

            return newUrl;
        }
        public MpUrl() { }


        public async Task<object> CreateFromLogs(string urlGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var urlDr = await MpDb.GetDbObjectByTableGuidAsync("MpUrl", urlGuid);
            MpUrl url = null;
            if (urlDr == null) {
                url = new MpUrl();
            } else {
                url = urlDr as MpUrl;
            }
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpUrlGuid":
                        url.UrlGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "fk_MpIconinId":
                        var icon = await MpDb.GetDbObjectByTableGuidAsync("MpIcon", li.AffectedColumnValue) as MpIcon;
                        url.IconId = icon.Id;
                        break;
                    case "UrlPath":
                        url.UrlPath = li.AffectedColumnValue;
                        break;
                    case "UrlTitle":
                        url.UrlTitle = li.AffectedColumnValue;
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            return url;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var url = new MpUrl() {
                UrlGuid = System.Guid.Parse(objParts[0])
            };

            var icon = await MpDb.GetDbObjectByTableGuidAsync("MpIcon", objParts[1]) as MpIcon;
            url.IconId = icon.Id;
            url.UrlPath = objParts[2];
            url.UrlTitle = objParts[3];
            return url;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}",
                ParseToken,
                UrlGuid.ToString(),
                MpDataModelProvider.GetItem<MpIcon>(IconId).Guid,
                UrlPath,
                UrlTitle);
        }

        public Type GetDbObjectType() {
            return typeof(MpUrl);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            MpUrl other = null;
            if (drOrModel is MpUrl) {
                other = drOrModel as MpUrl;
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpUrl();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(UrlGuid, other.UrlGuid,
                "MpUrlGuid",
                diffLookup,
                UrlGuid.ToString());
            diffLookup = CheckValue(IconId, other.IconId,
                "fk_MpIconId",
                diffLookup,
                MpDataModelProvider.GetItem<MpIcon>(IconId).Guid);
            diffLookup = CheckValue(UrlPath, other.UrlPath,
                "UrlPath",
                diffLookup);
            diffLookup = CheckValue(UrlTitle, other.UrlTitle,
                "UrlTitle",
                diffLookup);
            return diffLookup;
        }

    }
}
