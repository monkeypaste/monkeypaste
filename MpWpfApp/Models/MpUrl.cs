using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpUrl : MpDbModelBase, MonkeyPaste.MpISyncableDbObject {
        private static List<MpUrl> _AllUrlList = null;
        public static int TotalUrlCount = 0;

        public int UrlId { get; set; }
        public Guid UrlGuid { get; set; }

        public int UrlDomainId { get; set; }
        public string UrlPath { get; set; }
        public string UrlTitle { get; set; }

        public MpUrlDomain UrlDomain { get; set; }

        public MpUrl() { }

        public MpUrl(string urlPath, string urlTitle) : this() {
            UrlGuid = Guid.NewGuid();
            UrlPath = urlPath;
            UrlTitle = urlTitle;
            UrlDomain = MpUrlDomain.GetUrlDomainByPath(MpHelpers.Instance.GetUrlDomain(urlPath));            
        }
        public MpUrl(int urlId) : this() {
            var dt = MpDb.Instance.Execute(
                "select * from MpUrl where pk_MpUrlId=@urlid",
                new Dictionary<string, object> {
                    { "@urlid", urlId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpUrl(DataRow dr) {
            LoadDataRow(dr);
        }
        public static List<MpUrl> GetAllUrls() {
            if(_AllUrlList == null) {
                _AllUrlList = new List<MpUrl>();
                DataTable dt = MpDb.Instance.Execute("select * from MpUrl", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow r in dt.Rows) {
                        _AllUrlList.Add(new MpUrl(r));
                    }
                }
            }
            
            return _AllUrlList;
        }
        public static MpUrl GetUrlById(int urlId) {
            if (_AllUrlList == null) {
                GetAllUrls();
            }
            var udbpl = _AllUrlList.Where(x => x.UrlId == urlId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public override void LoadDataRow(DataRow dr) {
            UrlId = Convert.ToInt32(dr["pk_MpUrlId"].ToString());
            UrlGuid = Guid.Parse(dr["MpUrlGuid"].ToString());
            UrlPath = dr["UrlPath"].ToString();
            UrlTitle = dr["UrlTitle"].ToString();
            UrlDomainId = Convert.ToInt32(dr["fk_MpUrlDomainId"].ToString());
            UrlDomain = MpUrlDomain.GetUrlDomainById(UrlDomainId);
        }
        
        public override void WriteToDatabase() {
            if (IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }
        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (UrlId == 0) {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpUrl(MpUrlGuid,UrlPath,UrlTitle,fk_MpUrlDomainId) values(@ug,@up,@ut,@udid)",
                    new Dictionary<string, object> {
                        { "@ug", UrlGuid.ToString() },
                        { "@up", UrlPath },
                        { "@ut", UrlTitle },
                        { "@udid", UrlDomainId }
                    },UrlGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
                UrlId = MpDb.Instance.GetLastRowId("MpUrl", "pk_MpUrlId");
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpUrl set MpUrlGuid=@ug, UrlPath=@up, UrlTitle=@ut, fk_MpUrlDomainId=@udid where pk_MpUrlId=@uid",
                    new Dictionary<string, object> {
                        { "@ug", UrlGuid.ToString() },
                        { "@uid",UrlId },
                        { "@up", UrlPath },
                        { "@ut", UrlTitle },
                        { "@udid", UrlDomainId }
                    },UrlGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
            }

            var urldl = GetAllUrls().Where(x => x.UrlId == UrlId).ToList();
            if (urldl.Count > 0) {
                _AllUrlList[_AllUrlList.IndexOf(urldl[0])] = this;
            } else {
                _AllUrlList.Add(this);
            }
        }

        public void DeleteFromDatabase() {
            if (IsSyncing) {
                DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                DeleteFromDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }
        public override void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            MpDb.Instance.ExecuteWrite(
                "delete from MpUrl where pk_MpUrlId=@tid",
                new Dictionary<string, object> {
                    { "@tid", UrlId }
                },UrlGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);

            var urldl = GetAllUrls().Where(x => x.UrlId == UrlId).ToList();
            if (urldl.Count > 0) {
                _AllUrlList.RemoveAt(_AllUrlList.IndexOf(urldl[0]));
            }
        }

        public async Task<object> CreateFromLogs(string urlGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            await Task.Delay(1);
            var urlDr = MpDb.Instance.GetDbDataRowByTableGuid("MpUrl", urlGuid);
            MpUrl url = null;
            if (urlDr == null) {
                url = new MpUrl();
            } else {
                url = new MpUrl(urlDr);
            }
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpUrlGuid":
                        url.UrlGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "fk_MpUrlDomainId":
                        url.UrlDomain = MpDb.Instance.GetDbObjectByTableGuid("MpUrlDomain", li.AffectedColumnValue) as MpUrlDomain;
                        url.UrlDomainId = url.UrlDomain.UrlDomainId;
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
            url.UrlDomainId = url.UrlDomain.UrlDomainId;
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
            if (drOrModel is DataRow) {
                other = new MpUrl(drOrModel as DataRow);
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
