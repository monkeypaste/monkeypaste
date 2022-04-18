using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;
using MonkeyPaste.Plugin;

namespace MonkeyPaste {
    public class MpUrl : MpDbModelBase, MpISourceItem {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpUrlId")]
        public override int Id { get; set; }

        [Column("MpUrlGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Indexed]
        public string UrlPath { get; set; }

        public string UrlDomainPath { get; set; }

        public string UrlTitle { get; set; }

        public int UrlRejected { get; set; } = 0;

        public int DomainRejected { get; set; } = 0;

        [ForeignKey(typeof(MpIcon))]
        [Column("fk_MpIconId")]
        public int IconId { get; set; } = 0;

        #endregion

        #region Fk Objects

        //[OneToOne(CascadeOperations = CascadeOperation.All)]
        //public MpIcon Icon { get; set; }

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
                return UrlRejected == 1;
            }
            set {
                UrlRejected = value ? 1 : 0;
            }
        }

        #endregion

        #region MpICopyItemSource Implementation
        [Ignore]
        public bool IsUser => false;
        [Ignore]
        public bool IsUrl => true;

        [Ignore]
        public bool IsRejected => IsDomainRejected;

        [Ignore]
        public bool IsSubRejected => IsUrlRejected;

        [Ignore]
        public bool IsDll => false;

        [Ignore]
        public bool IsExe => false;

        [Ignore]
        public string SourcePath => UrlPath;

        [Ignore]
        public string SourceName => UrlPath;

        [Ignore]
        public int RootId => Id;
        #endregion

        public static async Task<MpUrl> Create(
            string urlPath = "",
            string urlTitle = "",
            string urlIconPath = "",
            int urlIconId = 0,
            bool suppressWrite = false) {
            var dupCheck = await MpDataModelProvider.GetUrlByPath(urlPath);
            if(dupCheck != null) {
                dupCheck = await MpDb.GetItemAsync<MpUrl>(dupCheck.Id);
                return dupCheck;
            }

            urlTitle = string.IsNullOrEmpty(urlTitle) ? await MpUrlHelpers.GetUrlTitle(urlPath) : urlTitle;

            var domainStr = MpUrlHelpers.GetUrlDomain(urlPath);
            var newUrl = new MpUrl() {
                UrlGuid = System.Guid.NewGuid(),
                UrlPath = urlPath,
                UrlTitle = urlTitle,
                UrlDomainPath = domainStr
            };
            if(string.IsNullOrEmpty(domainStr)) {
                MpConsole.WriteTraceLine("Ignoring mproperly formatted source url: " + urlPath);
                return null;
            }
            MpIcon icon = null;
            if(urlIconId > 0) {
                icon = await MpDb.GetItemAsync<MpIcon>(urlIconId);
            } else if(!string.IsNullOrEmpty(urlIconPath)) {
                icon = await MpIcon.Create2(
                    iconUrl: urlIconPath, 
                    suppressWrite: suppressWrite);
            }
            if(icon == null) {
                string favIconImg64 = await MpUrlHelpers.GetUrlFavIconAsync(domainStr);
                if (favIconImg64 == MpBase64Images.UnknownFavIcon || favIconImg64 == null) {
                    //url has no and result is google's default
                    favIconImg64 = MpBase64Images.QuestionMark;
                }
                icon = await MpIcon.Create(
                    iconImgBase64: favIconImg64,
                    suppressWrite: suppressWrite);
            }
            
            newUrl.IconId = icon.Id;
            if (newUrl.IconId == 0) {
                newUrl.IconId = MpPreferences.ThisAppSource.PrimarySource.IconId;
            }

            if(!suppressWrite) {
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
                MpDb.GetItem<MpIcon>(IconId).Guid,
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
                MpDb.GetItem<MpIcon>(IconId).Guid);
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
