using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpUrl : MpDbModelBase, MpICopyItemSource {
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpUrlId")]
        public override int Id { get; set; }

        [Column("MpUrlGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

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

        public int IsRejected { get; set; } = 0;

        [Ignore]
        public bool IsDomainRejected {
            get {
                return IsRejected == 1;
            }
            set {
                IsRejected = value ? 1 : 0;
            }
        }

        [Indexed]
        public string UrlPath { get; set; }
        public string UrlTitle { get; set; }

        [ForeignKey(typeof(MpIcon))]
        [Column("fk_MpIconId")]
        public int IconId { get; set; } = 0;

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpIcon Icon { get; set; }

        public static MpUrl Create(string urlPath,string urlTitle) {
            var dupCheck = MpDb.Instance.GetItems<MpUrl>().Where(x => x.UrlPath == urlPath).FirstOrDefault();
            if(dupCheck != null) {
                return dupCheck;
            }
            
            var newUrl = new MpUrl() {
                UrlGuid = System.Guid.NewGuid(),
                UrlPath = urlPath,
                UrlTitle = urlTitle
            };
            var domainStr = MpHelpers.Instance.GetUrlDomain(urlPath);
            if(string.IsNullOrEmpty(domainStr)) {
                MpConsole.WriteTraceLine("Ignoring mproperly formatted source url: " + urlPath);
                return null;
            } else {
                var favIconImg64 = MpHelpers.Instance.GetUrlFavicon(domainStr);
                newUrl.Icon = MpIcon.Create(favIconImg64);
                newUrl.IconId = newUrl.Icon.Id;
            }

            MpDb.Instance.AddItem<MpUrl>(newUrl);
            
            return newUrl;
        }
        public MpUrl() { }

        public static async Task<MpUrl> GetUrlByPath(string urlPath) {
            var allUrls = await MpDb.Instance.GetItemsAsync<MpUrl>();
            return allUrls.Where(x => x.UrlPath.ToLower() == urlPath.ToLower()).FirstOrDefault();
        }

        public static async Task<MpUrl> GetUrlById(int urlId) {
            var allUrls = await MpDb.Instance.GetItemsAsync<MpUrl>();
            var udbpl = allUrls.Where(x => x.Id == urlId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }


        #region MpICopyItemSource Implementation
        [Ignore]
        public MpIcon SourceIcon {
            get {
                if (Icon == null) {
                    return null;
                }
                return Icon;
            }
        }

        [Ignore]
        public string SourcePath => UrlPath;

        [Ignore]
        public string SourceName => UrlPath;

        [Ignore]
        public int RootId => Id;
        #endregion

        public async Task<object> CreateFromLogs(string urlGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var urlDr = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpUrl", urlGuid);
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
                        url.Icon = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpIcon", li.AffectedColumnValue) as MpIcon;
                        url.IconId = url.Icon.Id;
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

            url.Icon = MpDb.Instance.GetDbObjectByTableGuid("MpIcon", objParts[1]) as MpIcon;
            url.IconId = url.Icon.Id;
            url.UrlPath = objParts[2];
            url.UrlTitle = objParts[3];
            return url;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}",
                ParseToken,
                UrlGuid.ToString(),
                Icon.IconGuid.ToString(),
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
                Icon.IconGuid.ToString());
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
