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

        [ForeignKey(typeof(MpUrlDomain))]
        [Column("fk_MpUrlDomainId")]
        public int UrlDomainId { get; set; }

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpUrlDomain UrlDomain { get; set; }

        [Indexed]
        public string UrlPath { get; set; }
        public string UrlTitle { get; set; }

        public static MpUrl Create(string urlPath,string urlTitle) {
            var dupCheck = MpDb.Instance.GetItems<MpUrl>().Where(x => x.UrlPath == urlPath).FirstOrDefault();
            if(dupCheck != null) {
                return dupCheck;
            }
            var domainStr = MpHelpers.Instance.GetUrlDomain(urlPath);
            var newUrl = new MpUrl() {
                UrlGuid = System.Guid.NewGuid(),
                UrlDomain = MpUrlDomain.Create(domainStr),
                UrlPath = urlPath,
                UrlTitle = urlTitle
            };
            newUrl.UrlDomainId = newUrl.UrlDomain.Id;

            MpDb.Instance.AddItem<MpUrl>(newUrl);
            
            return newUrl;
        }
        public MpUrl() {
        }
        public MpUrl(string urlPath, string urlTitle) : this() {
            UrlPath = urlPath;
            UrlTitle = urlTitle;
            UrlDomain = MpUrlDomain.GetUrlDomainByPath(MpHelpers.Instance.GetUrlDomain(urlPath));            
        }

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
                if (UrlDomain == null || UrlDomain.FavIcon == null) {
                    return null;
                }
                return UrlDomain.FavIcon;
            }
        }

        [Ignore]
        public string SourcePath => UrlPath;

        [Ignore]
        public string SourceName => UrlPath;

        [Ignore]
        public int RootId {
            get {
                if (UrlDomain == null) {
                    return 0;
                }
                return UrlDomain.Id;
            }
        }

        [Ignore]
        public bool IsSubSource => true;

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
                    case "fk_MpUrlDomainId":
                        url.UrlDomain = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpUrlDomain", li.AffectedColumnValue) as MpUrlDomain;
                        url.UrlDomainId = url.UrlDomain.Id;
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

            url.UrlDomain = MpDb.Instance.GetDbObjectByTableGuid("MpUrlDomain", objParts[1]) as MpUrlDomain;
            url.UrlDomainId = url.UrlDomain.Id;
            url.UrlPath = objParts[2];
            url.UrlTitle = objParts[3];
            return url;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}",
                ParseToken,
                UrlGuid.ToString(),
                UrlDomain.UrlDomainGuid.ToString(),
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
            diffLookup = CheckValue(UrlDomainId, other.UrlDomainId,
                "fk_MpUrlDomainId",
                diffLookup,
                UrlDomain.UrlDomainGuid.ToString());
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
