using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpUrl : MpDbModelBase {
        private static List<MpUrl> _AllUrlList = null;
        public static int TotalUrlCount = 0;

        public int UrlId { get; set; }
        public Guid UrlGuid { get; set; }

        public int UrlDomainId { get; set; }
        public string UrlPath { get; set; }
        public string UrlTitle { get; set; }

        public MpUrlDomain UrlDomain { get; set; }

        public MpUrl(string urlPath, string urlTitle) {
            UrlGuid = Guid.NewGuid();
            UrlPath = urlPath;
            UrlTitle = urlTitle;
            UrlDomain = MpUrlDomain.GetUrlDomainByPath(MpHelpers.Instance.GetUrlDomain(urlPath));            
        }
        public MpUrl(int urlId) {
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
            if (UrlId == 0) {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpUrl(MpUrlGuid,UrlPath,UrlTitle,fk_MpUrlDomainId) values(@ug,@up,@ut,@udid)",
                    new Dictionary<string, object> {
                        { "@ug", UrlGuid.ToString() },
                        { "@up", UrlPath },
                        { "@ut", UrlTitle },
                        { "@udid", UrlDomainId }
                    },UrlGuid.ToString());
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
                    },UrlGuid.ToString());
            }

            var urldl = GetAllUrls().Where(x => x.UrlId == UrlId).ToList();
            if (urldl.Count > 0) {
                _AllUrlList[_AllUrlList.IndexOf(urldl[0])] = this;
            } else {
                _AllUrlList.Add(this);
            }
        }
        
        public void DeleteFromDatabase() {            
            MpDb.Instance.ExecuteWrite(
                "delete from MpUrl where pk_MpUrlId=@tid",
                new Dictionary<string, object> {
                    { "@tid", UrlId }
                },UrlGuid.ToString());

            var urldl = GetAllUrls().Where(x => x.UrlId == UrlId).ToList();
            if (urldl.Count > 0) {
                _AllUrlList.RemoveAt(_AllUrlList.IndexOf(urldl[0]));
            }
        }
    }
}
