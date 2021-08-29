using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using System.Threading;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpUrlDomain : MpDbModelBase, MpISyncableDbObject {
        #region Static Cache
        private static List<MpUrlDomain> _AllUrlDomainList = null;
        public static int TotalUrlDomainCount = 0;

        public static async Task<List<MpUrlDomain>> GetAllUrlDomains() {
            if (_AllUrlDomainList == null) {
                _AllUrlDomainList = await MpDb.Instance.QueryAsync<MpUrlDomain>("select * from MpUrlDomain", null);
            }
            return _AllUrlDomainList;
        }

        public static MpUrlDomain GetUrlDomainById(int Id) {
            if (_AllUrlDomainList == null) {
                GetAllUrlDomains();
            }
            var udbpl = _AllUrlDomainList.Where(x => x.Id == Id).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static MpUrlDomain GetUrlDomainByPath(string urlDomain) {
            if (_AllUrlDomainList == null) {
                GetAllUrlDomains();
            }
            var udbpl = _AllUrlDomainList.Where(x => x.UrlDomainPath.ToLower().Contains(urlDomain)).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static bool IsUrlDomainRejectedByHandle(string fullUrl) {
            return false;
            //string urlDomainPath = MpHelpers.Instance.GetUrlDomain(fullUrl);
            //foreach (MpUrlDomain urlDomain in GetAllUrlDomains()) {
            //    if (urlDomain.UrlDomainPath == urlDomainPath && urlDomain.IsUrlDomainRejected) {
            //        return true;
            //    }
            //}
            //return false;
        }
        public static async Task<List<MpUrlDomain>> GetAllRejectedUrlDomains() {
            await Task.Run(() => Thread.Sleep(10));
            return new List<MpUrlDomain>();
            //return GetAllUrlDomains().Where(x => x.IsUrlDomainRejected == true).ToList();
        }
        #endregion

        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpUrlDomainId")]
        public override int Id { get; set; }

        [Column("MpUrlDomainGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid UrlDomainGuid {
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

        public string UrlDomainPath { get; set; } = string.Empty;
        public string UrlDomainTitle { get; set; } = string.Empty;

        [Column("IsUrlDomainRejected")]
        public int IsRejected { get; set; } = 0;

        [ForeignKey(typeof(MpIcon))]
        [Column("fk_MpIconId")]
        public int FavIconId { get; set; } = 0;

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpIcon FavIcon { get; set; }

        #endregion

        #region Fk objects
        [OneToMany]
        public List<MpUrl> Urls { get; set; }
        #endregion

        [Ignore]
        public bool IsUrlDomainRejected
        {
            get
            {
                return IsRejected == 1;
            }
            set
            {
                if (IsUrlDomainRejected != value)
                {
                    IsRejected = value ? 1 : 0;
                }
            }
        }

        public async Task<object> CreateFromLogs(string urlDomainGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var urlDomain = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpUrlDomain", urlDomainGuid) as MpUrlDomain;

            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpUrlDomainGuid":
                        urlDomain.UrlDomainGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "fk_MpIconId":
                        urlDomain.FavIcon = MpDb.Instance.GetDbObjectByTableGuid("MpIcon", li.AffectedColumnValue) as MpIcon;
                        urlDomain.FavIconId = urlDomain.FavIcon.Id;
                        break;
                    case "UrlDomainPath":
                        urlDomain.UrlDomainPath = li.AffectedColumnValue;
                        break;
                    case "UrlDomainTitle":
                        urlDomain.UrlDomainTitle = li.AffectedColumnValue;
                        break;
                    case "IsUrlDomainRejected":
                        urlDomain.IsUrlDomainRejected = li.AffectedColumnValue == "1";
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            return urlDomain;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var urld = new MpUrlDomain() {
                UrlDomainGuid = System.Guid.Parse(objParts[0])
            };

            urld.FavIcon = MpDb.Instance.GetDbObjectByTableGuid("MpIcon", objParts[1]) as MpIcon;
            urld.FavIconId = urld.FavIcon.Id;

            urld.UrlDomainPath = objParts[2];
            urld.UrlDomainTitle = objParts[3];
            urld.IsUrlDomainRejected = objParts[4] == "1";
            return urld;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}",
                ParseToken,
                UrlDomainGuid.ToString(),
                FavIcon.IconGuid.ToString(),
                UrlDomainPath,
                UrlDomainTitle,
                IsUrlDomainRejected ? "1" : "0");
        }

        public Type GetDbObjectType() {
            return typeof(MpUrlDomain);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            MpUrlDomain other = null;
            if (drOrModel is MpUrlDomain) {
                other = drOrModel as MpUrlDomain;
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpUrlDomain();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(UrlDomainGuid, other.UrlDomainGuid,
                "MpUrlDomainGuid",
                diffLookup,
                UrlDomainGuid.ToString());
            diffLookup = CheckValue(FavIconId, other.FavIconId,
                "fk_MpIconId",
                diffLookup,
                FavIcon.IconGuid.ToString());
            diffLookup = CheckValue(UrlDomainPath, other.UrlDomainPath,
                "UrlDomainPath",
                diffLookup);
            diffLookup = CheckValue(UrlDomainTitle, other.UrlDomainTitle,
                "UrlDomainTitle",
                diffLookup);
            diffLookup = CheckValue(IsUrlDomainRejected, other.IsUrlDomainRejected,
                "IsUrlDomainRejected",
                diffLookup,
                IsUrlDomainRejected ? "1" : "0");

            return diffLookup;
        }

        public MpUrlDomain() {
        }
    }
}
